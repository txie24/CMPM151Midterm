using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerOrbInteractor : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject interactionPrompt;

    [Header("settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private OrbParticleToggle currentOrb;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        FindOrb();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(currentOrb != null);
        }

        if (currentOrb != null && WasInteractPressed())
        {
            currentOrb.ToggleOrb();
        }
    }

    private void FindOrb()
    {
        currentOrb = null;

        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactionLayers,
            QueryTriggerInteraction.Ignore
        ))
        {
            currentOrb = hit.collider.GetComponentInParent<OrbParticleToggle>();
        }
    }

    private bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame
        )
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.E))
        {
            return true;
        }
#endif

        return false;
    }
}