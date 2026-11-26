using System.Collections.Generic;
using System.Linq;
using MuseumOfLies.Common;
using UnityEngine;

namespace MuseumOfLies.CentralHall
{
    /// <summary>
    /// 중앙 홀: 네 개의 똑같은 자동문. 어느 문 뒤에 어떤 전시가 있는지는 방문마다 무작위 (MuseumSession).
    /// 기록: 입장 질문, 문이 열린 순서, 실제로 들어간 문(위치)과 전시, 첫 선택까지 시간,
    ///       첫 선택 전 각 문을 바라본 시간(gaze dwell — 머리 방향 기준).
    /// </summary>
    public class CentralHallController : MonoBehaviour
    {
        public XRRig rig;
        public List<Gate> gates = new List<Gate>();
        public EntrySurvey entrySurvey;
        public Transform defaultSpawn;
        public AudioSource roomTone;

        [Header("Experiment")]
        [Tooltip("Shuffle which hall is behind which door for each visit (recommended)")]
        public bool randomizeMapping = true;
        [Tooltip("Head-direction cone (deg) that counts as looking at a door")]
        public float gazeConeDeg = 12f;

        public static readonly Dictionary<string, string> HallScene = new Dictionary<string, string>
        {
            { "Weight", "Weight" }, { "Body", "Body" }, { "Ability", "Ability" }, { "DoNothing", "DoNothing" }
        };
        public static readonly Dictionary<string, string> HallName = new Dictionary<string, string>
        {
            { "Weight", "무게를 팝니다" }, { "Body", "내 몸은 몇 개까지 가능할까?" }, { "Ability", "내 손이 먼저 알고 있었다" }, { "DoNothing", "아무것도 하지 않는 게임" }
        };

        ExperimentLogger _log;
        float _t0;
        bool _entering;
        readonly float[] _gaze = new float[4];
        bool _choiceMade;
        readonly HashSet<int> _unavailableShown = new HashSet<int>();

        void Start()
        {
            if (rig == null) rig = FindFirstObjectByType<XRRig>();
            bool returning = MuseumSession.Started;
            if (!returning) MuseumSession.Begin(randomizeMapping);

            _log = new ExperimentLogger("CentralHall", randomizeMapping ? "random_mapping" : "fixed_mapping");
            _t0 = Time.time;
            _choiceMade = MuseumSession.GateOrder.Count > 0;
            _log.Log(0, 0f, returning ? "return" : "visit_start",
                $"visit={MuseumSession.VisitId};mapping={MuseumSession.MappingString};completed={string.Join("|", MuseumSession.Completed)};last_gate={MuseumSession.LastGate}");

            foreach (var g in gates)
            {
                g.SetCompleted(MuseumSession.Completed.Contains(MuseumSession.GateToHall[g.index]));
                g.ShowComingSoon(false);
                var gate = g;
                g.door.Opened += () => _log.Log(MuseumSession.GateOrder.Count, Time.time - _t0, "door_open", $"gate={gate.positionLabel}");
            }

            // spawn: in front of the gate you came out of, otherwise the entrance
            Transform spawn = defaultSpawn;
            if (returning && MuseumSession.LastGate >= 0)
            {
                var g = gates.FirstOrDefault(x => x.index == MuseumSession.LastGate);
                if (g && g.returnSpawn) spawn = g.returnSpawn;
            }
            if (spawn && rig && rig.origin) rig.origin.SetPositionAndRotation(spawn.position, spawn.rotation);

            if (entrySurvey)
            {
                bool ask = MuseumSession.EntryConfidence < 0;
                entrySurvey.gameObject.SetActive(ask);
                entrySurvey.Answered += v => _log.Log(0, Time.time - _t0, "entry_confidence", v.ToString());
            }
            if (roomTone) { roomTone.clip = ProceduralSfx.RoomTone(); roomTone.loop = true; roomTone.Play(); }
        }

        void Update()
        {
            if (rig == null || rig.head == null || _entering) return;
            Vector3 head = rig.head.position;

            // gaze dwell before the very first choice
            if (!_choiceMade)
                foreach (var g in gates)
                {
                    Vector3 to = g.transform.position + Vector3.up * 1.5f - head;
                    if (Vector3.Angle(rig.head.forward, to) < gazeConeDeg) _gaze[g.index] += Time.deltaTime;
                }

            foreach (var g in gates)
            {
                if (!g.door.IsOpen) continue;
                if (g.DepthInside(head) > g.enterDepth) { Enter(g); break; }
            }
        }

        void Enter(Gate g)
        {
            string hall = MuseumSession.GateToHall[g.index];
            string scene = HallScene[hall];
            bool available = SceneTransition.CanLoad(scene);
            if (!available && _unavailableShown.Contains(g.index)) return;
            if (!available) _unavailableShown.Add(g.index);
            int order = MuseumSession.GateOrder.Count + 1;
            string gaze = _choiceMade ? "" : ";gaze=" + string.Join("|", gates.OrderBy(x => x.index).Select(x => $"{x.positionLabel}:{_gaze[x.index]:F1}"));
            _log.Log(order, Time.time - _t0, available ? "gate_enter" : "gate_enter_unavailable",
                $"gate={g.positionLabel};hall={hall};order={order};first_choice={(!_choiceMade)}{gaze}");

            if (!available) { g.ShowComingSoon(true); return; }  // hall not built yet → message at the vestibule end
            _entering = true;
            _choiceMade = true;
            MuseumSession.Enter(g.index);
            _log.Close();
            SceneTransition.Load(scene);
        }

        void OnDestroy() { _log?.Close(); }
    }
}
