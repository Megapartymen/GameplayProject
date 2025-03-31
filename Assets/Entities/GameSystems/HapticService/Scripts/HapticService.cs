using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;
using Zenject;

public class HapticService : MonoBehaviour, IGlobalInitializable
{
    //min amplitude - 0.004
    //min duration - 0.001

    #region InjectServices-----------------------------------------------------------------------------------------

    private VRInputSystem _vrInputSystem;

    [Inject]
    private void Construct(VRInputSystem vrInputSystem)
    {
        _vrInputSystem = vrInputSystem;
    }

    #endregion

    #region HapticEffects-----------------------------------------------------------------------------------------

    public void OnInitialize()
    {
    }
    
    public void PlayExplosion(ActionBasedController controller)
    {
    }

    public void PlayLoadingHit()
    {
        _vrInputSystem.SendHapticImpulse(0.1f, 0.1f, VRController.Left);
        _vrInputSystem.SendHapticImpulse(0.1f, 0.1f, VRController.Right);
    }


    public void PlayTouch(VRController vrController, ActionBasedController controller = null)
    {
        if (controller != null)
        {
            _vrInputSystem.SendHapticImpulse(0.1f, 0.08f, controller);
        }
        else
        {
            _vrInputSystem.SendHapticImpulse(0.1f, 0.08f, vrController);
        }
    }

    public void PlayApplyEffect(VRController vrController, ActionBasedController controller = null)
    {
        if (controller != null)
        {
            Sequence applyEffect = DOTween.Sequence();
            applyEffect
                .AppendCallback(() => _vrInputSystem.SendHapticImpulse(0.02f, 0.02f, controller))
                .AppendInterval(0.1f)
                .AppendCallback(() => _vrInputSystem.SendHapticImpulse(0.02f, 0.02f, controller));
        }
        else
        {
            Sequence applyEffect = DOTween.Sequence();
            applyEffect
                .AppendCallback(() => _vrInputSystem.SendHapticImpulse(0.02f, 0.02f, vrController))
                .AppendInterval(0.1f)
                .AppendCallback(() => _vrInputSystem.SendHapticImpulse(0.02f, 0.02f, vrController));
        }
    }

    public void PlayWatchNotificationEffect(VRController vrController, ActionBasedController controller = null)
    {
        if (controller != null)
        {
            Sequence applyEffect = DOTween.Sequence();
            applyEffect
                .InsertCallback(0.1f, () => _vrInputSystem.SendHapticImpulse(0.4f, 0.1f, vrController))
                .InsertCallback(0.3f, () => _vrInputSystem.SendHapticImpulse(0.4f, 0.1f, vrController))
                .InsertCallback(0.5f, () => _vrInputSystem.SendHapticImpulse(0.4f, 0.1f, vrController))
                .InsertCallback(0.8f, () => _vrInputSystem.SendHapticImpulse(0.1f, 0.05f, vrController))
                .InsertCallback(1.1f, () => _vrInputSystem.SendHapticImpulse(0.1f, 0.05f, vrController));
        }
        else
        {
            Sequence applyEffect = DOTween.Sequence();
            applyEffect
                .InsertCallback(0.1f, () => _vrInputSystem.SendHapticImpulse(0.4f, 0.1f, vrController))
                .InsertCallback(0.3f, () => _vrInputSystem.SendHapticImpulse(0.4f, 0.1f, vrController))
                .InsertCallback(0.5f, () => _vrInputSystem.SendHapticImpulse(0.4f, 0.1f, vrController))
                .InsertCallback(0.8f, () => _vrInputSystem.SendHapticImpulse(0.1f, 0.05f, vrController))
                .InsertCallback(1.1f, () => _vrInputSystem.SendHapticImpulse(0.1f, 0.05f, vrController));
        }
    }


    public void PlayShoot(ActionBasedController controller)
    {
    }

    public void PlayHit(VRController vrController, float duration)
    {
        _vrInputSystem.SendHapticImpulse(0.15f, duration, vrController);
    }

    public void PlayHit(ActionBasedController controller, float duration)
    {
        _vrInputSystem.SendHapticImpulse(0.15f, duration, controller);
    }

    public void PlayHitDelayed(VRController vrController)
    {
        _vrInputSystem.SendHapticImpulse(0.15f, 0.7f, vrController);
    }

    public void PlayHitDelayed(ActionBasedController controller)
    {
        _vrInputSystem.SendHapticImpulse(0.15f, 0.7f, controller);
    }

    public void PlayLightImpact(VRController vrController, float amplitudeMultiplier = 1.0f)
    {
        _vrInputSystem.SendHapticImpulse(0.01f * amplitudeMultiplier, 0.01f, vrController);
    }

    public void PlayMediumImpact(VRController vrController, float amplitudeMultiplier = 1.0f)
    {
        _vrInputSystem.SendHapticImpulse(0.05f * amplitudeMultiplier, 0.1f, vrController);
    }

    public void PlayHeavyImpact(VRController vrController, float amplitudeMultiplier = 1.0f)
    {
        _vrInputSystem.SendHapticImpulse(0.15f * amplitudeMultiplier, 0.1f, vrController);
    }

    #endregion
}