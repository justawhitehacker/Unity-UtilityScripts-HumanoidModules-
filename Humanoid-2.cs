using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public enum HumanoidStateType { 
    Running, 
    Walking, 
    Jumping, 
    Idle, 
    Died,  
    Airborne,
    Grounded,
    Neutral };
[Serializable] public enum HumanoidOwnerType { Player, AI, Neutral };

[Serializable] [RequireComponent(typeof(Rigidbody))]
public class Humanoid : MonoBehaviour 
{ 
    /* Player's properties for humanoid, such as Health, MaxHealth, WalkSpeed, etc.                     *
     * I'll make them all as public variables, because those properties are readable to other classes. */    

     /* Serialized Internal properties */
    #region SerializedReferences

    /* max references */
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxWalkSpeed = 12;
    [SerializeField] private float maxRunningSpeed = 18;
    [SerializeField] private float maxJumpPower = 15;
    [SerializeField] private float maxStamina = 200;
    
    /* humanoid states*/
    /* for stateType, I won't make it serialized, it's belonging to this class action. */
    [SerializeField] private HumanoidOwnerType ownerType = HumanoidOwnerType.Neutral;

    /* physics */
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Vector3 linearVelocity = Vector3.zero;
    [SerializeField] private Vector3 angularVelocity = Vector3.zero;
    [SerializeField] private Vector3 lastMoveDirection = Vector3.forward;
    [SerializeField] private Vector3 facingDirection = Vector3.forward;
    [SerializeField] private Transform rootPart;
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private float maxSlopeAngle = 45.0f;
    [SerializeField] private float walkToStoppingDistance = 0.5f;
    [SerializeField] private Vector3 groundNormal = Vector3.up;
    [SerializeField] private Vector3 cameraOffset = Vector3.zero;
    [SerializeField] private float toGroundHeight = 1.5f;
    [SerializeField] private float safeFromFallDistance = 3.5f;
    [SerializeField] private float fallDamageMultiplier = 10.0f; 

    /* booleans */
    [SerializeField] private bool platformStanding = false;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool autoRotate = true;
    [SerializeField] private bool healthRegenerationEnabled = true;
    [SerializeField] private bool staminaRegenerationEnabled = true;
    [SerializeField] private bool canApplyFallDamage = true;

    /* references */
    [SerializeField] private float health;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float stamina;
    [SerializeField] private float staminaDecrementAmount = 2.0f;
    [SerializeField] private float staminaDecrementTick = 0.2f;
    [SerializeField] private float healthRegenerationAmount = 1.0f;
    [SerializeField] private float healthRegenerationTick = 0.8f;
    [SerializeField] private float healthRegenerationDelay = 3.0f;
    [SerializeField] private float staminaRegenerationAmount = 5.0f;
    [SerializeField] private float staminaRegenerationTick = 0.75f;
    [SerializeField] private float staminaRegenerationDelay = 2.0f;

    #endregion

    #region UnserializedReferences

    /* booleans */
    private bool isAlive;
    private bool isGrounded;
    private bool isMoving;
    private float fallDistance;
    private bool isJumping = false;

    /* owner type */
    private HumanoidStateType stateType = HumanoidStateType.Neutral;

    /* physics */
    private Collider floorCollider;
    private PhysicsMaterial floorMaterial;
    private Vector3 targetPoint;
    private float fallStartY;
    private Vector3 moveDirection;

    #endregion

    #region PrivateHelpers
    /* timers */
    private float lastStaminaUseTime;
    private float staminaRegenTimer;
    private float lastDamagedTime;
    private float healthRegenTimer;

    private float EPS_TIMER = 0.05f;
    #endregion

    #region ReadReferences
    /* Max references */
    public float MaxHealth => maxHealth;
    public float MaxWalkSpeed => maxWalkSpeed;
    public float MaxRunningSpeed => maxRunningSpeed;
    public float MaxJumpPower => maxJumpPower;
    public float MaxStamina => maxStamina;

