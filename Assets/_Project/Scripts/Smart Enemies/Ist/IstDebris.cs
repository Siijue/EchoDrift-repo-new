//using UnityEngine;
//using System.Collections;

//public class IstDebris : MonoBehaviour
//{
//    private float _damage;
//    private Vector3 _targetPos;
//    private float _speed = 8f;

//    public void Init(float damage, Vector3 targetPos)
//    {
//        _damage = damage;
//        _targetPos = targetPos;
//        StartCoroutine(FallSequence());
//    }

//    private IEnumerator FallSequence()
//    {
//        while (Vector3.Distance(transform.position, _targetPos) > 0.1f)
//        {
//            transform.position = Vector3.MoveTowards(transform.position, _targetPos, _speed * Time.deltaTime);
//            yield return null;
//        }

//        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.8f);
//        foreach (var hit in hits)
//        {
//            if (!hit.CompareTag("Player")) continue;
//            Debug.Log($"УРОН ИГРОКУ ОТ ОСКОЛКА: {_damage}");
//            hit.GetComponent<PlayerHealth>()?.TakeDamage(_damage);
//        }

//        Destroy(gameObject, 1f);
//    }
//}

using UnityEngine;
using System.Collections;

public class IstDebris : MonoBehaviour
{
    private float _damage;
    private Vector3 _targetPos;
    private float _speed = 8f;

    private bool _hasDealtDamage = false;

    public void Init(float damage, Vector3 targetPos)
    {
        _damage = damage;
        _targetPos = targetPos;
        StartCoroutine(FallSequence());
    }

    private IEnumerator FallSequence()
    {
        while (Vector3.Distance(transform.position, _targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, _speed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasDealtDamage || !other.CompareTag("Player")) return;

        _hasDealtDamage = true; 

        Debug.Log($"УРОН ИГРОКУ ОТ ОСКОЛКА (ТРИГГЕР): {_damage}");

        other.GetComponent<PlayerHealth>()?.TakeDamage(_damage);
    }
}