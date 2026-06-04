using System;
using UnityEngine;

[Serializable] [RequireComponent(typeof(Humanoid))] [RequireComponent(typeof(Rigidbody))] [RequireComponent(typeof(Collider))]
public class HumanoidMotor : MonoBehaviour
{
    #region SerializedPreferences

    [Header("Humanoid")]
     /* Humanoid assigner (IMPORTANT) */
    [SerializeField] private Humanoid humanoid;
    
    [Header("References")]
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Transform rootPart;
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private Transform groundCheck;

    [Header("Movement")]
    [SerializeField] private float acceleration = 45.0f;
    [SerializeField] private float deceleration = 50.0f;
    [SerializeField] private float airAcceleration = 10.0f;
    [SerializeField] private float airDeceleration = 8.0f;
    [SerializeField] private float movementStrength = 15.0f;
    [SerializeField] private bool momentumOnAir = true;

    [Header("Rotator")]
    [SerializeField] private bool autoRotate = false;
    [SerializeField] private float rotationSpeed = 8.0f;
    [SerializeField] private bool onlyRotateByMoving = true;

    [Header("Ground")]
    [SerializeField] private LayerMask layer;
    [SerializeField] private float checkRadius = 0.35f;
    [SerializeField] private float checkDistance = 0.65f;
    [SerializeField] private float groundedStickForce = 8.0f;
    [SerializeField] private float slopeSlideAcceleration = 18.0f;
    [SerializeField] private float ignoreGroundAfterJump = 0.08f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2.2f;
    [SerializeField] private float jumpCooldown = 0.45f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Mass")]
    [SerializeField] private float gravityScale = 1.0f;
    [SerializeField] private float fallingGravityMultiplier = 1.8f;
    [SerializeField] private float lowJumpGravityMultiplier = 2.2f;
    [SerializeField] private float maxFallingSpeed = 50.0f;
    [SerializeField] private float airResistance = 0.05f;
    [SerializeField] private Vector3 windVelocity;
    [SerializeField] private float windInfluence = 0.35f;

    [Header("Obstacle")]
    [SerializeField] private bool enableStepUp = true;
    [SerializeField] private float stepHeight = 0.4f;
    [SerializeField] private float stepCheckDistance = 0.45f;
    [SerializeField] private float stepSmoothness = 12.0f;

    #endregion

    #region UnserializedReferences

    private Vector3 moveInput;
    private Vector3 targetMoveDirection;
    private Vector3 groundNormal;
    private Vector3 moveDirection;
    private Vector3 lastMoveDirection;

    private PhysicsMaterial floorMaterial;
    private Collider floorCollider;

    private bool runReady;
    private bool jumpRequested;
    private bool jumpHolding;
    private bool motorEnabled = true;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isOnSlope;
    private bool isSliding;

    private float lastGroundedTime;
    private float lastJumpRequestTime;
    private float lastJumpTime;
    private float staminaUsedTimer;
    private float groundIgnoreTimer;

    #endregion

    #region Events

    public event Action Grounded;
    public event Action Sliding;

    public event Action OnJumping;

    public event Action<Vector3> OnWalking;
    public event Action<Vector3> OnRunning;

    #endregion

    #region ReadReferences

    #endregion

    #region APIs

    /// <summary>
    /// Moving the Humanoid with desired or target direction from the world space, based to Unity's physics from Humanoid
    /// </summary>
    /// <param name="Direction">World-Space Direction, must be Vector3</param>
    /// <param name="Running">True/False for running</param>
    public void Move(Vector3 Direction, bool Running)
    {
        if (!motorEnabled || !humanoid.IsAlive || !humanoid.CanMove)
            return;

        Vector3 dir = Direction;
        dir.y = 0f;

        bool canRun = Running && humanoid.Stamina > humanoid.StaminaDecrementAmount;

        if (dir.sqrMagnitude > 0.05f)
            dir.Normalize();
        else
            dir = Vector3.zero;

        moveInput = dir;
        runReady = canRun;
    }

    /// <summary>
    /// Helper: stop moving the motor of Humanoid
    /// </summary>
    public void StopMove()
    {
        moveInput = Vector3.zero;
        runReady = false;
    }

    /// <summary>
    /// Commanding Humanoid to jump from the ground
    /// </summary>
    public void Jump()
    {
        if (!motorEnabled || !humanoid.IsAlive || !humanoid.CanJump)
            return;

        jumpRequested = true;
        jumpHolding = true;
        lastJumpRequestTime = Time.time;
    }

    /// <summary>
    /// Releasing from jumping repeatedly
    /// </summary>
    public void StopJumping()
    {
        jumpHolding = false;
    }

    /// <summary>
    /// Giving impulse force to Humanoid
    /// </summary>
    /// <param name="Direction">Direction must be Vector3</param>
    public void AddImpulse(Vector3 Direction)
    {
        humanoid.AddForce(Direction, ForceMode.Impulse);
    }

