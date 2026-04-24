using UnityEngine;

public class CameraFollowNoRotation : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0f, 8f, -10f);
    public Vector3 fixedRotation = new Vector3(45f, 0f, 0f);

    public float followSmoothness = 10f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSmoothness * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}