using UnityEngine;

[CreateAssetMenu(fileName = "NewMapZone", menuName = "Map/MapZoneSO")]
public class MapZoneSO : ScriptableObject
{
    public string zoneID;
    public string zoneName;

    public Sprite mapBoundsSprite;
    public Rect mapRectOnCanvas;

    public Vector2 worldCenter;
    public Vector2 worldSize;
}
