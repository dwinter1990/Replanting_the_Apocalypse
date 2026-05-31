using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement PMInstance { get; private set; }
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float groundDrag = 5f;

    [Header("Jumping")]
    [SerializeField] private float jumpImpulse = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    public bool isGrounded;
    private bool jumpStarted;
    private bool jumpHeld;  

    [Header("Camera")]
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private FirstPersonCameraFeedback cameraFeedback;

    [Header("Wifi Range Settings")]
    [SerializeField] private float range = 3f;
    [SerializeField] private Transform centrePoint;

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private bool _sprintHeld;
    public bool isSprint => _sprintHeld;
    private bool _wasGrounded;

    private bool _hasMoved;
    public bool hasMoved => _hasMoved;
    private bool _hasSprinted;
    public bool hasSprinted => _hasSprinted;
    private bool _hasJumped;
    public bool hasJumped => _hasJumped;
    private void Awake()
    {
        PMInstance = this;

        _rb = GetComponent<Rigidbody>();
        _rb.linearDamping = groundDrag;
        _rb.freezeRotation = true;

        if (cam == null)
        {
            cam = FindAnyObjectByType<CinemachineCamera>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!_hasMoved)
        {
            _hasMoved = true;
        }
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            _sprintHeld = true;

            if (!_hasSprinted)
            {
                _hasSprinted = true;
            }
        }
        else if (context.canceled)
        {
            _sprintHeld = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
        {
            if (!_hasJumped)
            {
                _hasJumped = true;
            }
            jumpStarted = true;
        }
        

        if (context.performed && PlayerStats.PSInstance.currentPower > 0f)
        {
            jumpHeld = true;
        }

        if (context.canceled)
        {
            jumpHeld = false;
            return;
        }
    }

    private void Update()
    {
        isGrounded = CheckGroundedNow();

        if (isGrounded && !_wasGrounded)
        {
            cameraFeedback?.NotifyLand();
        }

        _wasGrounded = isGrounded;

        bool hasMoveInput = _moveInput.sqrMagnitude > 0.01f;
        bool isSprinting = _sprintHeld && hasMoveInput && isGrounded;
        cameraFeedback?.SetMovementState(hasMoveInput, isSprinting);

        if(isGrounded)
        {
            PlayerStats.PSInstance.StartCoroutine(PlayerStats.PSInstance.PowerRecharge());
        } 
        else
        {
            PlayerStats.PSInstance.StopCoroutine(PlayerStats.PSInstance.PowerRecharge());
        }
    }

    private void FixedUpdate()
    {
        isGrounded = CheckGroundedNow();

        Vector3 inputDir = new Vector3(_moveInput.x, 0f, _moveInput.y);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        Vector3 camForward = cam != null ? cam.transform.forward : transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam != null ? cam.transform.right : transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

        if (centrePoint != null)
        {
            Vector3 offset = transform.position - centrePoint.position;
            Vector3 directionFromCentre = offset.normalized;

            if (offset.magnitude >= range && Vector3.Dot(moveDir, directionFromCentre) > 0f)
            {
                moveDir = Vector3.ProjectOnPlane(moveDir, directionFromCentre).normalized;
            }
        }

        if (jumpStarted)
        {
            cameraFeedback?.NotifyJump();

            Vector3 velocity = _rb.linearVelocity;
            velocity.y = 0f;
            _rb.linearVelocity = velocity;
            _rb.AddForce(Vector3.up * jumpImpulse * PlayerStats.PSInstance.jumpHeightMultiplier, ForceMode.Impulse);
            //jumpStarted = false;

            Debug.Log("Jump initiated with impulse: " + (jumpImpulse * PlayerStats.PSInstance.jumpHeightMultiplier).ToString("F2"));

        }

        if (jumpHeld && !isGrounded && PlayerStats.PSInstance.currentPower > 0f)
        {
            
            _rb.AddForce(Vector3.up * PlayerStats.PSInstance.jetpackThrust, ForceMode.Acceleration);
            Debug.Log("Applying jetpack thrust: " + PlayerStats.PSInstance.jetpackThrust.ToString("F2"));
            PlayerStats.PSInstance.UsePower(PlayerStats.PSInstance.jetpackFuelConsumptionRate * Time.deltaTime);
        }

        float speed = _sprintHeld && isGrounded ? sprintSpeed : walkSpeed;

        Vector3 targetHorizontalVelocity = moveDir * speed;

        Vector3 current = _rb.linearVelocity;
        Vector3 currentHorizontal = new Vector3(current.x, 0f, current.z);
        Vector3 newHorizontal = Vector3.MoveTowards(currentHorizontal, targetHorizontalVelocity, acceleration * Time.fixedDeltaTime);

        _rb.linearVelocity = new Vector3(newHorizontal.x, current.y, newHorizontal.z);

        _rb.linearDamping = isGrounded && inputDir.sqrMagnitude < 0.01f ? groundDrag : 0f;

        if (isGrounded && _rb.linearVelocity.y <= 0.01f)
        {
            PlayerStats.PSInstance.PowerRecharge();
            jumpStarted = false;
            jumpHeld = false;
        }

    }

    private bool CheckGroundedNow()
    {
        if (groundCheckPoint == null)
        {
            return false;
        }

        return Physics.CheckSphere(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}
