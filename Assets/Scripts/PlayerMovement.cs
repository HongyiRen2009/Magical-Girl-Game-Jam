using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]

    [SerializeField] private InputActionReference movementAction;

    [SerializeField] private InputActionReference dashAction;

    [Header("Movement")]

    [SerializeField] private float movementSpeed = 12f;

    [Header("Dash")]
    //duration and distance determine speed
    [SerializeField] private float dashDistance = 4f;

    [SerializeField] private float dashDuration = 0.3f;

    [SerializeField] private float dashCooldown = 0.55f;

    //this lets you press dash x time before its available
    [SerializeField] private float dashBufferDuration = 0.12f;
    
    //time after dash that parry lasts
    [SerializeField] private float parryBuffer = 0f;

    //% of progress throught the dash that the player is fastest
    [Range(0.05f, 0.95f)]
    [SerializeField] private float dashPeakTime = 0.4f;
    //test
    [SerializeField] private Color parryColor = new Color(0.85f, 0.65f, 1f, 1f);

    public bool IsDashing => dashTimeRemaining > 0f;

    public bool IsParrying =>
        parryTimeRemaining > 0f && !hasParriedThisDash;

    private Vector2 lastMoveDirection = Vector2.down;
    private Vector2 dashDirection;

    //also for testing
    private SpriteRenderer playerSprite;
    private Color normalColor;

    private float parryTimeRemaining;
    private float dashTimeRemaining;
    private float dashElapsed;
    private float dashTravelled;
    private float dashCooldownTimer;
    private float dashBufferTimer; 
    private bool hasParriedThisDash;

    private void Awake()
    {
        //testicles
        playerSprite = GetComponent<SpriteRenderer>();
        normalColor = playerSprite.color;
    }

    private void OnEnable()
    {
        movementAction.action.Enable();
        dashAction.action.Enable();
        dashAction.action.performed += QueueDash;
    }

    private void OnDisable()
    {
        dashAction.action.performed -= QueueDash;
        movementAction.action.Disable();
        dashAction.action.Disable();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (dashCooldownTimer > 0f)
            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - deltaTime);

        if (parryTimeRemaining > 0f)
            parryTimeRemaining = Mathf.Max(0f, parryTimeRemaining - deltaTime);

        if (dashBufferTimer > 0f)
            dashBufferTimer = Mathf.Max(0f, dashBufferTimer - deltaTime);

        Vector2 movementInput = movementAction.action.ReadValue<Vector2>();

        if (movementInput.sqrMagnitude > 1f)
            movementInput.Normalize();

        if (movementInput.sqrMagnitude > 0.001f)
            lastMoveDirection = movementInput.normalized;

        if (!IsDashing && dashBufferTimer > 0f && dashCooldownTimer <= 0f)
            StartDash();

        if (IsDashing)
            UpdateDash(deltaTime);
        else
            UpdateMovement(movementInput, deltaTime);

        //TESTING
        playerSprite.color = parryTimeRemaining > 0f ? parryColor : normalColor;
    }

    private void UpdateMovement(Vector2 movementInput, float deltaTime)
    {
        transform.position += (Vector3)(movementInput * movementSpeed * deltaTime);
    }

    private void QueueDash(InputAction.CallbackContext _)
    {
        dashBufferTimer = dashBufferDuration;
    }

    private void StartDash()
    {
        dashBufferTimer = 0f;
        dashCooldownTimer = dashCooldown;

        dashDirection = lastMoveDirection.normalized;
        dashTimeRemaining = dashDuration;
        dashElapsed = 0f;
        dashTravelled = 0f;
        parryTimeRemaining = Mathf.Max(0f, dashDuration + parryBuffer);
        hasParriedThisDash = false;
    }

    private void UpdateDash(float deltaTime)
    {
        dashElapsed = Mathf.Min(dashElapsed + deltaTime, dashDuration);

        float targetDistance = GetDashDistanceAt(dashElapsed / dashDuration);
        float distanceThisFrame = targetDistance - dashTravelled;
        //this lets the dashes follow a curve-like acceleration and deceleration for speed
        transform.position += (Vector3)(dashDirection * distanceThisFrame);

        dashTravelled = targetDistance;
        dashTimeRemaining = dashDuration - dashElapsed;

        if (dashTimeRemaining <= 0f)
            dashTimeRemaining = 0f;
    }
    //this is an attempt to copy the just shapes and beats dashes
    private float GetDashDistanceAt(float progress)
    {
        progress = Mathf.Clamp01(progress);

        float peakSpeed = (2f * dashDistance / dashDuration) - movementSpeed;

        float curveArea;

        if (progress <= dashPeakTime)
        {
            curveArea = (progress * progress) / (2f * dashPeakTime);
        }
        else
        {
            float timeAfterPeak = progress - dashPeakTime;

            curveArea =
                (dashPeakTime * 0.5f) +
                timeAfterPeak -
                (timeAfterPeak * timeAfterPeak) /
                (2f * (1f - dashPeakTime));
        }

        return dashDuration *
            (movementSpeed * progress + (peakSpeed - movementSpeed) * curveArea);
    }

    // enemy projectiles call this
    public bool TryParry()
    {
        if (!IsParrying)
            return false;

        hasParriedThisDash = true;
        dashCooldownTimer = 0f; //dash cd reset, feel free to remove this
        return true;
    }
    //i added this to prevent errors but it got really fucking annoying but if you guys want it you can have it
    // private void OnValidate()
    // {
    //     movementSpeed = Mathf.Max(0f, movementSpeed);
    //     dashDistance = Mathf.Max(0.01f, dashDistance);
    //     dashDuration = Mathf.Max(0.01f, dashDuration);
    //     dashCooldown = Mathf.Max(0f, dashCooldown);
    //     dashBufferDuration = Mathf.Max(0f, dashBufferDuration);
    //     dashPeakTime = Mathf.Clamp(dashPeakTime, 0.05f, 0.95f);
    // }

    public void Damaged(GameObject projectile)
    {
        Debug.Log("I have been hit!!!!");
    }
}