    /* Player's state */
    public HumanoidStateType StateType => stateType;
    public HumanoidOwnerType OwnerType => ownerType;

    /* Physics */
    public Vector3 LinearVelocity => linearVelocity;
    public Vector3 AngularVelocity => angularVelocity;
    public float VerticalVelocity => linearVelocity.y;
    public float HorizontalSpeed { get { Vector3 vc = linearVelocity; vc.y = 0; return vc.magnitude; } }
    public Vector3 MoveDirection => moveDirection;
    public Vector3 LastMoveDirection => lastMoveDirection;
    public Vector3 FacingDirection => facingDirection;
    public Rigidbody RigidBody => rigidBody;
    public Transform RootPart => rootPart;
    public Collider BodyCollider => bodyCollider;
    public float MaxSlopeAngle => maxSlopeAngle;
    public float WalkToStoppingDistance => walkToStoppingDistance;
    public Vector3 GroundNormal => groundNormal;
    public Collider FloorCollider => floorCollider;
    public PhysicsMaterial FloorMaterial => floorMaterial;
    public Vector3 TargetPoint => targetPoint;
    public float FallStartY => fallStartY;
    public float FallDistance => fallDistance;
    public float SafeFromFallDistance => safeFromFallDistance;
    public float FallDamageMultiplier => fallDamageMultiplier;
    public float ToGroundHeight => toGroundHeight;
    public bool IsOnSlope => Vector3.Angle(groundNormal, Vector3.up) > 1f;
    public float CurrentSlopeAngle => Vector3.Angle(groundNormal, Vector3.up);

    /* References [References = MaxReferences]                         *
     * If [References > MaxReferences] then References = MaxReferences */
    public float Health => health;
    public float WalkSpeed => walkSpeed;
    public float RunningSpeed => runningSpeed;
    public float JumpPower => jumpPower;
    public float Stamina => stamina;
    public float StaminaDecrementAmount => staminaDecrementAmount;
    public float StaminaDecrementTick => staminaDecrementTick;
    public float HealthRegenerationAmount => healthRegenerationAmount;
    public float HealthRegenerationTick => healthRegenerationTick;
    public float HealthRegenerationDelay => healthRegenerationDelay;
    public float StaminaRegenerationAmount => staminaRegenerationAmount;
    public float StaminaRegenerationTick => staminaRegenerationTick;
    public float StaminaRegenerationDelay => staminaRegenerationDelay;
    public Vector3 CameraOffset => cameraOffset;

    /* booleans */
    public bool IsAlive => isAlive;
    public bool IsGrounded => isGrounded;
    public bool IsMoving => isMoving;
    public bool PlatformStanding => platformStanding;
    public bool CanMove => canMove;
    public bool CanJump => canJump;
    public bool AutoRotate => autoRotate;
    public bool IsJumping => isJumping;
    public bool CanApplyFallDamage => canApplyFallDamage;
    public bool HealthRegenerationEnabled => healthRegenerationEnabled;
    public bool StaminaRegenerationEnabled => staminaRegenerationEnabled;

    #endregion


    /* Private members                                             *
     * Used as the internal utilities of this class                */

    #region Actions
    /* Callbacks */
    public event Action Died;
    public event Action Grounded;
    public event Action OnJumping;
    public event Action OnRunning;
    public event Action OnWalking;
    public event Action OnFlying;
    public event Action OnHealthRegenerationEnabledChanged;
    public event Action OnStaminaRegenerationEnabledChanged;

