using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 180f;

    private void Update() => transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
}
