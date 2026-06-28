using UnityEngine;
using UnityEngine.UI;

public class TorchIndicatorPanel : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] indicators;

    [SerializeField] private GameObject[] torches;

    [SerializeField] private Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color activeColor = new Color(1f, 0.8f, 0.2f, 1f);

    [SerializeField] private string torchEvent = "TorchActivated";

    private void Awake()
    {
        foreach (var indicator in indicators)
        {
            if (indicator != null) indicator.color = inactiveColor;
        }
        if (!string.IsNullOrEmpty(torchEvent))
        {
            GameEventBus.Instance?.Subscribe(torchEvent, OnTorchActivated);
        }
    }

    private void OnTorchActivated(object sender)
    {
        if (sender == null) return;

        GameObject senderObject = null;

        if (sender is MonoBehaviour mono)
        {
            senderObject = mono.gameObject;
        }
        else if (sender is GameObject go)
        {
            senderObject = go;
        }

        if (senderObject == null) return;

        for (int i = 0; i < torches.Length; i++)
        {
            if (torches[i] != null && torches[i] == senderObject)
            {
                ActivateIndicator(i);
                return;
            }
        }

    }

    private void ActivateIndicator(int index)
    {
        if (index < 0 || index >= indicators.Length) return;
        if (indicators[index] == null) return;

        indicators[index].color = activeColor;
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(torchEvent))
        {
            GameEventBus.Instance?.Unsubscribe(torchEvent, OnTorchActivated);
        }
    }
}