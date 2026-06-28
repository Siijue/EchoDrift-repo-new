using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class GlitchEffectSystem : MonoBehaviour
{
    public static GlitchEffectSystem Instance { get; private set; }

    [SerializeField] private Camera gameCamera;
    [SerializeField] private Image screenOverlay;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private RectTransform[] screenFragments = new RectTransform[4];
    [SerializeField] private GameObject invertOverlay;
    [SerializeField] private Image invertImage;

    private Vector2[] _fragmentOrigPos;
    private Vector3 _cameraOrigPos;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (screenOverlay != null) SetOverlayAlpha(0f);

        if(screenFragments != null)
        {
            _fragmentOrigPos = new Vector2[screenFragments.Length];
            for(int i = 0; i < screenFragments.Length; i++)
            {
                if(screenFragments[i] != null) _fragmentOrigPos[i] = screenFragments[i].anchoredPosition;
            }
        }
    }


    // инвертация управления
    public void StartControlInvertion(float duration)
    {
        PlayerController player = GetPlayerController();
        if (player == null) return;
        StartCoroutine(ControlInversionCrtn(player, duration));
    }

    // фрагментация экрана
    public void StartScreenFragmentation(float duration)
    {
        if(screenFragments == null || screenFragments.Length == 0) return;
        RunGlitch(ScreenFragmentationCrtn(duration));
    }

    // копия игрока
    public void StartPlayerMirror(float duration) => RunGlitch(PlayerMirrorCrtn(duration));

    // смещение камеры
    public void StartCameraShift(float shiftTiles, float duration)
    {
        if (gameCamera == null) return;
        RunGlitch(CameraShiftCrtn(shiftTiles, duration));
    }

    // ложное угасание лучины
    public void StartTorchBlackout(float duration) => RunGlitch(TorchBlackoutCrtn(duration));

    // инверсия цветов
    public void StartColorInversion(float duration) => RunGlitch(ColorInversionCrtn(duration));


    public void PlayPhaseTransitionGlitch(System.Action onComplete = null) => StartCoroutine(PhaseTransitionGlitchCrtn(onComplete));


    // Корутины

    private IEnumerator ControlInversionCrtn(PlayerController player, float duration)
    {
        player.SetInputInverted(true);
        UIManager.Instance?.ShowHint("Истукан пытается подчинить вашу волю. Управление инвертировано", 0f);
        yield return new WaitForSeconds(duration);
        player.SetInputInverted(false);
        UIManager.Instance?.HideHint();
    }

    private IEnumerator ScreenFragmentationCrtn(float duration)
    {
        yield return FlashOverlay(Color.white, 0.2f);
        float[] offsets = { -30f, 25f, -20f, 35f };

        for(int i = 0; i < screenFragments.Length; i++)
        {
            if(screenFragments[i] == null) continue;
            float offset = i < offsets.Length ? offsets[i] : 0f;
            float vertOffset = (i % 2 == 0) ? -15f : 15f;
            screenFragments[i].anchoredPosition = _fragmentOrigPos[i] + new Vector2(offset, vertOffset);
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < screenFragments.Length; i++) if (screenFragments[i] != null) screenFragments[i].anchoredPosition = _fragmentOrigPos[i];
    }

    private IEnumerator PlayerMirrorCrtn(float duration)
    {
        GameObject player = playerTransform?.gameObject;
        if (player == null) yield break;

        SpriteRenderer playerSprRender = player.GetComponent<SpriteRenderer>();
        if(playerSprRender == null) yield break;

        GameObject ghost = new GameObject("PlayerGhost");
        SpriteRenderer ghostSprRender = ghost.AddComponent<SpriteRenderer>();
        ghostSprRender.sprite = playerSprRender.sprite;
        ghostSprRender.sortingOrder = playerSprRender.sortingOrder - 1;

        float elapsed = 0f;
        Vector3 ghostPos = player.transform.position;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ghostPos = Vector3.Lerp(ghostPos, player.transform.position, 3f * Time.deltaTime);
            ghost.transform.position = ghostPos + new Vector3(Random.Range(-0.2f, 0.2f), 0f, 0f);
            ghostSprRender.sprite = playerSprRender.sprite;
            ghostSprRender.flipX = playerSprRender.flipX;

            yield return null;
        }

        Destroy(ghost);
    }

    private IEnumerator CameraShiftCrtn(float shiftTiles, float duration)
    {
        if (gameCamera == null) yield break;
        _cameraOrigPos = gameCamera.transform.position;

        int cycles = 3;
        float cycleDuration = duration / cycles;

        for (int i = 0; i < cycles; i++)
        {
            float dirX = Random.value > 0.5f ? 1f : -1f;
            float dirY = Random.value > 0.5f ? 1f : -1f;
            Vector3 shiftPos = _cameraOrigPos + new Vector3(dirX * shiftTiles, dirY * shiftTiles, 0f);

            float elapsed = 0f;
            float halfDuration = cycleDuration * 0.5f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                gameCamera.transform.position = Vector3.Lerp(_cameraOrigPos, shiftPos, elapsed / halfDuration);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                gameCamera.transform.position = Vector3.Lerp(shiftPos, _cameraOrigPos, elapsed / halfDuration);
                yield return null;
            }
        }

        gameCamera.transform.position = _cameraOrigPos;
    }

    private IEnumerator TorchBlackoutCrtn(float duration)
    {
        PlayerController player = GetPlayerController();
        player?.SetTorchVisualOnly(false);

        yield return new WaitForSeconds(duration);

        player?.SetTorchVisualOnly(true);
    }

    private IEnumerator ColorInversionCrtn(float duration)
    {
        yield return FlashOverlay(Color.white, 0.15f);

        invertOverlay.SetActive(true);

        float fadeTime = 0.2f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            Color c = invertImage.color;
            c.a = t; 
            invertImage.color = c;
            yield return null;
        }

        float hold = Mathf.Max(0f, duration - fadeTime * 2f);
        yield return new WaitForSeconds(hold);

        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = 1f - elapsed / fadeTime;
            Color c = invertImage.color;
            c.a = t;
            invertImage.color = c;
            yield return null;
        }

        invertOverlay.SetActive(false);
    }


    private IEnumerator PhaseTransitionGlitchCrtn(System.Action onComplete)
    {
        yield return FlashOverlay(Color.white, 0.3f);

        for(int i = 0; i < 6; i++)
        {
            SetOverlayAlpha(0.6f);
            yield return new WaitForSeconds(0.05f);
            SetOverlayAlpha(0f);
            yield return new WaitForSeconds(0.03f);
        }

        onComplete?.Invoke();

        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(0.4f);
        Time.timeScale = 1f;
    }


    private void RunGlitch(IEnumerator coroutine) => StartCoroutine(coroutine);

    private IEnumerator FlashOverlay(Color color, float duration)
    {
        if(screenOverlay == null) yield break;
        screenOverlay.color = color;
        SetOverlayAlpha(0.8f);
        yield return new WaitForSeconds(duration);
        SetOverlayAlpha(0f);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (screenOverlay == null) return;
        Color color = screenOverlay.color;
        color.a = alpha;
        screenOverlay.color = color;
    }

    private PlayerController GetPlayerController()
    {
        if(playerTransform == null) return null;
        return playerTransform.GetComponent<PlayerController>();
    }
}
