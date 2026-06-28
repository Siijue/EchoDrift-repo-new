//using UnityEngine;
//using System.Collections;
//using UnityEngine.Rendering.Universal;

//public class ArenaCrystal : MonoBehaviour, IInteractable
//{
//    [SerializeField] private IstAI boss;
//    [SerializeField] private Transform arenaCenter;
//    [SerializeField] private SpriteRenderer crystalSprRend;
//    [SerializeField] private Collider2D rayCollider;
//    [SerializeField] private float rayDuration = 3f;
//    [SerializeField] private int crystalDamage = 2;
//    [SerializeField] public Light2D crystalLight;

//    [SerializeField] private string hintActive = "[E] Активировать кристалл";
//    [SerializeField] private string hintNoTorch = "Зажгите лучину";
//    [SerializeField] private string hintInactive = "Кристалл восстанавливается...";

//    [SerializeField] private Color colorActive = new Color(0.3f, 0.8f, 1f);
//    [SerializeField] private Color colorFiring = Color.white;
//    [SerializeField] private Color colorInactive = new Color(0.3f, 0.2f, 0.3f);

//    [Header("Настройки света")]
//    [SerializeField] private float lightIntensity = 5f;
//    [SerializeField] private float lightRange = 15f;

//    public bool IsActive { get; private set; } = true;
//    public bool IsRayActive { get; private set; }

//    private bool _hasDealtDamage;
//    private Coroutine _rayCoroutine;

//    private void Start()
//    {
//        if (crystalSprRend == null) crystalSprRend = GetComponent<SpriteRenderer>();
//        if (rayCollider != null) rayCollider.enabled = false;

//        if (crystalLight != null)
//        {
//            crystalLight.enabled = false;
//            crystalLight.intensity = 0f;
//        }

//        UpdateVisual();
//    }

//    private IstAI Boss => IstAI.Instance ?? FindAnyObjectByType<IstAI>();

//    public void Interact(PlayerController player)
//    {
//        if (!IsActive)
//        {
//            UIManager.Instance?.ShowHint(hintInactive, 1.5f);
//            return;
//        }

//        if (!player.IsTorchLit())
//        {
//            UIManager.Instance.ShowHint(hintNoTorch, 1.5f);
//            return;
//        }

//        if (IsRayActive) return;

//        StartRay();
//    }

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (!other.CompareTag("Player") || !IsActive) return;
//        PlayerController player = other.GetComponent<PlayerController>();
//        bool lit = player != null && player.IsTorchLit();
//        string hint = !IsActive ? hintInactive : lit ? hintActive : hintNoTorch;
//        HintManager.Instance?.RegisterHint(this, hint, priotiry: 5f, duration: 0);
//    }

//    private void OnTriggerExit2D(Collider2D other)
//    {
//        if (other.CompareTag("Player")) HintManager.Instance?.RemoveHintsFromSource(this);
//    }

//    private void StartRay()
//    {
//        if (_rayCoroutine != null) StopCoroutine(_rayCoroutine);
//        _rayCoroutine = StartCoroutine(RaySequence());
//    }

//    private IEnumerator RaySequence()
//    {
//        IsRayActive = true;
//        _hasDealtDamage = false;

//        if (rayCollider != null) rayCollider.enabled = true;

//        StartCoroutine(ActivateLight());

//        crystalSprRend.color = colorFiring;
//        Debug.Log("Луч активирован");

//        float elapsed = 0f;
//        while (elapsed < rayDuration)
//        {
//            elapsed += Time.deltaTime;

//            if (CheckBossOnRay() && !_hasDealtDamage)
//            {
//                Debug.Log($"Урон от кристалла: {crystalDamage}");
//                Boss?.OnCrystalHit(crystalDamage);
//                _hasDealtDamage = true;
//                yield return new WaitForSeconds(0.5f);
//                elapsed += 0.5f;
//            }
//            yield return null;
//        }

//        IsRayActive = false;
//        if (rayCollider != null) rayCollider.enabled = false;

//        StartCoroutine(DeactivateLight());

//        IsActive = false;
//        UpdateVisual();

//        yield return new WaitForSeconds(5f);
//        IsActive = true;
//        UpdateVisual();
//    }
//    private IEnumerator ActivateLight()
//    {
//        if (crystalLight == null) yield break;

