using UnityEngine;
using UnityEngine.InputSystem;

public class MovementScript : MonoBehaviour
{
    public float mouseSensitivity = 500f;

    //Change degree number in Unity Editor for ease of use
    public float topClamp = -90f;
    public float bottomClamp = 90f;

    [Header("Gun Feel")]
    [SerializeField] private float recoilRecoverySpeed = 10f;

    float xRotation = 0f;
    float yRotation = 0f;
    private Vector2 recoilOffset;

    public static MovementScript Instance { get; private set; }

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        //Locking the cursor to the middle of the screen and making it invisible
        Cursor.lockState = CursorLockMode.Locked;

    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        recoilOffset = Vector2.Lerp(recoilOffset, Vector2.zero, recoilRecoverySpeed * Time.deltaTime);

        //Getting mouse inputs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //Look up and down
        xRotation -= mouseY;
        xRotation -= recoilOffset.x;

        //Stops camera from looking up/down past set degrees
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);

        //Look left and right
        yRotation += mouseX;
        yRotation += recoilOffset.y;

        //Apply rotations to transform
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

    }

    public void ApplyRecoilKick(float pitchKick, float yawKick)
    {
        recoilOffset += new Vector2(pitchKick, yawKick);
    }
}
