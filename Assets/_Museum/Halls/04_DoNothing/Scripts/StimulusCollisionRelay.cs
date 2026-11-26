using UnityEngine;

namespace MuseumOfLies.DoNothing
{
    /// <summary>Forwards collision impacts of a stimulus object (e.g. coin) to its DoNothingStimulus for sound.</summary>
    public class StimulusCollisionRelay : MonoBehaviour
    {
        public DoNothingStimulus owner;

        void OnCollisionEnter(Collision c)
        {
            if (owner != null) owner.OnTargetCollision(c.relativeVelocity.magnitude);
        }
    }
}
