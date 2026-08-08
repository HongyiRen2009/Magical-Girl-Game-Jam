using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference movementAction;
    [SerializeField] private InputActionReference dashAction;
    private Vector2 movementInput;
    [Header("Player Stats")]
    [SerializeField] private float movementSpeed = 5;
    [SerializeField] private float dashCooldown = 2;
    [SerializeField] private float dashDistance = 2;
    private float dashCooldownTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        movementAction.action.Enable();
        dashAction.action.Enable();
    }
    private void OnDisable()
    {
        movementAction.action.Disable();
        dashAction.action.Disable();
    }
    void Start()
    {
        
        dashAction.action.performed += (InputAction.CallbackContext callback) =>
        {
            if (dashCooldownTimer <= 0)
            {
                transform.position += (Vector3)movementInput * dashDistance;
                dashCooldownTimer = dashCooldown;
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        movementInput = movementAction.action.ReadValue<Vector2>();
        transform.position += (Vector3)movementInput.normalized * movementSpeed*Time.deltaTime;
    }
}
