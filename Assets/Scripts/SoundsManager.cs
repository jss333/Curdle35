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
    Hyena_Dies,
    Hyenas_Harvest,
    Hyenas_Upgrade_Spawn_Rate,
    Hyenas_Generate_Spawn_Marker
}

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance { get; private set; }

    [Header("BGM - Config")]
    [SerializeField] private Transform allBGMRoot;
    [SerializeField] private float fadeDuration = 1f;

    [Header("BGM - State")]
    [SerializeField] private float masterBGMVolume = 1f;
    [SerializeField] private BGMAudioSource lastPlayedBGMAudioSrc;
    private Dictionary<BGM, BGMAudioSource> bgmAudioSources = new();
    private readonly Dictionary<AudioSource, Coroutine> bgmAudioSrcCoroutines = new();


    [Header("SFX - Config")]
    [SerializeField] private Transform allSFXRoot;
    [SerializeField] private AudioSource oneShotAudioSource;

    [Header("SFX - State")]
    [SerializeField] private float masterSFXVolume = 1f;
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
        HashSet<BGM> allFoundBGM = new();

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
                allFoundBGM.Add(bgm);
            }
            else
            {
                Debug.LogWarning($"[SoundsManager] No BGMAudioSource found on: {child.name}");
            }
        }

        List<BGM> bgmWithNoMapping = Enum.GetValues(typeof(BGM))
            .Cast<BGM>()
            .Where(sfx => !allFoundBGM.Contains(sfx))
            .ToList();

        foreach (BGM bgm in bgmWithNoMapping)
        {
            Debug.LogWarning($"[SoundsManager] No BGMAudioSource object mapped to BGM {bgm}");
        }
    }

    private void InitializeSFXLookup()
    {
        HashSet<SFX> allFoundSFX = new();

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
                allFoundSFX.Add(sfx);
            }
            else if (child.TryGetComponent(out SFXOneShot oneShot))
            {
                sfxOneShots[sfx] = oneShot;
                allFoundSFX.Add(sfx);
            }
            else
            {
                Debug.LogWarning($"[SoundsManager] No SFXAudioSource or SFXOneShot found on: {child.name}");
            }
        }

        List<SFX> sfxWithNoMapping = Enum.GetValues(typeof(SFX))
            .Cast<SFX>()
            .Where(sfx => sfx != SFX.NONE)
            .Where(sfx => !allFoundSFX.Contains(sfx))
            .ToList();

        foreach (SFX sfx in sfxWithNoMapping)
        {
            Debug.LogWarning($"[SoundsManager] No SFXAudioSource or SFXOneShot object mapped to SFX {sfx}");
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

    public void PlayMusic(BGM bgm)
    {
        if (bgmAudioSources.TryGetValue(bgm, out var bgmAudioSrc))
        {
            SwitchMusic(bgmAudioSrc);
        }
        else
        {
            Debug.LogWarning($"No AudioSource mapped for BGM: {bgm}");
        }
    }

    private void SwitchMusic(BGMAudioSource newBGM)
    {
        PauseOrStopMusicIfAny();

        if (newBGM != null)
        {
            KillPreviousAndStartNewFadingCoroutine(newBGM.GetAudioSource(), 0, newBGM.GetOriginalVolume() * masterBGMVolume, fadeDuration);
            lastPlayedBGMAudioSrc = newBGM;
        }
    }

    public void PauseOrStopMusicIfAny()
    {
        if (lastPlayedBGMAudioSrc != null)
        {
            AudioSource lastAudioSrc = lastPlayedBGMAudioSrc.GetAudioSource();
            KillPreviousAndStartNewFadingCoroutine(lastAudioSrc, lastAudioSrc.volume, 0, fadeDuration, lastPlayedBGMAudioSrc.pauseInsteadOfStop);
            lastPlayedBGMAudioSrc = null;
        }
    }

    private void KillPreviousAndStartNewFadingCoroutine(AudioSource audioSrc, float startVolume, float endVolume, float duration, bool pauseInsteadOfStopWhenFadingOut = false)
    {
        // Check if we have a coroutine that is operating on the same AudioSource, and stop it
        if (bgmAudioSrcCoroutines.ContainsKey(audioSrc))
        {
            Coroutine oldCoroutine = bgmAudioSrcCoroutines[audioSrc];
            if (oldCoroutine != null) // I think coroutines become null after they end
            {
                StopCoroutine(oldCoroutine);
            }
            bgmAudioSrcCoroutines.Remove(audioSrc);
        }

        Coroutine newCoroutine = StartCoroutine(FadeAudio(audioSrc, startVolume, endVolume, duration, pauseInsteadOfStopWhenFadingOut));
        bgmAudioSrcCoroutines.Add(audioSrc, newCoroutine);
    }

    private IEnumerator FadeAudio(AudioSource audioSource, float startVolume, float targetVolume, float duration, bool pauseInsteadOfStopWhenFadingOut)
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

        // Pause/stop if volume was faded out to 0
        if (Mathf.Approximately(targetVolume, 0f))
        {
            if (pauseInsteadOfStopWhenFadingOut)
            {
                audioSource.Pause();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    public void SetMasterBGMVolume(float newVolume)
    {
        masterBGMVolume = Mathf.Clamp01(newVolume);
        if (lastPlayedBGMAudioSrc != null)
        {
            lastPlayedBGMAudioSrc.GetAudioSource().volume = lastPlayedBGMAudioSrc.GetOriginalVolume() * masterBGMVolume;
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

    public float GetMasterSFXVolume()
    {
        return masterSFXVolume;
    }

    public void SetMasterSFXVolume(float newVolume)
    {
        masterSFXVolume = Mathf.Clamp01(newVolume);
        if (lastPlayedBGMAudioSrc != null)
        {
            lastPlayedBGMAudioSrc.GetAudioSource().volume = lastPlayedBGMAudioSrc.GetOriginalVolume() * masterBGMVolume;
        }
    }

    #endregion
}