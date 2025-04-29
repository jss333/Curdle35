using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMAudioSource : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public bool pauseInsteadOfStop = false;

    [Header("State")]
    [SerializeField] private float originalVolume;
    [SerializeField] private bool initialized = false;
    [SerializeField] private AudioSource source;

    void Awake()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        if (TryGetComponent<AudioSource>(out var source))
        {
            this.source = source;
            originalVolume = source.volume;
        }
        else
        {
            Debug.LogWarning($"No AudioSource configured for BGMAudioSource {name} - play requests will be ignored");
        }

        if (source.clip == null)
        {
            Debug.LogWarning($"No AudioClip configured for BGMAudioSource {name} - play requests will be ignored");
        }

        initialized = true;
    }

    public void Play()
    {
        if (source == null || source.clip == null) return;

        source.Play();
    }

    public AudioSource GetAudioSource()
    {
        if (!initialized)
        {
            Initialize();
        }
        return this.source;
    }

    public float GetOriginalVolume()
    {
        if (!initialized)
        {
            Initialize();
        }
        return originalVolume;
    }
}
