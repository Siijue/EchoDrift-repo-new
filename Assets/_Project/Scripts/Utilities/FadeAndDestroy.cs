using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class FadeAndDestroy
{
    public enum FadeType
    {
        Alpha,
        Scale,
        Both    
    }
    public enum EaseType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    public static void Destroy(GameObject obj, float duration = 0.5f)
    {
        FadeAndDestroyObject(obj, duration, FadeType.Both, EaseType.EaseOut);
    }

    public static void Destroy(GameObject obj, float duration, FadeType fadeType)
    {
        FadeAndDestroyObject(obj, duration, fadeType, EaseType.EaseOut);
    }

    public static void FadeAndDestroyObject(
        GameObject obj,
        float duration = 0.5f,
        FadeType fadeType = FadeType.Both,
        EaseType easeType = EaseType.EaseOut,
        System.Action onComplete = null)
    {
        if (obj == null) return;
        if (duration <= 0f)
        {
            Object.Destroy(obj);
            onComplete?.Invoke();
            return;
        }
        CoroutineRunner runner = obj.GetComponent<CoroutineRunner>();
        if (runner == null) runner = obj.AddComponent<CoroutineRunner>();

        runner.StartCoroutine(
            FadeCoroutine(obj, duration, fadeType, easeType, onComplete)
        );
    }

    public static void FadeOut(
    GameObject obj,
    float duration = 0.5f,
    FadeType fadeType = FadeType.Both,
    EaseType easeType = EaseType.EaseOut,
    System.Action onComplete = null)
    {
        if (obj == null) return;
        if (duration <= 0f)
        {
            onComplete?.Invoke();
            return;
        }

        if (CoroutineRunner.Instance == null)
        {
            Debug.LogError("[FadeAndDestroy] CoroutineRunner не найден!");
            onComplete?.Invoke();
            return;
        }

        CoroutineRunner.Instance.StartCoroutine(
            FadeOutCoroutine(obj, duration, fadeType, easeType, onComplete)
        );
    }

    private static IEnumerator FadeCoroutine(GameObject obj, float duration, FadeType fadeType, EaseType easeType, System.Action onComplete)
    {
        float elapsed = 0f;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Image uiImage = obj.GetComponent<Image>();
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();

        Color startColor = Color.white;
        Vector3 startScale = obj.transform.localScale;

        if (sr != null) startColor = sr.color;
        else if (uiImage != null) startColor = uiImage.color;
        else if (tmp != null) startColor = tmp.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = ApplyEasing(t, easeType);

            if (fadeType == FadeType.Alpha || fadeType == FadeType.Both)
            {
                Color fadedColor = startColor;
                fadedColor.a = Mathf.Lerp(1f, 0f, easedT);

                if (sr != null) sr.color = fadedColor;
                else if (uiImage != null) uiImage.color = fadedColor;
                else if (tmp != null) tmp.color = fadedColor;
                else if (cg != null) cg.alpha = Mathf.Lerp(1f, 0f, easedT);
            }

            if (fadeType == FadeType.Scale || fadeType == FadeType.Both)
            {
                float scale = Mathf.Lerp(1f, 0f, easedT);
                obj.transform.localScale = startScale * scale;
            }

            yield return null;
        }

        if (obj != null) Object.Destroy(obj);
        onComplete?.Invoke();
    }


    private static IEnumerator FadeOutCoroutine(GameObject obj, float duration, FadeType fadeType, EaseType easeType, System.Action onComplete)
    {
        float elapsed = 0f;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Image uiImage = obj.GetComponent<Image>();
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();

        float startAlpha = 1f;
        if (sr != null) startAlpha = sr.color.a;
        else if (uiImage != null) startAlpha = uiImage.color.a;
        else if (tmp != null) startAlpha = tmp.color.a;
        else if (cg != null) startAlpha = cg.alpha;

        Color startColor = Color.white;
        if (sr != null) startColor = sr.color;
        else if (uiImage != null) startColor = uiImage.color;
        else if (tmp != null) startColor = tmp.color;

        Vector3 startScale = obj.transform.localScale;

        while (elapsed < duration)
        {
            if (obj == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = ApplyEasing(t, easeType);

            if (fadeType == FadeType.Alpha || fadeType == FadeType.Both)
            {
                float currentAlpha = Mathf.Lerp(startAlpha, 0f, easedT);

                if (sr != null)
                {
                    Color c = startColor;
                    c.a = currentAlpha;
                    sr.color = c;
                }
                if (uiImage != null)
                {
                    Color c = startColor;
                    c.a = currentAlpha;
                    uiImage.color = c;
                }
                if (tmp != null)
                {
                    Color c = startColor;
                    c.a = currentAlpha;
                    tmp.color = c;
                }
                if (cg != null)
                    cg.alpha = currentAlpha;
            }

            if (fadeType == FadeType.Scale || fadeType == FadeType.Both)
            {
                float scale = Mathf.Lerp(1f, 0f, easedT);
                obj.transform.localScale = startScale * scale;
            }

            yield return null;
        }
        onComplete?.Invoke();
    }

    private static float ApplyEasing(float t, EaseType easeType)
    {
        switch (easeType)
        {
            case EaseType.Linear:
                return t;

            case EaseType.EaseIn:
                return t * t;

            case EaseType.EaseOut:
                return 1f - (1f - t) * (1f - t);

            case EaseType.EaseInOut:
                if (t < 0.5f)
                    return 2f * t * t;
                else
                    return 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

            default:
                return t;
        }
    }
}