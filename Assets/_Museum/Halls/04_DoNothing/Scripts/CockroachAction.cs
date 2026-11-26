using UnityEngine;

namespace MuseumOfLies.DoNothing
{
    /// <summary>
    /// [새 제안] 신체 위협 자극 — 바퀴벌레.
    /// 바닥 배수구에서 나와 멈칫거리며 발 쪽으로 기어오고 → 다리(가 있을 위치)를 타고 올라와 →
    /// 손등 위를 걸어 다니며 컨트롤러가 걸음마다 미세하게 진동한다.
    /// - 다른 손으로 털어내면(=만지면) Temptation 규칙에 따라 타이머 초기화 (원안 규칙 그대로).
    /// - 손을 세게 흔들어 떨어뜨리는 것은 초기화가 아니라 "반사 반응"으로만 기록한다.
    /// 모델은 기본 도형 — 사실감은 움직임(멈칫거림, 다리, 더듬이)과 소리, 진동이 만든다.
    /// </summary>
    public class CockroachAction : StimulusAction
    {
        enum State { Hidden, Floor, Climb, OnHand, Leave }

        [Header("Parts")]
        public Transform body;
        public Transform[] legsLeft = new Transform[3];
        public Transform[] legsRight = new Transform[3];
        public Transform antennaL, antennaR;
        public Temptation temptation;
        public AudioSource skitterAudio;

        [Header("Path")]
        public Vector3 drainPosition = new Vector3(0.55f, 0f, 1.35f);
        public float floorSpeed = 0.22f;
        public float climbSpeed = 0.16f;
        public float onHandSeconds = 10f;
        [Tooltip("Hand speed (m/s) that flings the roach off — logged as a reflex, not a reset")]
        public float shakeOffSpeed = 1.6f;

        State _state = State.Hidden;
        Vector3 _floorTarget;
        float _pauseTimer, _moveTimer, _stateTime, _hapticTimer, _gait;
        Transform _hand;
        Vector3 _lastHandPos;
        Vector3 _fallVelocity;
        float _circleAngle;

        protected override void Awake()
        {
            base.Awake();
            if (skitterAudio) { skitterAudio.clip = MuseumOfLies.Common.ProceduralSfx.Skitter(); skitterAudio.loop = true; }
            // Hiding is done by DoNothingStimulus.ResetStimulus → OnReset. Don't reset here: Awake may run
            // for the first time inside OnFire (when the object is activated) and would hide it again.
        }

        public override void OnFire()
        {
            Running = true;
            transform.SetParent(null, true);
            transform.position = drainPosition + Vector3.up * 0.006f;
            gameObject.SetActive(true);
            _state = State.Floor;
            _stateTime = 0f;
            _moveTimer = Random.Range(0.3f, 0.9f);
            _pauseTimer = 0f;
            if (skitterAudio) skitterAudio.Play();
        }

        public override void OnReset()
        {
            Running = false;
            _state = State.Hidden;
            _hand = null;
            if (temptation) temptation.ignoreHand = null;
            if (skitterAudio) skitterAudio.Stop();
            transform.SetParent(null, true);
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (_state == State.Hidden || Rig == null) return;
            _stateTime += Time.deltaTime;
            switch (_state)
            {
                case State.Floor: TickFloor(); break;
                case State.Climb: TickClimb(); break;
                case State.OnHand: TickOnHand(); break;
                case State.Leave: TickLeave(); break;
            }
        }

        // Stop–go skitter toward a point in front of the player's feet
        void TickFloor()
        {
            var head = Rig.head;
            Vector3 feet = new Vector3(head.position.x, 0f, head.position.z) + Game.PlayerForward * 0.12f + Game.PlayerRight * 0.1f;
            _floorTarget = feet;
            Skitter(_floorTarget, floorSpeed, Vector3.up, 0.006f);
            if (new Vector2(transform.position.x - feet.x, transform.position.z - feet.z).magnitude < 0.04f)
            {
                _hand = PickHand();
                // The hand it is climbing onto must not count as "touching" (the roach comes to you).
                // Touching it with the OTHER hand still resets — that is swatting.
                if (temptation) temptation.ignoreHand = _hand;
                _state = State.Climb;
                _stateTime = 0f;
                Game.LogEvent("cockroach_at_feet", "");
            }
        }

        // Climb straight up where the leg would be, toward the hand
        void TickClimb()
        {
            if (_hand == null) { _state = State.Leave; return; }
            Vector3 target = _hand.position + _hand.up * 0.03f;
            Vector3 p = transform.position;
            Vector3 dir = target - p;
            float step = climbSpeed * Time.deltaTime * (Mathf.Sin(Time.time * 18f) > -0.3f ? 1f : 0.2f);
            transform.position = dir.magnitude <= step ? target : p + dir.normalized * step;
            transform.rotation = Quaternion.LookRotation(dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.up, -Game.PlayerForward);
            AnimateLegs(1f);
            if (dir.magnitude < 0.03f)
            {
                transform.SetParent(_hand, true);
                if (temptation) temptation.ignoreHand = _hand;
                _state = State.OnHand;
                _stateTime = 0f;
                _lastHandPos = _hand.position;
                Game.LogEvent("cockroach_on_hand", _hand == Rig.leftHand ? "left" : "right");
            }
        }

