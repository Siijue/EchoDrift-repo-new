using UnityEngine;

public class EventActivator : MonoBehaviour
{
    [SerializeField] private string activationEvent = "ActivatedEvent";
    [SerializeField] private GameObject targetObject;
    [SerializeField] private bool deactivateWhenDisable = false;
    [SerializeField] private bool isDeactivate = false;
    [SerializeField] private InteractablesSaveTracker tracker;

    private void Awake()
    {
        if (tracker == null && targetObject != null)
        {
            tracker = targetObject.GetComponent<InteractablesSaveTracker>();
        }
    }

    private void OnEnable() => GameEventBus.Instance.Subscribe(activationEvent, OnEventReceived);

    private void OnDisable()
    {
        GameEventBus.Instance.Unsubscribe(activationEvent, OnEventReceived);
        if (deactivateWhenDisable && targetObject != null) targetObject.SetActive(false);
        if (tracker != null) GameEventBus.Instance?.SendEvent($"Deactivate_{tracker.ObjectID}", this);
    }

    private void OnEventReceived(object sender)
    {
        if (targetObject == null) return;
        if (!isDeactivate)
        {
            targetObject.SetActive(true);
            if (tracker != null) GameEventBus.Instance?.SendEvent($"Activate_{tracker.ObjectID}", this);
        }
        else 
        { 
            targetObject.SetActive(false);
            if (tracker != null) GameEventBus.Instance?.SendEvent($"Deactivate_{tracker.ObjectID}", this);
        }
    }
}
