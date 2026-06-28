using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
public class StatusDataSystem : MonoBehaviour
{
    private readonly Dictionary<StatusType, StatusData> _active = new Dictionary<StatusType, StatusData>();

    private PlayerHealth _health;

    public UnityEvent<StatusType, float> onStatusAdded;
    public UnityEvent<StatusType> onStatusRemoved;

    public bool CanMove => !HasBlockingStatus(st => st.BlockMovement);
    public bool CanJump => !HasBlockingStatus(st => st.BlockJump);


    public float SpeedMultiplyer
    {
        get
        {
            float result = 1f;
            foreach (var data in _active.Values) result += data.SpeedMultiplier;
            return result;
        }
    }

    public bool HasStatus(StatusType type) => _active.ContainsKey(type);

    private void Awake() => _health = GetComponent<PlayerHealth>();

    private void Update()
    {
        List<StatusType> toRemove = null;

        foreach (var item in _active)
        {
            StatusData data = item.Value;
            data.Tick(Time.deltaTime);

            if (data.Type == StatusType.Burn && _health != null) _health.TakeDamage(data.DemageForSeconds * Time.deltaTime);
            if (data.IsExpired)
            {
                toRemove ??= new List<StatusType>();
                toRemove.Add(item.Key);
            }
        }
        if(toRemove != null)
        {
            foreach(var type in toRemove)
            {
                _active.Remove(type);
                onStatusRemoved?.Invoke(type);
            }
        }
    }

    public void AddStatus(StatusData data)
    {
        bool isNew = !_active.ContainsKey(data.Type);
        _active[data.Type] = data;
        if (isNew) onStatusAdded?.Invoke(data.Type, data.Duration);
    }

    public void RemoveStatus(StatusType type)
    {
        if (_active.Remove(type)) onStatusRemoved?.Invoke(type);  
    }

    public void ClearAll()
    {
        foreach(var stat in new List<StatusType>(_active.Keys))
        {
            onStatusRemoved?.Invoke(stat);
        }
        _active.Clear();
    }

    private bool HasBlockingStatus(Func<StatusData, bool> predicate)
    {
        foreach (var data in _active.Values)
        {
            if (predicate(data)) return true;
        }
        return false;
    }
}
