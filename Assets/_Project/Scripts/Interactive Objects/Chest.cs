using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public enum ChestType { Wooden, Silver, Golden }

    [SerializeField] private ChestType chestType;
    [SerializeField] private string hintText = "[E] Открыть сундук";
    [SerializeField] private float hintDuration = 2f;
    [SerializeField] private string OnOpneEvent = "ChestOpened";
    [SerializeField] private bool isNeedEvent = false;
    [SerializeField] private InteractablesSaveTracker tracker;

    private bool isOpen = false;

    private int currentEchoReward;
    private int currentXpReward;

    private void Awake()
    {
        CalculateRewards();
    }

    private void Start()
    {
        if (tracker != null) GameEventBus.Instance?.Subscribe($"ObjectActivated_{tracker.ObjectID}", OnRestoredFromSave);
    }

    public void Interact(PlayerController player)
    {
        if (isOpen) return;

        isOpen = true;
        if (tracker != null) GameEventBus.Instance?.SendEvent($"Activate_{tracker.ObjectID}", this);
        ApplyRewards();
        FadeAndDestroy.FadeAndDestroyObject(gameObject, 0.5f, fadeType: FadeAndDestroy.FadeType.Alpha, FadeAndDestroy.EaseType.EaseOut);
        if (isNeedEvent) GameEventBus.Instance?.SendEvent(OnOpneEvent, this);

        HintManager.Instance?.RemoveHintsFromSource(this);
    }

    private void CalculateRewards()
    {
        switch (chestType)
        {
            case ChestType.Wooden:
                currentEchoReward = Random.Range(3, 6);
                currentXpReward = Random.Range(10, 21);
                break;
            case ChestType.Silver:
                currentEchoReward = Random.Range(8, 15);
                currentXpReward = Random.Range(25, 41); 
                break;
            case ChestType.Golden:
                currentEchoReward = Random.Range(12, 17);
                currentXpReward = Random.Range(35, 51); 
                break;
        }
    }

    private void ApplyRewards()
    {
        EconomyManager.Instance?.AddEcho(currentEchoReward);
        EconomyManager.Instance?.AddXP(currentXpReward);
    }

    private void OnRestoredFromSave(object sender)
    {
        if (this == null) return;
        isOpen = true;
        FadeAndDestroy.FadeAndDestroyObject(gameObject, 0.1f, fadeType: FadeAndDestroy.FadeType.Alpha, FadeAndDestroy.EaseType.EaseOut);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            HintManager.Instance?.RegisterHint(this, hintText, priotiry: 4, duration: hintDuration);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HintManager.Instance?.RemoveHintsFromSource(this);
        }
    }

    private void OnDestroy()
    {
        if (tracker != null) GameEventBus.Instance?.Unsubscribe($"ObjectActivated_{tracker.ObjectID}", OnRestoredFromSave);
    }
}