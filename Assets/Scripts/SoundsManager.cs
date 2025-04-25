using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System;
using TMPro;

public interface IPlayableSFX
{
    void Play();
    float GetClipLength();
}

public enum BGM
{
    Day,
    Night,
    Victory,
    Defeat
}

public enum SFX
{
    NONE,
    Day_Begins,
    Player_Selects_Unit,
    Player_Deselects_Unit,
    Player_Selects_Command,
    Player_Deselects_Command,
    Player_Confirms_Move,
    Player_Unit_Moves,
    Player_Unit_Attacks,
    Player_Unit_Is_Hit,
    Player_Unit_Dies,
    Player_Confirms_Build,
    Player_Confirms_End_Of_Turn,
    Turret_Shoots,
    Turret_Is_Hit,
    Turret_Is_Destroyed,
    Player_Harvests,
    Night_Begins,
    Hyenas_Spawn,
    Hyena_Moves,
    Hyena_Attacks,
    NOT_USED_Hyena_Hits_Turret,
    Hyena_Dies,
    Hyenas_Harvest,
    Hyenas_Upgrade_Spawn_Rate,
    Hyenas_Generate_Spawn_Marker
}

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private Transform allBGMRoot;
    [SerializeField] private Transform allSFXRoot;
    [SerializeField] private AudioSource oneShotAudioSource;

    [Header("Config - All BGMs")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool muteAllBGM = false;

    private Dictionary<BGM, BGMAudioSource> bgmAudioSources = new();
    private readonly Dictionary<AudioSource, Coroutine> bgmAudioSrcCoroutines = new();
    [SerializeField] private AudioSource lastPlayedBGMAudioSrc; // Used to check if tehre are any coroutines still running

    private Dictionary<SFX, SFXAudioSource> sfxAudioSources = new();
    private Dictionary<SFX, SFXOneShot> sfxOneShots = new();

    void Awake()
    {
        Instance = this;
        InitializeSFXLookup();
        InitializeBGMLookup();
    }

    private void InitializeBGMLookup()
    {
        foreach (Transform child in allBGMRoot)
        {
            if (!TryParseBGMEnum(child.name, out BGM bgm))
            {
                Debug.LogWarning($"[SoundsManager] Failed to map BGM enum for object: {child.name}");
                continue;
            }

            if (child.TryGetComponent(out BGMAudioSource source))
            {
                bgmAudioSources[bgm] = source;
            }
            else
            {
                Debug.LogWarning($"[SoundsManager] No BGMAudioSource found on: {child.name}");
            }
        }
    }

    private void InitializeSFXLookup()
    {
        foreach (Transform child in allSFXRoot)
        {
            if (!TryParseSFXEnum(child.name, out SFX sfx))
            {
                Debug.LogWarning($"[SoundsManager] Failed to map SFX enum for object: {child.name}");
                continue;
            }

            if (child.TryGetComponent(out SFXAudioSource audioSource))
            {
                sfxAudioSources[sfx] = audioSource;
            }
            else if (child.TryGetComponent(out SFXOneShot oneShot))
            {
                sfxOneShots[sfx] = oneShot;
            }
            else
            {
                Debug.LogWarning($"[SoundsManager] No SFXAudioSource or SFXOneShot found on: {child.name}");
            }
        }
    }

    private bool TryParseBGMEnum(string objName, out BGM result)
    {
        string expectedPrefix = "BGM ";
        result = default;
        if (!objName.StartsWith(expectedPrefix)) return false;

        string enumName = objName.Substring(expectedPrefix.Length).Trim();
        return System.Enum.TryParse(enumName, out result);
    }

    private bool TryParseSFXEnum(string objName, out SFX result)
    {
        string expectedPrefix = "SFX ";
        result = default;
        if (!objName.StartsWith(expectedPrefix)) return false;

        string enumName = objName.Substring(expectedPrefix.Length).Trim();
        return System.Enum.TryParse(enumName, out result);
    }

    #region BGM Playback

    public void PlayMusic(BGM bgm, bool pausePrev = false)
    {
        if (muteAllBGM) return;

        if (bgmAudioSources.TryGetValue(bgm, out var bgmAudioSrc))
        {
            SwitchMusic(bgmAudioSrc.GetAudioSource(), pausePrev);
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

    #endregion

    #region SFX Playback

    public AudioSource GetOneShotAudioSource()
    {
        return oneShotAudioSource;
    }

    private IPlayableSFX GetPlayableSFX(SFX sfx)
    {
        if (sfx == SFX.NONE)
        {
            return null;
        }
        else if (sfxAudioSources.TryGetValue(sfx, out var audioSrc))
        {
            return audioSrc;
        }
        else if (sfxOneShots.TryGetValue(sfx, out var oneShot))
        {
            return oneShot;
        }
        else
        {
            Debug.Log($"[SoundsManager] No SFX mapping for: {sfx}");
            return null;
        }
    }

    public void PlaySFX(SFX sfx)
    {
        IPlayableSFX playable = GetPlayableSFX(sfx);
        if (playable != null)
        {
            playable.Play();
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

        List<IPlayableSFX> playableSFXs = sfxInOrder
            .Select(GetPlayableSFX)
            .Where(playable => playable != null)
            .ToList();

        Sequence sequence = DOTween.Sequence();
        foreach (var playableSFX in playableSFXs)
        {
            sequence.AppendCallback(() => playableSFX.Play());
            sequence.AppendInterval(playableSFX.GetClipLength());
        }
        if (callback != null)
        {
            sequence.AppendCallback(() => callback.Invoke());
        }
        sequence.Play();
    }

    #endregion
}