//        crystalLight.enabled = true;
//        crystalLight.intensity = 0f;
//        crystalLight.pointLightOuterRadius = 0f;

//        float duration = 0.3f;
//        float elapsed = 0f;

//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / duration;

//            crystalLight.intensity = Mathf.Lerp(0f, lightIntensity, t);
//            crystalLight.pointLightOuterRadius = Mathf.Lerp(0f, lightRange, t);

//            yield return null;
//        }

//        crystalLight.intensity = lightIntensity;
//        crystalLight.pointLightOuterRadius = lightRange;
//    }
//    private IEnumerator DeactivateLight()
//    {
//        if (crystalLight == null) yield break;

//        float duration = 0.5f;
//        float elapsed = 0f;
//        float startIntensity = crystalLight.intensity;
//        float startRadius = crystalLight.pointLightOuterRadius;

//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / duration;

//            crystalLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
//            crystalLight.pointLightOuterRadius = Mathf.Lerp(startRadius, 0f, t);

//            yield return null;
//        }

//        crystalLight.enabled = false;
//        crystalLight.intensity = 0f;
//        crystalLight.pointLightOuterRadius = 0f;
//    }

//    private bool CheckBossOnRay()
//    {
//        if (Boss == null || rayCollider == null) return false;

//        Collider2D[] results = new Collider2D[8];
//        int count = rayCollider.Overlap(ContactFilter2D.noFilter, results);

//        for (int i = 0; i < count; i++)
//        {
//            if (results[i] == null) continue;
//            if (results[i].CompareTag("FinalBoss")) return true;
//        }
//        return false;
//    }

//    public Vector2 GetRayDirection()
//    {
//        if (arenaCenter == null) return Vector2.right;
//        return ((Vector2)arenaCenter.position - (Vector2)transform.position).normalized;
//    }

//    public bool IsPositionOnRay(Vector2 position)
//    {
//        if (!IsRayActive || rayCollider == null) return false;
//        return rayCollider.bounds.Contains(new Vector3(position.x, position.y, 0f));
//    }
//    public void FireAtPlayer()
//    {
//        if (Boss == null || Boss.playerTransform == null) return;
//        if (!IsActive || IsRayActive) return;

//        Debug.Log($"[ArenaCrystal] Выстрел в игрока от {name}");
//        StartRay();
//    }

//    private void UpdateVisual()
//    {
//        if (crystalSprRend == null) return;
//        crystalSprRend.color = IsActive ? colorActive : colorInactive;
//    }
//}


