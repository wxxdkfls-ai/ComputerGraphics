using System.Collections.Generic;
using MuseumOfLies.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MuseumOfLies.CentralHall
{
    /// <summary>
    /// 입장 질문 (원안 §5.4): "당신은 자신의 감각과 행동을 얼마나 정확하게 알고 있다고 생각합니까?" 1–7.
    /// Answer is stored in MuseumSession and compared again in the Truth Room.
    /// </summary>
    public class EntrySurvey : MonoBehaviour
    {
        public List<MuseumButton> scaleButtons = new List<MuseumButton>(); // 1..7
        public MuseumButton confirmButton;
        public System.Action<int> Answered;
        int _choice = -1;

        void Start()
        {
            for (int i = 0; i < scaleButtons.Count; i++)
            {
                int v = i + 1;
                scaleButtons[i].Button.onClick.AddListener(() => Select(v));
            }
            if (confirmButton) confirmButton.Button.onClick.AddListener(Confirm);
            Refresh();
        }

        void Select(int v) { _choice = v; Refresh(); }

        void Refresh()
        {
            for (int i = 0; i < scaleButtons.Count; i++)
                scaleButtons[i].SetStyle(_choice == i + 1 ? MuseumButton.Style.Primary : MuseumButton.Style.Secondary);
            if (confirmButton) confirmButton.SetStyle(_choice > 0 ? MuseumButton.Style.Primary : MuseumButton.Style.Disabled);
        }

        void Confirm()
        {
            if (_choice < 1) return;
            MuseumSession.EntryConfidence = _choice;
            Answered?.Invoke(_choice);
            gameObject.SetActive(false);
        }
    }
}
