using UnityEngine;

public class PortalZone : MonoBehaviour
{
    [SerializeField] public Transform playerTransform;
    private SphereCollider zoneCollider;
    private bool playerInside = false;

    private void Start()
    {
        zoneCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (playerInside && playerTransform != null)
        {
            // Calculate distance between player and center of portal
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            // Map distance to a 0.0 (edge of circle) to 1.0 (dead center) scale
            float maxRadius = zoneCollider.radius * transform.lossyScale.x;
            float normalizedDistance = 1f - Mathf.Clamp01(distance / maxRadius);

            // Send to Pure Data
            OSCManager.Instance.SendFloat("/portalDistance", normalizedDistance);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            // Send 0.0 when leaving so the sound cuts out cleanly
            OSCManager.Instance.SendFloat("/portalDistance", 0f);
        }
    }
}