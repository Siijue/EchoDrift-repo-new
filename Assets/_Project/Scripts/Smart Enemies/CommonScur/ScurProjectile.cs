using UnityEngine;

public class ScurProjectile : MonoBehaviour
{
    private float _danage;
    private float _knockbackForce;
    private float _speed;
    private Vector2 _direction;

    private const float lifetime = 3f;

    private Rigidbody2D _rb;


    public void Init(Vector2 direction, float speed, float knockbackForce, float damage)
    {
        _direction = direction;
        _speed = speed;
        _knockbackForce = knockbackForce;
        _danage = damage;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.freezeRotation = true;
        _rb.gravityScale = 0.3f;
    }

    private void Start()
    {
        Debug.Log("СНАРЯД");
        _rb.linearVelocity = _direction * _speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            health?.TakeDamage(_danage);

            PlayerController controller = other.GetComponent<PlayerController>();
            if(controller != null) controller.ApplyKnockback(_direction, _knockbackForce);

            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger) Destroy(gameObject);
    }
}
