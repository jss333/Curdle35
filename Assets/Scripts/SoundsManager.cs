using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System;
using UnityEngine.InputSystem;

public enum BGM
{
    Day,
    Night,
    Victory,
    Defeat
}

public enum SFX
{
    Horns,
    Roar
}

interface IEnumAudioSourcePair<TEnum>
{
    public TEnum GetKey();
    public AudioSource GetValue();
}

[Serializable]
public struct BGMAudioSourcePair: IEnumAudioSourcePair<BGM>
{
    public BGM key;
    public AudioSource value;
    public BGM GetKey() { return key; }
    public AudioSource GetValue() { return value; }
}

[Serializable]
public struct SFXAudioSourcePair : IEnumAudioSourcePair<SFX>
{
    public SFX key;
    public AudioSource value;
    public SFX GetKey() { return key; }
    public AudioSource GetValue() { return value; }
}

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance { get; private set; }
    
    [Header("Config - Music mappings")]
    [SerializeField] private List<BGMAudioSourcePair> bgmMappings = new();

    [Header("Config - SFX mappings")]
    [SerializeField] private List<SFXAudioSourcePair> sfxMappings = new();

    [Header("Config")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("State")]
    [SerializeField] private AudioSource lastPlayedBGMAudioSrc; // Used to check if tehre are any coroutines still running

    private readonly Dictionary<AudioSource, Coroutine> bgmAudioSrcCoroutines = new();
    private Dictionary<BGM, AudioSource> bgmLookup;
    private Dictionary<SFX, AudioSource> sfxLookup;

    void Awake()
    {
        Instance = this;
        bgmLookup = LoadMappings<BGMAudioSourcePair, BGM>(bgmMappings);
        sfxLookup = LoadMappings<SFXAudioSourcePair, SFX>(sfxMappings);
    }

    private static Dictionary<TEnum, AudioSource> LoadMappings<TPair, TEnum>(List<TPair> mappings) where TPair: IEnumAudioSourcePair<TEnum>
    {
        Dictionary<TEnum, AudioSource> lookup = new();

        foreach (TPair mapping in mappings)
        {
            TEnum key = mapping.GetKey();
            AudioSource value = mapping.GetValue();

            if (!lookup.ContainsKey(key))
            {
                if (value != null && value.clip != null)
                {
                    lookup.Add(key, value);
                }
                else
                {
                    Debug.LogWarning($"No AudioSource or no Clip assigned to BGM/SFX key: {key}");
                }
            }
            else
            {
                Debug.LogWarning($"Duplicate BGM/SFX key found: {key}");
            }
        }

        return lookup;
    }

    public void PlayMusic(BGM bgm, bool pausePrev = false)
    {
        if (bgmLookup.TryGetValue(bgm, out var bgmAudioSrc))
        {
            SwitchMusic(bgmAudioSrc, pausePrev);
        }
        else
        {
            Debug.LogWarning($"No AudioSource mapped for BGM: {bgm}");
        }
    }

    private void SwitchMusic(AudioSource newBGM, bool pausePrev)
    {
        PauseOrStopMusicIfAny(pausePrev);

        if (newBGM != null)
        {
            KillPreviousAndStartNewFadingCoroutine(newBGM, 0, 1, fadeDuration, pausePrev);
            lastPlayedBGMAudioSrc = newBGM;
        }
    }

    public void PauseOrStopMusicIfAny(bool pauseInsteadOfStop)
    {
        if (lastPlayedBGMAudioSrc != null)
        {
            KillPreviousAndStartNewFadingCoroutine(lastPlayedBGMAudioSrc, 1, 0, fadeDuration, pauseInsteadOfStop);
            lastPlayedBGMAudioSrc = null;
        }
    }

    private void KillPreviousAndStartNewFadingCoroutine(AudioSource audioSrc, float startVolume, float endVolume, float duration, bool pauseInsteadOfStopPrevMusic)
    {
        // Check if we have a coroutine that is operating on the same AudioSource, and stop it
        if (bgmAudioSrcCoroutines.ContainsKey(audioSrc))
        {
            StopCoroutine(bgmAudioSrcCoroutines[audioSrc]);
            bgmAudioSrcCoroutines.Remove(audioSrc);
        }

        Coroutine co = StartCoroutine(FadeAudio(audioSrc, startVolume, endVolume, duration, pauseInsteadOfStopPrevMusic));
        bgmAudioSrcCoroutines.Add(audioSrc, co);
    }

    private IEnumerator FadeAudio(AudioSource audioSource, float startVolume, float targetVolume, float duration, bool pauseInsteadOfStopPrevMusic)
    {
        if (audioSource == null) yield break;

        audioSource.volume = startVolume;
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = Mathf.Clamp01(elapsedTime / duration);
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVolume;

        // Auto-stop if volume was faded out to 0
        if (Mathf.Approximately(targetVolume, 0f))
        {
            if (pauseInsteadOfStopPrevMusic)
            {
                audioSource.Pause();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    public void PlaySFX(SFX sfx)
    {
        if (sfxLookup.TryGetValue(sfx, out var audioSrc))
        {
            audioSrc.Play();
        }
        else
        {
            Debug.LogWarning($"No AudioSource mapped for SFX: {sfx}");
        }
    }

    public void PlaySFXSequence(Action callback = null, params SFX[] sfxInOrder)
    {
        if (sfxInOrder == null || sfxInOrder.Length == 0)
        {
            Debug.LogWarning("No SFX provided to play sequence.");
            callback?.Invoke();
            return;
        }

        foreach (var sfx in sfxInOrder.Where(sfx => !sfxLookup.ContainsKey(sfx)))
        {
            Debug.LogWarning($"No AudioSource mapped for SFX: {sfx}");
        }

        List<AudioSource> audioSrcs = sfxInOrder
            .Where(sfxLookup.ContainsKey)
            .Select(sfx => sfxLookup[sfx])
            .ToList();

        Sequence sequence = DOTween.Sequence();
        foreach (var audioSrc in audioSrcs)
        {
            sequence.AppendCallback(() => audioSrc.Play());
            sequence.AppendInterval(audioSrc.clip.length);
            Debug.Log($"The SFX clip is {audioSrc.clip.length} long");
        }
        if (callback != null)
        {
            sequence.AppendCallback(() => callback.Invoke());
        }
        sequence.Play();
    }
}