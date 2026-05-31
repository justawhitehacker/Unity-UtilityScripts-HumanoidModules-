using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Analytics;
using UnityEditor.Search;
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

    
    /* booleans */
    [SerializeField] private bool platformStanding;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool autoRotate = true;
    [SerializeField] private bool healthRegenerationEnabled = true;
    [SerializeField] private bool staminaRegenerationEnabled = true;

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
    private bool isJumping = false;
    private bool hasTargetPoint;

    /* owner type */
    private HumanoidStateType stateType = HumanoidStateType.Neutral;

    /* physics */
    private Collider floorCollider;
    private PhysicsMaterial floorMaterial;
    private Vector3 targetPoint;
    private float fallStartY;
    private float fallDistance;
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
    public Vector3 GlobalMoveDirection => moveDirection;
    public Vector3 LocalMoveDirection { get { return transform.InverseTransformDirection(moveDirection); } }
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
    public float ToGroundHeight => toGroundHeight;

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
    public bool PlatformStanding => platformStanding;
    public bool CanMove => canMove;
    public bool CanJump => canJump;
    public bool AutoRotate => autoRotate;
    public bool IsJumping => isJumping;
    public bool HasTargetPoint => hasTargetPoint;
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
    public event Action<float> Damaged;

    /* Events */
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
    public event Action OnHealthRegenerationEnabledChanged;
    public event Action OnStaminaRegenerationEnabledChanged;
    public event Action<float, float> OnWalkSpeedChanged;
    public event Action<float, float> OnRunningSpeedChanged;
    public event Action<float, float> OnJumpPowerChanged;
    public event Action<Vector3, Vector3> OnCameraOffsetChanged;
    public event Action<HumanoidStateType, HumanoidStateType> OnStateChanged;
    public event Action<HumanoidOwnerType, HumanoidOwnerType> OnOwnerChanged;
    #endregion

    #region Tables
    private readonly Dictionary<KeyCode, Action> actions = new();
    private readonly HashSet<string> statuses = new();
    private readonly HashSet<HumanoidStateType> disableStates = new();
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
            ChangeState(HumanoidStateType.Grounded);
            isGrounded = true;
        }
    }

    private void StopMovement()
    {
        canMove = false;
        canJump = false;
    }

    private void HandleFloorInfo()
    {
        if (Physics.Raycast(rootPart.position, Vector3.down, out RaycastHit info, toGroundHeight))
        {
            floorCollider = info.collider;
            floorMaterial = info.collider.sharedMaterial;
        }
        else
        {
            floorCollider = null;
            floorMaterial = null;
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
        healthRegenerationAmount = Mathf.Max(amount, health);

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
        staminaRegenerationAmount = Mathf.Max(amount, stamina);

        if (old != staminaRegenerationAmount)
            OnStaminaRegenerationAmountChanged?.Invoke(old, staminaRegenerationAmount);
    }

    // Set Humanoid's stamina regeneration tick time
    public void SetHumanoidStaminaRegenerationTick(float amount)
    {
        float old = staminaRegenerationTick;
        staminaRegenerationTick = Mathf.Max(amount, EPS_TIMER);

        if (old != staminaRegenerationTick)
            OnHealthRegenerationTickChanged?.Invoke(old, staminaRegenerationTick);
    }

    // Set Humanoid's stamina regeneration delay
    public void SetHumanoidStaminaRegenerationDelay(float amount)
    {
        float old = staminaRegenerationDelay;
        staminaRegenerationDelay = Mathf.Max(amount, EPS_TIMER);

        if (old != staminaRegenerationDelay)
            OnHealthRegenerationTickChanged?.Invoke(old, staminaRegenerationDelay);
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

    // Set Humanoid's target point helper to a new point
    public void SetHumanoidTargetPoint(Vector3 point)
    {
        targetPoint = point;
        hasTargetPoint = true;
    }

    // Set Humanoid's target point to zero vector but killed the target point boolean info
    public void ClearHumanoidTargetPoint()
    {
        targetPoint = Vector3.zero;
        hasTargetPoint = false;
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

    /* extra methods */

    // add an action with keycode to execute the function
    public void BindAction(KeyCode keyCode, Action func)
    {
        if (func == null)
            return;

        if (actions.ContainsKey(keyCode))
            return;
        
        actions.Add(keyCode, func);
    }

    // return the bool states by comparing the current status with desired status 
    public bool HasStatus(string statusName)
    {
        return statuses.Contains(statusName);
    }

    // adding a new status into Humanoid
    public void AddStatus(string statusName)
    {
        if (statuses.Contains(statusName)) return;
        statuses.Add(statusName);
    }

    // removing an available status from the Humanoid
    public void RemoveStatus(string statusName)
    {
        if (!statuses.Contains(statusName)) return;
        statuses.Remove(statusName);
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
        if (!isAlive)
            return;
        
        if (Direction.sqrMagnitude <= 0.001f)
        {
            ChangeState(HumanoidStateType.Idle);
            return;
        }

        Direction.Normalize();

        float speed = (Running && stamina > 0) ? runningSpeed : walkSpeed;
        Vector3 velocity = Direction * speed;

        velocity.y = rigidBody.linearVelocity.y;
        rigidBody.linearVelocity = velocity;

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
        if (!isAlive)
            return;
        
        if (!isGrounded)
            return;

        Vector3 velocity = rigidBody.linearVelocity;
        velocity.y = 0;
        rigidBody.linearVelocity = velocity;

        rigidBody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

        ChangeState(HumanoidStateType.Jumping);
        OnJumping?.Invoke();
    }

    #endregion

    #region UnityHelpers
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();

        health = maxHealth;
        walkSpeed = maxWalkSpeed;
        runningSpeed = maxRunningSpeed;
        jumpPower = maxJumpPower;
        stamina = maxStamina;
    }

    private void Update()
    {
        foreach (var action in actions)
        {
            if (Input.GetKey(action.Key))
                action.Value?.Invoke();
        }

        HandleFallTracking();
        HandleHealthRegeneration();
        HandleFloorInfo();
    }

    private void FixedUpdate()
    {
        linearVelocity = rigidBody.linearVelocity;
        angularVelocity = rigidBody.angularVelocity;
    }
    #endregion


}