    public event Action<float> Damaged;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action<float, float> OnStaminaDecrementAmountChanged;
    public event Action<float, float> OnStaminaDecrementTickChanged;
    public event Action<float, float> OnHealthRegenerationAmountChanged;
    public event Action<float, float> OnHealthRegenerationTickChanged;
    public event Action<float, float> OnHealthRegenerationDelayChanged;
    public event Action<float, float> OnStaminaRegenerationAmountChanged;
    public event Action<float, float> OnStaminaRegenerationTickChanged;
    public event Action<float, float> OnStaminaRegenerationDelayChanged;
    public event Action<float, float> OnWalkSpeedChanged;
    public event Action<float, float> OnRunningSpeedChanged;
    public event Action<float, float> OnJumpPowerChanged;
    public event Action<float, float> OnMaxSlopeAngleChanged;
    public event Action<float, float> OnSafeFromFallDistance;
    public event Action<float, float> OnFallDamageMultiplierChanged;
    public event Action<float, float> OnWalkToStoppingDistanceChanged;

    public event Action<Vector3, Vector3> OnCameraOffsetChanged;

    public event Action<HumanoidStateType, HumanoidStateType> OnStateChanged;
    public event Action<HumanoidOwnerType, HumanoidOwnerType> OnOwnerChanged;

    public event Action<bool, bool> OnAutoRotateChanged;
    public event Action<bool, bool> OnCanApplyFallDamageChanged;
    public event Action<bool, bool> OnPlatformStandingChanged;
    public event Action<bool, bool> OnCanMoveChanged;
    public event Action<bool, bool> OnCanJumpChanged;

    #endregion

    #region Tables
    private readonly HashSet<string> statuses = new();
    private readonly HashSet<HumanoidStateType> disableStates = new();
    private readonly Dictionary<string, float> temporaryStatuses = new();
    #endregion

    #region Helpers
    private void HandleHealthRegeneration()
    {
        if (!healthRegenerationEnabled || !isAlive)
            return;

        if (Time.time - lastDamagedTime <= healthRegenerationDelay)
            return;
        
        if (health >= maxHealth)
        {
            health = maxHealth;
            return;
        }

        healthRegenTimer += Time.deltaTime;

        if (healthRegenTimer >= healthRegenerationTick)
        {
            SetHumanoidHealth(health + healthRegenerationAmount);
            healthRegenTimer = 0.0f;
        }
    }

    private void HandleFallTracking()
    {
        if (!isGrounded && linearVelocity.y < 0)
        {
            if (stateType != HumanoidStateType.Airborne)
            {
                fallStartY = rootPart.position.y;
                ChangeState(HumanoidStateType.Airborne);

                isGrounded = false;
            }
            fallDistance = fallStartY - rootPart.position.y;
        }

        if (isGrounded && stateType == HumanoidStateType.Airborne)
        {
            ApplyFallDamage();

            ChangeState(HumanoidStateType.Grounded);
            isGrounded = true;
        }
    }

    private void HandleFloorInfo()
    {
        bool oldGrounded = isGrounded;

        if (Physics.Raycast(rootPart.position, Vector3.down, out RaycastHit info, toGroundHeight))
        {
            floorCollider = info.collider;
            floorMaterial = info.collider.sharedMaterial;
            groundNormal = info.normal;
            
            float slopeAngle = Vector3.Angle(info.normal, Vector3.up);
            isGrounded = slopeAngle <= maxSlopeAngle;
        }
        else
        {
            floorCollider = null;
            floorMaterial = null;
            groundNormal = Vector3.up;

            isGrounded = false;
        }

        if (!oldGrounded && isGrounded)
            Grounded?.Invoke();
    }

    private void ApplyFallDamage()
    {
        if (!canApplyFallDamage || !isAlive)
            return;
        
        if (safeFromFallDistance >= fallDistance)
            return;

        float totalDamage = (fallDistance - safeFromFallDistance) * fallDamageMultiplier;
        GiveDamage(totalDamage);
    }

    private void HandleTemporaryStatus()
    {
        foreach (var status in temporaryStatuses)
        {
            if (Time.time > status.Value)
                temporaryStatuses.Remove(status.Key);
        }
    }
    #endregion

    #region Methods
    /* Methods functions */

