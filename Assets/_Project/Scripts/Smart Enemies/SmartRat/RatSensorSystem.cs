using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RatSensorSystem : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;

    private Transform playerTransform;
    private Light2D playerLight;

    private RatAI _ratAI;

    public bool playerDetected { get; private set;  }
    public bool playerInAttackRange {  get; private set; }
    public bool lightHitRat {  get; private set; }

    public Vector2 directionToPlayer {  get; private set; }
    public float distanceToPlayer { get; private set; }


    private void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        _ratAI = GetComponent<RatAI>();

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerLight = playerObj.GetComponent<Light2D>();
        }
    }

    public void Tick()
    {
        UpdatePlayerSensor();
        UpdateLightSensor();
    }

    private void UpdatePlayerSensor()
    {
        if(playerTransform == null)
        {
            playerDetected = false;
            playerInAttackRange = false;
            return;
        }

        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;

        distanceToPlayer = toPlayer.magnitude;

        directionToPlayer = toPlayer.normalized;

        playerDetected = distanceToPlayer <= detectionRadius;
        playerInAttackRange = distanceToPlayer <= _ratAI?.attackRaduis;
    }

    private void UpdateLightSensor() => lightHitRat = LightSourceRegistry.IsPositionLit(transform.position);

    public Transform GetPlayerTransform() => playerTransform;

}