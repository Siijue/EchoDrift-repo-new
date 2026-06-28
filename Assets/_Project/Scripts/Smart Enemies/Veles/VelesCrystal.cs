using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class VelesCrystal : MonoBehaviour, IInteractable
{
    [SerializeField] private VelesAI boss;
    [SerializeField] private SpriteRenderer crystalSpr;
    [SerializeField] private Collider2D crystalCollider;
    [SerializeField] private Color activeColor = new Color(0.3f, 0.8f, 1f);
    [SerializeField] private Color inactiveColor = new Color(0.1f, 0.2f, 0.3f);
    [SerializeField] private Color extinguishColor = new Color(0.05f, 0.05f, 0.1f);
    [SerializeField] private string hintText = "[E] Направить свет кристалла";
    [SerializeField] private string noTorchHint = "Зажгите лучину";
    [SerializeField] private Light2D beamLight;
    [SerializeField] private float beamDuration = 2f;

    public bool IsActive => _isActive;

    private bool _isActive = true;
    private bool _extinguished;

    public void Interact(PlayerController player)
    {
        if (!_isActive) return;

        if (!player.IsTorchLit())
        {
            UIManager.Instance?.ShowHint(noTorchHint, 1.5f);
            return;
        }

        FireAtBoss();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !_isActive) return;

        PlayerController player = other.GetComponent<PlayerController>();
        bool torchLit = player != null && player.IsTorchLit();

        string hint = torchLit ? hintText : noTorchHint;
        HintManager.Instance?.RegisterHint(this, hint, priotiry: 5f, duration: 3);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player")) HintManager.Instance?.RemoveHintsFromSource(this);
    }

    private void Start()
    {
        if (crystalSpr == null) crystalSpr = GetComponent<SpriteRenderer>();
        if(crystalCollider == null) crystalCollider = GetComponent<Collider2D>();
        if (beamLight != null) beamLight.enabled = false;
        UpdateVisual();
    }

    private void FireAtBoss()
    {
        if (boss == null || boss.IsDead) return;

        Collider2D[] results = new Collider2D[8];
        int count = crystalCollider.Overlap(ContactFilter2D.noFilter, results);

        bool bossInRay = false;

        for(int i = 0; i < count; i++)
        {
            if(results[i] == null) continue;
            if (results[i].CompareTag("Veles"))
            {
                bossInRay = true;
                break;
            }
        }

        if (!bossInRay) return;

        boss.OnCrystalHit();

        UIManager.Instance?.ShowHint("Успешное попадание!", 1f);
        StartCoroutine(ActivateLightBeam());
        StartCoroutine(FlashActive());
    }

    public void Extinguish(float duration)
    {
        if (!_isActive) return;
        StartCoroutine(ExtinguishCoroutine(duration));
    }

    private IEnumerator ExtinguishCoroutine(float duration)
    {
        _isActive = false;
        _extinguished = true;
        UpdateVisual();

        yield return new WaitForSeconds(duration);

        _isActive = true;
        _extinguished = false;
        UpdateVisual();
    }

    private IEnumerator FlashActive()
    {
        if (crystalSpr == null) yield break;
        crystalSpr.color = Color.white;
        yield return new WaitForSeconds(1f);
        UpdateVisual();
    }

    private IEnumerator ActivateLightBeam()
    {
        if(beamLight != null)
        {
            beamLight.enabled = true;
            yield return new WaitForSeconds(beamDuration);
            beamLight.enabled = false;
        }
    }

    private void UpdateVisual()
    {
        if (crystalSpr == null) return;
        crystalSpr.color = _extinguished ? extinguishColor : _isActive ? activeColor : inactiveColor;
    }
}