    // Giving damage to Humanoid, by decreasing the health of the humanoid
    public void GiveDamage(float amount)
    {
        // Checking the value of "damageSize"
        if (amount <= 0)
            return;
        
        if (!isAlive)
            return;

        float oldHealth = health;
        SetHumanoidHealth(health - amount);

        float damageAmount = oldHealth - health;

        if (damageAmount > 0)
            Damaged?.Invoke(damageAmount);
        
        lastDamagedTime = Time.time;
    }

    // Giving some health to Humanoid
    public void Heal(float amount)
    {
        if (!isAlive)
            return;
        
        amount = Mathf.Max(1, amount);
        SetHumanoidHealth(amount + health);
    }

    // Callingback or Revive Humanoid to the world, with gaining some reviveHealthAmount
    public void Revive(float reviveHealthAmount)
    {
        if (isAlive)
            return;

        isAlive = true;
        SetHumanoidHealth(Mathf.Clamp(reviveHealthAmount, 1, maxHealth));
        ChangeState(HumanoidStateType.Idle);
    }

    // Comparing to current stamina and the decrement amount, in order to decrease the current stamina of Humanoid
    public bool TryUsingStamina(float amount)
    {
        if (amount > stamina)
            return false;

        SetHumanoidStamina(stamina - amount);
        lastStaminaUseTime = Time.time;
        return true;
    }

     // Set Humanoid's health to 0
    public void Kill()
    {   
        if (!isAlive)
            return;

        float oldHealth = health;

        isAlive = false;
        health = 0;

        if (oldHealth != health)
            OnHealthChanged?.Invoke(oldHealth, health);

        ChangeState(HumanoidStateType.Died);
        Died?.Invoke();
    }

    /* setters */

    // Set Humanoid's health in amount, up to the MaxHealth
    public void SetHumanoidHealth(float amount)
    {
        float oldHealth = health;
        health = Mathf.Clamp(amount, 0, maxHealth);
            
        if (health != oldHealth)
            OnHealthChanged?.Invoke(oldHealth, health);
            
        if (health <= 0)
            Kill();
    }

    // Set Humanoid's walking speed in amount, up to the MaxWalkSpeed
    public void SetHumanoidWalkSpeed(float amount)
    {
        if (amount > runningSpeed)
            amount = runningSpeed;
        
        float oldWalkSpeed = walkSpeed;
        walkSpeed = Mathf.Clamp(amount, 0, maxWalkSpeed);

        if (oldWalkSpeed != walkSpeed)
            OnWalkSpeedChanged?.Invoke(oldWalkSpeed, walkSpeed);
    }

    // Set Humanoid's running speed in amount, up to MaxRunningSpeed
    public void SetHumanoidRunningSpeed(float amount)
    {
        if (amount < walkSpeed)
            amount = walkSpeed;

        float oldRunningSpeed = runningSpeed;
        runningSpeed = Mathf.Clamp(amount, 0, maxRunningSpeed);

        if (oldRunningSpeed != runningSpeed)
            OnRunningSpeedChanged?.Invoke(oldRunningSpeed, runningSpeed);
    }

    // Set Humanoid's jump power in amount, up to MaxJumpPower
    public void SetHumanoidJumpPower(float amount)
    {
        float oldJumpPower = jumpPower;
        jumpPower = Mathf.Clamp(amount, 0, maxJumpPower);

        if (oldJumpPower != jumpPower)
            OnJumpPowerChanged?.Invoke(oldJumpPower, jumpPower);
    }

    // Set Humanoid's stamina in amount, up to MaxStamina
    public void SetHumanoidStamina(float amount)
    {
        float oldStamina = stamina;
        stamina = Mathf.Clamp(amount, 0, maxStamina);

        if (oldStamina != stamina)
            OnStaminaChanged?.Invoke(oldStamina, stamina);
    }

    // Set Humanoid's stamina decrement amount, up to Stamina itself
    public void SetHumanoidStaminaDecrementAmount(float amount)
    {
        float oldStaminaDecAmount = staminaDecrementAmount;
        staminaDecrementAmount = Mathf.Clamp(amount, 0, stamina);

        if (oldStaminaDecAmount != staminaDecrementAmount)
            OnStaminaDecrementAmountChanged?.Invoke(oldStaminaDecAmount, staminaDecrementAmount);
    }

