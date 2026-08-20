using UnityEngine;
using UnityEngine.InputSystem;

public enum KnockbackEndSpeed
{
    Stop,
    NormalMovementSpeed
}

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement current; // making the player a singleton for easy reference. if anyone wants to change this thats ok just talk with foonji on discord first

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

    [SerializeField] private Collider2D parryHitbox;

    [Min(0f)]
    [SerializeField] private float parryHitboxDashTimeReduction = 0.05f;

    [Header("Iframes after parry")]
    [SerializeField] private float iFrameDuration = 0.25f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("Iframes after damage")]
    [SerializeField] private float hitIFrameDuration = 0.25f;

    [SerializeField] private KnockbackEndSpeed knockbackEndSpeed = KnockbackEndSpeed.Stop;

    public bool IsDashing => dashTimeRemaining > 0f;

    public bool IsParrying =>
        parryTimeRemaining > 0f && !hasParriedThisDash;
    
    public bool IsKnockingBack => knockbackTimeRemaining > 0f;

    public bool IsInvincible =>
        iFrameTimeRemaining > 0f || IsKnockingBack;

    private Vector2 lastMoveDirection = Vector2.down;
    private Vector2 dashDirection;
    private Vector2 currentMoveDirection;
    private Vector2 knockbackDirection;
    private Vector3 parryHitboxRestLocalPosition;
    private Vector3 parryHitboxDashStartPosition;
    private float parryHitboxDashElapsed;

    //also for testing
    private SpriteRenderer playerSprite;
    private Color normalColor;

    private float parryTimeRemaining;
    private float iFrameTimeRemaining;
    private float knockbackTimeRemaining;
    private float knockbackElapsed;
    private float knockbackStartSpeed;
    private float dashTimeRemaining;
    private float dashElapsed;
    private float dashTravelled;
    private float dashCooldownTimer;
    private float dashBufferTimer; 
    private bool hasParriedThisDash;

    private void Awake()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        normalColor = playerSprite.color;

        parryHitboxRestLocalPosition = parryHitbox.transform.localPosition;
        parryHitbox.enabled = false;

        Debug.Log("setting the singleton");
        current = this;
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

        if (iFrameTimeRemaining > 0f)
            iFrameTimeRemaining = Mathf.Max(0f, iFrameTimeRemaining - deltaTime);

        if (dashBufferTimer > 0f)
            dashBufferTimer = Mathf.Max(0f, dashBufferTimer - deltaTime);

        Vector2 movementInput = movementAction.action.ReadValue<Vector2>();

        if (movementInput.sqrMagnitude > 1f)
            movementInput.Normalize();

        currentMoveDirection = movementInput.sqrMagnitude > 0.001f
            ? movementInput.normalized
            : Vector2.zero;

        if (currentMoveDirection.sqrMagnitude > 0f)
            lastMoveDirection = currentMoveDirection;

        if (!IsDashing && !IsKnockingBack &&
            dashBufferTimer > 0f && dashCooldownTimer <= 0f)
        {
            StartDash();
        }

        if (IsKnockingBack)
            UpdateKnockback(deltaTime);
        else if (IsDashing)
            UpdateDash(deltaTime);
        else
            UpdateMovement(movementInput, deltaTime);

        if (!IsDashing || !IsParrying)
            ResetParryHitboxPosition();

        parryHitbox.enabled = IsParrying;

        //TESTING
        Color displayColor = parryTimeRemaining > 0f ? parryColor : normalColor;
        displayColor.a = IsInvincible ? normalColor.a * 0.5f : normalColor.a;
        playerSprite.color = displayColor;
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
        parryHitboxDashElapsed = 0f;
        parryHitboxDashStartPosition = parryHitbox.transform.position;
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

        UpdateParryHitboxDash(deltaTime);

        dashTravelled = targetDistance;
        dashTimeRemaining = dashDuration - dashElapsed;

        if (dashTimeRemaining <= 0f)
            dashTimeRemaining = 0f;
    }

    private void UpdateParryHitboxDash(float deltaTime)
    {
        if (parryHitbox == null || !IsParrying)
            return;

        float hitboxDashDuration = Mathf.Max(
            0.01f,
            dashDuration - parryHitboxDashTimeReduction
        );

        parryHitboxDashElapsed = Mathf.Min(
            parryHitboxDashElapsed + deltaTime,
            hitboxDashDuration
        );

        float hitboxDistance = GetDashDistanceAt(
            parryHitboxDashElapsed / hitboxDashDuration
        );

        parryHitbox.transform.position =
            parryHitboxDashStartPosition +
            (Vector3)(dashDirection * hitboxDistance);
    }

    private void ResetParryHitboxPosition()
    {
        if (parryHitbox != null)
            parryHitbox.transform.localPosition = parryHitboxRestLocalPosition;
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

        parryTimeRemaining = 0f;

        parryHitbox.enabled = false;

        ResetParryHitboxPosition();

        dashCooldownTimer = 0f;

        StartIFrames(iFrameDuration);
        StartKnockback(-currentMoveDirection);

        return true;
    }

    public void StartIFrames(float duration)
    {
        iFrameTimeRemaining = Mathf.Max(
            iFrameTimeRemaining,
            Mathf.Max(0f, duration)
        );
    }

    private float GetCurrentDashSpeed()
    {
        if (!IsDashing)
            return movementSpeed;

        float progress = dashElapsed / dashDuration;
        float peakSpeed = (2f * dashDistance / dashDuration) - movementSpeed;

        float curveValue = progress <= dashPeakTime
            ? progress / dashPeakTime
            : (1f - progress) / (1f - dashPeakTime);

        return Mathf.Lerp(movementSpeed, peakSpeed, curveValue);
    }

    private void StartKnockback(Vector2 direction)
    {
        knockbackDirection = direction.sqrMagnitude > 0f
            ? direction.normalized
            : Vector2.down;

        knockbackStartSpeed = GetCurrentDashSpeed();

        dashTimeRemaining = 0f;
        dashBufferTimer = 0f;

        knockbackElapsed = 0f;
        knockbackTimeRemaining = Mathf.Max(0.01f, knockbackDuration);
    }

    private void UpdateKnockback(float deltaTime)
    {
        float duration = Mathf.Max(0.01f, knockbackDuration);
        float progress = Mathf.Clamp01(knockbackElapsed / duration);

        float endSpeed = knockbackEndSpeed == KnockbackEndSpeed.NormalMovementSpeed
            ? movementSpeed
            : 0f;

        float speed = Mathf.Lerp(knockbackStartSpeed, endSpeed, progress);

        transform.position += (Vector3)(knockbackDirection * speed * deltaTime);

        knockbackElapsed += deltaTime;
        knockbackTimeRemaining = Mathf.Max(0f, duration - knockbackElapsed);
    }

    //if the player sits on a bullet they keep taking damage
    private void OnTriggerStay2D(Collider2D other)
    {
        // update this.
        
        // if (other.CompareTag("Bullet"))
        //     Damaged(other.gameObject);
    }

    private void RemoveBullet(GameObject bullet)
    {
        Projectile projectile = bullet.GetComponent<Projectile>();

        if (projectile != null)
            projectile.Despawn();
        else
            Destroy(bullet);
    }

    public void Damaged(Hazard cause)
    {
        // this will have to be updated. the argument "cause" will hold a public varaible called "parryable". That should be enough?

        // Projectile projectile = projectileObject.GetComponentInParent<Projectile>();

        // // While parrying, parryable bullets are destroyed
        // // Non parryable bullets pass through plauer
        // if (IsParrying)
        // {
        //     if (projectile != null && projectile.IsParryable && TryParry())
        //     {
        //         projectile.Despawn();
        //         Debug.Log("Parried!");
        //     }

        //     return;
        // }

        // if (IsInvincible)
        //     return;

        // StartIFrames(hitIFrameDuration);
        // Debug.Log("youch");
    }
}