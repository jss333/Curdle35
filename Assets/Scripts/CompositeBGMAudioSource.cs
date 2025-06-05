using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeBGMAudioSource : MonoBehaviour, IPlayableBGM
{
    [Header("Config")]
    [SerializeField] public bool randomOrder = false;

    [Header("State")]
    [SerializeField] private bool initialized = false;
    [SerializeField] private int currentIndex = -1;
    private List<IPlayableBGM> componentBGMs = new();

    void Awake()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out BGMAudioSource source))
            {
                componentBGMs.Add(source);
            }
        }

        initialized = true;
    }

    public void Play()
    {
        if (randomOrder)
        {
            currentIndex = Random.Range(0, componentBGMs.Count);
        }
        else
        {
            currentIndex = (currentIndex + 1) % componentBGMs.Count;
        }

        componentBGMs[currentIndex].Play();
    }

    public void StopOrPause()
    {
        componentBGMs[currentIndex].StopOrPause();
    }

    public void ScaleVolume(float masterVolume)
    {
        componentBGMs[currentIndex].ScaleVolume(masterVolume);
    }
}
