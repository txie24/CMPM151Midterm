using UnityEngine;
using UnityEngine.InputSystem;

public class PortalZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Transform playerTransform;
    [SerializeField] private GameObject portalInteractionPrompt; // <-- Drag your Canvas prompt here!

    private SphereCollider zoneCollider;
    private bool playerInside = false;

    private void Start()
    {
        zoneCollider = GetComponent<SphereCollider>();

        // Ensure the prompt is hidden when the game starts
        if (portalInteractionPrompt != null)
        {
            portalInteractionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInside && playerTransform != null)
        {
            // 1. Proximity Hum Logic (Continuous)
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            float maxRadius = zoneCollider.radius * transform.lossyScale.x;
            float normalizedDistance = 1f - Mathf.Clamp01(distance / maxRadius);
            OSCManager.Instance.SendFloat("/portalDistance", normalizedDistance);

            // 2. Interaction Logic (Press E to Pulse)
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("Portal Activated!");
                OSCManager.Instance.SendTrigger("/sfx/portal");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            // Show the UI prompt when the player gets close
            if (portalInteractionPrompt != null)
            {
                portalInteractionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            // Hide the UI prompt when leaving
            if (portalInteractionPrompt != null)
            {
                portalInteractionPrompt.SetActive(false);
            }

            // Send 0.0 when leaving so the distance hum cuts out cleanly
            OSCManager.Instance.SendFloat("/portalDistance", 0f);
        }
    }

    // Safety Net: Cuts the sound if the object is destroyed or the scene changes while you are inside
    private void OnDisable()
    {
        if (OSCManager.Instance != null)
        {
            OSCManager.Instance.SendFloat("/portalDistance", 0f);
        }
    }
}