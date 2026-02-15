using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        Vector3 direction = transform.position - cam.position;
        direction.y = 0f; // Keep upright (no tilt)

        transform.rotation = Quaternion.LookRotation(direction);
    }
}