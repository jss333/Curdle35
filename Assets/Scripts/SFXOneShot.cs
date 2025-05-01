using UnityEngine;

public class SFXOneShot : MonoBehaviour, IPlayableSFX
{
    [Header("Config")]
    [SerializeField] private AudioClip clip;
    [SerializeField] private float pitchRandomization = 0.1f;

    void Awake()
    {
        if (clip  == null)
        {
            Debug.LogWarning($"No AudioCLip configured for SFXOneShot {name} - play requests will be ignored");
        }
    }

    public void Play()
    {
        if (clip == null) return;

        AudioSource source = SoundsManager.Instance.GetOneShotAudioSource();

        source.volume = SoundsManager.Instance.MasterSFXVolume;
        source.pitch = 1f + Random.Range(-pitchRandomization, pitchRandomization);
        source.PlayOneShot(clip);
    }

    public float GetClipLength()
    {
        if (clip == null) return 0;

        return clip.length;
    }
}
