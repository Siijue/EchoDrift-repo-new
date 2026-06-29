using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float musicFadeDuaration = 1f;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private int sfxPoolSize = 10;

    private AudioSource[] sfxPool;
    private int currentSfxIndex = 0;

    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (sfxSource != null)
        {
            sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
            {
                sfxPool[i] = Instantiate(sfxSource, transform);
                sfxPool[i].gameObject.name = $"SFX_{i}";
            }
        }
        LoadVolumeSettings();
    }

    private void Start()
    {
        SetMusicVolume(musicVolume);
        SetSfxVolume(sfxVolume);
    }

    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void PlayMusic(AudioClip clip, bool fadeIn = true)
    {
        if (musicSource == null || clip == null) return;

        if (fadeIn)
        {
            StartCoroutine(FadeMusic(clip, true));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public void StopMusic(bool fadeOut = true)
    {
        if (musicSource == null) return;

        if (fadeOut)
        {
            StartCoroutine(FadeMusic(null, false));
        }
        else
        {
            musicSource.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }

        Debug.Log($"Music volume: {musicVolume}");
    }

    public float GetMusicVolume() => musicVolume;

    private IEnumerator FadeMusic(AudioClip newClip, bool isFadeIn)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float targetVolume = isFadeIn ? musicVolume : 0f;
        float elapsed = 0f;

        if (isFadeIn && newClip != null)
        {
            musicSource.clip = newClip;
            musicSource.Play();
        }

        while (elapsed < musicFadeDuaration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / musicFadeDuaration;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        musicSource.volume = targetVolume;

        if (!isFadeIn)
        {
            musicSource.Stop();
        }
    }
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxPool == null || sfxPool.Length == 0) return;

        AudioSource source = sfxPool[currentSfxIndex];
        currentSfxIndex = (currentSfxIndex + 1) % sfxPool.Length;

        source.clip = clip;
        source.volume = volume * sfxVolume;
        source.Play();
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source = sfxPool[currentSfxIndex];
        currentSfxIndex = (currentSfxIndex + 1) % sfxPool.Length;

        source.transform.position = position;
        source.clip = clip;
        source.volume = volume * sfxVolume;
        source.spatialBlend = 1f;
        source.Play();

        StartCoroutine(ResetSpatialBlend(source));
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
        if (sfxPool != null)
        {
            foreach (var source in sfxPool)
            {
                if (source != null)
                {
                    source.volume = sfxVolume;
                }
            }
        }
    }

    public float GetSfxVolume() => sfxVolume;

    private IEnumerator ResetSpatialBlend(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        source.spatialBlend = 0f;
    }

    public void StopAllSFX()
    {
        if (sfxPool != null)
        {
            foreach (var source in sfxPool)
            {
                if (source != null) source.Stop();
            }
        }
    }

    public void PauseAll()
    {
        if (musicSource != null) musicSource.Pause();
        if (sfxPool != null)
        {
            foreach (var source in sfxPool)
            {
                if (source != null) source.Pause();
            }
        }
    }

    public void ResumeAll()
    {
        if (musicSource != null) musicSource.UnPause();
        if (sfxPool != null)
        {
            foreach (var source in sfxPool)
            {
                if (source != null) source.UnPause();
            }
        }
    }
}