    // Set Humanoid's stamina decrement tick time
    public void SetHumanoidStaminaDecrementTick(float amount)
    {
        float oldStaminaDecTick = staminaDecrementTick;
        staminaDecrementTick = Mathf.Max(amount, EPS_TIMER);

        if (oldStaminaDecTick != staminaDecrementTick)
            OnStaminaDecrementTickChanged?.Invoke(oldStaminaDecTick, staminaDecrementTick);
    }

    // Set Humanoid's health regeneration amount, up to health size
    public void SetHumanoidHealthRegenerationAmount(float amount)
    {
        float old = healthRegenerationAmount;
        healthRegenerationAmount = Mathf.Max(0, amount);

        if (old != healthRegenerationAmount)
            OnHealthRegenerationAmountChanged?.Invoke(old, healthRegenerationAmount);
    }

    // Set Humanoid's health regeneration tick time
    public void SetHumanoidHealthRegenerationTick(float amount)
    {
        float old = healthRegenerationTick;
        healthRegenerationTick = Mathf.Max(amount, EPS_TIMER);

        if (old != healthRegenerationTick)
            OnHealthRegenerationTickChanged?.Invoke(old, healthRegenerationTick);
    }

    // Set Humanoid's health regeneration delay
    public void SetHumanoidHealthRegenerationDelay(float amount)
    {
        float old = healthRegenerationDelay;
        healthRegenerationDelay = Mathf.Max(amount, EPS_TIMER);

        if (old != healthRegenerationDelay)
            OnHealthRegenerationDelayChanged?.Invoke(old, healthRegenerationDelay);
    }

    // Set Humanoid's stamina regeneration amount, up to stamina size
    public void SetHumanoidStaminaRegenerationAmount(float amount)
    {
        float old = staminaRegenerationAmount;
        staminaRegenerationAmount = Mathf.Max(0, amount);

        if (old != staminaRegenerationAmount)
            OnStaminaRegenerationAmountChanged?.Invoke(old, staminaRegenerationAmount);
    }

    // Set Humanoid's stamina regeneration tick time
    public void SetHumanoidStaminaRegenerationTick(float amount)
    {
        float old = staminaRegenerationTick;
        staminaRegenerationTick = Mathf.Max(amount, EPS_TIMER);

        if (old != staminaRegenerationTick)
            OnStaminaRegenerationTickChanged?.Invoke(old, staminaRegenerationTick);
    }

    // Set Humanoid's stamina regeneration delay
    public void SetHumanoidStaminaRegenerationDelay(float amount)
    {
        float old = staminaRegenerationDelay;
        staminaRegenerationDelay = Mathf.Max(amount, EPS_TIMER);

        if (old != staminaRegenerationDelay)
            OnStaminaRegenerationDelayChanged?.Invoke(old, staminaRegenerationDelay);
    }

    // Set Humanoid's health regeneration permission
    public void SetHumanoidHealthRegenerationEnabled(bool enabled)
    {
        bool old = healthRegenerationEnabled;
        healthRegenerationEnabled = enabled;

        if (old != healthRegenerationEnabled)
            OnHealthRegenerationEnabledChanged?.Invoke();
    }

    // Set Humanoid's stamina regeneration permission
    public void SetHumanoidStaminaRegenerationEnabled(bool enabled)
    {
        bool old = staminaRegenerationEnabled;
        staminaRegenerationEnabled = enabled;

        if (old != staminaRegenerationEnabled)
            OnStaminaRegenerationEnabledChanged?.Invoke();
    }

    // Set Humanoid's camera offset by the camera of player
    public void SetHumanoidCameraOffset(Vector3 offset)
    {
        Vector3 old = cameraOffset;
        cameraOffset = offset;

        if (!(old.x.Equals(cameraOffset.x) || old.y.Equals(cameraOffset.y) || old.z.Equals(cameraOffset.z)))
            OnCameraOffsetChanged?.Invoke(old, cameraOffset);
    }

