using System;
using UnityEngine;

public partial class AISetActive : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.CurrentPhase.OnValueChanged += OnPhaseChange;
    }

    private void OnDisable()
    {
        GameManager.Instance.CurrentPhase.OnValueChanged -= OnPhaseChange;
    }

    private void OnPhaseChange(GamePhase prev, GamePhase next)
    {
        if (next == GamePhase.Shooting)
        {
            foreach (var ai in FindObjectsOfType<AISetActive>())
            {
                ai.gameObject.SetActive(false);
            }
        }
        else if (next == GamePhase.HideAndSeek)
        {
            foreach (var ai in FindObjectsOfType<AISetActive>())
            {
                ai.gameObject.SetActive(true);
            }
        }
    }
}
