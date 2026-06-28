using UnityEngine;

public class TestDamage : MonoBehaviour
{
    [SerializeField] PlayerHealth playerHealth;
    //[SerializeField] private float damageAmount = 0.5f;
    [SerializeField] private LeshyAI leshy;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if(leshy != null) leshy.TakeDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (playerHealth != null) playerHealth.Heal(0.5f);
        }
    }
}
