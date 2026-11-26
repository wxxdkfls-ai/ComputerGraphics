using MuseumOfLies.Common;
using UnityEngine;

namespace MuseumOfLies.DoNothing
{
    /// <summary>
    /// [새 제안] 사회적 자극 — 누군가 바로 뒤에 선다.
    /// 입구 문에서 발소리가 다가와 플레이어 등 뒤(정면 기준 150°, 0.6 m)에 얼굴 없는 형체가 멈춰 서고,
    /// 귀 바로 뒤에서 숨소리가 들린다. 시간이 지나며 0.6 m → 0.3 m로 천천히 다가온다.
    /// 형체는 시야 밖에 있고 그림자만 발 근처 바닥에 비칠 수 있다. 돌아보면(고개 회전) 초기화.
    /// 형체를 손으로 만져도 초기화 (Temptation).
    /// </summary>
    public class FigureBehindAction : StimulusAction
    {
        public Transform figure;            // root of the mannequin (pivot at feet)
        public AudioSource footstepAudio;   // moves from the door to the figure position
        public AudioSource breathAudio;     // at ear height, just behind the head
        public Vector3 doorPosition = new Vector3(0.3f, 0f, -3.3f);
        public float walkSeconds = 6f;
        public float startDistance = 0.6f;
        public float endDistance = 0.32f;
        public float approachSeconds = 25f;
        [Tooltip("Angle from the player's front (deg). 180 = directly behind.")]
        public float angleFromFront = 150f;

        float _t;
        bool _arrived;
        Vector3 _standDir;

        protected override void Awake()
        {
            base.Awake();
            if (footstepAudio) footstepAudio.clip = ProceduralSfx.Footsteps(walkSeconds);
            if (breathAudio) { breathAudio.clip = ProceduralSfx.Breath(); breathAudio.loop = true; }
        }

        public override void OnFire()
        {
            Running = true;
            _t = 0f;
            _arrived = false;
            _standDir = Quaternion.Euler(0, angleFromFront, 0) * Game.PlayerForward; // fixed relative to the session's front
            if (figure) figure.gameObject.SetActive(false);
            if (footstepAudio) { footstepAudio.transform.position = doorPosition + Vector3.up * 0.1f; footstepAudio.Play(); }
        }

        public override void OnReset()
        {
            Running = false;
            _arrived = false;
            if (figure) figure.gameObject.SetActive(false);
            if (footstepAudio) footstepAudio.Stop();
            if (breathAudio) breathAudio.Stop();
        }

        Vector3 StandPoint(float dist)
        {
            var h = Rig.head.position;
            return new Vector3(h.x, 0f, h.z) + _standDir * dist;
        }

        void Update()
        {
            if (!Running || Rig == null || Rig.head == null) return;
            _t += Time.deltaTime;

            if (!_arrived)
            {
                float k = Mathf.Clamp01(_t / walkSeconds);
                if (footstepAudio) footstepAudio.transform.position = Vector3.Lerp(doorPosition, StandPoint(startDistance), k) + Vector3.up * 0.1f;
                if (k >= 1f)
                {
                    _arrived = true;
                    _t = 0f;
                    if (figure) figure.gameObject.SetActive(true);
                    if (breathAudio) breathAudio.Play();
                    Game.LogEvent("figure_arrived", $"angle={angleFromFront}");
                }
                return;
            }

            float d = Mathf.Lerp(startDistance, endDistance, Mathf.SmoothStep(0f, 1f, _t / approachSeconds));
            Vector3 p = StandPoint(d);
            if (figure)
            {
                figure.position = p;
                Vector3 look = Rig.head.position - p; look.y = 0f;
                if (look.sqrMagnitude > 1e-4f) figure.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
            }
            if (breathAudio)
            {
                // just behind the ear, slightly to the figure's side
                Vector3 ear = Rig.head.position + _standDir * Mathf.Min(d, 0.35f) + Vector3.down * 0.05f;
                breathAudio.transform.position = ear;
            }
        }
    }
}
