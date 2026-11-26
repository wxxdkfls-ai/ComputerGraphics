using MuseumOfLies.Common;
using UnityEngine;

namespace MuseumOfLies.DoNothing
{
    /// <summary>Base for scripted stimuli (cockroach, figure behind, floor collapse). Driven by DoNothingStimulus (Kind = Custom).</summary>
    public abstract class StimulusAction : MonoBehaviour
    {
        protected DoNothingGame Game { get; private set; }
        protected XRRig Rig { get; private set; }
        public bool Running { get; protected set; }

        protected virtual void Awake()
        {
            Game = FindFirstObjectByType<DoNothingGame>();
            Rig = FindFirstObjectByType<XRRig>();
        }

        public abstract void OnFire();
        public abstract void OnReset();
    }
}
