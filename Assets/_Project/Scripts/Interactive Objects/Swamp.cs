using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Swamp : MonoBehaviour
{
    [SerializeField] private float speedMult = 0.4f;
    [SerializeField] private float jumpMult = 0.6f;
    [SerializeField] private Color swampPlayerColor = Color.darkGreen;

    private void Awake()
    {
        Collider2D coll = GetComponent<Collider2D>();
        if (!coll.isTrigger) coll.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.SetEnvironmentSpeedMult(speedMult);
        player.SetEnvironmentJumpMult(jumpMult);
        ApplyTint(player, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.ResetEnvironmentEffects();
        ApplyTint(player, false);
    }

    private void ApplyTint(PlayerController player, bool apply)
    {
        SpriteRenderer spr = player.GetComponent<SpriteRenderer>();
        if (spr == null) return;

        if (apply)
        {
            if (!_origColorSaved)
            {
                _origColor = spr.color;
                _origColorSaved = true;
            }
            spr.color = swampPlayerColor;
        }
        else
        {
            if (_origColorSaved)
            {
                spr.color = _origColor;
                _origColorSaved = false;
            }
        }
    }

    private Color _origColor = Color.white;
    private bool _origColorSaved = false;
}