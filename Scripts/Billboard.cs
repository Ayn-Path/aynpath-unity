using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (!Camera.main) return;

        Vector3 camPos = Camera.main.transform.position;
        Vector3 dir = camPos - transform.position;
        dir.y = 0f;                 // keep upright
        transform.rotation = Quaternion.LookRotation(-dir);
    }
}
