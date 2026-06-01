using UnityEngine;
using UnityEngine.Events;

public class GeneratorShakeToggle : MonoBehaviour
{
    [Header("generator model")]
    [SerializeField] private Transform visualRoot;

    [Header("shake settings")]
    [SerializeField] private float positionShakeAmount = 0.025f;
    [SerializeField] private float rotationShakeAmount = 1.2f;
    [SerializeField] private float shakeSpeed = 28f;

    public bool IsOn { get; private set; }

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private float noiseSeed;

    private void Awake()
    {
        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        originalLocalPosition = visualRoot.localPosition;
        originalLocalRotation = visualRoot.localRotation;

        noiseSeed = Random.Range(0f, 1000f);

        SetGeneratorState(false, false);
    }

    private void Update()
    {
        if (!IsOn || visualRoot == null)
        {
            return;
        }

        ShakeGenerator();
    }

    public void ToggleGenerator()
    {
        SetGeneratorState(!IsOn, true);
    }

    public void TurnOnGenerator()
    {
        SetGeneratorState(true, true);
    }

    public void TurnOffGenerator()
    {
        SetGeneratorState(false, true);
    }

    private void SetGeneratorState(bool shouldBeOn, bool invokeEvents)
    {
        IsOn = shouldBeOn;

        if (!shouldBeOn)
        {
            ResetGeneratorPosition();
        }

        if (!invokeEvents)
        {
            return;
        }

if (shouldBeOn)
        {
            Debug.Log("generator turned on");
            OSCManager.Instance.SendTrigger("/sfx/machine");
            OSCManager.Instance.EvaluateMusicIntensity(); 
        }
        else
        {
            Debug.Log("generator turned off");
            OSCManager.Instance.EvaluateMusicIntensity();
        }
    }

    private void ShakeGenerator()
    {
        float time = Time.time * shakeSpeed;

        float xNoise = GetNoise(time, 0f);
        float yNoise = GetNoise(time, 20f);
        float zNoise = GetNoise(time, 40f);

        Vector3 positionOffset = new Vector3(
            xNoise,
            yNoise,
            zNoise
        ) * positionShakeAmount;

        Vector3 rotationOffset = new Vector3(
            zNoise,
            xNoise,
            yNoise
        ) * rotationShakeAmount;

        visualRoot.localPosition =
            originalLocalPosition + positionOffset;

        visualRoot.localRotation =
            originalLocalRotation *
            Quaternion.Euler(rotationOffset);
    }

    private float GetNoise(float time, float offset)
    {
        return (
            Mathf.PerlinNoise(noiseSeed + offset, time) - 0.5f
        ) * 2f;
    }

    private void ResetGeneratorPosition()
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.localPosition = originalLocalPosition;
        visualRoot.localRotation = originalLocalRotation;
    }

    private void OnDisable()
    {
        ResetGeneratorPosition();
    }
}