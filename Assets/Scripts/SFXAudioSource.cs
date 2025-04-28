using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXAudioSource : MonoBehaviour, IPlayableSFX
{
    [Header("Config")]
    [SerializeField] private float pitchRandomization = 0.1f;

    [Header("State")]
    [SerializeField] private float originalPitch;
    [SerializeField] private float originalVolume;

    private AudioSource source;

    void Awake()
    {
        if (TryGetComponent<AudioSource>(out var source))
        {
            this.source = source;
        }
        else
        {
            Debug.LogWarning($"No AudioSource configured for SFXAudioSource {name} - play requests will be ignored");
        }

        if (source.clip == null)
        {
            Debug.LogWarning($"No AudioClip configured for SFXAudioSource {name} - play requests will be ignored");
        }
        
        originalPitch = source.pitch;
        originalVolume = source.volume;
    }

    public void Play()
    {
        if (source == null || source.clip == null) return;

        source.pitch = originalPitch + Random.Range(-pitchRandomization, pitchRandomization);
        source.volume = SoundsManager.Instance.GetMasterSFXVolume() * originalVolume;
        source.Play();
    }

    public float GetClipLength()
    {
        if (source == null || source.clip == null) return 0;

        return source.clip.length;
    }
}
