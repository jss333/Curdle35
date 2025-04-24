using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMAudioSource : MonoBehaviour
{
    private AudioSource source;

    void Awake()
    {
        if (TryGetComponent<AudioSource>(out var source))
        {
            this.source = source;
        }
        else
        {
            Debug.LogWarning($"No AudioSource configured for BGMAudioSource {name} - play requests will be ignored");
        }

        if (source.clip == null)
        {
            Debug.LogWarning($"No AudioClip configured for BGMAudioSource {name} - play requests will be ignored");
        }
    }

    public void Play()
    {
        if (source == null || source.clip == null) return;

        source.Play();
    }

    public AudioSource GetAudioSource() => source;
}
