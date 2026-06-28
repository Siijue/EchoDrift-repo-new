using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]

public class ShadowOrb : MonoBehaviour
{
    private Vector2 _direction;
    private float _speed;
    private float _damage;
    private float _homingRadius;
    private Transform _player;
    private Rigidbody2D _rb;

    private bool _destroyed;

    public void Init(Vector2 direction, float speed, float damage, float homingRadius, Transform player)
    {
        _direction = direction;
        _speed = speed;
        _damage = damage;
        _homingRadius = homingRadius;
        _player = player;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        GetComponent<CircleCollider2D>().isTrigger = true;
        Destroy(gameObject, 8f);
    }

    private void FixedUpdate()
    {
        if(_destroyed ) return;
        if(_player == null)
        {
            _rb.linearVelocity = _direction * _speed;
            return;
        }
        float dist = Vector2.Distance(transform.position, _player.position);

        if(dist <= _homingRadius)
        {
            _direction = Vector2.Lerp(_direction, ((Vector2)_player.position).normalized, 3f * Time.deltaTime);
            _rb.linearVelocity = _direction * _speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_destroyed) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(_damage);
            DestroyOrb();
            return;
        }

        if (LightSourceRegistry.IsPositionLit(transform.position)) DestroyOrb();
    }

    public void ExtinguishByLight()
    {
        if(_destroyed) DestroyOrb();
    }

    private void DestroyOrb()
    {
        _destroyed = true;
        // добавить vfx
        Destroy(gameObject, 0.05f);
    }
}
