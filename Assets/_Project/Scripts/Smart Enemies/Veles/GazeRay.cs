using UnityEngine;
using System.Collections;

public class GazeRay : MonoBehaviour
{
    private Vector2 _direction;
    private float _duration;
    private float _speed = 3f;
    private float _damage = 0.5f;

    public void Init(Vector2 direction, float duration)
    {
        _direction = direction;
        _duration = duration;
    }

    private void Start()
    {
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        StartCoroutine(MoveAndDie());
    }

    private IEnumerator MoveAndDie()
    {
        float elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            transform.position += (Vector3)(_direction * _speed * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) other.GetComponent<PlayerHealth>()?.TakeDamage(_damage);
    }
}