    // Change Humanoid's state type to newer state
    public void ChangeState(HumanoidStateType newType)
    {
        if (!IsStateEnabled(newType))
            return;
        
        HumanoidStateType oldState = stateType;
        stateType = newType;

        if (oldState != stateType)
            OnStateChanged?.Invoke(oldState, stateType);
    }

    // Adding a force to rigidbody of Humanoid
    public void AddForce(Vector3 direction)
    {
        if (!isAlive)
            return;
        
        rigidBody.AddForce(direction);
    }

    // Overload: Adding a force with desired foce mode to rigidbody of Humanoid
    public void AddForce(Vector3 direction, ForceMode forceMode)
    {
        if (!isAlive)
            return;

        rigidBody.AddForce(direction, forceMode);    
    }

    // Change Humanoid's state to become enabled or disabled
    public void SetStateEnabled(HumanoidStateType state, bool enable)
    {
        if (enable)
            disableStates.Remove(state);
        else
            disableStates.Add(state);
    }

    // Comparing a state that enabled in this Humanoid
    public bool IsStateEnabled(HumanoidStateType state)
    {
        return (!disableStates.Contains(state));
    }

    public void ChangeOwner(HumanoidOwnerType newType)
    {
        HumanoidOwnerType oldType = ownerType;
        ownerType = newType;

        if (oldType != ownerType)
            OnOwnerChanged?.Invoke(oldType, ownerType);
    }

    // Comparing current state with a state status
    public bool IsInState(HumanoidStateType type)
    {
        return stateType == type;
    }

    /* max properties */

    // Set Humanoid's max health
    public void SetHumanoidMaxHealth(float amount)
    {
        maxHealth = Mathf.Max(1, amount);
        SetHumanoidHealth(Mathf.Min(health, maxHealth));
    }

    // Set Humanoid's max walk speed
    public void SetHumanoidMaxWalkSpeed(float amount)
    {
        maxWalkSpeed = Mathf.Max(1, amount);
        SetHumanoidWalkSpeed(Mathf.Min(walkSpeed, maxWalkSpeed));
    }

    // Set Humanoid's max running speed
    public void SetHumanoidMaxRunningSpeed(float amount)
    {
        maxRunningSpeed = Mathf.Max(1, amount);
        SetHumanoidRunningSpeed(Mathf.Min(runningSpeed, maxRunningSpeed));
    }

    // Set Humanoid's max jump power
    public void SetHumanoidMaxJumpPower(float amount)
    {
        maxJumpPower = Mathf.Max(1, amount);
        SetHumanoidJumpPower(Mathf.Min(jumpPower, maxJumpPower));
    }

    // Set Humanoid's max stamina
    public void SetHumanoidMaxStamina(float amount)
    {
        maxStamina = Mathf.Max(1, amount);
        SetHumanoidStamina(Mathf.Min(stamina, maxStamina));
    }

    // Set Humanoid's can move
    public void SetHumanoidCanMove(bool enable)
    {
        bool old = canMove;
        canMove = enable;

        if (old != canMove)
            OnCanMoveChanged?.Invoke(old, enable);
    }

    // Set Humanoid's can jump
    public void SetHumanoidCanJump(bool enable)
    {
        bool old = canJump;
        canJump = enable;

        if (old != canJump)
            OnCanJumpChanged?.Invoke(old, enable);
    }

    // Set Humanoid's walk to stopping distance
    public void SetHumanoidWalkToStoppingDistance(float amount)
    {
        float old = walkToStoppingDistance;
        walkToStoppingDistance = amount;

        if (!old.Equals(walkToStoppingDistance))
            OnWalkToStoppingDistanceChanged?.Invoke(old, walkToStoppingDistance);
    }

