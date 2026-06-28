using UnityEngine;
using System.Collections;

public class PuzzleDoor : MonoBehaviour
{
    public enum ActivationType
    {
        SingleEvent,
        MultipleEvents,
        Sequence
    }

    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right,
        Custom
    }

    [SerializeField] private ActivationType activationType = ActivationType.SingleEvent;
    [SerializeField] private string activationEvent = "PuzzleSolved";
    [SerializeField] private int requiredCount = 1;
    [SerializeField] private string[] sequenceEvents;
    [SerializeField] private MoveDirection direction = MoveDirection.Up;
    [SerializeField] private Vector2 customDirection = Vector2.up;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool smoothMove = true;
    [SerializeField] private GameObject openedVisual;
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private bool isOneTime = true;
    [SerializeField] private bool autoClose = false;
    [SerializeField] private float autoCloseDelay = 5f;
    [SerializeField] private bool isOpen = false;
    [SerializeField] private string onResetEvent = "ResetPuzzle";
    [SerializeField] private InteractablesSaveTracker tracker;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private int currentCount = 0;
    private int currentSequenceIndex = 0;
    private Coroutine moveCoroutine;
    private Coroutine autoCloseCoroutine;
    private Coroutine resetCoroutine;
    private bool isLoadingFromSave = false;

    private void Awake()
    {
        startPosition = transform.position;
        targetPosition = startPosition + GetDirectionVector() * moveDistance;

        if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
        if (tracker == null) tracker = GetComponent<InteractablesSaveTracker>();
    }

    private void Start()
    {
        if (tracker != null)
        {
            Invoke(nameof(SubscribeToTracker), 0.1f);
        }
    }

    private void SubscribeToTracker()
    {
        if (tracker == null) return;
        isLoadingFromSave = true;

        GameEventBus.Instance?.Subscribe($"ObjectActivated_{tracker.ObjectID}", OnRestoredFromSave);
        GameEventBus.Instance?.Subscribe($"ObjectDeactivated_{tracker.ObjectID}", OnRestoredFromSave);
        Invoke(nameof(DisableLoading), 0.2f);
    }

    private void DisableLoading()
    {
        isLoadingFromSave = false;
    }

    private Vector2 GetDirectionVector()
    {
        return direction switch
        {
            MoveDirection.Up => Vector2.up,
            MoveDirection.Down => Vector2.down,
            MoveDirection.Left => Vector2.left,
            MoveDirection.Right => Vector2.right,
            MoveDirection.Custom => customDirection.normalized,
            _ => Vector2.up
        };
    }

    private void OnEnable()
    {
        if (GameEventBus.Instance == null) return;

        if (activationType == ActivationType.SingleEvent || activationType == ActivationType.MultipleEvents) GameEventBus.Instance.Subscribe(activationEvent, OnEventReceived);
        else if (activationType == ActivationType.Sequence && sequenceEvents != null)
        {
            foreach (string eventName in sequenceEvents)
            {
                GameEventBus.Instance.Subscribe(eventName, OnSequenceEventReceived);
            }
        }
    }

    private void OnDisable()
    {
        GameEventBus.Instance?.Unsubscribe(activationEvent, OnEventReceived);

        if (activationType == ActivationType.Sequence && sequenceEvents != null)
        {
            foreach (string eventName in sequenceEvents)
            {
                GameEventBus.Instance?.Unsubscribe(eventName, OnSequenceEventReceived);
            }
        }

        if (tracker != null)
        {
            GameEventBus.Instance?.Unsubscribe($"ObjectActivated_{tracker.ObjectID}", OnRestoredFromSave);
            GameEventBus.Instance?.Unsubscribe($"ObjectDeactivated_{tracker.ObjectID}", OnRestoredFromSave);
        }

        StopAllCoroutines();
    }

    private void OnEventReceived(object sender)
    {
        if (isOpen && isOneTime) return;

        if (activationType == ActivationType.SingleEvent)OpenDoor();
        else if (activationType == ActivationType.MultipleEvents)
        {
            currentCount++;
            if (currentCount >= requiredCount) OpenDoor();
        }
    }

    private string GetEventName(object sender)
    {
        if (sender is string eventName) return eventName;
        if (sender is GameObject obj) return obj.name;
        if (sender is Component comp) return comp.gameObject.name;
        return "Unknown";
    }

    public void OpenDoor()
    {
        if (isOpen && isOneTime) return;

        if (!gameObject.activeInHierarchy) return;

        isOpen = true;
        GameEventBus.Instance?.SendEvent(activationEvent);

        if (tracker != null)
        {
            GameEventBus.Instance?.SendEvent($"Activate_{tracker.ObjectID}", this);
        }

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveTo(targetPosition));

        if (doorCollider != null) doorCollider.enabled = false;

        UpdateVisuals();
        if (autoClose && !isOneTime)
        {
            if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
        }
    }

    public void CloseDoor()
    {
        if (!isOpen) return;

        if (tracker != null) GameEventBus.Instance?.SendEvent($"Deactivate_{tracker.ObjectID}", this);

        isOpen = true;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveTo(startPosition));

        isOpen = false;
        if (doorCollider != null) doorCollider.enabled = true;
        UpdateVisuals();
    }

    private IEnumerator MoveTo(Vector2 target)
    {
        if (!smoothMove)
        {
            transform.position = target;
            yield break;
        }

        Vector2 start = transform.position;
        float elapsed = 0f;
        float duration = Vector2.Distance(start, target) / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector2.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
    }

    private void UpdateVisuals()
    {
        if (openedVisual != null) openedVisual.SetActive(isOpen);
        if (closedVisual != null) closedVisual.SetActive(!isOpen);
    }


    private IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseDoor();
    }

    public void ResetPuzzle()
    {
        currentCount = 0;
        currentSequenceIndex = 0;
        CloseDoor();
    }

    private void OnSequenceEventReceived(object sender)
    {
        if (isOpen && isOneTime) return;

        string eventName = GetEventName(sender);

        if (sequenceEvents == null || sequenceEvents.Length == 0) return;

        if (currentSequenceIndex < sequenceEvents.Length && eventName == sequenceEvents[currentSequenceIndex])
        {
            currentSequenceIndex++;
            RestartResetTimer();

            if (currentSequenceIndex >= sequenceEvents.Length)
            {
                StopResetTimer();
                OpenDoor();
            }
        }
        else ResetSequence();
    }

    public void ResetSequence()
    {
        currentSequenceIndex = 0;
        StopResetTimer();
        GameEventBus.Instance?.SendEvent(onResetEvent);
    }

    private void RestartResetTimer()
    {
        StopResetTimer();
    }

    private void StopResetTimer()
    {
        if(resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
    }

    private void OnRestoredFromSave(object sender)
    {
        if (tracker == null) return;
        if (!isLoadingFromSave) return;
        if (tracker.CurrentState)
        {
            transform.position = targetPosition;
            isOpen = true;
            if (doorCollider != null) doorCollider.enabled = false;
            UpdateVisuals();
        }
        else
        {
            transform.position = startPosition;
            isOpen = false;
            if (doorCollider != null) doorCollider.enabled = true;
            UpdateVisuals();
        }
    }
}