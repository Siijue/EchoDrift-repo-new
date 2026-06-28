using UnityEngine;
using System.Collections.Generic;

public class RatSpawnManager : MonoBehaviour
{
    private GameObject _ratPrefab;
    private Transform[] _spawnPoints;
    private int _maxRats;
    private Transform _ownerTransform;

    private float _patrolRadius = 5f;
    private float _patrolYOffset = 0f;
    private bool _useGroundDetection = true;

    private List<GameObject> _activeRats = new List<GameObject>();


    public void Initialize(GameObject ratPrefab, int maxRats, Transform ownerTransform, float patrolRadius = 5f, float patrolYOffset = 0f)
    {
        _ratPrefab = ratPrefab;
        _maxRats = maxRats;
        _ownerTransform = ownerTransform;
        _patrolRadius = patrolRadius;
        _patrolYOffset = patrolYOffset;
    }

    private void Update() => _activeRats.RemoveAll(rat => rat == null);


    public bool CanSpawn()
    {
        if (_ratPrefab == null) return false;

        return AliveCount < _maxRats;
    }

    public int AliveCount => _activeRats.Count;

    public void SpawnRat()
    {
        if (!CanSpawn()) return;

        Vector3 spawnPos = GetSpawnPosition();
        GameObject rat = Object.Instantiate(_ratPrefab, spawnPos, Quaternion.identity);

        RatAI ratAI = rat.GetComponent<RatAI>();
        if(ratAI != null)
        {
            float patrolY = GetPatrolHeight(spawnPos);
            RatPatrolZone zone = new RatPatrolZone(_ownerTransform.position.x, _patrolRadius, patrolY);
            ratAI.SetPatrolZone(zone);
        }

        _activeRats.Add(rat);
    }

    public void DismissAllRats(float delay = 5f)
    {
        foreach (GameObject rat in _activeRats)
        {
            if(rat == null) continue;

            RatAI ratAI = rat.GetComponent<RatAI>();

            if(ratAI != null) ratAI.TransitionTo(ratAI.GetFleeState());

            Object.Destroy(rat, delay);
        }
        _activeRats.Clear();
    }

    private float GetPatrolHeight(Vector3 spawnPos)
    {
        if (_useGroundDetection)
        {
            Vector2 rayOrig = new Vector2(spawnPos.x, spawnPos.y + 10f);
            RaycastHit2D hit = Physics2D.Raycast(rayOrig, Vector2.down, 20f);

            if (hit.collider != null) return hit.point.y + _patrolYOffset;
        }

        return _ownerTransform.position.y + _patrolYOffset;
    }

    private Vector3 GetSpawnPosition()
    {
        if(_spawnPoints != null && _spawnPoints.Length > 0)
        {
            int index = Random.Range(0, _spawnPoints.Length);
            return _spawnPoints[index].position;
        }

        Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), 0f, 0f);

        return _ownerTransform.position + offset;
    }
}