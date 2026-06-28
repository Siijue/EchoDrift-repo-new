using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ScurSensorSystem : MonoBehaviour
{
    [SerializeField] private float activationRadius = 12f;

    public bool playerInrange { get; private set; }
    public bool lightHitsScur { get; private set; }

    private Transform _playerTransform;
    private Light2D _playerLight;


    private void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _playerLight = playerObj.GetComponent<Light2D>();
        }
    }

    private void FixedUpdate()
    {
        UpdatePlayerSensor();
        UpdateLightSensor();
    }

    private void UpdatePlayerSensor()
    {
        if (_playerTransform == null)
        {
            playerInrange = false;
            return;
        }

        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        playerInrange = dist <= activationRadius;

    }

    private void UpdateLightSensor() => lightHitsScur = LightSourceRegistry.IsPositionLit(transform.position);
    //{
    //    if(_playerLight == null || !_playerLight.enabled)
    //    {
    //        lightHitsScur = false;
    //        return;
    //    }

    //    if(_playerTransform == null)
    //    {
    //        lightHitsScur = false;
    //        return;
    //    }

    //    float distToLight = Vector2.Distance(transform.position, _playerTransform.position);
    //    lightHitsScur = distToLight <= _playerLight.pointLightOuterRadius;
    //}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }

    public Transform GetPlayerTransform() => _playerTransform;
}