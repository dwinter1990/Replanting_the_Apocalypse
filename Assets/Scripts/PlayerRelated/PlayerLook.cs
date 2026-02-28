using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerLook : MonoBehaviour
{
    private Camera mainCam;

    [Header("Look Settings")]
    [Range(0.1f, 100f)]
    [SerializeField] float mouseSensitivity;
    [SerializeField] float yClamp = 80f;

    private Vector2 lookInput;
    private float xRotation = 0f;
    [SerializeField] Rigidbody rb;
    private void Awake()
    {
        mainCam = GetComponentInChildren<Camera>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        HandleLook();
    }

    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -yClamp, yClamp);
        mainCam.transform.localRotation = Quaternion.Euler(xRotation,0,0);

        Quaternion rot = Quaternion.Euler(0f, transform.eulerAngles.y + mouseX, 0f);
        rb.MoveRotation(rot);
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
}
