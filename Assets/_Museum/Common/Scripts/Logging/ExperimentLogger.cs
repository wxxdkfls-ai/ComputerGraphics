using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace MuseumOfLies.Common
{
    /// <summary>
    /// Shared CSV logger. One file per session.
    /// Editor: <project>/ExperimentLogs/   Device: Application.persistentDataPath/ExperimentLogs/
    /// Columns: session_id, hall, condition, t_session, attempt, t_attempt, event, detail, head_yaw, head_pitch
    /// </summary>
    public class ExperimentLogger
    {
        public string SessionId { get; }
        public string FilePath { get; }
        readonly string _hall;
        readonly string _condition;
        readonly float _startTime;
        StreamWriter _writer;

        public ExperimentLogger(string hall, string condition)
        {
            _hall = hall;
            _condition = condition;
            _startTime = Time.realtimeSinceStartup;
            SessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

#if UNITY_EDITOR
            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath), "ExperimentLogs");
#else
            string dir = Path.Combine(Application.persistentDataPath, "ExperimentLogs");
#endif
            Directory.CreateDirectory(dir);
            FilePath = Path.Combine(dir, $"{hall}_{SessionId}.csv");
            _writer = new StreamWriter(FilePath, false, new UTF8Encoding(true));
            _writer.WriteLine("session_id,hall,condition,t_session,attempt,t_attempt,event,detail,head_yaw,head_pitch");
            _writer.Flush();
        }

        public void Log(int attempt, float tAttempt, string evt, string detail = "", float headYaw = float.NaN, float headPitch = float.NaN)
        {
            if (_writer == null) return;
            var ci = CultureInfo.InvariantCulture;
            float t = Time.realtimeSinceStartup - _startTime;
            _writer.WriteLine(string.Join(",",
                SessionId, _hall, _condition,
                t.ToString("F3", ci), attempt.ToString(ci), tAttempt.ToString("F3", ci),
                Escape(evt), Escape(detail),
                float.IsNaN(headYaw) ? "" : headYaw.ToString("F1", ci),
                float.IsNaN(headPitch) ? "" : headPitch.ToString("F1", ci)));
            _writer.Flush();
        }

        public void Close()
        {
            _writer?.Flush();
            _writer?.Dispose();
            _writer = null;
        }

        static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Contains(",") || s.Contains("\"") ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }
    }
}