    // Set Humanoid's ground height
    public void SetHumanoidToGroundHeight(float amount)
    {
        amount = Mathf.Max(0, amount);
        toGroundHeight = amount;
    }

    // Set Humanoid's auto rotate
    public void SetHumanoidAutoRotate(bool enable)
    {
        bool old = autoRotate;
        autoRotate = enable;

        if (old != autoRotate)
            OnAutoRotateChanged?.Invoke(old, autoRotate);
    }

    // Set Humanoid's can apply fall damage
    public void SetHumanoidCanApplyFallDamage(bool enable)
    {
        bool old = canApplyFallDamage;
        canApplyFallDamage = enable;

        if (old != canApplyFallDamage)
            OnCanApplyFallDamageChanged?.Invoke(old, canApplyFallDamage);
    }

    // Set Humanoid's max slope angle
    public void SetHumanoidMaxSlopeAngle(float amount)
    {
        float old = maxSlopeAngle;
        maxSlopeAngle = amount;

        if (old != maxSlopeAngle)
            OnMaxSlopeAngleChanged?.Invoke(old, maxSlopeAngle);
    }

    // Set Humanoid's platform standing
    public void SetHumanoidPlatformStanding(bool enable)
    {
        bool old = platformStanding;
        platformStanding = enable;

        if (old != platformStanding)
            OnPlatformStandingChanged?.Invoke(old, platformStanding);
    }

    // Set Humanoid's safe from fall damage distance
    public void SetHumanoidSafeFromFallDistance(float amount)
    {
        float old = safeFromFallDistance;
        safeFromFallDistance = Mathf.Max(0, amount);

        if (old != safeFromFallDistance)
            OnSafeFromFallDistance?.Invoke(old, safeFromFallDistance);
    }

    // Set Humanoid's fall damage multiplier
    public void SetHumanoidFallDamageMultiplier(float amount)
    {
        float old = fallDamageMultiplier;
        fallDamageMultiplier = Mathf.Max(1, amount);

        if (old != fallDamageMultiplier)
            OnFallDamageMultiplierChanged?.Invoke(old, fallDamageMultiplier);
    }

    /* getters */
    // Get Humanoid's move direction in local/relative space of this transform
    public Vector3 GetLocalMoveDirection()
    {
        return transform.InverseTransformDirection(moveDirection);
    }

    // Get Humanoid's move direction in world space
    public Vector3 GetGlobalMoveDirection()
    {
        return moveDirection;
    }

    // Get Humanoid's facing direction in world space
    public Vector3 GetFacingDirection()
    {
        return facingDirection;
    }

    // Get amount between [1, -1] of forward movement direction from Humanoid
    public float GetForwardAmount()
    {
        return Vector3.Dot(transform.forward, moveDirection);
    }

    // Get amount between [1, -1] of right movement direction from Humanoid
    public float GetRightAmount()
    {
        return Vector3.Dot(transform.right, moveDirection);
    }

    /* extra methods */

    // return the bool states by comparing the current status with desired status 
    public bool HasStatus(string statusName)
    {
        return statuses.Contains(statusName);
    }

    public bool HasTemporaryStatus(string statusName)
    {
        return temporaryStatuses.TryGetValue(statusName, out float end) && Time.time < end;
    }

    // adding a new status into Humanoid
    public void AddStatus(string statusName)
    {
        if (statuses.Contains(statusName)) return;
        statuses.Add(statusName);
    }

    public void AddTemporaryStatus(string statusName, float timeout)
    {
        temporaryStatuses[statusName] = Time.time + timeout; 
    }

    // removing an available status from the Humanoid
    public void RemoveStatus(string statusName)
    {
        if (statuses.Contains(statusName))
            statuses.Remove(statusName);
    }

    public void RemoveTemporaryStatus(string statusName)
    {
        if (temporaryStatuses.ContainsKey(statusName))
            temporaryStatuses.Remove(statusName);
    }

