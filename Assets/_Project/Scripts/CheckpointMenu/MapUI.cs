using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class MapUI : MonoBehaviour, IDragHandler, IScrollHandler
{
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private Transform zonesContainer;
    [SerializeField] private Transform iconsContainer;

    [SerializeField] private GameObject zoneBackgroundPrefab;
    [SerializeField] private GameObject checkpointIconPrefab;

    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2f;
    [SerializeField] private float zoomSpeed = 0.1f;

    [SerializeField] private GameObject teleportConfirmPanel;
    [SerializeField] Button teleportButton;

    private string selectedCheckpointID;
    private Dictionary<string, GameObject> spawnedZoneObjects = new Dictionary<string, GameObject>();
    private List<GameObject> spawnedIcons = new List<GameObject>();

    public UnityEvent OnTeleportRequested = new UnityEvent();

    public bool IsMapOpen => mapPanel.activeSelf;

    private void Awake()
    {
        if (mapPanel != null) mapPanel.SetActive(false);
        if(teleportConfirmPanel != null) teleportConfirmPanel.SetActive(false);

        teleportButton?.onClick.AddListener(OnTeleportConfirmed);
    }

    private void Start()
    {
        if (MapManager.Instance != null) MapManager.Instance.OnMapDataChanged.AddListener(OnMapDataChanged);
    }

    public void OpenMap()
    {
        mapPanel.SetActive(true);
        RefreshZones();
        RefreshCheckpointIcons();
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
        if (teleportConfirmPanel != null) teleportConfirmPanel.SetActive(false);
        selectedCheckpointID = null;
        if (PauseScreen.Instance != null) PauseScreen.Instance.IgnoreNextEscape();
    }

    private void OnMapDataChanged()
    {
        if (IsMapOpen)
        {
            RefreshCheckpointIcons();
            RefreshZones();
        }
    }

    private void Update()
    {
        if (!mapPanel.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Escape)) CloseMap();
    }


    private void RefreshZones()
    {
        if (MapManager.Instance == null) return;

        var discoveredZones = MapManager.Instance.GetDiscoveredZones();
        if (discoveredZones == null) return;

        foreach(var zone in MapManager.Instance?.GetDiscoveredZones())
        {
            if (spawnedZoneObjects.ContainsKey(zone.zoneID)) continue;

            GameObject obj = Instantiate(zoneBackgroundPrefab, zonesContainer);
            var img = obj.GetComponent<Image>();
            img.sprite = zone.mapBoundsSprite;

            RectTransform rectTr = obj.GetComponent<RectTransform>();
            rectTr.anchoredPosition = zone.mapRectOnCanvas.position;
            rectTr.sizeDelta = zone.mapRectOnCanvas.size;

            spawnedZoneObjects[zone.zoneID] = obj;
        }
    }

    private void RefreshCheckpointIcons()
    {
        foreach (var obj in spawnedIcons)
        {
            if(obj != null) Destroy(obj);
        }
        spawnedIcons.Clear();

        if (MapManager.Instance == null) return;
        var activatedCheckpoints = MapManager.Instance.GetActivatedCheckpoints();
        Debug.Log($"активных чекпоинов: {MapManager.Instance.GetActivatedCheckpointsIDs().Count()}");
        foreach (var checkpoint in activatedCheckpoints)
        {
            if (checkpoint != null)
            {
                Debug.Log($"Активный чекпоинт: {checkpoint.CheckpointID} - {checkpoint.gameObject.name}");
            }
        }
        if (activatedCheckpoints == null) return;

        foreach(var checkpoint in MapManager.Instance?.GetActivatedCheckpoints())
        {
            GameObject iconObj = Instantiate(checkpointIconPrefab, iconsContainer);
            RectTransform rectTr = iconObj.GetComponent<RectTransform>();
            rectTr.anchoredPosition = checkpoint.MapIconPosition;

            bool isCurrent = checkpoint.CheckpointID == MapManager.Instance?.CurrentCheckpointID;

            CheckpointIconUI iconUI = iconObj.GetComponent<CheckpointIconUI>();
            iconUI.Initialize(checkpoint.CheckpointID, isCurrent, this);

            spawnedIcons.Add(iconObj);
        }
    }

    public void OnCheckpointClicked(string checkpointID, RectTransform iconRect)
    {
        selectedCheckpointID = checkpointID;

        if (teleportConfirmPanel != null)
        {
            teleportConfirmPanel.SetActive(true);
            RectTransform panelRect = teleportConfirmPanel.GetComponent<RectTransform>();
            float panelHeight = panelRect.sizeDelta.y;
            panelRect.anchoredPosition = new Vector2(iconRect.anchoredPosition.x, iconRect.anchoredPosition.y - panelHeight);
        }
    }
    private void OnTeleportConfirmed()
    {
        if (string.IsNullOrEmpty(selectedCheckpointID)) return;
        MapManager.Instance?.TeleportToCheckpoint(selectedCheckpointID);
        CloseMap();
        OnTeleportRequested?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mapContent == null) return;
        mapContent.anchoredPosition += eventData.delta;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (mapContent == null) return;
        float scrollDelta = eventData.scrollDelta.y * zoomSpeed;
        Vector3 newScale = mapContent.localScale + Vector3.one * scrollDelta;
        newScale = ClampScale(newScale);

        mapContent.localScale = newScale;
    }

    private Vector3 ClampScale(Vector3 scale)
    {
        float clamped = Mathf.Clamp(scale.x, minZoom, maxZoom);
        return new Vector3(clamped, clamped, 1f);
    }


    private void OnDestroy()
    {
        if (MapManager.Instance != null) MapManager.Instance.OnMapDataChanged.RemoveListener(OnMapDataChanged);
        teleportButton?.onClick.RemoveListener(OnTeleportConfirmed);
    }
}
