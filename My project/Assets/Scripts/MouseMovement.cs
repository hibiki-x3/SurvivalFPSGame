using UnityEngine;
using UnityEngine.InputSystem;

public class MovementScript : MonoBehaviour
{
    public float mouseSensitivity = 500f;

    //Change degree number in Unity Editor for ease of use
    public float topClamp = -90f;
    public float bottomClamp = 90f;

    float xRotation = 0f;
    float yRotation = 0f;
    void Start()
    {
        //Locking the cursor to the middle of the screen and making it invisible
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Update()
    {
        //Getting mouse inputs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //Look up and down
        xRotation -= mouseY;

        //Stops camera from looking up/down past set degrees
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);

        //Look left and right
        yRotation += mouseX;

        //Apply rotations to transform
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

    }
}
