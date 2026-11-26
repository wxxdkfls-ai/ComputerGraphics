using System.Collections.Generic;
using UnityEngine;

namespace MuseumOfLies.Common
{
    /// <summary>
    /// One museum visit, kept across scene loads (static).
    /// - Which hall is behind which gate (shuffled per visit so a "first choice" reflects the door, not the content)
    /// - Visit order, completed halls, entry confidence answer, the gate the player last used
    /// </summary>
    public static class MuseumSession
    {
        public static readonly string[] HallIds = { "Weight", "Body", "Ability", "DoNothing" };

        public static bool Started { get; private set; }
        public static string VisitId { get; private set; }
        public static string[] GateToHall { get; private set; } = new string[4];   // index = gate (0..3)
        public static readonly List<string> VisitOrder = new List<string>();      // hall ids in entry order
        public static readonly List<int> GateOrder = new List<int>();              // gate indices in entry order
        public static readonly HashSet<string> Completed = new HashSet<string>();
        public static int EntryConfidence = -1;                                    // 1..7, -1 = not answered
        public static int LastGate = -1;
        public static string CurrentHall;

        public static void Begin(bool randomize, int seed = 0)
        {
            Started = true;
            VisitId = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            VisitOrder.Clear(); GateOrder.Clear(); Completed.Clear();
            EntryConfidence = -1; LastGate = -1; CurrentHall = null;
            var halls = new List<string>(HallIds);
            if (randomize)
            {
                var rng = seed == 0 ? new System.Random() : new System.Random(seed);
                for (int i = halls.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (halls[i], halls[j]) = (halls[j], halls[i]); }
            }
            GateToHall = halls.ToArray();
        }

        public static void Enter(int gate)
        {
            LastGate = gate;
            CurrentHall = GateToHall[gate];
            VisitOrder.Add(CurrentHall);
            GateOrder.Add(gate);
        }

        /// <summary>Called by a hall when its visit is complete (success or quit both count).</summary>
        public static void CompleteCurrentHall()
        {
            if (!string.IsNullOrEmpty(CurrentHall)) Completed.Add(CurrentHall);
        }

        public static bool AllCompleted => Completed.Count >= HallIds.Length;
        public static string MappingString => string.Join("|", GateToHall);
    }
}
