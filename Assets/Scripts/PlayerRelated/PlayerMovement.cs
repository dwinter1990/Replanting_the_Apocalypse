using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float groundDrag = 5f;

    [Header("Camera")]
    public Transform cameraTransform; // Cinemachine camera or player head

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
    }

    // Input System callback
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
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

        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
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