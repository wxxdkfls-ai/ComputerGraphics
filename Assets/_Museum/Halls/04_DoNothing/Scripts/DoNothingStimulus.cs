using MuseumOfLies.Common;
using TMPro;
using UnityEngine;

namespace MuseumOfLies.DoNothing
{
    /// <summary>
    /// One scheduled temptation event. Fires at triggerTime (seconds into the current attempt).
    /// All stimuli are reset when the timer resets, so every attempt faces the same sequence.
    /// </summary>
    public class DoNothingStimulus : MonoBehaviour
    {
        public enum Kind { Rise, Drop, SoundOnly, Roll, ChangeLabel, Custom }

        public string id = "stimulus";
        public string displayName = "자극";
        public Kind kind = Kind.Rise;
        public float triggerTime = 15f;
        [Tooltip("Included in the Minimal (low-stimulus) experiment condition")]
        public bool inMinimalCondition = false;
        [Tooltip("Uncheck to remove this stimulus (e.g. participant has a phobia of insects/heights)")]
        public bool includeInSession = true;
        public enum Phase { Curiosity, Social, BodyThreat }
        public Phase phase = Phase.Curiosity;

        [Header("Custom (Kind = Custom)")]
        public StimulusAction action;

        [Header("Target")]
        public Transform target;          // object to move / show
        public Vector3 hiddenLocalPos;    // Rise: start position
        public Vector3 shownLocalPos;     // Rise: end position, Drop/Roll: spawn position
        public float riseDuration = 2f;
        public Vector3 launchVelocity;    // Roll
        public bool hideUntilFired = true;

        [Header("Label (ChangeLabel)")]
        public TMP_Text label;
        [TextArea] public string newText;

        [Header("Sound")]
        public AudioSource audioSource;
        public enum Sfx { None, Rise, Clink, Knock, Ding, Roll }
        public Sfx sfx = Sfx.None;
        public bool playOnCollision = false; // Drop: clink when it lands

        public bool Fired { get; private set; }

        string _originalText;
        float _riseT = -1f;
        Rigidbody _rb;
        bool _landed;
        Quaternion _initialRot = Quaternion.identity;

        void Awake()
        {
            if (target != null) { _rb = target.GetComponent<Rigidbody>(); _initialRot = target.localRotation; }
            if (label != null) _originalText = label.text;
            if (audioSource != null) audioSource.clip = MakeClip();
            ResetStimulus();
        }

        AudioClip MakeClip()
        {
            switch (sfx)
            {
                case Sfx.Rise: return ProceduralSfx.Rise(riseDuration);
                case Sfx.Clink: return ProceduralSfx.Clink();
                case Sfx.Knock: return ProceduralSfx.Knock();
                case Sfx.Ding: return ProceduralSfx.Ding();
                case Sfx.Roll: return ProceduralSfx.Roll();
                default: return null;
            }
        }

        public void Fire()
        {
            if (Fired) return;
            Fired = true;
            switch (kind)
            {
                case Kind.Rise:
                    if (target) { target.gameObject.SetActive(true); target.localPosition = hiddenLocalPos; }
                    _riseT = 0f;
                    break;
                case Kind.Drop:
                case Kind.Roll:
                    if (target)
                    {
                        target.localPosition = shownLocalPos;
                        target.gameObject.SetActive(true);
                        if (_rb)
                        {
                            _rb.isKinematic = false;
                            _rb.linearVelocity = kind == Kind.Roll ? launchVelocity : Vector3.zero;
                            _rb.angularVelocity = Vector3.zero;
                        }
                    }
                    break;
                case Kind.ChangeLabel:
                    if (label) label.text = newText;
                    break;
                case Kind.Custom:
                    if (action) action.OnFire();
                    break;
            }
            if (audioSource && audioSource.clip && !playOnCollision) audioSource.Play();
        }

        public void ResetStimulus()
        {
            Fired = false;
            _riseT = -1f;
            _landed = false;
            if (audioSource) audioSource.Stop();
            if (label != null && _originalText != null) label.text = _originalText;
            if (kind == Kind.Custom && action) action.OnReset();
            if (target == null) return;
            if (_rb)
            {
                if (!_rb.isKinematic) { _rb.linearVelocity = Vector3.zero; _rb.angularVelocity = Vector3.zero; }
                _rb.isKinematic = true;
            }
            switch (kind)
            {
                case Kind.Rise:
                    target.localPosition = hiddenLocalPos;
                    if (hideUntilFired) target.gameObject.SetActive(false);
                    break;
                case Kind.Drop:
                case Kind.Roll:
                    target.localPosition = shownLocalPos;
                    target.localRotation = _initialRot;
                    if (hideUntilFired) target.gameObject.SetActive(false);
                    break;
            }
        }

        void Update()
        {
            if (_riseT < 0f || target == null) return;
            _riseT += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_riseT / Mathf.Max(0.01f, riseDuration)));
            target.localPosition = Vector3.Lerp(hiddenLocalPos, shownLocalPos, k);
            if (k >= 1f) _riseT = -1f;
        }

        /// <summary>Called by StimulusCollisionRelay on the dropped object.</summary>
        public void OnTargetCollision(float impactSpeed)
        {
            if (!playOnCollision || audioSource == null || audioSource.clip == null) return;
            if (impactSpeed < 0.4f) return;
            audioSource.volume = _landed ? 0.35f : 1f; // softer bounces after first impact
            audioSource.Play();
            _landed = true;
        }
    }
}
