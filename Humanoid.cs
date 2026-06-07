/* ************************************************************************************************** *
 * Humanoid, module for Humanoid system of character, Unity. Works for Unity >6.                      *
 * Created by Raihan, May 28 - 2 June, 2026. v1.0.0                                                   *
 * Free usage, MIT License in GitHub repository.                                                      *
 *                                                                                                    *
 * Structures:                                                                                        *
 * Humanoid                                                                                           *
 *     \_ HumanoidMotor                                                                               *
 *     \_ HumanoidAnimator                                                                            *
 *     \_ HumanoidCombat                                                                              *
 *     \_ HumanoidInputController                                                                     *
 *     \_ AIHumanoidHandler                                                                           *
 *                                                                                                    *
 * If you have some problems of my modules or giving recommendation, chat:                            *
 * @raihanaufal_77 in Instagram.                                                                      * 
 *                                                                                                    *
 * But, I'm not too active in media socials. Therefore, you need some times for waiting my responses. *
 * ************************************************************************************************** */
 

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enumerations of Humanoid's State Type, when Humanoid is in a conceptual state by the world.
/// </summary>
[Serializable] public enum HumanoidStateType 
{ 
    /// <summary>
    /// Humanoid is Running, duality condition with Walking, happened when Humanoid is on walking with running enabled from Motor
    /// </summary>
    Running, 
    /// <summary>
    /// Humanoid is Walking, duality condition with Running, happened when Humanoid is on walking with no running enabled from Motor
    /// </summary>
    Walking,
    /// <summary>
    /// Humanoid is Jumping, happened when Humanoid is using Jump() from Motor
    /// </summary> 
    Jumping,
    /// <summary>
    /// Humanoid is Idling, happened when there is no condition happened
    /// </summary>
    Idle,
    /// <summary>
    /// Humanoid is Died, happened when Humanoid's health is 0
    /// </summary>   
    Died,
    /// <summary>
    /// Humanoid is in Airborne, happened when Humanoid is beginning to flying on air by the physics
    /// </summary>
    Airborne,
    /// <summary>
    /// Humanoid is in falling, happened when Humanoid is getting pulled back by gravity from air to ground
    /// </summary>
    FreeFalling,
    /// <summary>
    /// Humanoid is on ground or landed, happened when Humanoid landed on ground after free falling
    /// </summary>
    Grounded,
    /// <summary>
    /// Humanoid is getting slid, happened when Humanoid is on the slope that surpass the limit of max slope angle from this Humanoid
    /// </summary>
    Sliding,
    /// <summary>
    /// Humanoid is crouching, happened when Humanoid is using Crouched() from Motor
    /// </summary>
    Crouch,
    /// <summary>
    /// Humanoid is on prone, happened when Humanoid is using Prone() from Motor
    /// </summary>
    Prone,
    /// <summary>
    /// Humanoid is not in written state, there is no conceptual state of the Humanoid
    /// </summary>
    Neutral
};
/// <summary>
/// Enumerations of Humanoid's Owner Type, the owner type of controller from the Humanoid.
/// </summary>
[Serializable] public enum HumanoidOwnerType 
{ 
    /// <summary>
    /// Owner type of Humanoid that being controlled by human/player through Input conditions, player allowed to do input control
    /// </summary>
    Player,
    /// <summary>
    /// Owner type of Humanoid that being controlled by script, player can't do Input conditions for this owner type
    /// </summary>
    AI, 
    /// <summary>
    /// Owner type of Humanoid that has no owner both from AI/Player
    /// </summary>
    Neutral 
};

