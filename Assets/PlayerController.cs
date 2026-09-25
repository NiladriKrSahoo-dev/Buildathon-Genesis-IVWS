 using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Static inventory properties with aliases for backwards and instance compatibility
    public static bool hasTapeStatic = false;
    public static bool hasTape1Static = false;
    public static bool hasTape2Static = false;
    public static bool hasKeyStatic = false;

    public static bool hasTape
    {
        get => hasTapeStatic || hasTape1Static || hasTape2Static;
        set 
        {
            hasTapeStatic = value;
            if (!value)
            {
                hasTape1Static = false;
                hasTape2Static = false;
            }
        }
    }

    public static bool hasTape1
    {
        get => hasTape1Static;
        set => hasTape1Static = value;
    }

    public static bool hasTape2
    {
        get => hasTape2Static;
        set => hasTape2Static = value;
    }

    public static bool hasKey
    {
        get => hasKeyStatic;
        set => hasKeyStatic = value;
    }

    // Instance accessors for scripts calling playerInstance.hasKey or playerInstance.hasTape
    public bool HasTape => hasTape;
    public bool HasKey => hasKeyStatic;

    [Header("Movement Settings")]
    public float walkSpeed = 15f; 
    public float gravity = -15f;

    [Header("Camera & Interaction")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;
    public float interactRange = 5f; 

    private CharacterController characterController;
    private Vector3 verticalVelocity;
    private float xRotation = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) playerCamera = cam.transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleInteraction();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // PRESENTATION BYPASSES: Press these if clicking fails during the live demo
        if (Input.GetKeyDown(KeyCode.T))
        {
            hasTapeStatic = true;
            Debug.Log("DEMO BYPASS: Tape Acquired!");
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            hasKeyStatic = true;
            Debug.Log("DEMO BYPASS: Key Acquired!");
        }
#endif
    }

    private void HandleInteraction()
    {
        // Allow interaction via E key OR Left Mouse Click
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) && playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                // Use GetComponentInParent so clicking child mesh colliders on 3D models triggers interaction
                Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable != null) interactable.Interact();
            }
        }
    }

    private void HandleMovement()
    {
        if (characterController == null) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Clamp diagonal input vector magnitude to 1 to prevent diagonal speed boost
        Vector3 moveInput = transform.right * x + transform.forward * z;
        moveInput = Vector3.ClampMagnitude(moveInput, 1f);

        // Ground check and vertical velocity calculation
        if (characterController.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += gravity * Time.deltaTime;

        // Combine horizontal movement and gravity into a SINGLE characterController.Move call
        Vector3 finalVelocity = (moveInput * walkSpeed) + verticalVelocity;
        characterController.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f); 
        transform.Rotate(Vector3.up * mouseX); 
    }
}