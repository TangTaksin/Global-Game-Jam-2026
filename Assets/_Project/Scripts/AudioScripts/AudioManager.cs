using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Event Channel (Required)")]
    [SerializeField] private AudioEventChannelSO audioChannel;

    [Header("Music Source")]
    [SerializeField] private AudioSource musicSource;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 10;
    [SerializeField] private bool sfx3D = false;           // true = 3D spatial sound
    [SerializeField] private float sfxMinDistance = 1f;
    [SerializeField] private float sfxMaxDistance = 25f;

    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 1f;

    [Header("Keep Across Scenes")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private List<AudioSource> sfxPool;
    private int poolIndex = 0;

    void Awake()
    {
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        // Music source
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        // Build SFX pool
        BuildSfxPool();
    }

    private void BuildSfxPool()
    {
        sfxPool = new List<AudioSource>(sfxPoolSize);

        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject child = new GameObject($"SFX_{i}");
            child.transform.SetParent(transform);

            AudioSource src = child.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;

            // 2D/3D settings
            src.spatialBlend = sfx3D ? 1f : 0f;
            if (sfx3D)
            {
                src.minDistance = sfxMinDistance;
                src.maxDistance = sfxMaxDistance;
                src.rolloffMode = AudioRolloffMode.Logarithmic;
            }

            sfxPool.Add(src);
        }
    }

    void OnEnable()
    {
        if (audioChannel == null)
        {
            Debug.LogError("[AudioManager] Missing AudioEventChannelSO reference.");
            return;
        }

        audioChannel.OnSfxRequested += PlaySfxPooled;
        audioChannel.OnMusicRequested += PlayMusic;
        audioChannel.OnStopMusicRequested += StopMusic;
    }

    void OnDisable()
    {
        if (audioChannel == null) return;

        audioChannel.OnSfxRequested -= PlaySfxPooled;
        audioChannel.OnMusicRequested -= PlayMusic;
        audioChannel.OnStopMusicRequested -= StopMusic;
    }

    // ---- SFX (POOL) ----
    private void PlaySfxPooled(AudioClip clip, float volume, float pitch, Vector3? position)
    {
        if (clip == null || sfxPool == null || sfxPool.Count == 0) return;

        // Find an available source (not playing). If none, use round-robin (cuts the oldest).
        AudioSource src = GetAvailableSfxSource();

        // Optional position (for 3D)
        if (position.HasValue)
            src.transform.position = position.Value;
        else
            src.transform.localPosition = Vector3.zero;

        src.pitch = pitch;
        src.volume = volume * sfxVolume * masterVolume;

        src.clip = clip;
        src.Play();

        // Reset pitch after play to avoid side effects (optional)
        // (not required if you always set pitch)
    }

    private AudioSource GetAvailableSfxSource()
    {
        // Try to find a free one starting from poolIndex
        for (int i = 0; i < sfxPool.Count; i++)
        {
            int idx = (poolIndex + i) % sfxPool.Count;
            if (!sfxPool[idx].isPlaying)
            {
                poolIndex = (idx + 1) % sfxPool.Count;
                return sfxPool[idx];
            }
        }

        // None free → use round-robin (interrupt oldest)
        AudioSource forced = sfxPool[poolIndex];
        poolIndex = (poolIndex + 1) % sfxPool.Count;
        forced.Stop();
        return forced;
    }

    // ---- MUSIC ----
    private void PlayMusic(AudioClip clip, float volume)
    {
        if (clip == null) return;

        float v = volume * musicVolume * masterVolume;

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            musicSource.volume = v;
            return;
        }

        musicSource.clip = clip;
        musicSource.volume = v;
        musicSource.Play();
    }

    private void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }
}
