using System.Collections;
using UnityEngine;

namespace MuseumOfLies.Common
{
    /// <summary>
    /// 햅틱 어휘 — 진동은 역할별로 하나의 패턴만 쓴다. 이 클래스 밖에서 진동을 직접 호출하지 않는다.
    ///
    ///  역할           언제                        패턴                               손
    ///  ─────────────────────────────────────────────────────────────────────────────────
    ///  UI 확인        UI 버튼을 눌렀을 때           아주 짧은 톡 (0.15, 20 ms)          양손
    ///  진행           나가기 길게 누르는 중          점점 강해지는 박동 (0.05→0.35, 5 Hz)  양손
    ///  규칙: 실패     타이머 초기화                 묵직한 한 번 (0.6, 150 ms)          양손
    ///  규칙: 성공     도전 성공                     밝게 세 번 (0.3, 0.4, 0.5 × 60 ms)   양손
    ///  신체 접촉      몸에 무언가 닿음(바퀴벌레 발)  작고 불규칙 (0.08–0.22, 20 ms)       닿은 손만
    ///  환경 진동      땅이 흔들림(바닥 붕괴)         길고 낮게 (0.35, 1.2 s)             양손
    ///  ─────────────────────────────────────────────────────────────────────────────────
    ///  금지: 물체에 hover/근접만 했을 때 (유혹 물체의 단서가 됨), 자동문 등 몸에 닿지 않는 환경 이벤트 (소리로만)
    /// </summary>
    public static class MuseumHaptics
    {
        static XRRig Rig => XRRig.Instance;
        static float _nextProgress;

        public static void UITick() { if (Rig) Rig.HapticBoth(0.15f, 0.02f); }

        /// <summary>Call every frame while holding; t = 0..1 progress.</summary>
        public static void Progress(float t)
        {
            if (!Rig || t <= 0f || Time.unscaledTime < _nextProgress) return;
            _nextProgress = Time.unscaledTime + 0.2f;
            Rig.HapticBoth(Mathf.Lerp(0.05f, 0.35f, Mathf.Clamp01(t)), 0.03f);
        }

        public static void Fail() { if (Rig) Rig.HapticBoth(0.6f, 0.15f); }

        public static void Success() { if (Rig) Rig.StartCoroutine(SuccessRoutine()); }
        static IEnumerator SuccessRoutine()
        {
            foreach (var a in new[] { 0.3f, 0.4f, 0.5f })
            {
                Rig.HapticBoth(a, 0.06f);
                yield return new WaitForSecondsRealtime(0.12f);
            }
        }

        public static void Contact(Transform hand) { if (Rig) Rig.Haptic(hand, Random.Range(0.08f, 0.22f), 0.02f); }

        public static void Rumble() { if (Rig) Rig.HapticBoth(0.35f, 1.2f); }
    }
}
