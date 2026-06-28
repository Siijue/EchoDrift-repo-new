using UnityEngine;
using System.Collections.Generic;

public class ColorMixController : MonoBehaviour
{
    [Header("Узлы")]
    [SerializeField] private ColorNode[] nodes;

    [Header("Миксер (центральный круг)")]
    [SerializeField] private SpriteRenderer mixerSprite;
    [SerializeField] private Color emptyMixerColor = new Color(0.15f, 0.15f, 0.15f, 1f);

    [Header("Цель и проверка")]
    [SerializeField] private Color targetColor = Color.magenta; 
    [SerializeField] private float matchThreshold = 0.12f; 
    [SerializeField] private string solvedEvent = "ColorPuzzleSolved";

    [Header("Настройки")]
    [SerializeField] private bool isSolved = false;
    [SerializeField] private string resetEvent = "ResetColorPuzzle";

    [Header("Кристаллы для сброса")]
    [SerializeField] private ChainCrystal[] crystals;

    private List<Color> activeColors = new List<Color>();

    private void OnEnable() => GameEventBus.Instance?.Subscribe(resetEvent, ResetPuzzle);
    private void OnDisable() => GameEventBus.Instance?.Unsubscribe(resetEvent, ResetPuzzle);

    public void OnNodeActivated(ColorNode node)
    {
        if (isSolved) return;
        if (!activeColors.Contains(node.nodeColor))
        {
            activeColors.Add(node.nodeColor);
        }
        UpdateMixerVisual();
        CheckSolution();
    }

    private void UpdateMixerVisual()
    {
        if (activeColors.Count == 0)
        {
            mixerSprite.color = emptyMixerColor;
            return;
        }

        Color mixed = Color.black;
        foreach (var c in activeColors) mixed += c;

        mixed.r = Mathf.Clamp01(mixed.r);
        mixed.g = Mathf.Clamp01(mixed.g);
        mixed.b = Mathf.Clamp01(mixed.b);
        mixed.a = 1f;

        mixerSprite.color = mixed;
    }

    private void ResetPuzzle(object sender)
    {
        isSolved = false;
        activeColors.Clear();

        foreach (var node in nodes) node.ResetNode();

        foreach (var crystal in crystals)
        {
            if (crystal != null) crystal.ResetCrystal();
        }

        mixerSprite.color = emptyMixerColor;
        Debug.Log("Головоломка сброшена");
    }

    private void CheckSolution()
    {
        if (isSolved) return;

        Color current = mixerSprite.color;
        float diff = Vector3.Distance(
            new Vector3(current.r, current.g, current.b),
            new Vector3(targetColor.r, targetColor.g, targetColor.b)
        );

        if (diff < matchThreshold)
        {
            isSolved = true;
            Debug.Log("Головоломка решена! Цвет совпал.");
            GameEventBus.Instance?.SendEvent(solvedEvent);
        }
    }
}