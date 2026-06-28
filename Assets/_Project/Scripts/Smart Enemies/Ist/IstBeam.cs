using UnityEngine;
using System.Collections;

public class IstBeam : MonoBehaviour
{
    private Vector2 _direction;
    private float _duration;
    private float _torchDrain;

    public void Init(Vector2 direction, float duration, float torchDrain)
    {
        _direction = direction;
        _duration = duration;
        _torchDrain = torchDrain;

        StartCoroutine(BeamLifetime());
    }

    private IEnumerator BeamLifetime()
    {
        yield return new WaitForSeconds(_duration);
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        player?.DrainTorch(_torchDrain * Time.deltaTime);
    }
}