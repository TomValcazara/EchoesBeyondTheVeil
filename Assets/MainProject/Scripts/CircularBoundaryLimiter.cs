using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CircularBoundaryLimiter : MonoBehaviour
{
    public Transform centerPoint;
    public float maxRadius = 100f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        Vector3 center = centerPoint.position;

        Vector3 flatPlayerPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatCenter = new Vector3(center.x, 0, center.z);

        float distance = Vector3.Distance(flatPlayerPos, flatCenter);

        if (distance > maxRadius)
        {
            Vector3 direction = (flatPlayerPos - flatCenter).normalized;
            Vector3 clampedPosition = flatCenter + direction * maxRadius;

            Vector3 newPosition = new Vector3(
                clampedPosition.x,
                transform.position.y,
                clampedPosition.z
            );

            controller.enabled = false;  // temporarily disable to avoid conflicts
            transform.position = newPosition;
            controller.enabled = true;
        }
    }
}
