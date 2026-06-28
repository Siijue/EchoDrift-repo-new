using System.Collections.Generic;
using UnityEngine;

public class InteractablesSaveTracker : MonoBehaviour
{
    [SerializeField] private string objectID;
    [SerializeField] private bool initialState = false;

    public string ObjectID => objectID;
    public bool CurrentState { get; private set; }

    private void Awake()
    {
        if (string.IsNullOrEmpty(objectID)) objectID = $"{gameObject.name}_{transform.position.x : F1}_{transform.position.y : F1}";
        CurrentState = initialState;
    }

    private void Start()
    {
        CheckStateInSave();

        GameEventBus.Instance?.Subscribe($"Activate_{objectID}", OnActivate);
        GameEventBus.Instance?.Subscribe($"Deactivate_{objectID}", OnDeactivate);
    }

    private void CheckStateInSave()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData == null || saveData.activatedObjects == null) return;
        if (saveData.activatedObjects.Contains(objectID))
        {
            if(!CurrentState) SetState(true);
        }
    }

    public void SetState(bool newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;

        string eventName = newState ? $"ObjectActivated_{objectID}" : $"ObjectDeactivated_{objectID}";
        GameEventBus.Instance?.SendEvent(eventName, this);
        SaveState();
    }

    private void SaveState()
    {
        SaveData saveData = SaveSystem.Load();
        if(saveData == null)
        {
            saveData = new SaveData();
            saveData.hasStartedGame = true;
        }

        if (saveData.activatedObjects == null) saveData.activatedObjects = new List<string>();

        if (CurrentState)
        {
            if (!saveData.activatedObjects.Contains(objectID)) saveData.activatedObjects.Add(objectID);
        }
        else saveData.activatedObjects.Remove(objectID);

        SaveSystem.Save(saveData);
    }

    private void OnActivate(object sender) => SetState(true);
    private void OnDeactivate(object sender) => SetState(false);

    private void OnDestroy()
    {
        GameEventBus.Instance?.Unsubscribe($"Activate_{objectID}", OnActivate);
        GameEventBus.Instance?.Unsubscribe($"Deactivate_{objectID}", OnDeactivate);
    }
}
