using UnityEngine;

namespace MuseumOfLies.Common
{
    /// <summary>
    /// Finds head/hands on the XRI Starter "XR Origin (XR Rig)" and optionally disables locomotion
    /// (move/turn/teleport) so the virtual view only changes with the real head.
    /// </summary>
    public class XRRig : MonoBehaviour
    {
        public Transform origin;
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;
        public bool disableLocomotion = true;

        void Awake()
        {
            Resolve();
            if (disableLocomotion) DisableLocomotion();
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

        /// <summary>Controller vibration on one hand (no-op in editor without a device).</summary>
        public void Haptic(Transform hand, float amplitude, float duration)
        {
            if (hand == null) return;
            var player = hand.GetComponent<UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticImpulsePlayer>();
            if (player != null) player.SendHapticImpulse(Mathf.Clamp01(amplitude), duration);
        }

        public void HapticBoth(float amplitude, float duration) { Haptic(leftHand, amplitude, duration); Haptic(rightHand, amplitude, duration); }

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
