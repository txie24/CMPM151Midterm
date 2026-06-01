using UnityEngine;
using UnityEngine.Events;

public class OrbParticleToggle : MonoBehaviour
{
    [Header("particle systems")]
    [SerializeField] private ParticleSystem[] particleSystems;

    [Header("optional visuals")]
    [SerializeField] private GameObject[] visualsToToggle;

    public bool IsOn { get; private set; }

    private void Awake()
    {
        if (particleSystems == null || particleSystems.Length == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem == null)
            {
                continue;
            }

            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
        }

        SetOrbState(false, false);
    }

    public void ToggleOrb()
    {
        SetOrbState(!IsOn, true);
    }

    public void TurnOnOrb()
    {
        SetOrbState(true, true);
    }

    public void TurnOffOrb()
    {
        SetOrbState(false, true);
    }

    private void SetOrbState(bool shouldBeOn, bool invokeEvents)
    {
        IsOn = shouldBeOn;

        if (shouldBeOn)
        {
            SetVisualsActive(true);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem != null)
                {
                    particleSystem.Play(true);
                }
            }
        }
        else
        {
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem != null)
                {
                    particleSystem.Stop(
                        true,
                        ParticleSystemStopBehavior.StopEmittingAndClear
                    );
                }
            }

            SetVisualsActive(false);
        }

        if (!invokeEvents)
        {
            return;
        }

if (shouldBeOn)
        {
            Debug.Log("orb turned on");
            OSCManager.Instance.SendTrigger("/sfx/crystal");
            OSCManager.Instance.EvaluateMusicIntensity();
            
        }
        else
        {
            Debug.Log("orb turned off");
            OSCManager.Instance.EvaluateMusicIntensity();
        }
    }

    private void SetVisualsActive(bool active)
    {
        foreach (GameObject visual in visualsToToggle)
        {
            if (visual != null)
            {
                visual.SetActive(active);
            }
        }
    }
}