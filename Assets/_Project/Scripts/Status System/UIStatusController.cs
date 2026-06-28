using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIStatusController : MonoBehaviour
{
    [SerializeField] private Image coughOverlay;
    [SerializeField] private float coughmaxAlpha = 0.45f;

    private StatusDataSystem _statusSystem;
    private Coroutine _coughtCoroutine;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        _statusSystem = player.GetComponent<StatusDataSystem>();
        if (_statusSystem == null) return;

        _statusSystem.onStatusAdded.AddListener(OnStatusAdded);
        _statusSystem.onStatusRemoved.AddListener(OnStatusRemoved);
    }

    private void OnDestroy()
    {
        if (_statusSystem == null) return;

        _statusSystem.onStatusAdded.RemoveListener(OnStatusAdded);
        _statusSystem.onStatusRemoved.RemoveListener(OnStatusRemoved);
    }

    private void OnStatusAdded(StatusType type, float duration)
    {
        switch (type)
        {
            case StatusType.Cough:
                StartCough(duration); break;
            //case StatusType.Burn:
            //    StartBurn(duration); break;
        }
    }

    private void OnStatusRemoved(StatusType type)
    {
        switch (type)
        {
            case StatusType.Cough:
                StopCough(); break;
        }
    }

    private void StartCough(float duration)
    {
        if (_coughtCoroutine != null) StopCoroutine( _coughtCoroutine);
        _coughtCoroutine = StartCoroutine(CoughOverlayCoroutine(duration));
    }

    private void StopCough()
    {
        if(_coughtCoroutine != null)
        {
            StopCoroutine(_coughtCoroutine);
            _coughtCoroutine = null;
        }
        SetOverlayToAlpha(coughOverlay, 0f);
    }

    private IEnumerator CoughOverlayCoroutine(float duration)
    {
        if(coughOverlay == null) yield break;

        float fadeInTime = 0.1f;
        float fadeOutTime = 0.3f;
        float holdTime = duration - fadeInTime - fadeOutTime;

        yield return FadeOverlay(coughOverlay, 0f, coughmaxAlpha, fadeInTime);

        if(holdTime > 0f) yield return new WaitForSeconds(holdTime);

        yield return FadeOverlay(coughOverlay, coughmaxAlpha, 0f, fadeOutTime);

        _coughtCoroutine = null;
    }

    private IEnumerator FadeOverlay(Image overlay, float from, float to, float duration)
    {
        float elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / duration);
            SetOverlayToAlpha(overlay, Mathf.Lerp(from, to, time));
            yield return null;
        }
        SetOverlayToAlpha(overlay, to);
    }

    private void SetOverlayToAlpha(Image overlay, float alpha)
    {
        if (overlay == null) return;
        Color color = overlay.color;
        color.a = alpha;
        overlay.color = color;
    }
}
