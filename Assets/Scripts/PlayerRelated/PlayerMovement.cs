using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float groundDrag = 5f;

    [Header("Camera")]
    public CinemachineCamera cam; // Cinemachine camera or player head
    private bool isSprinting = false;
    private Rigidbody rb;
    private Vector2 moveInput;

    [Header("Wifi Range Settings")]
    [SerializeField] private float range = 3f;
    [SerializeField] Transform centrePoint;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = groundDrag;
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
        cam = FindAnyObjectByType<CinemachineCamera>();
    }

    // Input System callback
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveSpeed = 8f; // Increase speed by 50% when sprinting
            isSprinting = true;


        }
        else if (context.canceled)
        {
            moveSpeed = 5f; // Reset to normal speed when not sprinting
            isSprinting = false;

        }
    }

    private void Update()
    {
        if (isSprinting)
        {
            cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, 90f, 1.5f * Time.deltaTime); // Optional: widen FOV for sprinting effect
        }
        else if (!isSprinting)
        {
            cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, 60f, 1.5f * Time.deltaTime); // Reset FOV when not sprinting
        }
    }
    
    private void FixedUpdate()
    {
        Vector3 offset = transform.position - centrePoint.position;
        Vector3 directionFromCentre = offset.normalized;

        // Convert input to movement direction relative to camera
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputDir.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, 0.2f);
            return;
        }

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = camForward * inputDir.z + camRight * inputDir.x;
        moveDir.Normalize();

        Vector3 targetVelocity = moveDir * moveSpeed;
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange.y = 0f; // don’t affect vertical velocity


        if (offset.magnitude >= range && Vector3.Dot(moveDir, directionFromCentre) > 0)
        {
            moveDir = Vector3.ProjectOnPlane(moveDir, directionFromCentre);
        }

        transform.position += moveDir * moveSpeed * Time.fixedDeltaTime;

    }
}