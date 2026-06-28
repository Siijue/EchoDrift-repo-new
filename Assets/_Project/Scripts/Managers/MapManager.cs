using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [SerializeField] private Transform player;
    [SerializeField] private MapZoneSO[] allZones;
    [SerializeField] private Checkpoint[] allCheckpoints;

    private HashSet<string> discoveredZones = new HashSet<string>();
    private List<string> activatedCheckpointIDs = new List<string>();

    public string CurrentCheckpointID { get; private set; }

    public UnityEvent<string> OnZoneDiscovered = new UnityEvent<string>();
    public UnityEvent OnMapDataChanged = new UnityEvent();


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        DontDestroyOnLoad(gameObject);
    }

    public void LoadFromSave()
    {
        SaveData data = SaveSystem.Load();

        if (data.discoveredZones != null) discoveredZones = new HashSet<string>(data.discoveredZones);

        if(data.activeCheckpoints != null) activatedCheckpointIDs = new List<string>(data.activeCheckpoints);

        OnMapDataChanged?.Invoke();
    }

    public void DiscoverZone(string zoneID)
    {
        if (string.IsNullOrEmpty(zoneID)) return;
        if (discoveredZones.Contains(zoneID)) return;

        discoveredZones.Add(zoneID);
        OnZoneDiscovered?.Invoke(zoneID);
        OnMapDataChanged?.Invoke();

        SaveDiscoveredZones();
    }

    private void SaveDiscoveredZones()
    {
        SaveData data = SaveSystem.Load();
        data.discoveredZones = new List<string>(discoveredZones);
        SaveSystem.Save(data);
    }

    public bool IsZoneDiscovered(string zoneID) => discoveredZones.Contains(zoneID);

    public IEnumerable<MapZoneSO> GetDiscoveredZones() => allZones.Where(z => discoveredZones.Contains(z.zoneID));

    public IEnumerable<Checkpoint> GetActivatedCheckpoints() => allCheckpoints.Where(check => check != null && activatedCheckpointIDs.Contains(check.CheckpointID));

    public void SetCurrentCheckpoint(string checkpointID) => CurrentCheckpointID = checkpointID;

    public IEnumerable<string> GetActivatedCheckpointsIDs()
    {
        if (activatedCheckpointIDs == null) return Enumerable.Empty<string>();
        return activatedCheckpointIDs;
    }

    public void TeleportToCheckpoint(string checkpointID)
    {
        Checkpoint target = allCheckpoints.FirstOrDefault(c => c.CheckpointID == checkpointID);
        if (target == null) return;
        if (player == null) return;

        player.position = target.TeleportPoint.position;
        SetCurrentCheckpoint(checkpointID);

        Debug.Log($"Успешная телепортация на {checkpointID}");
    }

    public void RegisterActivatedCheckpoint(string checkpointID)
    {
        if (string.IsNullOrEmpty(checkpointID)) return;
        if (activatedCheckpointIDs.Contains(checkpointID)) return;
        activatedCheckpointIDs.Add(checkpointID);
    }

    // save
    public void LoadFromSaveData(SaveData data)
    {
        if (data == null) return;

        if (data.discoveredZones != null)
        {
            discoveredZones = new HashSet<string>(data.discoveredZones);
        }

        if (data.activeCheckpoints != null)
        {
            activatedCheckpointIDs = new List<string>(data.activeCheckpoints);
        }

        OnMapDataChanged?.Invoke();
    }

    public void SaveToSaveData(SaveData data)
    {
        if (data == null) return;

        data.discoveredZones = new List<string>(discoveredZones);
        data.activeCheckpoints = new List<string>(activatedCheckpointIDs);
    }

    // ONLY FOR DEBUG!
    [ContextMenu("Удалить сохранение")]
    public void DeleteSave() => SaveSystem.Delete();
}
