using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Audio/Audio Event Channel")]
public class AudioEventChannelSO : ScriptableObject
{
    //sfx clip, volune, pitch
    public event Action<AudioClip, float, float, Vector3?> OnSfxRequested;

    public event Action<AudioClip, float> OnMusicRequested;
    public event Action OnStopMusicRequested;

    public void RaiseSfx(AudioClip clip, float volume = 1f, float pitch = 1f, Vector3? possition = null)
    {
        if (clip == null) return;
        OnSfxRequested?.Invoke(clip, Mathf.Clamp01(volume), pitch, possition);
    }

    public void RaiseMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        OnMusicRequested?.Invoke(clip, Mathf.Clamp01(volume));
    }

    public void RaiseStopMusic()
    {
        OnStopMusicRequested?.Invoke();
    }

}