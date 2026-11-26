using System.Collections.Generic;
using System.Linq;
using MuseumOfLies.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MuseumOfLies.DoNothing
{
    /// <summary>
    /// 04. DO NOTHING — 원안 규칙
    /// - 3분 동안 아무것도 하지 않으면 성공.
    /// - 유혹 물체를 만지거나 고개를 일정 범위 이상 돌리면 타이머가 3:00으로 초기화. 초기화 횟수 제한 없음.
    /// - 언제든 별도 종료(B/Y 2초 길게 누르기)로 나갈 수 있고, 성공/종료 모두 관람 완료.
    /// - 자연스러운 머리 흔들림/추적 오차는 실패로 처리하지 않음 (지속 시간 필터).
    /// </summary>
    public class DoNothingGame : MonoBehaviour
    {
        public enum State { Intro, Countdown, Running, WaitFront, Result }
        public enum StimulusCondition { Full, Minimal }

        [Header("Rule (원안)")]
        public float goalSeconds = 180f;

        [Header("Head limits — 초기값, 실기 테스트로 조정 (미정 사항)")]
        public float yawLimit = 35f;
        public float pitchUpLimit = 30f;
        public float pitchDownLimit = 40f;
        [Tooltip("Head must stay beyond the limit this long to count (filters jitter / tracking spikes)")]
        public float sustainSeconds = 0.3f;
        [Tooltip("After a reset, look forward this long to resume")]
        public float resumeHoldSeconds = 0.5f;

        [Header("Flow")]
        public float countdownSeconds = 3f;
        public float exitHoldSeconds = 2f;
        public float resetFeedbackSeconds = 1.5f;

        [Header("Experiment")]
        public StimulusCondition condition = StimulusCondition.Full;

        [Header("Reflex logging (data only — never resets the timer)")]
        [Tooltip("Hand speed (m/s) counted as a reflexive flinch")]
        public float handFlinchSpeed = 1.2f;
        [Tooltip("Head translation speed (m/s) counted as a reflexive flinch")]
        public float headFlinchSpeed = 0.5f;
        public float flinchCooldown = 1f;

        [Header("Refs")]
        public XRRig rig;
        public InstructionPanel instructionPanel;
        public ResultPanel resultPanel;
        public TimerDisplay timer;
        public MuseumProgressBar exitProgress;
        public AudioSource uiAudio;
        public List<DoNothingStimulus> stimuli = new List<DoNothingStimulus>();
        public List<Temptation> temptations = new List<Temptation>();

        [Header("Events")]
        public UnityEvent onVisitComplete;

        public State CurrentState { get; private set; } = State.Intro;

        // session stats
        ExperimentLogger _log;
        int _attempt;
        float _attemptTime;
        float _sessionStart;
        float _bestStreak;
        float _firstResetAt = -1f;
        int _resetCount;
        readonly Dictionary<string, int> _reactionByStimulus = new Dictionary<string, int>();
        readonly Dictionary<string, int> _resetByCause = new Dictionary<string, int>();
        string _lastStimulusId = "none";
        float _lastStimulusTime;
        int _flinchCount;
        readonly Dictionary<string, int> _flinchByStimulus = new Dictionary<string, int>();
        Vector3 _prevL, _prevR, _prevHead;
        float _flinchCooldownTimer;

        // head
        float _baselineYaw;
        float _outOfRangeTime;
        float _frontHoldTime;
        float _countdown;
        float _feedbackTimer;
        float _exitHold;
        InputAction _exitAction;
        AudioClip _resetClip, _successClip;

        Color Accent => MuseumPalette.DoNothing;

        void Awake()
        {
            if (rig == null) rig = FindFirstObjectByType<XRRig>();
            _resetClip = ProceduralSfx.ResetTone();
            _successClip = ProceduralSfx.Success();

            _exitAction = new InputAction("Exit", InputActionType.Button);
            _exitAction.AddBinding("<XRController>{LeftHand}/secondaryButton");
            _exitAction.AddBinding("<XRController>{RightHand}/secondaryButton");
            _exitAction.AddBinding("<Keyboard>/escape"); // editor / simulator fallback
        }

        void OnEnable() { _exitAction.Enable(); }
        void OnDisable() { _exitAction.Disable(); }
        void OnDestroy() { _log?.Close(); _exitAction?.Dispose(); }

        void Start()
        {
            if (instructionPanel && instructionPanel.primaryButton)
                instructionPanel.primaryButton.Button.onClick.AddListener(BeginSession);
            if (resultPanel)
            {
                if (resultPanel.secondaryButton) resultPanel.secondaryButton.Button.onClick.AddListener(Retry);
                if (resultPanel.primaryButton) resultPanel.primaryButton.Button.onClick.AddListener(FinishVisit);
            }
            EnterIntro();
        }

        // ───────────────────────── States ─────────────────────────

        void EnterIntro()
        {
            CurrentState = State.Intro;
            foreach (var s in stimuli) s.ResetStimulus();
            if (resultPanel) resultPanel.Show(false);
            if (instructionPanel)
            {
                instructionPanel.Show(true);
                instructionPanel.SetContent(
                    "04 — DO NOTHING · 의지",
                    "아무것도 하지 마세요",
                    "3분 동안 아무것도 하지 않으면 성공입니다.\n\n" +
                    "•  물체를 만지거나 고개를 크게 돌리면 타이머가 3:00으로 돌아갑니다.\n" +
                    "•  도전 횟수에는 제한이 없습니다.\n" +
                    "•  체험 중 벌레, 높이(발밑), 등 뒤의 기척, 갑작스러운 소리가 나타날 수 있습니다.\n" +
                    "•  언제든 B 또는 Y 버튼을 2초간 누르면 나갈 수 있습니다.",
                    Accent, "시작");
            }
            if (timer)
            {
                timer.SetAccent(Accent);
                timer.SetState(TimerDisplay.State.Waiting);
                timer.SetTime(goalSeconds);
                timer.SetStatus("시작 버튼을 누르세요");
            }
            SetExitProgress(0f);
        }

        void BeginSession()
        {
            if (CurrentState != State.Intro) return;
            _log?.Close();
            _log = new ExperimentLogger("DoNothing", condition.ToString());
            _attempt = 0;
            _bestStreak = 0f;
            _firstResetAt = -1f;
            _resetCount = 0;
            _reactionByStimulus.Clear();
            _resetByCause.Clear();
            _flinchCount = 0;
            _flinchByStimulus.Clear();
            _sessionStart = Time.time;
            if (instructionPanel) instructionPanel.Show(false);
            _log.Log(0, 0f, "session_start", $"goal={goalSeconds};yaw={yawLimit};pitchUp={pitchUpLimit};pitchDown={pitchDownLimit};sustain={sustainSeconds}");
            CurrentState = State.Countdown;
            _countdown = countdownSeconds;
        }

        void StartAttempt()
        {
            _attempt++;
            _attemptTime = 0f;
            _outOfRangeTime = 0f;
            _lastStimulusId = "none";
            _lastStimulusTime = 0f;
            foreach (var s in stimuli) s.ResetStimulus();
            foreach (var t in temptations) t.ClearSelection();
            CurrentState = State.Running;
            SamplePrev();
            if (timer) { timer.SetState(TimerDisplay.State.Normal); timer.SetTime(goalSeconds); }
            UpdateStatusLine();
            _log.Log(_attempt, 0f, "attempt_start", "", HeadYaw(), HeadPitch());
        }

        void Update()
        {
            switch (CurrentState)
            {
                case State.Countdown: TickCountdown(); break;
                case State.Running: TickRunning(); break;
                case State.WaitFront: TickWaitFront(); break;
            }
            TickExit();
        }

        void TickCountdown()
        {
            _countdown -= Time.deltaTime;
            if (timer)
            {
                timer.SetState(TimerDisplay.State.Waiting);
                timer.SetRaw(Mathf.CeilToInt(Mathf.Max(0f, _countdown)).ToString());
                timer.SetStatus("정면의 타이머를 바라보세요");
            }
            if (_countdown <= 0f)
            {
                _baselineYaw = RawYaw(); // the player's own "front" for this session
                StartAttempt();
            }
        }

        void TickRunning()
        {
            _attemptTime += Time.deltaTime;

            // scheduled stimuli
            foreach (var s in stimuli)
            {
                if (s.Fired || _attemptTime < s.triggerTime) continue;
                if (!s.includeInSession) continue;
                if (condition == StimulusCondition.Minimal && !s.inMinimalCondition) continue;
                s.Fire();
                _lastStimulusId = s.id;
                _lastStimulusTime = _attemptTime;
                _log.Log(_attempt, _attemptTime, "stimulus", s.id, HeadYaw(), HeadPitch());
            }

            TrackFlinch();

            // touch
            foreach (var t in temptations)
            {
                if (t.ConsumeTouch(rig ? rig.leftHand : null, rig ? rig.rightHand : null))
                {
                    DoReset("touch:" + t.id, t.displayName + "을(를) 만졌습니다");
                    return;
                }
            }

            // head turn (sustained)
            float yaw = HeadYaw(), pitch = HeadPitch();
            string cause = null;
            if (Mathf.Abs(yaw) > yawLimit) cause = "head_yaw";
            else if (pitch > pitchUpLimit || -pitch > pitchDownLimit) cause = "head_pitch";
            _outOfRangeTime = cause != null ? _outOfRangeTime + Time.deltaTime : 0f;
            if (cause != null && _outOfRangeTime >= sustainSeconds)
            {
                DoReset(cause, cause == "head_yaw" ? "고개를 옆으로 돌렸습니다" : "고개를 위아래로 크게 움직였습니다");
                return;
            }

            float remaining = goalSeconds - _attemptTime;
            if (timer)
            {
                timer.SetTime(remaining);
                if (_feedbackTimer > 0f)
                {
                    _feedbackTimer -= Time.deltaTime;
                    if (_feedbackTimer <= 0f) { timer.SetState(TimerDisplay.State.Normal); UpdateStatusLine(); }
                }
            }

            if (remaining <= 0f) EndSession(true);
        }

        void TickWaitFront()
        {
            bool inRange = Mathf.Abs(HeadYaw()) <= yawLimit * 0.6f && HeadPitch() <= pitchUpLimit * 0.6f && -HeadPitch() <= pitchDownLimit * 0.6f;
            _frontHoldTime = inRange ? _frontHoldTime + Time.deltaTime : 0f;
            if (_feedbackTimer > 0f) _feedbackTimer -= Time.deltaTime;
            if (_frontHoldTime >= resumeHoldSeconds && _feedbackTimer <= 0f)
            {
                _log.Log(_attempt, 0f, "resume", "", HeadYaw(), HeadPitch());
                StartAttempt();
            }
        }

        void DoReset(string cause, string message)
        {
            _resetCount++;
            if (_firstResetAt < 0f) _firstResetAt = _attemptTime;
            _bestStreak = Mathf.Max(_bestStreak, _attemptTime);
            Count(_resetByCause, cause);
            if (_lastStimulusId != "none") Count(_reactionByStimulus, _lastStimulusId);

            _log.Log(_attempt, _attemptTime, "reset", $"{cause};last_stimulus={_lastStimulusId};since_stimulus={(_attemptTime - _lastStimulusTime):F2}", HeadYaw(), HeadPitch());

            foreach (var s in stimuli) s.ResetStimulus();
            if (uiAudio) uiAudio.PlayOneShot(_resetClip);
            if (timer)
            {
                timer.SetState(TimerDisplay.State.Reset);
                timer.SetTime(goalSeconds);
                timer.SetStatus($"↺  초기화 — {message}\n정면을 바라보면 다시 시작합니다");
            }
            _feedbackTimer = resetFeedbackSeconds;
            _frontHoldTime = 0f;
            CurrentState = State.WaitFront;
        }

        void TickExit()
        {
            bool active = CurrentState == State.Countdown || CurrentState == State.Running || CurrentState == State.WaitFront;
            if (!active) { _exitHold = 0f; SetExitProgress(0f); return; }
            _exitHold = _exitAction.IsPressed() ? _exitHold + Time.deltaTime : 0f;
            SetExitProgress(_exitHold / exitHoldSeconds);
            if (_exitHold >= exitHoldSeconds) EndSession(false);
        }

        void EndSession(bool success)
        {
            if (CurrentState == State.Running) _bestStreak = Mathf.Max(_bestStreak, _attemptTime);
            CurrentState = State.Result;
            foreach (var s in stimuli) s.ResetStimulus();
            SetExitProgress(0f);
            float total = Time.time - _sessionStart;
            _log.Log(_attempt, _attemptTime, success ? "success" : "exit",
                $"resets={_resetCount};best={_bestStreak:F2};first_reset={_firstResetAt:F2};total={total:F2}", HeadYaw(), HeadPitch());

            if (timer)
            {
                timer.SetState(success ? TimerDisplay.State.Success : TimerDisplay.State.Waiting);
                timer.SetTime(success ? 0f : goalSeconds - _attemptTime);
                timer.SetStatus(success ? "✓  도전 성공" : "■  도전 종료");
            }
            if (success && uiAudio) uiAudio.PlayOneShot(_successClip);

            if (resultPanel)
            {
                resultPanel.Show(true);
                resultPanel.SetHeader("04 — DO NOTHING · 기록", success ? "3분 동안 아무것도 하지 않았습니다" : "여기까지 기록되었습니다", Accent);
                resultPanel.SetOutcome(success ? ResultPanel.Outcome.Success : ResultPanel.Outcome.Ended, success ? "도전 성공" : "도전 종료");
                resultPanel.ClearRows();
                resultPanel.AddRow("최고 연속 기록", TimerDisplay.FormatElapsed(Mathf.Min(_bestStreak, goalSeconds)), true);
                resultPanel.AddRow("초기화 횟수", $"{_resetCount}회");
                resultPanel.AddRow("첫 초기화까지", _firstResetAt < 0f ? "—" : TimerDisplay.FormatElapsed(_firstResetAt));
                resultPanel.AddRow("가장 많이 반응한 자극", TopReaction(), false, false);
                resultPanel.AddRow("몸이 먼저 반응한 횟수", $"{_flinchCount}회");
                resultPanel.AddRow("총 체험 시간", TimerDisplay.FormatElapsed(total));
                resultPanel.SetFootnote("성공과 종료 모두 이 전시의 관람이 완료된 것으로 기록됩니다.");
            }
        }

        void Retry()
        {
            if (CurrentState != State.Result) return;
            _log?.Log(_attempt, 0f, "retry");
            EnterIntro();
            BeginSession();
        }

        void FinishVisit()
        {
            _log?.Log(_attempt, 0f, "visit_complete");
            _log?.Close();
            _log = null;
            onVisitComplete?.Invoke();
            MuseumSession.CompleteCurrentHall();
            if (SceneTransition.CanLoad("CentralHall")) SceneTransition.Load("CentralHall");
            else EnterIntro();
        }

        // ───────────────────────── Public API for stimuli ─────────────────────────

        /// <summary>The player's front for this session (horizontal, world space).</summary>
        public Vector3 PlayerForward
        {
            get
            {
                var q = Quaternion.Euler(0, _baselineYaw, 0);
                var f = q * Vector3.forward;
                return rig && rig.origin ? rig.origin.TransformDirection(f) : f;
            }
        }
        public Vector3 PlayerRight => Vector3.Cross(Vector3.up, PlayerForward);

        public void LogEvent(string evt, string detail)
        {
            _log?.Log(_attempt, _attemptTime, evt, detail, HeadYaw(), HeadPitch());
        }

        // ───────────────────────── Reflex (flinch) logging ─────────────────────────

        void SamplePrev()
        {
            if (rig == null) return;
            if (rig.leftHand) _prevL = rig.leftHand.position;
            if (rig.rightHand) _prevR = rig.rightHand.position;
            if (rig.head) _prevHead = rig.head.position;
        }

        /// <summary>Sudden hand/head movement = the body reacted. Logged only; the rule is unchanged.</summary>
        void TrackFlinch()
        {
            if (rig == null || Time.deltaTime <= 0f) return;
            float dt = Time.deltaTime;
            float vl = rig.leftHand ? (rig.leftHand.position - _prevL).magnitude / dt : 0f;
            float vr = rig.rightHand ? (rig.rightHand.position - _prevR).magnitude / dt : 0f;
            float vh = rig.head ? (rig.head.position - _prevHead).magnitude / dt : 0f;
            SamplePrev();
            if (_flinchCooldownTimer > 0f) { _flinchCooldownTimer -= dt; return; }
            // > 8 m/s is a tracking glitch / controller re-acquire, not a human movement
            if (vl > 8f) vl = 0f; if (vr > 8f) vr = 0f; if (vh > 8f) vh = 0f;
            string part = vh > headFlinchSpeed ? "head" : Mathf.Max(vl, vr) > handFlinchSpeed ? (vl > vr ? "left_hand" : "right_hand") : null;
            if (part == null) return;
            _flinchCount++;
            if (_lastStimulusId != "none") Count(_flinchByStimulus, _lastStimulusId);
            _flinchCooldownTimer = flinchCooldown;
            _log?.Log(_attempt, _attemptTime, "flinch",
                $"{part};hand={Mathf.Max(vl, vr):F2};head={vh:F2};last_stimulus={_lastStimulusId};since_stimulus={(_attemptTime - _lastStimulusTime):F2}",
                HeadYaw(), HeadPitch());
        }

        // ───────────────────────── Helpers ─────────────────────────

        string TopReaction()
        {
            if (_reactionByStimulus.Count == 0) return _resetCount > 0 ? "자극 전 (스스로)" : "없음";
            var top = _reactionByStimulus.OrderByDescending(kv => kv.Value).First();
            var s = stimuli.FirstOrDefault(x => x.id == top.Key);
            return $"{(s ? s.displayName : top.Key)} ({top.Value}회)";
        }

        void UpdateStatusLine()
        {
            if (timer) timer.SetStatus($"초기화 {_resetCount}회   ·   최고 {TimerDisplay.FormatElapsed(_bestStreak)}");
        }

        void SetExitProgress(float v) { if (exitProgress) exitProgress.SetValue(v); }

        static void Count(Dictionary<string, int> d, string k) { d.TryGetValue(k, out int n); d[k] = n + 1; }

        Vector3 HeadForwardLocal()
        {
            if (rig == null || rig.head == null) return Vector3.forward;
            Transform space = rig.origin != null ? rig.origin : null;
            return space ? space.InverseTransformDirection(rig.head.forward) : rig.head.forward;
        }

        float RawYaw()
        {
            var f = HeadForwardLocal();
            return Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
        }

        /// <summary>Yaw relative to the player's front captured at countdown end (deg, + = right).</summary>
        public float HeadYaw() => Mathf.DeltaAngle(_baselineYaw, RawYaw());

        /// <summary>Pitch relative to horizon (deg, + = up).</summary>
        public float HeadPitch()
        {
            var f = HeadForwardLocal();
            return Mathf.Asin(Mathf.Clamp(f.y, -1f, 1f)) * Mathf.Rad2Deg;
        }
    }
}
