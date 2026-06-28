using UnityEngine;

public class LightSource : MonoBehaviour
{
    [SerializeField] public float radius = 4f;
    [SerializeField] public LightSourceType srcType = LightSourceType.Torch;

    public Vector2 position => transform.position;
    public bool IsActive => gameObject.activeInHierarchy && enabled;

    private void OnEnable() => LightSourceRegistry.Register(this);
    private void OnDisable() => LightSourceRegistry.Unregister(this);

}

public enum LightSourceType
{
    Torch, 
    Crystal,
    Bonfire,
    PlayerTorch
}
