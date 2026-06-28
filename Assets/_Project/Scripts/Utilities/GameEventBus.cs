using System;
using System.Collections.Generic;
using UnityEngine;

public class GameEventBus : MonoBehaviour
{
    public static GameEventBus Instance { get; private set; }
    private Dictionary<string, List<Action<object>>> eventListeners = new Dictionary<string, List<Action<object>>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Subscribe(string eventName, Action<object> listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null) return;

        if (!eventListeners.ContainsKey(eventName))
        {
            eventListeners[eventName] = new List<Action<object>>();
        }

        if (!eventListeners[eventName].Contains(listener))
        {
            eventListeners[eventName].Add(listener);
        }
    }
    public void Unsubscribe(string eventName, Action<object> listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null) return;

        if (eventListeners.ContainsKey(eventName))
        {
            eventListeners[eventName].Remove(listener);

            if (eventListeners[eventName].Count == 0)
            {
                eventListeners.Remove(eventName);
            }
        }
    }
    public void SendEvent(string eventName, object sender = null)
    {
        if (string.IsNullOrEmpty(eventName)) return;

        if (sender == null) sender = eventName;

        if (eventListeners.ContainsKey(eventName))
        {
            List<Action<object>> listeners = new List<Action<object>>(eventListeners[eventName]);

            foreach (var listener in listeners)
            {
                listener?.Invoke(sender);
            }
        }
    }

    public void ClearAll() => eventListeners.Clear();
    public void ClearListeners(object target)
    {
        foreach (var kvp in eventListeners)
        {
            kvp.Value.RemoveAll(listener => listener.Target == target);
        }
    }
}