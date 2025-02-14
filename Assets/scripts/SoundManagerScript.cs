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
        CatClick,
        LionMovement,
        LionTower,
        JaguarMovement,
        UIClick,

        CatResourceCollection,
        HyenaResourceCollection,

        HyenaSpawnRateIncreased,
        TowerAttack,
        TowerDestroyed,
        CatDeath,

        MAX_COUNT,
    };

    [SerializeField] private AudioSource[] music;
    [SerializeField] private AudioSource[] effects;

    void OnValidate()
    {
        if(music == null || music.Length != (int)Music.MAX_COUNT){
            AudioSource[] newMusic = new AudioSource[(int)Music.MAX_COUNT];
            for(int i = 0; i < Mathf.Min(music.Length, (int)Music.MAX_COUNT); i++){
                newMusic[i] = music[i];
            }
            music = newMusic;
        }
        
        if(effects == null || effects.Length != (int)Effects.MAX_COUNT){
            AudioSource[] newEffects = new AudioSource[(int)Effects.MAX_COUNT];
            for(int i = 0; i < Mathf.Min(effects.Length, (int)Effects.MAX_COUNT); i++){
                newEffects[i] = effects[i];
            }
            effects = newEffects;
        }
    }

    public void PlayMusic(Music musicVal){
        if(music[(int)musicVal] != null){
            //stop the last srack, then play the next
        }
    }
    public void StopMusic(Music musicVal){ //might want to remove the paramter if stopping all music, depends on the usage
        if(music[(int)musicVal] != null){

        }
    }

    public void PlayEffect(Effects effectVal){
        if(effects[(int)effectVal] != null){
            //play effect
        }
    }
    public void StopEffect(Effects effectVal){
        //idk if we'll ever care to stop an effect
    }

}
