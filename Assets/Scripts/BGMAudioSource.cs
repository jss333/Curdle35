using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMAudioSource : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public bool pauseInsteadOfStop = false;
    [SerializeField] public float silencePrefix = 0f;
    [SerializeField] public float fadeInDuration = 0f;
    [SerializeField] public float fadeOutDuration = 0f;

    [Header("State")]
    [SerializeField] private float originalVolume;
    [SerializeField] private bool initialized = false;
    [SerializeField] private bool skipPlay = false;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private Coroutine lastFadingCoroutine;
    [SerializeField] private bool lastFadingCoroutineIsRunning = false;

    private bool SkipPlay
    {
        get
        {
            return skipPlay || !gameObject.activeInHierarchy;
        }
        set => skipPlay = value;
    }

    void Awake()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        if (!TryGetComponent<AudioSource>(out var audioSrc))
        {
            Debug.LogWarning($"[BGMAudioSource] No AudioSource configured for BGMAudioSource {name} - play requests will be ignored");
            skipPlay = true;
        }
        else if (audioSrc.clip == null)
        {
            Debug.LogWarning($"[BGMAudioSource] No AudioClip configured for BGMAudioSource {name} - play requests will be ignored");
            skipPlay = true;
        }
        else
        {
            this.audioSrc = audioSrc;
            originalVolume = audioSrc.volume;
            skipPlay = false;
        }
        
        initialized = true;
    }

    public void Play()
    {
        if (SkipPlay) return;

        StopLastFadingCoroutineIfAny();
        lastFadingCoroutineIsRunning = true;
        lastFadingCoroutine = StartCoroutine(FadeAudioIn());
    }

    public void StopOrPause()
    {
        if (SkipPlay) return;

        StopLastFadingCoroutineIfAny();
        lastFadingCoroutineIsRunning = true;
        lastFadingCoroutine = StartCoroutine(FadeAudioOut());
    }

    private void StopLastFadingCoroutineIfAny()
    {
        if (lastFadingCoroutineIsRunning && lastFadingCoroutine != null)
        {
            StopCoroutine(lastFadingCoroutine);
            lastFadingCoroutineIsRunning = false;
        }
    }

    private IEnumerator FadeAudioIn()
    {
        float startVolume;

        if (audioSrc.isPlaying)
        {
            // if already playing, ignore silence prefix and do the fade-in based on current volume
            startVolume = audioSrc.volume / VolumeScalingFactor();
        }
        else
        {
            yield return new WaitForSeconds(silencePrefix);

            startVolume = 0;
            audioSrc.Play();
        }

        float targetVolume = 1;
        float rate = 1 / fadeInDuration;

        yield return InterpolateVolumeOverTime(startVolume, targetVolume, rate);

        lastFadingCoroutineIsRunning = false;
    }

    private IEnumerator FadeAudioOut()
    {
        float startVolume = audioSrc.volume / VolumeScalingFactor();
        float targetVolume = 0;
        float rate = 1 / fadeOutDuration;

        yield return InterpolateVolumeOverTime(startVolume, targetVolume, rate);

        if (pauseInsteadOfStop)
        {
            audioSrc.Pause();
        }
        else
        {
            audioSrc.Stop();
        }

        lastFadingCoroutineIsRunning = false;
    }

    private IEnumerator InterpolateVolumeOverTime(float unscaledStartVolume, float unscaledTargetVolume, float normalizedRatePerSec)
    {
        float totalTime = Mathf.Abs(unscaledTargetVolume - unscaledStartVolume) / normalizedRatePerSec;

        float elapsedTime = 0f;
        while (elapsedTime < totalTime)
        {
            float t = Mathf.Clamp01(elapsedTime / totalTime);
            audioSrc.volume = Mathf.Lerp(unscaledStartVolume, unscaledTargetVolume, t) * VolumeScalingFactor();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSrc.volume = unscaledTargetVolume * VolumeScalingFactor();
    }

    private float VolumeScalingFactor()
    {
        return originalVolume * SoundsManager.Instance.MasterBGMVolume;
    }

    public void ScaleVolume(float masterVolume)
    {
        if (skipPlay) return;

        // If we are not currently fading, we can scale the volume directly
        if (!lastFadingCoroutineIsRunning)
        {
            audioSrc.volume = originalVolume * masterVolume;
        }
    }
}