    /// <summary>
    /// Giving force to Humanoid
    /// </summary>
    /// <param name="Direction">Direction must be Vector3</param>
    public void AddForce(Vector3 Direction)
    {
        humanoid.AddForce(Direction, ForceMode.Force);
    }

    /// <summary>
    /// Translating transform of Humanoid position to desired location
    /// </summary>
    /// <param name="Location">Location must be Vector3</param>
    public void Teleport(Vector3 Location)
    {
        if (!humanoid.IsAlive)
            return;
        
        rigidBody.position = Location;
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
    }
    #endregion

    #region PrivateHelpers

    private void CheckGround()
    {
        bool oldGrounded = isGrounded;

        if (Time.time < groundIgnoreTimer)
        {
            isGrounded = false;
            isOnSlope = false;
            isSliding = false;
            groundNormal = Vector3.up;
            floorMaterial = null;
            floorCollider = null;

            return;
        }

        Vector3 origin;

        if (groundCheck != null)
            origin = groundCheck.position;
        else
            origin = rootPart.position + Vector3.up * 0.1f;

        float distance = checkDistance;

        bool isChecked = Physics.SphereCast(
            origin,
            checkRadius,
            Vector3.down,
            out RaycastHit info,
            distance,
            layer,
            QueryTriggerInteraction.Ignore
        );

        if (isChecked)
        { 
            groundNormal = info.normal;
            floorMaterial = info.collider.sharedMaterial;
            floorCollider = info.collider;

            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            isOnSlope = slopeAngle > 1f;
            isSliding = slopeAngle > humanoid.MaxSlopeAngle;

            isGrounded = true;
            lastGroundedTime = Time.time;

            humanoid.SetHumanoidIsGrounded(true);
        }
        else
        {
            groundNormal = Vector3.up;
            isOnSlope = false;
            isSliding = false;
            isGrounded = false;
            
            groundNormal = Vector3.zero;
            floorMaterial = null;
            floorCollider = null;

            humanoid.SetHumanoidIsGrounded(false);
        }

        Debug.Log($"IsChecked: {isChecked}, Grounded: {isGrounded}, canMove: {humanoid.CanMove}, canJump: {humanoid.CanJump}, move: {moveInput}, vel: {rigidBody.linearVelocity}");


        if (!oldGrounded && isGrounded)
            Grounded?.Invoke();
    }

    private void HandleMovement()
    {
        Vector3 velocity = rigidBody.linearVelocity;
        Vector3 horizontalVelocity = velocity;
        horizontalVelocity.y = 0f;

        targetMoveDirection = moveInput;

        if (targetMoveDirection.sqrMagnitude <= 0.01f)
        {
            if (humanoid.StateType != HumanoidStateType.Airborne || humanoid.StateType != HumanoidStateType.FreeFalling)
            {
                humanoid.ChangeState(HumanoidStateType.Idle);
                humanoid.SetHumanoidIsMoving(false);
            }
        }

        if (isGrounded && !isSliding)
            targetMoveDirection = Vector3.ProjectOnPlane(targetMoveDirection, groundNormal).normalized;

        float speed = runReady ? humanoid.RunningSpeed : humanoid.WalkSpeed;
        Vector3 targetHorizontalVelocity = targetMoveDirection * speed;
        bool onInput = targetHorizontalVelocity.sqrMagnitude > 0.001f;

        float accel;

        if (isGrounded)
            accel = onInput ? acceleration : deceleration;
        else
            accel = onInput ? airAcceleration : airDeceleration;

        if (!isGrounded && momentumOnAir && !onInput)
            targetHorizontalVelocity = horizontalVelocity;

        Vector3 newHorizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetHorizontalVelocity,
            accel * Time.fixedDeltaTime
        );

        velocity.x = newHorizontalVelocity.x;
        velocity.z = newHorizontalVelocity.z;

        rigidBody.linearVelocity = velocity;
        humanoid.SetHumanoidIsMoving(onInput);

        moveDirection = onInput ? targetMoveDirection.normalized : Vector3.zero;
        if (onInput)
            lastMoveDirection = targetMoveDirection.normalized;

        if (humanoid.StateType != HumanoidStateType.Airborne || humanoid.StateType != HumanoidStateType.FreeFalling || isGrounded)
            humanoid.ChangeState(runReady ? HumanoidStateType.Running : HumanoidStateType.Walking);

