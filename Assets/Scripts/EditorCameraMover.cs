using UnityEngine;

#if UNITY_EDITOR   // Only runs in the Editor, ignored on phones
public class EditorCameraMover : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float lookSpeed = 2f;
    public bool enableMouseLook = true;

    private float rotX = 0f;
    private float rotY = 0f;

    void Start() 
    {
        Vector3 e = transform.localEulerAngles;
        rotX = e.x;
        rotY = e.y;
    }

    CharacterController controller;

void Awake()
{
    controller = GetComponent<CharacterController>();
}

void Update()
{
    // Move with WASD
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");

    Vector3 move = (transform.forward * v + transform.right * h) * moveSpeed;

    if (controller != null)
        controller.Move(move * Time.deltaTime);
    else
        transform.position += move * Time.deltaTime;

    // Look with mouse
    if (enableMouseLook)
    {
        rotX -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotY += Input.GetAxis("Mouse X") * lookSpeed;

        rotX = Mathf.Clamp(rotX, -70f, 70f);
        transform.localRotation = Quaternion.Euler(rotX, rotY, 0f);
    }
}

}
#endif