    // changing an available status from the Humanoid to another desired status
    public void ChangeStatus(string fromStatus, string toStatus)
    {
        if (!statuses.Contains(fromStatus) || statuses.Contains(toStatus))
            return;
        
        statuses.Remove(fromStatus);
        statuses.Add(toStatus);
    }
    #endregion

    #region API
    public void Move(Vector3 Direction, bool Running)
    {
        if (!isAlive || !canMove || platformStanding)
            return;
        
        if (Direction.sqrMagnitude <= 0.001f)
        {
            ChangeState(HumanoidStateType.Idle);
            isMoving = false;

            return;
        }

        Direction.Normalize();

        float speed = (Running && stamina > 0) ? runningSpeed : walkSpeed;
        Vector3 velocity = Direction * speed;

        velocity.y = rigidBody.linearVelocity.y;
        rigidBody.linearVelocity = velocity;

        if (autoRotate)
            FaceDirection(Direction);

        isMoving = true;

        if (Direction.sqrMagnitude > 0.001f)
        {
            moveDirection = Direction.normalized;
            lastMoveDirection = moveDirection;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        ChangeState(Running ? HumanoidStateType.Running : HumanoidStateType.Walking);
        if (Running)
            OnRunning?.Invoke();
        else
            OnWalking?.Invoke();
    }

    public void Jump()
    {
        if (!isAlive || !isGrounded || !canJump || platformStanding)
            return;

        Vector3 velocity = rigidBody.linearVelocity;
        velocity.y = 0;
        rigidBody.linearVelocity = velocity;

        rigidBody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

        ChangeState(HumanoidStateType.Jumping);
        OnJumping?.Invoke();
    }

    public void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return;
        
        direction.y = 0;

        // double-check after set y of direction to 0
        if (direction.sqrMagnitude <= 0.001f)
            return;

        facingDirection = direction.normalized;

        Quaternion lookDir = Quaternion.LookRotation(facingDirection, Vector3.up);
        rigidBody.MoveRotation(lookDir);
    }

    #endregion

    #region UnityHelpers
    private void Awake()
    {
        if (rigidBody == null) rigidBody = GetComponent<Rigidbody>();
        if (rootPart == null) rootPart = transform;
        if (bodyCollider == null) bodyCollider = GetComponent<Collider>(); 

        isAlive = health > 0;
        isMoving = moveDirection.sqrMagnitude > 0.001f;

        health = maxHealth;
        walkSpeed = maxWalkSpeed;
        runningSpeed = maxRunningSpeed;
        jumpPower = maxJumpPower;
        stamina = maxStamina;

        lastStaminaUseTime = 0;
    }

    private void Update()
    {
        // physics
        HandleFallTracking();
        HandleFloorInfo();

        // character
        HandleHealthRegeneration();

        // status
        HandleTemporaryStatus();
    }

    private void FixedUpdate()
    {
        linearVelocity = rigidBody.linearVelocity;
        angularVelocity = rigidBody.angularVelocity;
    }
    
    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        maxWalkSpeed = Mathf.Max(1, maxWalkSpeed);
        maxRunningSpeed = Mathf.Max(1, maxRunningSpeed);
        maxJumpPower = Mathf.Max(1, maxJumpPower);
        maxStamina = Mathf.Max(1, maxStamina);

        health = Mathf.Clamp(health, 0, maxHealth);
        walkSpeed = Mathf.Clamp(walkSpeed, 1, maxWalkSpeed);
        runningSpeed = Mathf.Clamp(runningSpeed, 1, maxRunningSpeed);
        jumpPower = Mathf.Clamp(jumpPower, 1, maxJumpPower);
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        staminaDecrementTick = Mathf.Max(EPS_TIMER, staminaDecrementTick);
        healthRegenerationTick = Mathf.Max(EPS_TIMER, healthRegenerationTick);
        staminaRegenerationTick = Mathf.Max(EPS_TIMER, staminaRegenerationTick);
    }

    private void Reset()
    {
        rigidBody = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<Collider>();
        rootPart = transform;
    }
    #endregion


}
