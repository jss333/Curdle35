using UnityEngine;
using System.Collections;

public class SoundsManager : MonoBehaviour
{
    #region Singleton

    public static SoundsManager Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
    }

    #endregion
    
    public enum MusicType
    {
        Victory,
        Defeat,
        Day,
        Night
    }

    [Header("Music References")]
    public AudioSource victoryMusic;
    public AudioSource defeatMusic;
    public AudioSource dayMusic;
    public AudioSource nightMusic;

    [Header("Day Music Special Case")]
    public AudioSource daySFX1; // First SFX for day music special case
    public AudioSource daySFX2; // Second SFX for day music special case

    [Header("Settings")]
    public float fadeDuration = 2.0f; // Time to raise or lower volume gradually

    [Header("Selected Music Type")]
    public MusicType selectedMusic; // Current selected music type

    private Coroutine currentMusicCoroutine; // To track the currently running coroutine
    private AudioSource currentMusic; // The currently playing music AudioSource
    private AudioSource lastSelectedMusic; // The Last playing music AudioSource

    private void Start()
    {
        UpdateMusicType();
    }


    public void UpdateMusicType()
    {
        if (currentMusicCoroutine != null)
        {
            StopCoroutine(currentMusicCoroutine);
        }
        
        switch (selectedMusic)
        {
            case MusicType.Victory:
                currentMusicCoroutine = StartCoroutine(SwitchMusic(victoryMusic));
                break;

            case MusicType.Defeat:
                currentMusicCoroutine = StartCoroutine(SwitchMusic(defeatMusic));
                break;

            case MusicType.Day:
                currentMusicCoroutine = StartCoroutine(PlayDaySequence());
                break;

            case MusicType.Night:
                currentMusicCoroutine = StartCoroutine(SwitchMusic(nightMusic));
                break;
        }
    }

    public void UpdateCurrentMusic(MusicType music)
    {
        selectedMusic = music;

        UpdateMusicType();
    }

    
    // Coroutine to Switch Music (fade out previous and fade in new)
    private IEnumerator SwitchMusic(AudioSource newMusic)
    {
        FadeOutLastSelectedMusic();

        // Fade in the new music
        currentMusic = newMusic;
        if (currentMusic != null)
        {
            currentMusic.volume = 0;
            currentMusic.Play();

            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                currentMusic.volume = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            currentMusic.volume = 1; 
        }
    }

    // Coroutine for Special Case: Play Two SFXs Then Day Music
    private IEnumerator PlayDaySequence()
    {
        FadeOutLastSelectedMusic();

        // Play the first SFX
        if (daySFX1 != null)
        {
            daySFX1.Play();
            yield return new WaitForSeconds(daySFX1.clip.length);
        }

        // Play the second SFX
        if (daySFX2 != null)
        {
            daySFX2.Play();
            yield return new WaitForSeconds(daySFX2.clip.length);
        }

        // Fade in the day music
        currentMusic = dayMusic;
        if (currentMusic != null)
        {
            currentMusic.volume = 0;
            currentMusic.Play();

            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                currentMusic.volume = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            currentMusic.volume = 1; 
        }
    }

    private void FadeOutLastSelectedMusic()
    {
        // Fade out the currently playing music (if any)
        if (currentMusic != null)
        {
            lastSelectedMusic = currentMusic;
            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                lastSelectedMusic.volume = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
            }
            lastSelectedMusic.volume = 0;
            lastSelectedMusic.Stop();
        } 
    }
}