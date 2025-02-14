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
    };
    public enum Effects{
        CatAttack,
        HyenaSpawn,
        HyenaAttack,
        HyenaMovementSound,
        CatClickSound,
        LionMovementSound,
        LionTowerSound,
        JaguarMovementSound,
        UIClickSound,

        CatResourceCollectionSound,
        HyenaResourceCollectionSound,

        HyenaSpawnRateIncreasedSound,
        TowerAttackSound,
        TowerDestroyedSound,

    };

    [SerializeField] private GameObject[] music;
    [SerializeField] private GameObject[] effects;

    public void PlayMusic(Music musicVal){
        if(music[(int)musicVal] != null){

        }
    }
    public void StopMusic(Effects effectsVal){
        if(effects[(int)effectsVal] != null){
            
        }
    }

}