using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class ArenaCrystal : MonoBehaviour, IInteractable
{
    [SerializeField] private IstAI boss;
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private SpriteRenderer crystalSprRend;
    [SerializeField] private Collider2D rayCollider;
    [SerializeField] private float rayDuration = 3f;
    [SerializeField] private int crystalDamage = 2;
    [SerializeField] public Light2D crystalLight;

    [SerializeField] private string hintActive = "[E] Активировать кристалл";
    [SerializeField] private string hintNoTorch = "Зажгите лучину";
    [SerializeField] private string hintInactive = "Кристалл восстанавливается...";

    [SerializeField] private Color colorActive = new Color(0.3f, 0.8f, 1f);
    [SerializeField] private Color colorFiring = Color.white;
    [SerializeField] private Color colorInactive = new Color(0.3f, 0.2f, 0.3f);

    [Header("Настройки света")]
    [SerializeField] private float lightIntensity = 5f;
    [SerializeField] private float lightRange = 15f;

    public bool IsActive { get; private set; } = true;
    public bool IsRayActive { get; private set; }

    private bool _hasDealtDamage;
    private bool _isBossFired;
    private Coroutine _rayCoroutine;

    private void Start()
    {
        if (crystalSprRend == null) crystalSprRend = GetComponent<SpriteRenderer>();
        if (rayCollider != null) rayCollider.enabled = false;

        if (crystalLight != null)
        {
            crystalLight.enabled = false;
            crystalLight.intensity = 0f;
        }

        UpdateVisual();
    }

    private IstAI Boss => IstAI.Instance ?? FindAnyObjectByType<IstAI>();

    public void Interact(PlayerController player)
    {
        if (!IsActive)
        {
            UIManager.Instance?.ShowHint(hintInactive, 1.5f);
            return;
        }

        if (!player.IsTorchLit())
        {
            UIManager.Instance.ShowHint(hintNoTorch, 1.5f);
            return;
        }

        if (IsRayActive) return;

        _isBossFired = false;
        StartRay();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !IsActive) return;
        PlayerController player = other.GetComponent<PlayerController>();
        bool lit = player != null && player.IsTorchLit();
        string hint = !IsActive ? hintInactive : lit ? hintActive : hintNoTorch;
        HintManager.Instance?.RegisterHint(this, hint, priotiry: 5f, duration: 0);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) HintManager.Instance?.RemoveHintsFromSource(this);
    }

    private void StartRay()
    {
        if (_rayCoroutine != null) StopCoroutine(_rayCoroutine);
        _rayCoroutine = StartCoroutine(RaySequence());
    }

    private IEnumerator RaySequence()
    {
        IsRayActive = true;
        _hasDealtDamage = false;

        if (rayCollider != null) rayCollider.enabled = true;
        StartCoroutine(ActivateLight());

        crystalSprRend.color = colorFiring;
        Debug.Log($"Луч активирован (bossFired: {_isBossFired})");

        float elapsed = 0f;
        while (elapsed < rayDuration)
        {
            elapsed += Time.deltaTime;

            if (!_isBossFired && CheckBossOnRay() && !_hasDealtDamage)
            {
                Debug.Log($"Урон от кристалла: {crystalDamage}");
                Boss?.OnCrystalHit(crystalDamage);
                _hasDealtDamage = true;
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;
            }
            yield return null;
        }

        IsRayActive = false;
        if (rayCollider != null) rayCollider.enabled = false;
        StartCoroutine(DeactivateLight());

        IsActive = false;
        UpdateVisual();

        yield return new WaitForSeconds(5f);
        IsActive = true;
        UpdateVisual();
    }

    private IEnumerator ActivateLight()
    {
        if (crystalLight == null) yield break;

        crystalLight.enabled = true;
        crystalLight.intensity = 0f;
        crystalLight.pointLightOuterRadius = 0f;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            crystalLight.intensity = Mathf.Lerp(0f, lightIntensity, t);
            crystalLight.pointLightOuterRadius = Mathf.Lerp(0f, lightRange, t);

            yield return null;
        }

        crystalLight.intensity = lightIntensity;
        crystalLight.pointLightOuterRadius = lightRange;
    }

    private IEnumerator DeactivateLight()
    {
        if (crystalLight == null) yield break;

        float duration = 0.5f;
        float elapsed = 0f;
        float startIntensity = crystalLight.intensity;
        float startRadius = crystalLight.pointLightOuterRadius;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            crystalLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
            crystalLight.pointLightOuterRadius = Mathf.Lerp(startRadius, 0f, t);

            yield return null;
        }

        crystalLight.enabled = false;
        crystalLight.intensity = 0f;
        crystalLight.pointLightOuterRadius = 0f;
    }

    private bool CheckBossOnRay()
    {
        if (Boss == null || rayCollider == null) return false;

        Collider2D[] results = new Collider2D[8];
        int count = rayCollider.Overlap(ContactFilter2D.noFilter, results);

        for (int i = 0; i < count; i++)
        {
            if (results[i] == null) continue;
            if (results[i].CompareTag("FinalBoss")) return true;
        }
        return false;
    }

    public Vector2 GetRayDirection()
    {
        if (arenaCenter == null) return Vector2.right;
        return ((Vector2)arenaCenter.position - (Vector2)transform.position).normalized;
    }

    public bool IsPositionOnRay(Vector2 position)
    {
        if (!IsRayActive || rayCollider == null) return false;
        return rayCollider.bounds.Contains(new Vector3(position.x, position.y, 0f));
    }
    public void FireAtPlayer()
    {
        if (Boss == null || Boss.playerTransform == null) return;
        if (!IsActive || IsRayActive) return;

        _isBossFired = true;
        StartRay();
    }

    private void UpdateVisual()
    {
        if (crystalSprRend == null) return;
        crystalSprRend.color = IsActive ? colorActive : colorInactive;
    }
}