/// <summary>
/// Humanoid's module script, as Humanoid "component" of this trasform.
/// </summary>
[Serializable, RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
public class Humanoid : MonoBehaviour 
{ 
    /* Player's properties for humanoid, such as Health, MaxHealth, WalkSpeed, etc.                     *
     * I'll make them all as public variables, because those properties are readable to other classes. */    
    
     /* Serialized Internal properties */
    #region SerializedReferences

    [Header("References")]
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Transform rootPart;
    [SerializeField] private Collider bodyCollider;

    [Header("Owner")]
    [SerializeField] private HumanoidOwnerType ownerType = HumanoidOwnerType.Neutral;

    [Header("Physics Informations")]
    [SerializeField] private Vector3 linearVelocity = Vector3.zero;
    [SerializeField] private Vector3 angularVelocity = Vector3.zero;
    [SerializeField] private Vector3 lastMoveDirection = Vector3.forward;
    [SerializeField] private Vector3 facingDirection = Vector3.forward;

    [Header("Details")]
    [SerializeField] private bool canApplyFallDamage = true;
    [SerializeField] private float safeFromFallDistance = 3.5f;
    [SerializeField] private float fallDamageMultiplier = 10.0f;
    [SerializeField] private float walkToStoppingDistance = 0.5f;

    [SerializeField] private Vector3 cameraOffset = Vector3.zero;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canProne = true;
    [SerializeField] private bool platformStanding = false;

    [Header("Max Attributes")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxWalkSpeed = 12;
    [SerializeField] private float maxRunningSpeed = 18;
    [SerializeField] private float maxJumpPower = 15;
    [SerializeField] private float maxStamina = 200;   

    /* booleans */
    [Header("More")]
    [SerializeField] private float staminaDecrementAmount = 2.0f;
    [SerializeField] private float staminaDecrementTick = 0.2f;

    [SerializeField] private bool healthRegenerationEnabled = true;
    [SerializeField] private float healthRegenerationAmount = 1.0f;
    [SerializeField] private float healthRegenerationTick = 0.8f;
    [SerializeField] private float healthRegenerationDelay = 3.0f;

    [SerializeField] private bool staminaRegenerationEnabled = true;
    [SerializeField] private float staminaRegenerationAmount = 5.0f;
    [SerializeField] private float staminaRegenerationTick = 0.75f;
    [SerializeField] private float staminaRegenerationDelay = 2.0f;

    [Header("Attributes")]
    [SerializeField] private float health;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float stamina;

    #endregion

    #region UnserializedReferences

    /* booleans */
    private bool isAlive;
    private bool isMoving;
    private bool isJumping = false;
    private bool isCrouching;
    private bool isProning;
    private bool hasTargetPoint = false;

    /* ai helpers */
    private Vector3 targetPoint;

    /* owner type */
    private HumanoidStateType stateType = HumanoidStateType.Neutral;

    /* physics */
    private Collider floorCollider;
    private PhysicsMaterial floorMaterial;
    private Vector3 moveDirection;

    /* timers */
    private float lastStaminaUseTime;
    private float lastGroundedTime;
    private float staminaRegenTimer;
    private float staminaUsedTimer;
    private float lastDamagedTime;
    private float healthRegenTimer;

    private float EPS_TIMER = 0.05f;
    #endregion

    #region ReadReferences
    /* Max references */
    public float MaxHealth => maxHealth; // Humanoid's Max Health, clamping Humanoid's health
    public float MaxWalkSpeed => maxWalkSpeed; // Humanoid's Max Walk Speed, clamping Humanoid's Walk Speed
    public float MaxRunningSpeed => maxRunningSpeed; // Humanoid's Max Running Speed, clamping Humanoid's Running Speed
    public float MaxJumpPower => maxJumpPower; // Humanoid's Max Jump Power, clamping Humanoid's Jump Power
    public float MaxStamina => maxStamina; // Humanoid's Max Stamina, clamping Humanoid's Stamina

    /* Player's state */
    public HumanoidStateType StateType => stateType; // Humanoid's State Type, getting the current state of Humanoid
    public HumanoidOwnerType OwnerType => ownerType; // Humanoid's Owner Type, getting the current owner of Humanoid

    /* Physics */
    public Vector3 LinearVelocity => linearVelocity; // Humanoid's Linear Horizontal Velocity, getting the horizontal velocity of the rigidbody
    public Vector3 AngularVelocity => angularVelocity; // Humanoid's Angular Velocity, getting the angular velocity of the rigidbody
    public float VerticalVelocity => linearVelocity.y; // Humanoid's Linear Vertical Velocity, getting the vertical velocity of the rigidbody
    public float HorizontalSpeed { get { Vector3 vc = linearVelocity; vc.y = 0; return vc.magnitude; } } // Humanoid's moving speed in horizontal
    public Vector3 MoveDirection => moveDirection; // Humanoid's move direction (normalized)
    public Vector3 LastMoveDirection => lastMoveDirection; // Humanoid's last move direction (normalized)
    public Vector3 FacingDirection => facingDirection; // Humanoid's facing direction (normalized)
    public Rigidbody RigidBody => rigidBody; // 
    public Transform RootPart => rootPart;
    public Collider BodyCollider => bodyCollider;
    public float WalkToStoppingDistance => walkToStoppingDistance;
    public Collider FloorCollider => floorCollider;
    public PhysicsMaterial FloorMaterial => floorMaterial;
    public float LastGroundedTime => lastGroundedTime;
    public Vector3 TargetPoint => targetPoint;
    public float SafeFromFallDistance => safeFromFallDistance;
    public float FallDamageMultiplier => fallDamageMultiplier;

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
    public bool IsMoving => isMoving;
    public bool PlatformStanding => platformStanding;
    public bool CanMove => canMove;
    public bool CanJump => canJump;
    public bool CanCrouch => canCrouch;
    public bool CanProne => canProne;
    public bool IsJumping => isJumping;
    public bool CanApplyFallDamage => canApplyFallDamage;
    public bool HealthRegenerationEnabled => healthRegenerationEnabled;
    public bool StaminaRegenerationEnabled => staminaRegenerationEnabled;
    public bool HasTargetPoint => hasTargetPoint;

    #endregion


    /* Private members                                             *
     * Used as the internal utilities of this class                */

    #region Actions
    /* Callbacks */
    public event Action Died;
    public event Action Revived;

    public event Action OnHealthRegenerationEnabledChanged;
    public event Action OnStaminaRegenerationEnabledChanged;

    public event Action<float> Damaged;
    public event Action<float> Healed;

    public event Action<string> OnStatusAdded;
    public event Action<string> OnStatusRemoved;
    public event Action<string, string> OnStatusChanged;
    public event Action<string, float> OnTemporaryStatusAddedOrChanged;
    public event Action<string> OnTemporaryStatusExpired;
    public event Action<string> OnTemporaryStatusRemoved;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action<float, float> OnWalkSpeedChanged;
    public event Action<float, float> OnRunningSpeedChanged;
    public event Action<float, float> OnJumpPowerChanged;
    public event Action<float, float> OnMaxHealthChanged;
    public event Action<float, float> OnMaxStaminaChanged;
    public event Action<float, float> OnMaxWalkSpeedChanged;
    public event Action<float, float> OnMaxRunningSpeedChanged;
    public event Action<float, float> OnMaxJumpPowerChanged;
    public event Action<float, float> OnStaminaDecrementAmountChanged;
    public event Action<float, float> OnStaminaDecrementTickChanged;
    public event Action<float, float> OnHealthRegenerationAmountChanged;
    public event Action<float, float> OnHealthRegenerationTickChanged;
    public event Action<float, float> OnHealthRegenerationDelayChanged;
    public event Action<float, float> OnStaminaRegenerationAmountChanged;
    public event Action<float, float> OnStaminaRegenerationTickChanged;
    public event Action<float, float> OnStaminaRegenerationDelayChanged;
    public event Action<float, float> OnFallDamageMultiplierChanged;
    public event Action<float, float> OnSafeFromFallDistanceChanged;
    public event Action<float, float> OnWalkToStoppingDistanceChanged;

    public event Action<Vector3, Vector3> OnCameraOffsetChanged;
    public event Action<Vector3, Vector3> OnLinearVelocityChanged;
    public event Action<Vector3, Vector3> OnAngularVelocityChanged;

    public event Action<HumanoidStateType, HumanoidStateType> OnStateChanged;
    public event Action<HumanoidOwnerType, HumanoidOwnerType> OnOwnerChanged;

    public event Action<bool, bool> OnCanApplyFallDamageChanged;
    public event Action<bool, bool> OnPlatformStandingChanged;
    public event Action<bool, bool> OnCanMoveChanged;
    public event Action<bool, bool> OnCanJumpChanged;

    #endregion

    #region Tables
    /* hash tables */
    private readonly HashSet<string> statuses = new();
    private readonly HashSet<string> expiredStatuses = new();
    private readonly HashSet<HumanoidStateType> disableStates = new();

    /* dictionaries */
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

    private void HandleStaminaRegeneration()
    {
        if (!staminaRegenerationEnabled || !isAlive)
            return;

        if (Time.time - lastStaminaUseTime <= staminaRegenerationDelay)
            return;

        if (stamina >= maxStamina)
        {
            stamina = maxStamina;   
            return;
        }

        staminaRegenTimer += Time.deltaTime;
        if (staminaRegenTimer >= staminaRegenerationTick)
        {
            SetHumanoidStamina(stamina + staminaRegenerationAmount);
            staminaRegenTimer = 0;
        }
    }

    private void HandleTemporaryStatus()
    {
        // clear the hash tables of expires statuses
        expiredStatuses.Clear();

        foreach (var status in temporaryStatuses)
        {
            if (Time.time > status.Value)
                expiredStatuses.Add(status.Key);
        }

        foreach (string status in expiredStatuses)
        {
            temporaryStatuses.Remove(status);
            OnTemporaryStatusExpired?.Invoke(status);
        }
    }
    #endregion

    #region Methods
    /* Methods functions */

    /// <summary>
    /// Giving damage to Humanoid, by decreasing the health of the humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0</param>
    public void TakeDamage(float amount)
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

    /// <summary>
    /// Giving or healing some health to Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0</param>
    public void Heal(float amount)
    {
        if (!isAlive || amount <= 0)
            return;

        float oldHealth = health;        
        SetHumanoidHealth(amount + health);

        float healedHealth = health - oldHealth;
        if (healedHealth > 0)
            Healed?.Invoke(healedHealth);
    }

    /// <summary>
    /// Calling-back or Revive Humanoid to the world, with gaining some health amount as revived
    /// </summary>
    /// <param name="reviveHealthAmount">reviveHealthAmount must be >1 or same/less than maxHealth</param>
    public void Revive(float reviveHealthAmount)
    {
        if (isAlive)
            return;

        isAlive = true;
        SetHumanoidHealth(Mathf.Clamp(reviveHealthAmount, 1, maxHealth));
        ChangeState(HumanoidStateType.Idle);

        Revived?.Invoke();
    }

    /// <summary>
    /// Comparing to current stamina and the decrement amount, in order to decrease the current stamina of Humanoid
    /// </summary>
    /// <param name="amount">Amount must less than Stamina</param>
    /// <returns>True/False</returns>
    public bool TryUsingStamina(float amount)
    {
        if (amount > stamina)
            return false;

        SetHumanoidStamina(stamina - amount);
        lastStaminaUseTime = Time.time;
        return true;
    }

     /// <summary>
     /// Killing and set Humanoid's health to 0
     /// </summary>
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

    /// <summary>
    /// Stopping or killing the movement of Humanoid and set the state to Idle
    /// </summary>
    public void StopMovement()
    {
        Vector3 velocity = rigidBody.linearVelocity;
        velocity.x = 0; velocity.z = 0;

        rigidBody.linearVelocity = velocity;

        moveDirection = Vector3.zero;
        isMoving = false;

        ChangeState(HumanoidStateType.Idle);
    }

    /* setters */

    /// <summary>
    /// Setting the Linear Velocity of Humanoid
    /// </summary>
    /// <param name="direction">Direction must be Vector3</param>
    public void SetHumanoidLinearVelocity(Vector3 direction)
    {
        Vector3 old = linearVelocity;
        linearVelocity = direction;

        if (old != linearVelocity)
            OnLinearVelocityChanged?.Invoke(old, linearVelocity);
    }

    /// <summary>
    /// Setting the Angular Velocity of Humanoid
    /// </summary>
    /// <param name="direction">Direction must be Vector3</param>
    public void SetHumanoidAngularVelocity(Vector3 direction)
    {
        Vector3 old = angularVelocity;
        angularVelocity = direction;
        
        if (old != angularVelocity)
            OnAngularVelocityChanged?.Invoke(old, angularVelocity);
    }

    /// <summary>
    /// Setting the Health of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0 and same/less than MaxHealth</param>
    public void SetHumanoidHealth(float amount)
    {
        float oldHealth = health;
        health = Mathf.Clamp(amount, 0, maxHealth);
            
        if (health != oldHealth)
            OnHealthChanged?.Invoke(oldHealth, health);
            
        if (health <= 0)
            Kill();
    }

    /// <summary>
    /// Setting the WalkSpeed of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0 and same/less than MaxWalkSpeed</param>
    public void SetHumanoidWalkSpeed(float amount)
    {
        if (amount > runningSpeed)
            amount = runningSpeed;
        
        float oldWalkSpeed = walkSpeed;
        walkSpeed = Mathf.Clamp(amount, 0, maxWalkSpeed);

        if (oldWalkSpeed != walkSpeed)
            OnWalkSpeedChanged?.Invoke(oldWalkSpeed, walkSpeed);
    }

    /// <summary>
    /// Setting the RunningSpeed of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0 and same/less than MaxRunningSpeed</param>
    public void SetHumanoidRunningSpeed(float amount)
    {
        if (amount < walkSpeed)
            amount = walkSpeed;

        float oldRunningSpeed = runningSpeed;
        runningSpeed = Mathf.Clamp(amount, 0, maxRunningSpeed);

        if (oldRunningSpeed != runningSpeed)
            OnRunningSpeedChanged?.Invoke(oldRunningSpeed, runningSpeed);
    }

    /// <summary>
    /// Setting the JumpPower of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0 and same/less than MaxJumpPower</param>
    public void SetHumanoidJumpPower(float amount)
    {
        float oldJumpPower = jumpPower;
        jumpPower = Mathf.Clamp(amount, 0, maxJumpPower);

        if (oldJumpPower != jumpPower)
            OnJumpPowerChanged?.Invoke(oldJumpPower, jumpPower);
    }

    /// <summary>
    /// Setting the Stamina of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0 and same/less than MaxStamina</param>
    public void SetHumanoidStamina(float amount)
    {
        float oldStamina = stamina;
        stamina = Mathf.Clamp(amount, 0, maxStamina);

        if (oldStamina != stamina)
            OnStaminaChanged?.Invoke(oldStamina, stamina);
    }

    /// <summary>
    /// Setting the StaminaDecrementAmount of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0</param>
    public void SetHumanoidStaminaDecrementAmount(float amount)
    {
        float oldStaminaDecAmount = staminaDecrementAmount;
        staminaDecrementAmount = Mathf.Max(amount, 0);

        if (oldStaminaDecAmount != staminaDecrementAmount)
            OnStaminaDecrementAmountChanged?.Invoke(oldStaminaDecAmount, staminaDecrementAmount);
    }

    /// <summary>
    /// Setting the StaminaDecrementTick of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0</param>
    public void SetHumanoidStaminaDecrementTick(float amount)
    {
        float oldStaminaDecTick = staminaDecrementTick;
        staminaDecrementTick = Mathf.Max(amount, EPS_TIMER);

        if (oldStaminaDecTick != staminaDecrementTick)
            OnStaminaDecrementTickChanged?.Invoke(oldStaminaDecTick, staminaDecrementTick);
    }

    /// <summary>
    /// Setting the HealthRegenerationAmount of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >0</param>
    public void SetHumanoidHealthRegenerationAmount(float amount)
    {
        float old = healthRegenerationAmount;
        healthRegenerationAmount = Mathf.Max(0, amount);

        if (old != healthRegenerationAmount)
            OnHealthRegenerationAmountChanged?.Invoke(old, healthRegenerationAmount);
    }

    /// <summary>
    /// Setting the HealthRegenerationTick of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >=0</param>
    public void SetHumanoidHealthRegenerationTick(float amount)
    {
        float old = healthRegenerationTick;
        healthRegenerationTick = Mathf.Max(amount, EPS_TIMER);

        if (old != healthRegenerationTick)
            OnHealthRegenerationTickChanged?.Invoke(old, healthRegenerationTick);
    }

    /// <summary>
    /// Setting the HealthRegenerationDelay of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >=0.05</param>
    public void SetHumanoidHealthRegenerationDelay(float amount)
    {
        float old = healthRegenerationDelay;
        healthRegenerationDelay = Mathf.Max(amount, EPS_TIMER);

        if (old != healthRegenerationDelay)
            OnHealthRegenerationDelayChanged?.Invoke(old, healthRegenerationDelay);
    }

    /// <summary>
    /// Setting the StaminaRegenerationAmount of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >=0</param>
    public void SetHumanoidStaminaRegenerationAmount(float amount)
    {
        float old = staminaRegenerationAmount;
        staminaRegenerationAmount = Mathf.Max(0, amount);

        if (old != staminaRegenerationAmount)
            OnStaminaRegenerationAmountChanged?.Invoke(old, staminaRegenerationAmount);
    }

    /// <summary>
    /// Setting the StaminaRegenerationTick of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >=0.05</param>
    public void SetHumanoidStaminaRegenerationTick(float amount)
    {
        float old = staminaRegenerationTick;
        staminaRegenerationTick = Mathf.Max(amount, EPS_TIMER);

        if (old != staminaRegenerationTick)
            OnStaminaRegenerationTickChanged?.Invoke(old, staminaRegenerationTick);
    }

    /// <summary>
    /// Setting the StaminaDecrementTick of Humanoid
    /// </summary>
    /// <param name="amount">Amount must be >=0.05</param>
    public void SetHumanoidStaminaRegenerationDelay(float amount)
    {
        float old = staminaRegenerationDelay;
        staminaRegenerationDelay = Mathf.Max(amount, EPS_TIMER);

        if (old != staminaRegenerationDelay)
            OnStaminaRegenerationDelayChanged?.Invoke(old, staminaRegenerationDelay);
    }

    /// <summary>
    /// Setting the HealthRegenerationEnabled of Humanoid
    /// </summary>
    /// <param name="enabled">True or False?</param>
    public void SetHumanoidHealthRegenerationEnabled(bool enabled)
    {
        bool old = healthRegenerationEnabled;
        healthRegenerationEnabled = enabled;

        if (old != healthRegenerationEnabled)
            OnHealthRegenerationEnabledChanged?.Invoke();
    }

    /// <summary>
    /// Setting the StaminaRegenerationEnabled of Humanoid
    /// </summary>
    /// <param name="enabled">True or False?</param>
    public void SetHumanoidStaminaRegenerationEnabled(bool enabled)
    {
        bool old = staminaRegenerationEnabled;
        staminaRegenerationEnabled = enabled;

        if (old != staminaRegenerationEnabled)
            OnStaminaRegenerationEnabledChanged?.Invoke();
    }

    /// <summary>
    /// Setting the Humanoid's camera offset that will be handled by HumanoidMotor
    /// </summary>
    /// <param name="offset">Offset must be Vector3</param>
    public void SetHumanoidCameraOffset(Vector3 offset)
    {
        Vector3 old = cameraOffset;
        cameraOffset = offset;

        if (old != offset)
            OnCameraOffsetChanged?.Invoke(old, cameraOffset);
    }

    public void SetHumanoidIsMoving(bool enabled)
    {
        if (isMoving == enabled) return;

        isMoving = enabled;
    }

    public void SetHumanoidIsJumping(bool enabled)
    {
        if (isJumping == enabled) return;
        isJumping = enabled;
    }

    public void SetHumanoidIsCrouching(bool enabled)
    {
        if (isCrouching == enabled) return;
        isCrouching = enabled;
    }

    public void SetHumanoidIsProning(bool enabled)
    {
        if (isProning == enabled) return;
        isProning = enabled;
    }

    /// <summary>
    /// Changing Humanoid's current state to desired state
    /// </summary>
    /// <param name="newType">NewType must be HumanoidStateType enums</param>
    public void ChangeState(HumanoidStateType newType)
    {
        if (!IsStateEnabled(newType))
            return;
        
        HumanoidStateType oldState = stateType;
        stateType = newType;

        if (oldState != stateType)
            OnStateChanged?.Invoke(oldState, stateType);
    }

    /// <summary>
    /// Adding a force to RigidBody of this Humanoid
    /// </summary>
    /// <param name="direction">Direction must be Vector3</param>
    public void AddForce(Vector3 direction)
    {
        if (!isAlive)
            return;
        
        rigidBody.AddForce(direction);
    }

    /// <summary>
    /// Overload: Adding a force to Rigidbody of this Humanoid with desired ForceMode
    /// </summary>
    /// <param name="direction">Direction must be Vector3</param>
    /// <param name="forceMode">ForceMode must be ForceMode enums</param>
    public void AddForce(Vector3 direction, ForceMode forceMode)
    {
        if (!isAlive)
            return;

        rigidBody.AddForce(direction, forceMode);    
    }

    /// <summary>
    /// Adding a conditional state of Humanoid to be enabled/disabled
    /// </summary>
    /// <param name="state">State must be HumanoidStateType enums</param>
    /// <param name="enable">True/False</param>
    public void SetStateEnabled(HumanoidStateType state, bool enable)
    {
        if (enable)
            disableStates.Remove(state);
        else
            disableStates.Add(state);
    }

    /// <summary>
    /// Comparing an enabled state with desired state
    /// </summary>
    /// <param name="state">State must be HumanoidStateType enums</param>
    /// <returns>True/False</returns>
    public bool IsStateEnabled(HumanoidStateType state)
    {
        return !disableStates.Contains(state);
    }

    /// <summary>
    /// Changing the Humanoid user controller to desired owner type
    /// </summary>
    /// <param name="newType">NewType must be HumanoidOwnerType enums</param>
    public void ChangeOwner(HumanoidOwnerType newType)
    {
        HumanoidOwnerType oldType = ownerType;
        ownerType = newType;

        if (oldType != ownerType)
            OnOwnerChanged?.Invoke(oldType, ownerType);
    }

    /// <summary>
    /// Comparing if Humanoid is in desired state
    /// </summary>
    /// <param name="type">Type must be HumanoidStateType enums</param>
    /// <returns>True/False</returns>
    public bool IsInState(HumanoidStateType type)
    {
        return stateType == type;
    }

    /* max properties */

    /// <summary>
    /// Setting the Humanoid's MaxHealth, that clamping Humanoid's Health
    /// </summary>
    /// <param name="amount">Amount must be >=1</param>
    public void SetHumanoidMaxHealth(float amount)
    {
        float old = maxHealth;

        maxHealth = Mathf.Max(1, amount);
        SetHumanoidHealth(Mathf.Min(health, maxHealth));

        if (old != maxHealth)
            OnMaxHealthChanged?.Invoke(old, maxHealth);
    }

    /// <summary>
    /// Setting the Humanoid's MaxWalkSpeed, that clamping Humanoid's WalkSpeed
    /// </summary>
    /// <param name="amount">Amount must be >=1</param>
    public void SetHumanoidMaxWalkSpeed(float amount)
    {
        float old = maxWalkSpeed;

        maxWalkSpeed = Mathf.Max(1, amount);
        SetHumanoidWalkSpeed(Mathf.Min(walkSpeed, maxWalkSpeed));

        if (old != maxWalkSpeed)
            OnMaxWalkSpeedChanged?.Invoke(old, maxWalkSpeed);
    }

    /// <summary>
    /// Setting the Humanoid's MaxRunningSpeed, that clamping Humanoid's RunningSpeed
    /// </summary>
    /// <param name="amount">Amount must be >=1</param>
    public void SetHumanoidMaxRunningSpeed(float amount)
    {
        float old = maxRunningSpeed;

        maxRunningSpeed = Mathf.Max(1, amount);
        SetHumanoidRunningSpeed(Mathf.Min(runningSpeed, maxRunningSpeed));

        if (old != maxRunningSpeed)
            OnMaxRunningSpeedChanged?.Invoke(old, maxRunningSpeed);
    }

    /// <summary>
    /// Setting the Humanoid's MaxJumpPower, clamping Humanoid's JumpPower
    /// </summary>
    /// <param name="amount">Amount must be >=1</param>
    public void SetHumanoidMaxJumpPower(float amount)
    {
        float old = maxJumpPower;

        maxJumpPower = Mathf.Max(1, amount);
        SetHumanoidJumpPower(Mathf.Min(jumpPower, maxJumpPower));

        if (old != maxJumpPower)
            OnMaxJumpPowerChanged?.Invoke(old, maxJumpPower);
    }

    /// <summary>
    /// Setting the Humanoid's MaxStamina, clamping Humanoid's Stamina
    /// </summary>
    /// <param name="amount">Amount must be >=1</param>
    public void SetHumanoidMaxStamina(float amount)
    {
        float old = maxStamina;

        maxStamina = Mathf.Max(1, amount);
        SetHumanoidStamina(Mathf.Min(stamina, maxStamina));

        if (old != maxStamina)
            OnMaxStaminaChanged?.Invoke(old, maxStamina);
    }

    /// <summary>
    /// Setting Humanoid's access to move
    /// </summary>
    /// <param name="enable">True/False</param>
    public void SetHumanoidCanMove(bool enable)
    {
        bool old = canMove;
        canMove = enable;

        if (old != canMove)
            OnCanMoveChanged?.Invoke(old, enable);
    }

    /// <summary>
    /// Setting Humanoid's access to jump
    /// </summary>
    /// <param name="enable">True/False</param>
    public void SetHumanoidCanJump(bool enable)
    {
        bool old = canJump;
        canJump = enable;

        if (old != canJump)
            OnCanJumpChanged?.Invoke(old, enable);
    }

    /// <summary>
    /// Setting Humanoid's WalkToStoppingDistance, that used for Humanoid minimum distance to stop of MoveTo() or HumanoidMotor
    /// </summary>
    /// <param name="amount">Amount must be >=0</param>
    public void SetHumanoidWalkToStoppingDistance(float amount)
    {
        float old = walkToStoppingDistance;
        walkToStoppingDistance = Mathf.Max(0, amount);

        if (!old.Equals(walkToStoppingDistance))
            OnWalkToStoppingDistanceChanged?.Invoke(old, walkToStoppingDistance);
    }

    /// <summary>
    /// Setting Humanoid's CanApplyFallDamage, to set access for Humanoid can take damage when fell and hit the ground
    /// </summary>
    /// <param name="enable">True/False</param>
    public void SetHumanoidCanApplyFallDamage(bool enable)
    {
        bool old = canApplyFallDamage;
        canApplyFallDamage = enable;

        if (old != canApplyFallDamage)
            OnCanApplyFallDamageChanged?.Invoke(old, canApplyFallDamage);
    }

    /// <summary>
    /// Setting Humanoid's platform stand status, that if enabled, Humanoid cannot able to do movement or jumping
    /// </summary>
    /// <param name="enable">True/False</param>
    public void SetHumanoidPlatformStanding(bool enable)
    {
        bool old = platformStanding;
        platformStanding = enable;

        if (old != platformStanding)
            OnPlatformStandingChanged?.Invoke(old, platformStanding);
    }

    /// <summary>
    /// Setting Humanoid's distance for safe from falling on air, that prevents Humanoid taking damage after landed to ground
    /// </summary>
    /// <param name="amount">Amount must be >=0</param>
    public void SetHumanoidSafeFromFallDistance(float amount)
    {
        float old = safeFromFallDistance;
        safeFromFallDistance = Mathf.Max(0, amount);

        if (old != safeFromFallDistance)
            OnSafeFromFallDistanceChanged?.Invoke(old, safeFromFallDistance);
    }

    /// <summary>
    /// Setting Humanoid's damage scale for falling damage when Humanoid landed on ground and taking damage
    /// </summary>
    /// <param name="amount">Amount must be >=1</param>
    public void SetHumanoidFallDamageMultiplier(float amount)
    {
        float old = fallDamageMultiplier;
        fallDamageMultiplier = Mathf.Max(1, amount);

        if (old != fallDamageMultiplier)
            OnFallDamageMultiplierChanged?.Invoke(old, fallDamageMultiplier);
    }

    /// <summary>
    /// Setting Humanoid's target point for MoveTo function
    /// </summary>
    /// <param name="point">Point must be Vector3</param>
    public void SetHumanoidTargetPoint(Vector3 point)
    {
        targetPoint = point;
        hasTargetPoint = true;
    }

    /// <summary>
    /// Clearing Humanoid's target point history
    /// </summary>
    public void ClearHumanoidTargetPoint()
    {
        hasTargetPoint = false;
    }

    /* getters */

    /// <summary>
    /// Returning Vector3 of move direction of the Humanoid from Local Space
    /// </summary>
    /// <returns>Local-Spaced Vector3 from this Transform</returns>
    public Vector3 GetLocalMoveDirection()
    {
        return transform.InverseTransformDirection(moveDirection);
    }

    /// <summary>
    /// Returning Vector3 of move direction of the Humanoid from Global/World Space
    /// </summary>
    /// <returns>Vector3 of Direction</returns>
    public Vector3 GetGlobalMoveDirection()
    {
        return moveDirection;
    }

    /// <summary>
    /// Returning Vector3 of face direction of the Humanoid that updated from HumanoidMotor
    /// </summary>
    /// <returns>Vector3 of Face Direction</returns>
    public Vector3 GetFacingDirection()
    {
        return facingDirection;
    }

    /// <summary>
    /// Returning amount between 1 and -1 of right vector or X-Axis from the Humanoid's moving direction
    /// </summary>
    /// <returns>Float [1, -1]</returns>
    public float GetForwardAmount()
    {
        return Vector3.Dot(transform.forward, moveDirection);
    }

    /// <summary>
    /// Returning amount between 1 and -1 of forward vetor or Z-Axis from the Humanoid's moving direction
    /// </summary>
    /// <returns>Float [1, -1]</returns>
    public float GetRightAmount()
    {
        return Vector3.Dot(transform.right, moveDirection);
    }

    /* extra methods */

    /// <summary>
    /// Comparing if Humanoid has a conditional status
    /// </summary>
    /// <param name="statusName">statusName must be string</param>
    /// <returns>True/False</returns>
    public bool HasStatus(string statusName)
    {
        return statuses.Contains(statusName);
    }

    /// <summary>
    /// Returning a list of all statuses of Humanoid
    /// </summary>
    /// <returns>List -> Statuses</returns>
    public List<string> GetStatuses()
    {
        return new(statuses);
    }

    /// <summary>
    /// Comparing if Humanoid has a conditional temporary status
    /// </summary>
    /// <param name="statusName">statusName must be string</param>
    /// <returns>True/False</returns>
    public bool HasTemporaryStatus(string statusName)
    {
        return temporaryStatuses.TryGetValue(statusName, out float end) && Time.time < end;
    }

    /// <summary>
    /// Returning a list of all temporary statuses of Humanoid
    /// </summary>
    /// <returns>List -> Temporary Statuses</returns>
    public List<string> GetTemporaryStatuses()
    {
        return new(temporaryStatuses.Keys);
    }

    /// <summary>
    /// Adding a new status into Humanoid, permanent status 
    /// </summary>
    /// <param name="statusName">statusName must be string</param>
    public void AddStatus(string statusName)
    {
        if (statuses.Contains(statusName)) return;
        statuses.Add(statusName);

        OnStatusAdded?.Invoke(statusName);
    }

    /// <summary>
    /// Adding a new temporary status into Humanoid, that the status will be removed from Humanoid when timed out
    /// </summary>
    /// <param name="statusName">statusName must be string</param>
    /// <param name="timeout">timeout must be float</param>
    public void AddTemporaryStatus(string statusName, float timeout)
    {
        temporaryStatuses[statusName] = Time.time + timeout;
        OnTemporaryStatusAddedOrChanged?.Invoke(statusName, timeout);
    }

    /// <summary>
    /// Removing the permanent status from Humanoid
    /// </summary>
    /// <param name="statusName">statusName must be string</param>
    public void RemoveStatus(string statusName)
    {
        if (statuses.Contains(statusName))
        {
            statuses.Remove(statusName);
            OnStatusRemoved?.Invoke(statusName);
        }
    }

    /// <summary>
    /// Removing the temporary status from Humanoid
    /// </summary>
    /// <param name="statusName">statusName must be string</param>
    public void RemoveTemporaryStatus(string statusName)
    {
        if (temporaryStatuses.ContainsKey(statusName))
        {
            temporaryStatuses.Remove(statusName);
            OnTemporaryStatusRemoved?.Invoke(statusName);
        }
    }

    /// <summary>
    /// Replacing a remained permanent status from Humanoid with newer status
    /// </summary>
    /// <param name="fromStatus">fromStatus must be remained status</param>
    /// <param name="toStatus">toStatus must be new status</param>
    public void ChangeStatus(string fromStatus, string toStatus)
    {
        if (!statuses.Contains(fromStatus) || statuses.Contains(toStatus))
            return;
        
        statuses.Remove(fromStatus);
        statuses.Add(toStatus);

        OnStatusChanged?.Invoke(fromStatus, toStatus);
    }
    #endregion

    #region API
    
    /// <summary>
    /// Setting the face direction of Humanoid to desired vector direction
    /// </summary>
    /// <param name="direction">Direction must be Vector3</param>
    /// <param name="rotate">True/False that affects for Humanoid to rotate or not</param>
    public void FaceDirection(Vector3 direction, bool rotate)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return;
        
        direction.y = 0;

        // double-check after set y of direction to 0
        if (direction.sqrMagnitude <= 0.001f)
            return;

        facingDirection = direction.normalized;

        if (rotate)
        {
            Quaternion lookDir = Quaternion.LookRotation(facingDirection, Vector3.up);
            rigidBody.MoveRotation(lookDir);
        }
    }

    #endregion

    #region UnityHelpers
    private void Awake()
    {
        if (rigidBody == null) rigidBody = GetComponent<Rigidbody>();
        if (rootPart == null) rootPart = transform;
        if (bodyCollider == null) bodyCollider = GetComponent<Collider>(); 

        health = maxHealth;
        walkSpeed = maxWalkSpeed;
        runningSpeed = maxRunningSpeed;
        jumpPower = maxJumpPower;
        stamina = maxStamina;

        isAlive = health > 0;
        isMoving = moveDirection.sqrMagnitude > 0.001f;

        lastStaminaUseTime = 0;
    }

    private void Update()
    {
        // character
        HandleHealthRegeneration();
        HandleStaminaRegeneration();

        // status
        HandleTemporaryStatus();
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
