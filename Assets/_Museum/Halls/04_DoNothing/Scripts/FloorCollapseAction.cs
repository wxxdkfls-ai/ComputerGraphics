using System.Collections.Generic;
using MuseumOfLies.Common;
using UnityEngine;

namespace MuseumOfLies.DoNothing
{
    /// <summary>
    /// [새 제안] 공간(높이) 자극 — 발밑이 꺼진다.
    /// 플레이어가 선 좁은 슬래브(버튼 받침대 포함)만 남기고 주변 바닥 타일이 가까운 순서로 떨어져
    /// 깊은 수직 공간이 드러난다. 내려다보면(고개 아래 회전) 초기화. 카메라는 절대 움직이지 않는다(멀미 방지).
    /// 참고: Meehan et al. 2002 — 가상 낭떠러지("pit room")에서 실제 심박 상승.
    /// </summary>
    public class FloorCollapseAction : StimulusAction
    {
        public Transform tilesRoot;
        public AudioSource rumbleAudio;
        public Vector3 center = new Vector3(0f, 0f, 0.6f);
        public float firstDelay = 0.4f;
        public float delayPerMeter = 0.35f;
        public float killDepth = -22f;

        struct Tile { public Transform t; public Vector3 pos; public Quaternion rot; public float delay; public float vel; public Vector3 spin; }
        readonly List<Tile> _tiles = new List<Tile>();
        float _time;

        protected override void Awake()
        {
            base.Awake();
            if (rumbleAudio) rumbleAudio.clip = ProceduralSfx.Rumble();
            if (tilesRoot)
            {
                foreach (Transform t in tilesRoot)
                {
                    Vector3 flat = t.position - center; flat.y = 0;
                    _tiles.Add(new Tile
                    {
                        t = t, pos = t.position, rot = t.rotation,
                        delay = firstDelay + flat.magnitude * delayPerMeter + Random.Range(0f, 0.25f),
                        spin = new Vector3(Random.Range(-25f, 25f), Random.Range(-10f, 10f), Random.Range(-25f, 25f))
                    });
                }
            }
        }

        public override void OnFire()
        {
            Running = true;
            _time = 0f;
            for (int i = 0; i < _tiles.Count; i++) { var x = _tiles[i]; x.vel = 0f; _tiles[i] = x; }
            if (rumbleAudio) rumbleAudio.Play();
            if (Rig) Rig.HapticBoth(0.35f, 1.2f);
            Game.LogEvent("floor_collapse", $"tiles={_tiles.Count}");
        }

        public override void OnReset()
        {
            Running = false;
            if (rumbleAudio) rumbleAudio.Stop();
            foreach (var x in _tiles)
            {
                x.t.SetPositionAndRotation(x.pos, x.rot);
                x.t.gameObject.SetActive(true);
            }
        }

        void Update()
        {
            if (!Running) return;
            _time += Time.deltaTime;
            for (int i = 0; i < _tiles.Count; i++)
            {
                var x = _tiles[i];
                if (!x.t.gameObject.activeSelf || _time < x.delay) continue;
                x.vel += 9.81f * Time.deltaTime;
                x.t.position += Vector3.down * x.vel * Time.deltaTime;
                x.t.Rotate(x.spin * Time.deltaTime, Space.Self);
                if (x.t.position.y < killDepth) x.t.gameObject.SetActive(false);
                _tiles[i] = x;
            }
        }
    }
}
