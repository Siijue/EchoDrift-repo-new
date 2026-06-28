using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CheckpointIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    private string checkpointID;
    private MapUI mapUI;
    private Coroutine pulseCoroutine;

    public void Initialize(string id, bool isCurrent, MapUI own)
    {
        checkpointID = id;
        mapUI = own;

        if (button == null) Debug.Log("Button == null!");

        button.onClick.AddListener(() => {
            mapUI.OnCheckpointClicked(checkpointID, GetComponent<RectTransform>());
            Debug.Log("КНОПКА НАЖАТА");
        }
        );

        if (isCurrent) StartPulse();
        else StopPulse();
    }

    private void StartPulse()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseCrtn());
    }

    private void StopPulse()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        transform.localScale = Vector3.one;
    }

    private IEnumerator PulseCrtn()
    {
        float time = 0f;
        while (true)
        {
            time += Time.unscaledDeltaTime * 3f;
            float scale = 1f + Mathf.Sin(time) * 0.15f;
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
    }
}
