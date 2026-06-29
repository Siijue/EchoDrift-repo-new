using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MusicZoneTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip zoneMusic;
    [SerializeField] private bool fadeInOnEnter = true;
    [SerializeField] private bool fadeOutOnExit = true;
    [SerializeField] private bool stopMusicOnExit = true;
    [SerializeField] private bool isContinueWithoutZone = false;

    private bool isPlayerInside = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlayerInside) return;

        isPlayerInside = true;

        if (zoneMusic != null && AudioManager.Instance != null) AudioManager.Instance.PlayMusic(zoneMusic, fadeInOnEnter);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isPlayerInside) return;

        isPlayerInside = false;

        if (AudioManager.Instance != null)
        {
            if (stopMusicOnExit && !isContinueWithoutZone) AudioManager.Instance.StopMusic(fadeOutOnExit);
        }
    }
}