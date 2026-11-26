using TMPro;
using UnityEngine;

namespace MuseumOfLies.CentralHall
{
    /// <summary>
    /// One of the four identical exhibition gates. Which hall is behind it is decided at runtime
    /// (MuseumSession.GateToHall) — the gate itself carries no hint.
    /// The player "enters" when the head passes `enterDepth` metres into the vestibule behind the door.
    /// </summary>
    public class Gate : MonoBehaviour
    {
        [Tooltip("0..3 — stable id of this physical door position")]
        public int index;
        public string positionLabel = "L1";        // for logs: L1 L2 R1 R2 (left/right side, near/far)
        public AutoDoor door;
        public TMP_Text placardStatus;             // "관람 가능" / "✓ 관람 완료"
        public TMP_Text comingSoon;                // shown at the vestibule end if the hall isn't built yet
        public GameObject comingSoonRoot;          // canvas + board together (hidden by default so the vestibule reveals nothing)
        public Transform returnSpawn;              // where the player stands when coming back out of this gate
        public float enterDepth = 1.6f;

        public bool Completed { get; private set; }

        public void SetCompleted(bool done)
        {
            Completed = done;
            if (placardStatus)
            {
                placardStatus.text = done ? "✓  관람 완료" : "전시 관람 가능";
                placardStatus.color = done ? Common.MuseumPalette.Success : Common.MuseumPalette.Charcoal;
            }
        }

        /// <summary>How far the head is past the door plane into the vestibule (m). Gate forward points INTO the hall.</summary>
        public float DepthInside(Vector3 headPos)
        {
            Vector3 local = transform.InverseTransformPoint(headPos);
            return Mathf.Abs(local.x) < 1.3f ? local.z : -99f;
        }

        public void ShowComingSoon(bool on)
        {
            if (comingSoonRoot) comingSoonRoot.SetActive(on);
            else if (comingSoon) comingSoon.gameObject.SetActive(on);
        }
    }
}
