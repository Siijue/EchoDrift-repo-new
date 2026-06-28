using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private string zoneID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        MapManager.Instance?.DiscoverZone(zoneID);
    }
}
