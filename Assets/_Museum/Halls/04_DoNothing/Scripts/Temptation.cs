using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MuseumOfLies.DoNothing
{
    /// <summary>
    /// An object the player must NOT touch. Touch = a hand (controller) physically reaching the collider,
    /// or selecting it with the controller (grip/trigger, including ray). Either one resets the timer.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Temptation : MonoBehaviour
    {
        public string id = "object";
        public string displayName = "물체";
        [Tooltip("Hand distance (m) to the collider surface that counts as touching")]
        public float touchRadius = 0.05f;

        [Tooltip("This hand doesn't count as touching (e.g. the hand the cockroach is sitting on)")]
        public Transform ignoreHand;

        Collider _col;
        bool _selected;

        void Awake()
        {
            _col = GetComponent<Collider>();
            var interactable = GetComponent<XRSimpleInteractable>();
            if (interactable != null) interactable.selectEntered.AddListener(_ => _selected = true);
        }

        public bool IsActive => isActiveAndEnabled && _col != null && _col.enabled;

        /// <summary>Returns true once if touched since last call.</summary>
        public bool ConsumeTouch(Transform leftHand, Transform rightHand)
        {
            if (!IsActive) { _selected = false; return false; }
            bool touched = _selected || (leftHand != ignoreHand && Near(leftHand)) || (rightHand != ignoreHand && Near(rightHand));
            _selected = false;
            return touched;
        }

        bool Near(Transform hand)
        {
            if (hand == null || !hand.gameObject.activeInHierarchy) return false;
            Vector3 p = hand.position;
            Vector3 cp = _col.ClosestPoint(p);
            return (cp - p).sqrMagnitude <= touchRadius * touchRadius;
        }

        public void ClearSelection() { _selected = false; }
    }
}
