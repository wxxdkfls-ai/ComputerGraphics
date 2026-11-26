using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Feedback;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace MuseumOfLies.Common
{
    /// <summary>
    /// Wraps the XRI Starter "XR Origin (XR Rig)" so every scene behaves the same:
    /// - finds head / hands
    /// - movement policy  (Exhibit = no movement, Hub = LEFT stick walk + RIGHT stick snap turn, no teleport)
    /// - pointer policy   (short STRAIGHT ray, 4 m — no big curved arc when pointing at far things)
    /// - haptics policy   (all XRI default hover/select vibration OFF; only MuseumHaptics may vibrate)
    /// </summary>
    public class XRRig : MonoBehaviour
    {
        public static XRRig Instance { get; private set; }

        public Transform origin;
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        public enum LocomotionMode { None, LeftStickWalk }
        [Tooltip("None = real head/body only (exhibit halls). LeftStickWalk = hub spaces (left stick walk, right stick snap turn)")]
        public LocomotionMode locomotion = LocomotionMode.None;
        [Tooltip("Walk speed (m/s). Slow = comfortable")]
        public float walkSpeed = 1.4f;
        [Tooltip("Pointer ray length (m)")]
        public float rayLength = 4f;

        void Awake()
        {
            Instance = this;
            Resolve();
            ApplyPointerPolicy();
            ApplyHapticPolicy();
            if (locomotion == LocomotionMode.None) DisableLocomotion();
            else LeftStickWalk();
        }

        public void Resolve()
        {
            if (origin == null)
            {
                var o = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
                if (o != null) origin = o.transform;
            }
            if (origin == null) return;
            if (head == null) head = FindDeep(origin, "Main Camera");
            if (leftHand == null) leftHand = FindDeep(origin, "Left Controller");
            if (rightHand == null) rightHand = FindDeep(origin, "Right Controller");
        }

        // ── haptics ─────────────────────────────────────────────
        public void Haptic(Transform hand, float amplitude, float duration)
        {
            if (hand == null) return;
            var player = hand.GetComponent<UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticImpulsePlayer>();
            if (player != null) player.SendHapticImpulse(Mathf.Clamp01(amplitude), duration);
        }
        public void HapticBoth(float amplitude, float duration) { Haptic(leftHand, amplitude, duration); Haptic(rightHand, amplitude, duration); }

        /// <summary>XRI's default hover/select buzz would give hints (e.g. hovering a forbidden object) — turn it all off.</summary>
        void ApplyHapticPolicy()
        {
            if (origin == null) return;
            foreach (var f in origin.GetComponentsInChildren<SimpleHapticFeedback>(true)) f.enabled = false;
        }

        // ── pointer ─────────────────────────────────────────────
        void ApplyPointerPolicy()
        {
            if (origin == null) return;
            foreach (var c in origin.GetComponentsInChildren<CurveInteractionCaster>(true))
            {
                c.hitDetectionType = CurveInteractionCaster.HitDetectionType.Raycast; // straight, no cone snapping → no big arc
                c.castDistance = rayLength;
            }
            foreach (var v in origin.GetComponentsInChildren<CurveVisualController>(true))
                v.maxVisualCurveDistance = rayLength;
        }

        // ── locomotion ──────────────────────────────────────────
        /// <summary>Hub: LEFT stick = walk (head-relative), RIGHT stick = snap turn. Teleport, grab-move, climb, jump off.</summary>
        public void LeftStickWalk()
        {
            if (origin == null) return;
            var loco = origin.Find("Locomotion");
            if (loco != null)
            {
                foreach (var n in new[] { "Grab Move", "Climb", "Jump", "Teleportation" })
                {
                    var c = loco.Find(n);
                    if (c != null) c.gameObject.SetActive(false);
                }
                var move = loco.GetComponentInChildren<DynamicMoveProvider>(true);
                if (move != null) { move.gameObject.SetActive(true); move.moveSpeed = walkSpeed; }
                var turn = loco.Find("Turn");
                var cont = turn != null ? turn.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning.ContinuousTurnProvider>() : null;
                if (cont != null) cont.enabled = false;
            }
            SetController(leftHand, smoothMotion: true);
            SetController(rightHand, smoothMotion: false);
            foreach (var hand in new[] { leftHand, rightHand })
            {
                var tp = hand != null ? hand.Find("Teleport Interactor") : null;
                if (tp != null) tp.gameObject.SetActive(false);
            }
        }

        static void SetController(Transform hand, bool smoothMotion)
        {
            var m = hand != null ? hand.GetComponent<ControllerInputActionManager>() : null;
            if (m == null) return;
            m.smoothMotionEnabled = smoothMotion;
            m.smoothTurnEnabled = false; // snap turn only
        }

        public void DisableLocomotion()
        {
            if (origin == null) return;
            var loco = origin.Find("Locomotion");
            if (loco != null) loco.gameObject.SetActive(false);
            foreach (var name in new[] { "Left Controller", "Right Controller" })
            {
                var c = FindDeep(origin, name);
                var tp = c != null ? c.Find("Teleport Interactor") : null;
                if (tp != null) tp.gameObject.SetActive(false);
            }
        }

        static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform c in root)
            {
                var r = FindDeep(c, name);
                if (r != null) return r;
            }
            return null;
        }
    }
}
