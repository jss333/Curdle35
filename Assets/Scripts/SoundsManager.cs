using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System;
using TMPro;
using System.Collections.ObjectModel;

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

    [Header("BGM - State")]
    [SerializeField] private float masterBGMVolume = 1f;
    [SerializeField] private BGMAudioSource currentBGMAudioSrc;
    private readonly Dictionary<BGM, BGMAudioSource> bgmAudioSources = new();


    [Header("SFX - Config")]
    [SerializeField] private Transform allSFXRoot;
    [SerializeField] private AudioSource oneShotAudioSource;

    [Header("SFX - State")]
    [SerializeField] private float masterSFXVolume = 1f;
    private readonly Dictionary<SFX, SFXAudioSource> sfxAudioSources = new();
    private readonly Dictionary<SFX, SFXOneShot> sfxOneShots = new();

    #region Initialization

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
            if (TryParseEnum(child.name, "BGM ", out BGM bgm))
            {
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
        }

        LogUnmappedEnums(allFoundBGM);
    }

    private void InitializeSFXLookup()
    {
        HashSet<SFX> allFoundSFX = new();

        foreach (Transform child in allSFXRoot)
        {
            if (TryParseEnum(child.name, "SFX ", out SFX sfx))
            {
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
        }

        allFoundSFX.Add(SFX.NONE); // NONE is always "missing", no need to log it
        LogUnmappedEnums(allFoundSFX);
    }

    private bool TryParseEnum<TEnum>(string objName, string expectedPrefix, out TEnum result) where TEnum: struct
    {
        result = default;
        if (!objName.StartsWith(expectedPrefix)) return false;

        string enumName = objName.Substring(expectedPrefix.Length).Trim();
        if (Enum.TryParse(enumName, out result))
        {
            return true;
        }
        else
        {
            Debug.LogWarning($"[SoundsManager] Failed to map a {typeof(TEnum).Name} enum for object with name: {objName}");
            return false;
        }
    }

    private void LogUnmappedEnums<TEnum>(HashSet<TEnum> allMappedEnums)
    {
        List<TEnum> unmapped = Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Where(eachEnum => !allMappedEnums.Contains(eachEnum))
            .ToList();

        foreach (TEnum eachEnum in unmapped)
        {
            Debug.LogWarning($"[SoundsManager] No object mapped to enum {typeof(TEnum).Name}.{eachEnum}");
        }
    }

    #endregion

    #region BGM Playback

    public void PlayMusic(BGM bgm)
    {
        if (bgmAudioSources.TryGetValue(bgm, out var bgmAudioSrc))
        {
            StopOrPauseCurrentMusicIfAny();

            bgmAudioSrc.Play();
            currentBGMAudioSrc = bgmAudioSrc;
        }
        else
        {
            Debug.LogWarning($"[SoundsManager] No AudioSource mapped for BGM: {bgm}");
        }
    }

    public void StopOrPauseCurrentMusicIfAny()
    {
        if (currentBGMAudioSrc != null)
        {
            currentBGMAudioSrc.StopOrPause();
            currentBGMAudioSrc = null;
        }
    }

    public float MasterBGMVolume
    {
        get => masterBGMVolume;
        set
        {
            masterBGMVolume = Mathf.Clamp01(value);
            if (currentBGMAudioSrc != null)
            {
                currentBGMAudioSrc.ScaleVolume(masterBGMVolume);
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
            if (audioSrc.gameObject.activeInHierarchy)
            {
                return audioSrc;
            }
            return null;
        }
        else if (sfxOneShots.TryGetValue(sfx, out var oneShot))
        {
            if (oneShot.gameObject.activeInHierarchy)
            {
                return oneShot;
            }
            return null;
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

    public float MasterSFXVolume
    {
        get => masterSFXVolume;
        set
        {
            masterSFXVolume = Mathf.Clamp01(value);
        }
    }

    #endregion
}