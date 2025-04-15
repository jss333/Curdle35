using UnityEngine;
using DG.Tweening;

public class Reticle : MonoBehaviour
{
    private Tweener scalingTween;

    void Start()
    {
        TurretsManager turretsMngr = TurretsManager.Instance;

        // VFX to scale the reticle down
        transform.localScale = new Vector3(turretsMngr.reticleStartingScale, turretsMngr.reticleStartingScale, 1f);

        scalingTween = transform.DOScale(1f, turretsMngr.reticleDisplayTime)
            .SetEase(turretsMngr.reticleScaleDownEaseType);
    }

    void OnDestroy()
    {
        scalingTween.Kill();
    }
}
