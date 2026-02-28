using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -19.81f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private float deadzone = 0.01f;
    [SerializeField] private float eyeHeightOffset = 0.6f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _rotationX = 0f; // pitch
    private float _rotationY = 0f; // yaw
    private Keyboard _kb;
    private Mouse _mouse;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
        
        if (cameraTransform != null)
            cameraTransform.localPosition = new Vector3(0f, eyeHeightOffset, 0f);
        
        _kb = Keyboard.current;
        _mouse = Mouse.current;
        if (Cursor.lockState != CursorLockMode.Locked) LockCursor();
    }

    void Update()
    {
        if (_controller == null) return;
        RefreshInput();
        HandleMovement();
        HandleLook();
    }

    private void RefreshInput()
    {
        if (_kb == null) _kb = Keyboard.current;
        if (_mouse == null) _mouse = Mouse.current;
    }

    private void HandleMovement()
    {
        if (_kb == null)
        {
            Debug.Log("Keyboard is null!");
            return;
        }
        
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;

        float h = (_kb.dKey.isPressed || _kb.rightArrowKey.isPressed ? 1f : 0f)
                - (_kb.aKey.isPressed || _kb.leftArrowKey.isPressed ? 1f : 0f);
        float v = (_kb.wKey.isPressed || _kb.upArrowKey.isPressed ? 1f : 0f)
                - (_kb.sKey.isPressed || _kb.downArrowKey.isPressed ? 1f : 0f);

        if (h != 0 || v != 0)
            Debug.Log($"Input: h={h}, v={v}");

        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;
        float speed = (_kb.leftShiftKey.isPressed ? sprintSpeed : moveSpeed);
        if (_kb.spaceKey.wasPressedThisFrame && _isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(moveDir * speed * Time.deltaTime + _velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (cameraTransform == null || _mouse == null) return;
        Vector2 delta = _mouse.delta.ReadValue();
        if (Mathf.Abs(delta.x) < deadzone && Mathf.Abs(delta.y) < deadzone) return;
        _rotationY += delta.x * mouseSensitivity;
        _rotationX -= delta.y * mouseSensitivity;
        _rotationX = Mathf.Clamp(_rotationX, -maxLookAngle, maxLookAngle);
        transform.localRotation = Quaternion.Euler(0f, _rotationY, 0f);
        cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