        if (runReady)
        {
            staminaUsedTimer += Time.deltaTime;

            if (staminaUsedTimer >= humanoid.StaminaDecrementTick)
            {
                bool usingStamina = humanoid.TryUsingStamina(humanoid.StaminaDecrementAmount);
                staminaUsedTimer = 0;

                if (!usingStamina)
                    runReady = false;
            }

            OnRunning?.Invoke(targetMoveDirection.normalized);
        }
        else
        {
            OnWalking?.Invoke(targetMoveDirection.normalized);
        }
    }

    private void HandleJump()
    {
        bool hasTimeBuffer = jumpRequested && Time.time - lastJumpRequestTime <= jumpBufferTime;
        bool canCoyote = isGrounded || Time.time - lastGroundedTime <= coyoteTime;
        bool cooldownReady = Time.time - lastJumpTime >= jumpCooldown;

        if (!cooldownReady || !canCoyote || !hasTimeBuffer)
            return;

        float power = Mathf.Abs(Physics.gravity.y * gravityScale);

        if (power <= 0.01f)
            power = 9.81f;

        float jumpVelocity = Mathf.Sqrt(2f * power * jumpHeight);

        Vector3 velocity = new Vector3(rigidBody.linearVelocity.x, 0, rigidBody.linearVelocity.z);
        velocity.y += jumpVelocity;

        rigidBody.linearVelocity = velocity;

        jumpRequested = false;
        lastJumpRequestTime = -999f;

        lastJumpTime = Time.time;
        lastGroundedTime = -999f;

        isGrounded = false;
        humanoid.SetHumanoidIsGrounded(false);
        groundIgnoreTimer = Time.time + ignoreGroundAfterJump;

        humanoid.ChangeState(HumanoidStateType.Jumping);
        OnJumping?.Invoke();
    }

    private void HandleMass()
    {
        Vector3 velocity = rigidBody.linearVelocity;
        float gravityMultiplier = gravityScale;

        if (isGrounded)
        {
            if (velocity.y < 0f)
                velocity.y = -groundedStickForce;
            
            rigidBody.linearVelocity = velocity;
            return;
        }

        if (velocity.y < 0f)
        {
            gravityMultiplier *= fallingGravityMultiplier;
        }
        else if (velocity.y > 0f && !jumpHolding)
        {
            gravityMultiplier *= lowJumpGravityMultiplier;
            velocity.y *= jumpCutMultiplier;
        }

        Vector3 gravity = Physics.gravity * gravityMultiplier;
        velocity += gravity * Time.fixedDeltaTime;

        if (!isGrounded)
        {
            Vector3 windForce = windVelocity * windInfluence;
            velocity += windForce * Time.fixedDeltaTime;

            Vector3 horizontal = new Vector3(velocity.x, 0, velocity.y);

            horizontal = Vector3.Lerp
            (
                horizontal,
                Vector3.zero,
                airResistance * Time.fixedDeltaTime
            );

            velocity.x = horizontal.x;
            velocity.z = horizontal.z;

            if (velocity.y < -maxFallingSpeed)
                velocity.y = -maxFallingSpeed;

            rigidBody.linearVelocity = velocity;
        }
    }

    private void HandleSlopeSliding()
    {
        if (!isGrounded || !isSliding)
            return;

        Vector3 slidingDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

        rigidBody.AddForce(slidingDirection * slopeSlideAcceleration, ForceMode.Acceleration);

        humanoid.ChangeState(HumanoidStateType.Sliding);
        Sliding?.Invoke();
    }

    private void HandleRotation()
    {
        if (!autoRotate)
            return;

        Vector3 direction = targetMoveDirection;
        direction.y = 0;

        if (onlyRotateByMoving && direction.sqrMagnitude <= 0.001f)
            return;

        if (direction.sqrMagnitude <= 0.001f)
            direction = humanoid.FacingDirection;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        humanoid.FaceDirection(direction.normalized, false);
    }

    private void UpdateHumanoidRuntime()
    {
        humanoid.SetHumanoidLinearVelocity(rigidBody.linearVelocity);
        humanoid.SetHumanoidAngularVelocity(rigidBody.angularVelocity);
    }

    #endregion

    #region UnityHelpers

    private void Awake()
    { 
        humanoid = humanoid == null ? GetComponent<Humanoid>() : humanoid;
        rigidBody = rigidBody == null ? GetComponent<Rigidbody>() : rigidBody;
        bodyCollider = bodyCollider == null ? GetComponent<Collider>() : bodyCollider;
        rootPart = rootPart == null ? transform : rootPart;

        rigidBody.freezeRotation = true;
        rigidBody.useGravity = false;
        rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        if (!motorEnabled || !humanoid.IsAlive)
            return;
        
        wasGrounded = isGrounded;

        CheckGround();

        HandleJump();
        HandleMovement();

        HandleMass();
        HandleSlopeSliding();
        HandleRotation();
        UpdateHumanoidRuntime();
    }

    private void Reset()
    {
        humanoid = GetComponent<Humanoid>();
        rigidBody = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<Collider>();
        rootPart = transform;
    }

    #endregion
};
