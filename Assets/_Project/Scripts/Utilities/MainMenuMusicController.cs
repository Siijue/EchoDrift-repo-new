using UnityEngine;

public class MainMenuMusicController : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private float startDelay = 0.5f;

    private void Start()
    {
        if (menuMusic == null) return;
        Invoke(nameof(PlayMenuMusic), startDelay);
    }

    private void PlayMenuMusic()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic(menuMusic, fadeIn: false);
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic(fadeOut: false);
    }
}