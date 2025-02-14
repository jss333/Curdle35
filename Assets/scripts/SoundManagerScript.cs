using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public enum Music{
        DawnBegin,
        DayBegin,
        DuskBegin,
        NightBegin,

        LossMusic,
        WinMusic,
        MainMenuMusic, //idk

        MAX_COUNT,
    };
    public enum Effects{
        CatAttack, //
        HyenaSpawn, //
        HyenaAttack,
        HyenaMovement,
        HyenaDeath,
        LionMovement,
        LionTower,
        JaguarMovement,
        UIClick,
        UnitSelection,

        CatResourceCollection,
        HyenaResourceCollection,

        HyenaSpawnRateIncreased,
        TowerAttack,
        TowerDestroyed,
        CatDeath,

        MAX_COUNT,
    };

    float dayNightMusicTimer = 0.0f;

    [SerializeField] private AudioClip[] musicClips;
    [SerializeField] private AudioClip[] effectsClips;
    private AudioSource[] music;
    private AudioSource[] effects;

    void OnValidate()
    {
        if(musicClips == null || musicClips.Length != (int)Music.MAX_COUNT){
            AudioClip[] newMusic = new AudioClip[(int)Music.MAX_COUNT];
            for(int i = 0; i < Mathf.Min(musicClips.Length, (int)Music.MAX_COUNT); i++){
                newMusic[i] = musicClips[i];
            }
            musicClips = newMusic;
        }
        
        if(effectsClips == null || effectsClips.Length != (int)Effects.MAX_COUNT){
            AudioClip[] newEffects = new AudioClip[(int)Effects.MAX_COUNT];
            for(int i = 0; i < Mathf.Min(effectsClips.Length, (int)Effects.MAX_COUNT); i++){
                newEffects[i] = effectsClips[i];
            }
            effectsClips = newEffects;
        }
    }

    void Start(){
        music = new AudioSource[(int)Music.MAX_COUNT];
        effects = new AudioSource[(int)Effects.MAX_COUNT];
        for(int i = 0; i < musicClips.Length; i++){
            if(musicClips[i] == null){
                continue;
            }
            music[i] = gameObject.AddComponent<AudioSource>();
            music[i].clip = musicClips[i];
        }
        for(int i = 0; i < effectsClips.Length; i++){
            if(effectsClips[i] == null){
                continue;
            }
            effects[i] = gameObject.AddComponent<AudioSource>();
            effects[i].clip = effectsClips[i];
        }
    }

    public void PlayDayMusic(){
        Debug.Log("playing day music");
        music[(int)Music.DayBegin].loop = true;
        music[(int)Music.DayBegin].time = dayNightMusicTimer;
        music[(int)Music.DayBegin].Play();
        
    }
    public void PlayNightMusic(){
        Debug.Log("playing night music");
        music[(int)Music.NightBegin].loop = true;
        music[(int)Music.NightBegin].time = dayNightMusicTimer;
        music[(int)Music.NightBegin].Play();
    }    
    public void PlayMusic(Music musicVal){
        if(music[(int)musicVal] != null){
            //StopAllMusic();

            //stop the last srack, then play the next
            music[(int)musicVal].Play();
        }
    }
    public void StopMusic(Music musicVal){ //might want to remove the paramter if stopping all music, depends on the usage
        if(musicVal == Music.DayBegin || musicVal == Music.NightBegin){
            dayNightMusicTimer = music[(int)Music.NightBegin].time;
        }

        if(music[(int)musicVal] != null){
            music[(int)musicVal].Stop();
        }
    }
    public void StopAllMusic(){
        foreach(var sound in music){
            if(sound != null){
                sound.Stop();
            }
        }
    }

    public void PlayEffect(Effects effectVal){
        if(effects[(int)effectVal] != null){
            //play effect
            effects[(int)effectVal].Play();
        }
    }
    public void StopEffect(Effects effectVal){
        //idk if we'll ever care to stop an effect
        effects[(int)effectVal].Stop();
    }

}