        // Wander in small circles on the back of the hand; haptic tick per step
        void TickOnHand()
        {
            if (_hand == null) { _state = State.Leave; return; }
            _circleAngle += Time.deltaTime * (Mathf.PerlinNoise(Time.time * 2f, 0.3f) * 3f);
            Vector3 local = new Vector3(Mathf.Cos(_circleAngle) * 0.018f, 0.032f, Mathf.Sin(_circleAngle) * 0.025f - 0.01f);
            transform.localPosition = Vector3.Lerp(transform.localPosition, local, Time.deltaTime * 6f);
            Vector3 tangent = new Vector3(-Mathf.Sin(_circleAngle), 0, Mathf.Cos(_circleAngle));
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(tangent, Vector3.up), Time.deltaTime * 8f);
            AnimateLegs(1f);

            _hapticTimer -= Time.deltaTime;
            if (_hapticTimer <= 0f)
            {
                Rig.Haptic(_hand, Random.Range(0.08f, 0.22f), 0.02f);
                _hapticTimer = Random.Range(0.06f, 0.14f);
            }

            Vector3 handVel = (_hand.position - _lastHandPos) / Mathf.Max(Time.deltaTime, 1e-4f);
            float handSpeed = handVel.magnitude;
            _lastHandPos = _hand.position;
            if (handSpeed > shakeOffSpeed)
            {
                Game.LogEvent("cockroach_shaken_off", $"speed={handSpeed:F2}");
                _fallVelocity = handVel * 0.4f;
                Detach();
                return;
            }
            if (_stateTime > onHandSeconds) { _fallVelocity = Vector3.zero; Detach(); }
        }

        void Detach()
        {
            transform.SetParent(null, true);
            // keep ignoring the hand it just left until it has dropped to the floor (see TickLeave)
            _hand = null;
            _state = State.Leave;
            _stateTime = 0f;
        }

        // Drop to the floor and scurry away out of sight
        void TickLeave()
        {
            if (transform.position.y > 0.007f)
            {
                _fallVelocity += Physics.gravity * Time.deltaTime;
                transform.position += _fallVelocity * Time.deltaTime;
                if (transform.position.y <= 0.006f)
                {
                    var p = transform.position; p.y = 0.006f; transform.position = p; _fallVelocity = Vector3.zero;
                    if (temptation) temptation.ignoreHand = null; // landed — touching it again counts
                }
                return;
            }
            Vector3 away = transform.position + (transform.position - Rig.head.position).normalized * 3f;
            away.y = 0f;
            Skitter(away, floorSpeed * 1.6f, Vector3.up, 0.006f);
            if (_stateTime > 6f || transform.position.y < -1f) OnResetSilently();
        }

        void OnResetSilently()
        {
            _state = State.Hidden;
            if (skitterAudio) skitterAudio.Stop();
            gameObject.SetActive(false);
        }

        /// <summary>Move along the floor with insect-like stop-go rhythm. Returns true while moving.</summary>
        bool Skitter(Vector3 target, float speed, Vector3 up, float height)
        {
            if (_pauseTimer > 0f)
            {
                _pauseTimer -= Time.deltaTime;
                AnimateLegs(0f);
                if (skitterAudio) skitterAudio.volume = 0f;
                return false;
            }
            _moveTimer -= Time.deltaTime;
            if (_moveTimer <= 0f) { _pauseTimer = Random.Range(0.15f, 0.7f); _moveTimer = Random.Range(0.3f, 1.1f); }

            Vector3 p = transform.position;
            Vector3 dir = target - p; dir.y = 0f;
            if (dir.sqrMagnitude < 1e-6f) return false;
            float wobble = (Mathf.PerlinNoise(Time.time * 3f, 7f) - 0.5f) * 70f;
            Vector3 heading = Quaternion.Euler(0, wobble, 0) * dir.normalized;
            Vector3 next = p + heading * speed * Time.deltaTime;
            next.y = height;
            transform.position = next;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading, up), Time.deltaTime * 12f);
            AnimateLegs(1f);
            if (skitterAudio) skitterAudio.volume = 1f;
            return true;
        }

        void AnimateLegs(float speed)
        {
            _gait += Time.deltaTime * 32f * speed;
            for (int i = 0; i < 3; i++)
            {
                float a = Mathf.Sin(_gait + i * 2.1f) * 28f * speed;
                if (legsLeft[i]) legsLeft[i].localRotation = Quaternion.Euler(0, a, 0);
                if (legsRight[i]) legsRight[i].localRotation = Quaternion.Euler(0, -a, 0);
            }
            float ant = Mathf.Sin(Time.time * 7f) * 12f;
            if (antennaL) antennaL.localRotation = Quaternion.Euler(-20f + ant, -25f, 0);
            if (antennaR) antennaR.localRotation = Quaternion.Euler(-20f - ant, 25f, 0);
        }

        Transform PickHand()
        {
            var l = Rig.leftHand; var r = Rig.rightHand;
            if (l == null) return r;
            if (r == null) return l;
            return l.position.y < r.position.y ? l : r; // the lower (resting) hand
        }
    }
}
