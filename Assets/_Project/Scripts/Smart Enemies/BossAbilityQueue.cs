using UnityEngine;
using System.Collections.Generic;

public class BossAbilityQueue
{
    private readonly List<BossAbility> _abilities = new List<BossAbility>();

    public BossAbility LastSelected { get; private set; }

    public void Register(BossAbility ability) => _abilities.Add(ability);

    public void Unregister(BossAbility ability) => _abilities.Remove(ability);

    public void Tick(float deltaTime)
    {
        foreach(var ability in _abilities) ability.Tick(deltaTime);
    }


    public BossAbility SelectBest()
    {
        BossAbility best = null;
        int bestPriority = int.MinValue;

        foreach(var ability in _abilities)
        {
            if (!ability.CanUse()) continue;
            if(ability.Priority > bestPriority)
            {
                bestPriority = ability.Priority;
                best = ability;
            }
        }
        LastSelected = best;
        return best;
    }

    public void ResetCooldown<T>() where T : BossAbility
    {
        foreach (var ability in _abilities) if (ability is T) ability.ResetCooldown();
    }


    // дебаг
    public void DebugAbilityQueue()
    {
        foreach (var ability in _abilities) Debug.Log($"{ability}");
    }
}
