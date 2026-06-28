using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ColorNode : MonoBehaviour
{
    [Header("Настройки цвета")]
    [SerializeField] public Color nodeColor = Color.red;
    [SerializeField] private string crystalEventName = "Crystal_Red_Activated";


    [Header("Визуал")]
    [SerializeField] private SpriteRenderer circleSprite;
    [SerializeField] private Light2D circleLight;
    [SerializeField] private Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 1f); 
    [SerializeField] private float inactiveIntensity = 0f;
    [SerializeField] private float activeIntensity = 1.5f;

    [Header("Состояние")]
    public bool IsActive { get; private set; }

    private void Awake()
    {
        if (circleSprite == null) circleSprite = GetComponent<SpriteRenderer>();
        if (circleLight == null) circleLight = GetComponent<Light2D>();
        UpdateVisual();
    }

    private void OnEnable() => GameEventBus.Instance?.Subscribe(crystalEventName, ActivateNode);
    private void OnDisable() => GameEventBus.Instance?.Unsubscribe(crystalEventName, ActivateNode);

    private void ActivateNode(object sender)
    {
        if (IsActive) return;
        IsActive = true;
        UpdateVisual();

        ColorMixController controller = FindFirstObjectByType<ColorMixController>();
        controller?.OnNodeActivated(this);
    }

    public void ResetNode()
    {
        IsActive = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (circleSprite != null)
        {
            circleSprite.color = IsActive ? nodeColor : inactiveColor;
        }

        if (circleLight != null)
        {
            circleLight.enabled = IsActive;
            circleLight.intensity = IsActive ? activeIntensity : inactiveIntensity;
        }
    }
}