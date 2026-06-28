using UnityEngine;
using System.Collections.Generic;

public class LightSourceRegistry : MonoBehaviour
{
    private static readonly HashSet<LightSource> _sources = new HashSet<LightSource>();

    public static void Register(LightSource source) => _sources.Add(source);

    public static void Unregister(LightSource source) => _sources.Remove(source);

    public static bool IsPositionLit(Vector2 position)
    {
        foreach (LightSource source in _sources)
        {
            if (source == null) continue;

            float dist = Vector2.Distance(position, source.position);

            //Debug.Log($"Checking: {source.name} | sourcePos={source.position} | checkPos: {position} | dist: {dist:F2}");
            if (dist <= source.radius) return true;
        }
        return false;
    }

    public static LightSource GetNearestLitSource(Vector2 position)
    {
        LightSource nearest = null;
        float nearestDist = float.MaxValue;

        foreach (LightSource source in _sources)
        {
            if(source == null) continue;

            float dist = Vector2.Distance(position, source.position);
            if (dist <= nearestDist && dist <= source.radius)
            {
                nearestDist = dist;
                nearest = source;
            }
        }

        return nearest;
    }

    public static bool IsPositionLitWithRaycast(Vector2 position, LayerMask layerMask)
    {
        foreach (LightSource source in _sources)
        {
            if (source == null) continue;

            float dist = Vector2.Distance(position, source.position);
            if (dist > source.radius) continue;

            Vector2 dir = (position - source.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(source.position, dir, dist, layerMask);

            if(hit.collider == null) return true;
        }
        return false;
    }

    public static void DebugSources(Vector2 checkPos)
    {
        Debug.Log($"LightSourceRegistry: {_sources.Count} источников");

        foreach (var source in _sources)
        {
            if (source == null) { Debug.Log("null source"); continue; }
            float dist = Vector2.Distance(checkPos, source.position);
            bool inRange = dist <= source.radius;
            Debug.Log($"имя: {source.gameObject.name} | ID: {source.gameObject.GetInstanceID()} | тип: {source.srcType} | dist: {dist:F2} | radius: {source.radius} | inRange: {inRange} | active: {source.IsActive}");
        }
    }

    public static int ActiveSourceCount => _sources.Count;
}
