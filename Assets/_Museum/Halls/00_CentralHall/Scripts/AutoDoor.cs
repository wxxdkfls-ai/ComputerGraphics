using MuseumOfLies.Common;
using UnityEngine;

namespace MuseumOfLies.CentralHall
{
    /// <summary>
    /// Automatic sliding double door. Opens when the player's head comes within `openDistance` (horizontal),
    /// closes when they walk away. Time-based easing, pneumatic sound. Identical for every gate (no hints).
    /// </summary>
    public class AutoDoor : MonoBehaviour
    {
        public Transform leafLeft, leafRight;
        public float slide = 0.98f;          // how far each leaf slides sideways (m)
        public float openDistance = 2.6f;
        public float closeDistance = 3.4f;
        public float openSeconds = 0.7f;
        public AudioSource audioSource;
        public bool locked;

        public bool IsOpen { get; private set; }
        public event System.Action Opened;

        Transform _head;
        Vector3 _l0, _r0;
        float _k, _vel;

        void Awake()
        {
            if (leafLeft) _l0 = leafLeft.localPosition;
            if (leafRight) _r0 = leafRight.localPosition;
            if (audioSource) audioSource.clip = ProceduralSfx.DoorWhoosh();
        }

        void Start()
        {
            var rig = FindFirstObjectByType<XRRig>();
            if (rig) _head = rig.head;
            if (_head == null && Camera.main) _head = Camera.main.transform;
        }

        void Update()
        {
            if (_head == null) return;
            Vector3 d = _head.position - transform.position; d.y = 0f;
            float dist = d.magnitude;
            bool want = !locked && (IsOpen ? dist < closeDistance : dist < openDistance);
            if (want != IsOpen)
            {
                IsOpen = want;
                if (audioSource) audioSource.Play();
                if (IsOpen) Opened?.Invoke();
            }
            _k = Mathf.SmoothDamp(_k, IsOpen ? 1f : 0f, ref _vel, openSeconds * 0.35f);
            if (leafLeft) leafLeft.localPosition = _l0 + Vector3.left * slide * _k;
            if (leafRight) leafRight.localPosition = _r0 + Vector3.right * slide * _k;
        }
    }
}
