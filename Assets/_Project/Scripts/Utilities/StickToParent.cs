using UnityEngine;

public class StickToParent : MonoBehaviour
{
    private Transform parentTransform;

    private void Start() => parentTransform = transform.parent;

    private void LateUpdate()
    {
        if (parentTransform == null) return;
        transform.rotation = Quaternion.identity;
    }
}