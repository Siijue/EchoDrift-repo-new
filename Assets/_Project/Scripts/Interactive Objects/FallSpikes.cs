using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class FallSpikes : MonoBehaviour
{
    public enum ActivationMode
    {
        Auto,
        ByButton
    }

    [SerializeField] private ActivationMode activationMode = ActivationMode.Auto;
    [SerializeField] private string buttonEventName = "ButtonPressed";
    [SerializeField] private float fallInterval = 3f;
    [SerializeField] private float fallDuration = 1.5f;
    [SerializeField] private float fallDistance = 10f;
    [Tooltip("вниз (0, -1, 0); вверх (0, 1, 0); влево (-1, 0, 0); вправо (1. 0, 0)")]
    [SerializeField] private Vector3 fallDirection = Vector3.down;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private bool useAcceleration = true;
    [SerializeField] private float cycleCooldown = 2f;

    private Rigidbody2D rb;
    private Collider2D coll;
    private SpriteRenderer spr;
    private Light2D light;
    private Vector3 ceilingPosition;
    private Vector3 floorPosition;
    private bool isActive = false;
    private bool isCycleRunning = false;
    private float lastCycleEndTime = -100f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        spr = GetComponent<SpriteRenderer>();
        light = GetComponent<Light2D>();

        ceilingPosition = transform.position;
        floorPosition = ceilingPosition + fallDirection.normalized * fallDistance;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
    }

    private void Start()
    {
        if (activationMode == ActivationMode.ByButton) GameEventBus.Instance?.Subscribe(buttonEventName, OnButtonPressed);

        if (activationMode == ActivationMode.Auto) StartCoroutine(FallCycle());
    }

    private void OnDestroy()
    {
        if (activationMode == ActivationMode.ByButton) GameEventBus.Instance?.Unsubscribe(buttonEventName, OnButtonPressed);
    }

    private void OnButtonPressed(object sender)
    {
        if (Time.time - lastCycleEndTime < cycleCooldown) return;
        if (!isCycleRunning) StartCoroutine(FallCycle());
    }

    private IEnumerator FallCycle()
    {
        isCycleRunning = true;

        if (activationMode != ActivationMode.ByButton) yield return new WaitForSeconds(fallInterval);

        Appear();

        yield return StartCoroutine(FallDown());
        yield return new WaitForSeconds(respawnDelay);

        Disappear();

        lastCycleEndTime = Time.time;
        isCycleRunning = false;

        if (activationMode == ActivationMode.Auto) StartCoroutine(FallCycle());
    }

    private IEnumerator FallDown()
    {
        isActive = true;
        coll.enabled = true;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float progression = useAcceleration ? t * t : t;
            transform.position = Vector3.Lerp(startPos, floorPosition, progression);
            yield return null;
        }

        transform.position = floorPosition;
        isActive = false;
        coll.enabled = false;
    }
    private void Disappear()
    {
        if (spr != null) spr.enabled = false;
        if(light != null) light.enabled = false;
        coll.enabled = false;
    }

    private void Appear()
    {
        transform.position = ceilingPosition;
        if (spr != null) spr.enabled = true;
        if(light != null) light.enabled = true;
        coll.enabled = true;
    }
}