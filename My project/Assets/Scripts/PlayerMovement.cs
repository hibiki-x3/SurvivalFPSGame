using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movement")]
    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    [Header("Sprint")]
    public float sprintSpeed = 20f;
    public float sprintFOV = 75f;
    public float normalFOV = 60f;
    public float fovTransitionSpeed = 8f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 25f;   // per second while sprinting
    public float staminaRegenRate = 15f;   // per second while not sprinting
    public float staminaRegenDelay = 1.5f; // seconds before regen starts

    [Header("Ground")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // ── Public read-only state ──────────────────────────────────────
    public float CurrentStamina  => currentStamina;
    public float MaxStamina      => maxStamina;
    public bool  IsSprinting     => isSprinting;

    // ── Private state ───────────────────────────────────────────────
    private Vector3 velocity;
    private bool    isGrounded;
    private bool    isSprinting;
    private float   currentStamina;
    private float   regenDelayTimer;

    // ── Cache ───────────────────────────────────────────────────────
    private Camera playerCamera;

    void Start()
    {
        controller     = GetComponent<CharacterController>();
        currentStamina = maxStamina;

        // Try to find the main camera (child of the player or scene-wide)
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
        }
    }

    void Update()
    {
        // ── Ground check ─────────────────────────────────────────────
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // ── Movement inputs ──────────────────────────────────────────
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // ── Sprint logic ─────────────────────────────────────────────
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isMovingForward = z > 0.1f; // only sprint when moving forward

        if (wantsToSprint && isMovingForward && currentStamina > 0f)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina  = Mathf.Max(currentStamina, 0f);
            regenDelayTimer = staminaRegenDelay; // reset regen delay
        }
        else
        {
            isSprinting = false;

            // Regen after delay
            if (regenDelayTimer > 0f)
            {
                regenDelayTimer -= Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina  = Mathf.Min(currentStamina, maxStamina);
            }
        }

        // ── Apply movement ───────────────────────────────────────────
        float currentSpeed = isSprinting ? sprintSpeed : speed;
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // ── Jump ─────────────────────────────────────────────────────
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ── FOV kick ─────────────────────────────────────────────────
        if (playerCamera != null)
        {
            float targetFOV = isSprinting ? sprintFOV : normalFOV;
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                fovTransitionSpeed * Time.deltaTime
            );
        }

        // ── Notify HUD ───────────────────────────────────────────────
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.SetStamina(currentStamina, maxStamina);
        }
    }
}
