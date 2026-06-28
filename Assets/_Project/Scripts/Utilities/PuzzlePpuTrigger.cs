////using UnityEngine;
////using UnityEngine.Rendering.Universal;

////public class PuzzlePpuTrigger : MonoBehaviour
////{
////    [Header("Камера")]
////    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;

////    [Header("Настройки PPU")]
////    [SerializeField] private int puzzlePPU = 16;

////    [Header("Настройки")]
////    [SerializeField] private bool restoreOnExit = true;

////    private int originalPPU;
////    private bool isApplied = false;

////    private void Awake()
////    {
////        if (pixelPerfectCamera == null) pixelPerfectCamera = Camera.main?.GetComponent<PixelPerfectCamera>();

////        if (pixelPerfectCamera != null) originalPPU = pixelPerfectCamera.assetsPPU;
////    }

////    private void OnTriggerEnter2D(Collider2D other)
////    {
////        if (!other.CompareTag("Player")) return;
////        if (isApplied) return;

////        ApplyPpu(puzzlePPU);
////        isApplied = true;
////    }

////    private void OnTriggerExit2D(Collider2D other)
////    {
////        if (!other.CompareTag("Player")) return;
////        if (!isApplied) return;

////        isApplied = false;

////        if (restoreOnExit) ApplyPpu(originalPPU);
////    }

////    private void ApplyPpu(int newPPU)
////    {
////        if (pixelPerfectCamera == null) return;

////        pixelPerfectCamera.assetsPPU = newPPU;
////    }
////}


//using UnityEngine;
//using UnityEngine.Rendering.Universal;
//using System.Collections;

//public class PuzzlePpuTrigger : MonoBehaviour
//{
//    [Header("Камера")]
//    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;

//    [Header("Настройки PPU")]
//    [SerializeField] private int puzzlePPU = 16;

//    [Header("Анимация")]
//    [SerializeField] private float transitionDuration = 1f;
//    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

//    [Header("Настройки")]
//    [SerializeField] private bool restoreOnExit = true;

//    private int originalPPU;
//    private bool isApplied = false;
//    private Coroutine ppuCoroutine;

//    private void Awake()
//    {
//        if (pixelPerfectCamera == null)
//            pixelPerfectCamera = Camera.main?.GetComponent<PixelPerfectCamera>();

//        if (pixelPerfectCamera != null)
//            originalPPU = pixelPerfectCamera.assetsPPU;
//    }

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (!other.CompareTag("Player")) return;
//        if (isApplied) return;
//        if (!gameObject.activeInHierarchy) return;
//        ApplyPpu(puzzlePPU);
//        isApplied = true;
//    }

//    private void OnTriggerExit2D(Collider2D other)
//    {
//        if (!other.CompareTag("Player")) return;
//        if (!isApplied) return;

//        isApplied = false;

//        if (restoreOnExit)
//        {
//            if (gameObject.activeInHierarchy)
//            {
//                ApplyPpu(originalPPU);
//            }
//            else
//            {
//                ApplyPpu(originalPPU);
//                Debug.LogWarning("[PuzzlePpuTrigger] Объект неактивен, PPU изменён мгновенно");
//            }
//        }
//    }

//    private void ApplyPpu(int targetPPU)
//    {
//        if (pixelPerfectCamera == null) return;
//        if (ppuCoroutine != null)
//            StopCoroutine(ppuCoroutine);

//        ppuCoroutine = StartCoroutine(PpuTransition(targetPPU)); 
//    }

//    private IEnumerator PpuTransition(int targetPPU)
//    {
//        int startPPU = pixelPerfectCamera.assetsPPU;

//        if (startPPU == targetPPU) yield break;

//        float elapsed = 0f;

//        while (elapsed < transitionDuration)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / transitionDuration);
//            float easedT = easeCurve.Evaluate(t);
//            int currentPPU = Mathf.RoundToInt(Mathf.Lerp(startPPU, targetPPU, easedT));

//            pixelPerfectCamera.assetsPPU = currentPPU;

//            yield return null;
//        }
//        pixelPerfectCamera.assetsPPU = targetPPU;
//    }
//}


using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PuzzlePpuTrigger : MonoBehaviour
{
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;
    [SerializeField] private int puzzlePPU = 16;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool restoreOnExit = true;

    private int originalPPU;
    private bool isApplied = false;
    private Coroutine ppuCoroutine;

    private void Awake()
    {
        if (pixelPerfectCamera == null) pixelPerfectCamera = Camera.main?.GetComponent<PixelPerfectCamera>();

        if (pixelPerfectCamera != null) originalPPU = pixelPerfectCamera.assetsPPU;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isApplied) return;

        ApplyPpu(puzzlePPU);
        isApplied = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isApplied) return;

        isApplied = false;

        if (restoreOnExit) ApplyPpu(originalPPU);
    }

    private void ApplyPpu(int targetPPU)
    {
        if (pixelPerfectCamera == null) return;

        if (ppuCoroutine != null) StopCoroutine(ppuCoroutine);
        if (gameObject.activeInHierarchy) ppuCoroutine = StartCoroutine(PpuTransition(targetPPU));
        else pixelPerfectCamera.assetsPPU = targetPPU;
    }

    private IEnumerator PpuTransition(int targetPPU)
    {
        int startPPU = pixelPerfectCamera.assetsPPU;
        if (startPPU == targetPPU) yield break;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / transitionDuration);
            float easedT = easeCurve.Evaluate(time);
            int currentPPU = Mathf.RoundToInt(Mathf.Lerp(startPPU, targetPPU, easedT));

            pixelPerfectCamera.assetsPPU = currentPPU;

            yield return null;
        }

        pixelPerfectCamera.assetsPPU = targetPPU;
    }
}