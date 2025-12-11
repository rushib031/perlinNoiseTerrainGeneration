using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 6f;
    public float mouseSensitivity = 0.07f; 
    public float gravity = -9.81f;

    CharacterController controller;
    Transform cameraTransform;
    
    GameControls inputActions;

    Vector2 moveInput;
    Vector2 lookInput;


    Vector3 velocity;
    float xRotation = 0f;

    void Awake()
    {
        inputActions = new GameControls();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero; 

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }



    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        controller.Move(move * moveSpeed * Time.deltaTime);

        // Gravity Logic
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}