using System;
using UnityEditor.Rendering;
using UnityEngine;

[Serializable, RequireComponent(typeof(Humanoid)), RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
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
    [SerializeField] private Transform headCheck;
    [SerializeField] private float bodyHeight = 2.0f;

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

    [Header("Feet")]
    [SerializeField] private LayerMask feetLayer;
    [SerializeField] private float maxSlopeAngle = 45.0f;
    [SerializeField] private float checkRadius = 0.35f;
    [SerializeField] private float checkDistance = 0.65f;
    [SerializeField] private float groundedStickForce = 8.0f;
    [SerializeField] private float slopeSlideAcceleration = 18.0f;
    [SerializeField] private float ignoreGroundAfterJump = 0.08f;
    [SerializeField] private float feetSkin = 0.05f;

    [Header("Head")]
    [SerializeField] private LayerMask headLayer;
    [SerializeField] private float headRadius = 0.35f;
    [SerializeField] private float headMaxDistance = 0.65f;
    [SerializeField] private float headSkin = 0.075f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2.2f;
    [SerializeField] private float jumpCooldown = 0.45f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchWalkingSpeed = 5.0f;
    [SerializeField] private float crouchTransitionSpeed = 6.0f;
    [SerializeField] private float uncrouchTransitionSpeed = 7.0f;
    [SerializeField] private float crouchFuzzyEquivalence = 0.1f;

    [Header("Crouch Agent")]
    [SerializeField] private bool crouchAgent = false;
    [SerializeField] private bool cantUncrouchOverCeiling = false;
    [SerializeField] private bool autoScaleOverCeiling = false;
    [SerializeField] private float autoScaleCrouchMultiplier = 0.2f;
    [SerializeField] private float autoScaleSmoothCrouch = 0.12f;
    [SerializeField] private float autoScaleMaxSpeed = 8f;

    [Header("Prone")]
    [SerializeField] private float proneHeight = 0.5f;
    [SerializeField] private float proneWalkingSpeed = 3.0f;
    [SerializeField] private float proneTransitionSpeed = 6.0f;
    [SerializeField] private float unproneTransitionSpeed = 7.0f;
    [SerializeField] private float proneFuzzyEquivalence = 0.1f;


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

    private Vector3 _moveInput;
    private Vector3 _targetMoveDirection;
    private Vector3 groundNormal;
    private Vector3 moveDirection;
    private Vector3 lastMoveDirection;

    private Vector3 _standingCenter;

    private PhysicsMaterial floorMaterial;
    private Collider floorCollider;

    private bool _runReady;
    private bool _jumpRequested;

    private bool _setCrouching;
    private bool _agentForceSetCrouching; 

    private bool _setProning;
    private bool _agentForceSetProning;

    private bool jumpHolding;
    private bool motorEnabled = true;

    private bool _crouched = false;
    private bool _proned = false;

    private bool _cantUncrouch = false;

    private bool _crouchStandCenterObtained = false;
    private bool _proneStandCenterObtained = false;


    private bool isGrounded;
    private bool isCeilingAbove;
    private bool isOnSlope;
    private bool isSliding;

    private float fallStartY;
    private float fallDistance;

    private float _ceilingDistance;

    private float _lastGroundedTime;
    private float _lastCeilingAboveTime;
    private float _lastJumpRequestTime;
    private float _lastJumpTime;
    private float _lastCrouchTime;
    private float _lastProneTime;
    private float _staminaUsedTimer;
    private float _groundIgnoreTimer;
    private float _idleStayingTimer;

    private float _idleStayingCounter;

    private int _movementLockCount;
    private int _jumpLockCount;

    private float _currentCrouchHeight;

    private bool canMove;
    private bool canJump;
    private bool canCrouch;
    private bool canProne;

    #endregion

    #region Events

    public event Action Grounded;
    public event Action Landed;
    public event Action Sliding;
    public event Action UnCrouched;
    public event Action UnProned;

    public event Action Crouching;
    public event Action Proning;
    
    public event Action OnCrouchBegin;
    public event Action OnProneBegin;
    public event Action OnAirborneBegin;
    public event Action OnFreeFallingBegin;
    public event Action CeilingAboveHeadEnter;
    public event Action CeilingAboveHead;
    public event Action CeilingAboveHeadExit;

    public event Action OnAirborne;
    public event Action OnFreeFalling;
    public event Action OnJumping;

    public event Action<Vector3> OnWalking;
    public event Action<Vector3> OnRunning;
    public event Action<Vector3> OnRotating;

    public event Action<float> Idle;
 
    #endregion

    #region ReadReferences

    public Humanoid Core => humanoid;

    public Vector3 GroundNormal => groundNormal;
    public Vector3 MoveDirection => moveDirection;
    public Vector3 LastMoveDirection => lastMoveDirection;
    public Vector3 WindVelocity => windVelocity;

    public Rigidbody RigidBody => rigidBody;
    public Transform RootPart => rootPart;
    public Collider BodyCollider => bodyCollider;
    public Transform GroundCheck => groundCheck;

    public PhysicsMaterial FloorMaterial => floorMaterial;
    public Collider FloorCollider => floorCollider;

    public LayerMask FeetLayer => feetLayer;
    public LayerMask HeadLayer => headLayer;

    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
    public float AirAcceleration => airAcceleration;
    public float AirDeceleration => airDeceleration;
    public float MovementStrength => movementStrength;

    public float RotationSpeed => rotationSpeed;

    public float MaxSlopeAngle => maxSlopeAngle;
    public float CheckRadius => checkRadius;
    public float CheckDistance => checkDistance;
    public float GroundedStickForce => groundedStickForce;
    public float SlopeSlideAcceleration => slopeSlideAcceleration;
    public float IgnoreGroundAfterJump => ignoreGroundAfterJump;

    public float JumpHeight => jumpHeight; 
    public float JumpCooldown => jumpCooldown;
    public float CoyoteTime => coyoteTime;
    public float JumpBufferTime => jumpBufferTime;
    public float JumpCutMultiplier => jumpCutMultiplier;

    public float GravityScale => gravityScale;
    public float FallingGravityMultiplier => fallingGravityMultiplier;
    public float LowJumpGravityMultiplier => lowJumpGravityMultiplier;
    public float MaxFallingSpeed => maxFallingSpeed;
    public float AirResistance => airResistance;
    public float WindInfluence => windInfluence;

    public float StepHeight => stepHeight;
    public float StepCheckDistance => stepCheckDistance;
    public float StepSmoothness => stepSmoothness;

    public float FallStartY => fallStartY;
    public float FallDistance => fallDistance;

    public bool MomentumOnAir => momentumOnAir;
    public bool AutoRotate => autoRotate;
    public bool OnlyRotateByMoving => onlyRotateByMoving;

    public bool EnableStepUp => enableStepUp;

    public bool JumpHolding => jumpHolding;
    public bool MotorEnabled => motorEnabled;

    public bool IsGrounded => isGrounded;
    public bool IsOnSlope => isOnSlope;
    public bool IsSliding => isSliding;
    public bool IsCeilingAbove => isCeilingAbove;

    #endregion

    #region APIs

    /// <summary>
    /// Moving the Humanoid with desired or target direction from the world space, based to Unity's physics from Humanoid
    /// </summary>
    /// <param name="Direction">World-Space Direction, must be Vector3</param>
    /// <param name="Running">True/False for running</param>
    public void Move(Vector3 Direction, bool Running)
    {
        if (!motorEnabled || !canMove)
            return;

        Vector3 dir = Direction;
        dir.y = 0f;

        bool canRun = Running &&
            humanoid.StateType != HumanoidStateType.Crouch &&
            humanoid.StateType != HumanoidStateType.Prone &&
            humanoid.Stamina > humanoid.StaminaDecrementAmount;

        if (dir.sqrMagnitude > 0.05f)
            dir.Normalize();
        else
            dir = Vector3.zero;

        _moveInput = dir;
        _runReady = canRun;
    }

    /// <summary>
    /// Helper: stop moving the motor of Humanoid
    /// </summary>
    public void StopMove()
    {
        _moveInput = Vector3.zero;
        _runReady = false;
    }

     /// <summary>
    /// Moving Humanoid to desired location with no running condition
    /// </summary>
    /// <param name="Location">Location must be Vector3</param>
    /// <returns>True/False</returns>
    public bool MoveTo(Vector3 Location)
    {
        return MoveTo(Location, false);
    }

    /// <summary>
    /// Moving Humanoid to desired location with optional running condition or not
    /// </summary>
    /// <param name="Location">Location must be Vector3</param>
    /// <param name="Running">Will Humanoid run or not? (True/False)</param>
    /// <returns>True/False</returns>
    public bool MoveTo(Vector3 Location, bool Running)
    {
        if (!canMove)
            return false;

        humanoid.SetHumanoidTargetPoint(Location);

        Vector3 distance = Location - rootPart.position;
        distance.y = 0;

        float stoppingDistance2 = humanoid.WalkToStoppingDistance;
        stoppingDistance2 *= humanoid.WalkToStoppingDistance;

        if (distance.sqrMagnitude <= stoppingDistance2)
        {
            humanoid.ClearHumanoidTargetPoint();
            humanoid.StopMovement();

            return true;
        }

        Move(distance.normalized, Running);
        return true;
    }

    /// <summary>
    /// Commanding Humanoid to jump from the ground
    /// </summary>
    public void Jump()
    {
        if (!motorEnabled || !canJump)
            return;

        _jumpRequested = true;
        jumpHolding = true;
        _lastJumpRequestTime = Time.time;
    }

    /// <summary>
    /// Releasing from jumping repeatedly
    /// </summary>
    public void StopJumping()
    {
        jumpHolding = false;
    }
    
    /// <summary>
    /// Setting Humanoid's collider to relative crouching height
    /// </summary>
    public void Crouch()
    {
        if (!motorEnabled || !canCrouch)
            return;

        _setCrouching = true;
        _lastCrouchTime = Time.time;
    }

    /// <summary>
    /// Stopping the crouch session of Humanoid
    /// </summary>
    public void UnCrouch()
    {
        if (_agentForceSetCrouching || _cantUncrouch) return;

        _setCrouching = false;
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

    /// <summary>
    /// Locking current movement of Humanoid
    /// </summary>
    public void LockMovement()
    {
        _movementLockCount++;
    }

    /// <summary>
    /// Unlocking the current movement of Humanoid
    /// </summary>
    public void UnlockMovement()
    {
        _movementLockCount = Mathf.Max(0, _movementLockCount - 1);
    }

    /// <summary>
    /// Locking jump condition of Humanoid
    /// </summary>
    public void LockJump()
    {
        _jumpLockCount++;
    }

    /// <summary>
    /// Unlocking the jump condition of Humanoid
    /// </summary>
    public void UnlockJump()
    {
        _jumpLockCount = Mathf.Max(0, _jumpLockCount - 1);
    }

    /// <summary>
    /// Facing Humanoid to desired direction in the world
    /// </summary>
    /// <param name="Direction">Direction must be Vector3</param>
    public void FaceDirection(Vector3 Direction)
    {
        Vector3 direction = Direction;
        direction.y = 0;

        if (onlyRotateByMoving && direction.sqrMagnitude <= 0.001f)
            return;

        if (direction.sqrMagnitude <= 0.001f)
            direction = humanoid.FacingDirection;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        humanoid.FaceDirection(direction.normalized, false);

        OnRotating?.Invoke(direction);
    }
    
    #endregion

    #region InternalHelpers

    private float GetT(float speed, float multiplier)
    {
        multiplier = Mathf.Max(0.01f, multiplier);
        return 1.0f - Mathf.Exp(-speed * multiplier * Time.fixedDeltaTime);
    }

    private void CeilingIsAboveHelper()
    {
        bool crouchAgentAllowed = false;
        if (crouchAgent && canCrouch)
            crouchAgentAllowed = true;

        if (crouchAgentAllowed && autoScaleOverCeiling)
            _agentForceSetCrouching = true;

        if (crouchAgentAllowed && cantUncrouchOverCeiling)
            _cantUncrouch = true;
        
        CeilingAboveHead?.Invoke();
        isCeilingAbove = true;
    }

    private void CeilingIsntAboveHelper()
    {
        bool crouchAgentAllowed = false;
        if (crouchAgent && canCrouch)
            crouchAgentAllowed = true;

        if (crouchAgentAllowed && cantUncrouchOverCeiling)
            _cantUncrouch = false;

        if (crouchAgentAllowed && autoScaleOverCeiling)
            _agentForceSetCrouching = false;

        if (isCeilingAbove)
            CeilingAboveHeadExit?.Invoke();
        
        _ceilingDistance = Mathf.Infinity;
        isCeilingAbove = false;
    }

    private float GetAutoScaleCrouchHeight(float ceilingDistance)
    {
        float availableHeight = ceilingDistance - headSkin;
        float rawHeight = Mathf.Clamp(availableHeight, crouchHeight, bodyHeight);

        _currentCrouchHeight = _currentCrouchHeight <= 0f ? bodyHeight : _currentCrouchHeight;

        float speed = rawHeight < _currentCrouchHeight ?
            crouchTransitionSpeed : uncrouchTransitionSpeed;
        
        _currentCrouchHeight = Mathf.MoveTowards(
            _currentCrouchHeight,
            rawHeight,
            speed * Time.fixedDeltaTime
        );

        return _currentCrouchHeight;
    }

    private bool TryCrouch(Collider collider, float standingHeight, float crouchHeight)
    {
        switch (collider)
        {
            case CapsuleCollider capsule:
            {
                float targetHeight = crouchHeight;

                if (!_crouchStandCenterObtained)
                {
                    _standingCenter = capsule.center;
                    _crouchStandCenterObtained = true;
                }
                float standingBottom = standingHeight - targetHeight;
                float targetY = _standingCenter.y - (standingBottom / 2);

                float t = GetT(crouchTransitionSpeed, autoScaleCrouchMultiplier);
                Vector3 targetCenter = new Vector3(_standingCenter.x, targetY, _standingCenter.z);

                float lerpHeight = Mathf.Lerp(capsule.height, targetHeight, t);
                Vector3 lerpCenter = Vector3.Lerp(capsule.center, targetCenter, t);

                capsule.height = lerpHeight;
                capsule.center = lerpCenter;

                bool fuzzyEq = 
                    Mathf.Abs(capsule.height - targetHeight) < crouchFuzzyEquivalence &&
                    Vector3.Distance(capsule.center, targetCenter) < crouchFuzzyEquivalence;
                
                if (fuzzyEq)
                {
                    capsule.height = targetHeight;
                    capsule.center = targetCenter;        
                }
                return true;
            }
            
            case BoxCollider box:
            {
                float targetHeight = crouchHeight;

                if (!_crouchStandCenterObtained)
                {
                    _standingCenter = box.center;
                    _crouchStandCenterObtained = true;        
                }
                float standingBottom = standingHeight - targetHeight;
                float targetY = _standingCenter.y - (standingBottom / 2);

                float t = GetT(crouchTransitionSpeed, autoScaleCrouchMultiplier);
                Vector3 targetCenter = new Vector3(_standingCenter.x, targetY, _standingCenter.z);
                
                float lerpHeight = Mathf.Lerp(box.size.y, targetHeight, t);
                Vector3 lerpCenter = Vector3.Lerp(box.center, targetCenter, t);

                Vector3 lerpSize = box.size;
                lerpSize.y = lerpHeight;

                box.size = lerpSize;
                box.center = lerpCenter;

                bool fuzzyEq = 
                    Mathf.Abs(box.size.y - targetHeight) < crouchFuzzyEquivalence &&
                    Vector3.Distance(box.center, targetCenter) < crouchFuzzyEquivalence;
                
                if (fuzzyEq)
                {
                    box.size = new Vector3(box.size.x, targetHeight, box.size.z);
                    box.center = targetCenter;        
                }

                
                return true;
            }

            case SphereCollider sphere:
            {
                float targetHeight = crouchHeight;
                float targetRadius = targetHeight / 2f;

                if (!_crouchStandCenterObtained)
                {
                    _standingCenter = sphere.center;
                    _crouchStandCenterObtained = true;
                }
                float standingBottom = standingHeight / 2;
                float radius_Delta = standingBottom - targetRadius;

                float targetY = _standingCenter.y - radius_Delta;

                float t = GetT(crouchTransitionSpeed, autoScaleCrouchMultiplier);
                Vector3 targetCenter = new Vector3(_standingCenter.x, targetY, _standingCenter.z);

                float lerpHeight = Mathf.Lerp(sphere.radius, targetRadius, t);
                Vector3 lerpCenter = Vector3.Lerp(sphere.center, targetCenter, t);

                sphere.radius = lerpHeight;
                sphere.center = lerpCenter;

                bool fuzzyEq =
                    Mathf.Abs(sphere.radius - targetHeight) < crouchFuzzyEquivalence &&
                    Vector3.Distance(sphere.center, targetCenter) < crouchFuzzyEquivalence;

                if (fuzzyEq)
                {
                    sphere.radius = targetHeight;
                    sphere.center = targetCenter;        
                }

                return true;
            }

            default:
                return false;
        }
    }

    private bool TryUncrouch(Collider collider, float standingHeight)
    {
        switch (collider)
        {
            case CapsuleCollider capsule:
            {
                float targetHeight = standingHeight;
                Vector3 targetCenter = _standingCenter;
                
                float t = GetT(uncrouchTransitionSpeed, autoScaleCrouchMultiplier);

                float lerpHeight = Mathf.Lerp(capsule.height, targetHeight, t);
                Vector3 lerpCenter = Vector3.Lerp(capsule.center, targetCenter, t);

                capsule.height = lerpHeight;
                capsule.center = lerpCenter;

                bool fuzzyEq = 
                    Mathf.Abs(capsule.height - targetHeight) < crouchFuzzyEquivalence &&
                    Vector3.Distance(capsule.center, targetCenter) < crouchFuzzyEquivalence;
                
                if (fuzzyEq)
                {
                    capsule.height = standingHeight;
                    capsule.center = targetCenter;

                    _crouchStandCenterObtained = false;        
                }
                return true;
            }
            
            case BoxCollider box:
            {
                float targetHeight = standingHeight;
                Vector3 targetCenter = _standingCenter;

                float t = GetT(uncrouchTransitionSpeed, autoScaleCrouchMultiplier);

                float lerpHeight = Mathf.Lerp(box.size.y, targetHeight, t);
                Vector3 lerpCenter = Vector3.Lerp(box.center, targetCenter, t);

                Vector3 lerpSize = box.size;
                lerpSize.y = lerpHeight;

                box.size = lerpSize;
                box.center = lerpCenter;

                bool fuzzyEq =
                    Mathf.Abs(box.size.y - targetHeight) < crouchFuzzyEquivalence &&
                    Vector3.Distance(box.center, targetCenter) < crouchFuzzyEquivalence;
                
                if (fuzzyEq)
                {
                    box.size = new Vector3(box.size.x, targetHeight, box.size.z);
                    box.center = targetCenter;

                    _crouchStandCenterObtained = false;        
                }

                return true;
            }

            case SphereCollider sphere:
            {
                float targetHeight = standingHeight;
                Vector3 targetCenter = _standingCenter;

                float t = GetT(uncrouchTransitionSpeed, autoScaleCrouchMultiplier);

                float lerpHeight = Mathf.Lerp(sphere.radius, targetHeight, t);
                Vector3 lerpCenter = Vector3.Lerp(sphere.center, targetCenter, t);

                sphere.radius = lerpHeight;
                sphere.center = lerpCenter;

                bool fuzzyEq =
                    Mathf.Abs(sphere.radius - targetHeight) < crouchFuzzyEquivalence &&
                    Vector3.Distance(sphere.center, targetCenter) < crouchFuzzyEquivalence;
                
                if (fuzzyEq)
                {
                    sphere.radius = targetHeight;
                    sphere.center = targetCenter;

                    _crouchStandCenterObtained = false;        
                }

                return true;
            }

            default:
                return false;
        }
    }

    private void ApplyGroundState(bool grounded)
    {
        isGrounded = grounded;

        if (!grounded)
        {
            floorCollider = null;
            floorMaterial = null;
            groundNormal = Vector3.up;

            isOnSlope = false;
            isSliding = false;
        }
    }

    private void ApplyGroundInfo(RaycastHit result)
    {
        floorCollider = result.collider;
        floorMaterial = result.collider.sharedMaterial;
        groundNormal = result.normal;

        float angle = Vector3.Angle(groundNormal, Vector3.up);
        isOnSlope = angle > 1.0f;
        isSliding = angle > maxSlopeAngle;

        isGrounded = true;
        _lastGroundedTime = Time.time;

    }

    private void ApplySimpleGroundInfo()
    {
        floorCollider = null;
        floorMaterial = null;
        groundNormal = Vector3.up;

        isOnSlope = false;
        isSliding = false;

        isGrounded = true;
        _lastGroundedTime = Time.time;

    }

    #endregion

    #region PrivateHelpers

    private void ApplyFallDamage()
    {
        if (!humanoid.CanApplyFallDamage || !humanoid.IsAlive)
            return;
        
        if (humanoid.SafeFromFallDistance >= fallDistance)
            return;

        float totalDamage = (fallDistance - humanoid.SafeFromFallDistance) * humanoid.FallDamageMultiplier;
        humanoid.TakeDamage(totalDamage);
    }

    private void HandleCrouching()
    {
        bool handleCrouch = _setCrouching || _agentForceSetCrouching;

        if (handleCrouch)
        {
            float targetHeight = crouchHeight;

            if (_agentForceSetCrouching && autoScaleOverCeiling && isCeilingAbove)
                targetHeight = GetAutoScaleCrouchHeight(_ceilingDistance);

            bool crouch = TryCrouch(bodyCollider, bodyHeight, targetHeight);

            if (crouch && !_crouched)
                OnCrouchBegin?.Invoke();
            
            _crouched = true;
            Crouching?.Invoke();

            humanoid.ChangeState(HumanoidStateType.Crouch);
            humanoid.SetHumanoidIsCrouching(true);
        }
        else
        {
            bool uncrouch = TryUncrouch(bodyCollider, bodyHeight);

            if (uncrouch && _crouched)
                UnCrouched?.Invoke();
            
            _crouched = false;

            humanoid.ChangeState(HumanoidStateType.Idle);
            humanoid.SetHumanoidIsCrouching(false);
        }
    }

    private void HandleFallTracking()
    {
        if (!isGrounded && rigidBody.linearVelocity.y > 1f)
        {
            if (humanoid.StateType != HumanoidStateType.Airborne)
            {
                fallStartY = rootPart.position.y;
                humanoid.ChangeState(HumanoidStateType.Airborne);

                isGrounded = false;
                OnAirborneBegin?.Invoke();
            }

            fallDistance = rootPart.position.y - fallStartY;
            OnAirborne?.Invoke();
        }

        if (!isGrounded && rigidBody.linearVelocity.y < -1f)
        {
            if (humanoid.StateType == HumanoidStateType.Airborne && humanoid.StateType != HumanoidStateType.FreeFalling)
            {
                humanoid.ChangeState(HumanoidStateType.FreeFalling);

                OnFreeFallingBegin?.Invoke();
            }

            OnFreeFalling?.Invoke();
        }

        if (isGrounded)
        {
            if (humanoid.StateType == HumanoidStateType.FreeFalling)
            {
                ApplyFallDamage();
                humanoid.ChangeState(HumanoidStateType.Grounded);

                isGrounded = true;

                Landed?.Invoke();
            }
            else
            {
                if (humanoid.StateType != HumanoidStateType.Airborne)
                {
                    if (humanoid.StateType != HumanoidStateType.Crouch && humanoid.StateType != HumanoidStateType.Prone)
                    {
                        if (_targetMoveDirection.sqrMagnitude <= 0.001f)
                        {
                            humanoid.ChangeState(HumanoidStateType.Idle);
                            humanoid.SetHumanoidIsMoving(false);

                            if (Time.time - _idleStayingTimer >= 1)
                            {
                                _idleStayingCounter += 1;
                                _idleStayingTimer = Time.time;
                            }

                            Idle?.Invoke(_idleStayingCounter);
                        }
                    }
                }
            }
        }
    }

    private void CheckGround()
    {
        bool oldGrounded = isGrounded;

        if (Time.time < _groundIgnoreTimer)
        {
            ApplyGroundState(false);
            return;
        }

        Vector3 multiplier = Vector3.up * (checkRadius + feetSkin);
        Vector3 preOrigin = groundCheck != null ?
            groundCheck.position : rootPart.position;

        Vector3 origin = preOrigin + multiplier;

        RaycastHit hitInfo;
        bool isChecked = Physics.SphereCast
        (
            origin,
            checkRadius,
            Vector3.down,
            out hitInfo,
            checkDistance,
            feetLayer,
            QueryTriggerInteraction.Ignore
        );

        if (isChecked)
        {
            ApplyGroundInfo(hitInfo);
        }
        else
        {
            isChecked = Physics.CheckSphere
            (
                preOrigin + Vector3.up * feetSkin,
                checkRadius * 0.95f,
                feetLayer,
                QueryTriggerInteraction.Ignore
            );

            if (isChecked)
            {
                RaycastHit alternateHit;
                bool ray = Physics.Raycast(origin, Vector3.down, out alternateHit, checkDistance + checkRadius + feetSkin, feetLayer, QueryTriggerInteraction.Ignore);

                if (ray)
                    ApplyGroundInfo(alternateHit);
                else
                    ApplySimpleGroundInfo();
            }
        }

        if (!isChecked)
            ApplyGroundState(false);

        if (!oldGrounded && isGrounded)
            Grounded?.Invoke();
    }

    private void CheckHead()
    {
        bool oldCeilingAbove = isCeilingAbove;

        if (Time.time < _lastCeilingAboveTime)
        {
            CeilingIsntAboveHelper();
            return;
        }

        Vector3 multiplier = Vector3.down * (headRadius + headSkin);
        Vector3 preOrigin = headCheck != null ?
            headCheck.position : rootPart.position;

        Vector3 origin = multiplier + preOrigin;

        RaycastHit hitInfo;
        bool isChecked = Physics.SphereCast(
            origin,
            headRadius,
            Vector3.up,
            out hitInfo,
            headMaxDistance,
            headLayer,
            QueryTriggerInteraction.Ignore
        );

        if (isChecked)
        {
            _ceilingDistance = hitInfo.distance;
            CeilingIsAboveHelper();
        }
        else
        {
            isChecked = Physics.CheckSphere(
                preOrigin + Vector3.up * headSkin,
                headRadius * 0.95f,
                headLayer,
                QueryTriggerInteraction.Ignore
            );

            if (isChecked)
            {
                RaycastHit alternateHit;
                bool ray = Physics.Raycast(origin, Vector3.up, out alternateHit, headMaxDistance + headRadius + headSkin, headLayer, QueryTriggerInteraction.Ignore);

                if (ray)
                {
                    _ceilingDistance = alternateHit.distance;
                    CeilingIsAboveHelper();
                }
                else
                {
                    CeilingIsntAboveHelper();
                }
            }
        }

        if (!isChecked)
            CeilingIsntAboveHelper();

        if (!oldCeilingAbove && isCeilingAbove)
            CeilingAboveHeadEnter?.Invoke();
    }

    private void HandleMovement()
    {
        Vector3 velocity = rigidBody.linearVelocity;
        Vector3 horizontalVelocity = velocity;
        horizontalVelocity.y = 0f;

        _targetMoveDirection = _moveInput;

        if (isGrounded && !isSliding)
            _targetMoveDirection = Vector3.ProjectOnPlane(_targetMoveDirection, groundNormal).normalized;

        bool onInput = _targetMoveDirection.sqrMagnitude > 0.001f;

        _idleStayingCounter = onInput ? 0 : _idleStayingCounter;

        float speed = _runReady ? humanoid.RunningSpeed : humanoid.WalkSpeed;

        // double check speed for crouch
        speed = humanoid.StateType == HumanoidStateType.Crouch ? crouchWalkingSpeed : speed;

        Vector3 targetHorizontalVelocity = onInput ? _targetMoveDirection * speed : Vector3.zero;

        float accel;
        
        if (isGrounded)
            accel = onInput ? acceleration : deceleration;
        else
            accel = onInput ? airAcceleration : airDeceleration;

        if (!isGrounded && momentumOnAir && !onInput)
        {
            targetHorizontalVelocity = Vector3.zero;
            accel = Mathf.Max(airDeceleration, deceleration * 0.35f);
        }

        Vector3 newHorizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetHorizontalVelocity,
            accel * Time.fixedDeltaTime
        );

        velocity.x = newHorizontalVelocity.x;
        velocity.z = newHorizontalVelocity.z;

        rigidBody.linearVelocity = velocity;
        humanoid.SetHumanoidIsMoving(onInput);

        moveDirection = onInput ? _targetMoveDirection.normalized : Vector3.zero;
        if (onInput)
            lastMoveDirection = _targetMoveDirection.normalized;

        if (onInput && 
            humanoid.StateType != HumanoidStateType.Airborne && 
            humanoid.StateType != HumanoidStateType.FreeFalling && 
            humanoid.StateType != HumanoidStateType.Crouch &&
            humanoid.StateType != HumanoidStateType.Prone &&
            isGrounded)
            humanoid.ChangeState(_runReady ? HumanoidStateType.Running : HumanoidStateType.Walking);

        if (_runReady)
        {
            _staminaUsedTimer += Time.deltaTime;

            if (_staminaUsedTimer >= humanoid.StaminaDecrementTick)
            {
                bool usingStamina = humanoid.TryUsingStamina(humanoid.StaminaDecrementAmount);
                _staminaUsedTimer = 0;

                if (!usingStamina)
                    _runReady = false;
            }

            OnRunning?.Invoke(_targetMoveDirection.normalized);
        }
        else
        {
            OnWalking?.Invoke(_targetMoveDirection.normalized);
        }
    }

    private void HandleJump()
    {
        bool hasTimeBuffer = _jumpRequested && Time.time - _lastJumpRequestTime <= jumpBufferTime;
        bool canCoyote = isGrounded || Time.time - _lastGroundedTime <= coyoteTime;
        bool cooldownReady = Time.time - _lastJumpTime >= jumpCooldown;

        if (!cooldownReady || !canCoyote || !hasTimeBuffer)
            return;

        float power = Mathf.Abs(Physics.gravity.y * gravityScale);

        if (power <= 0.01f)
            power = 9.81f;

        float jumpVelocity = Mathf.Sqrt(2f * power * jumpHeight);

        Vector3 velocity = new Vector3(rigidBody.linearVelocity.x, 0, rigidBody.linearVelocity.z);
        velocity.y += jumpVelocity;
        
        humanoid.SetHumanoidIsJumping(true);

        rigidBody.linearVelocity = velocity;

        _jumpRequested = false;
        _lastJumpRequestTime = -999f;

        _lastJumpTime = Time.time;
        _lastGroundedTime = -999f;

        isGrounded = false;
        _groundIgnoreTimer = Time.time + ignoreGroundAfterJump;

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

            Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);

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

        FaceDirection(_targetMoveDirection);
    }

    private void UpdateHumanoidRuntime()
    {
        humanoid.SetHumanoidLinearVelocity(rigidBody.linearVelocity);
        humanoid.SetHumanoidAngularVelocity(rigidBody.angularVelocity);
    }

    private void UpdatePermissionRuntime()
    {
        canMove = humanoid.CanMove && 
            humanoid.IsAlive &&
            !humanoid.PlatformStanding && 
            _movementLockCount <= 0;
        
        canJump = humanoid.CanJump &&
            humanoid.IsAlive &&
            !humanoid.PlatformStanding &&
            humanoid.StateType != HumanoidStateType.Airborne &&
            humanoid.StateType != HumanoidStateType.FreeFalling &&
            humanoid.StateType != HumanoidStateType.Crouch &&
            humanoid.StateType != HumanoidStateType.Prone &&
            isGrounded &&
            _jumpLockCount <= 0;

        canCrouch = humanoid.CanCrouch &&
            humanoid.IsAlive &&
            !humanoid.PlatformStanding &&
            humanoid.StateType != HumanoidStateType.Airborne &&
            humanoid.StateType != HumanoidStateType.FreeFalling &&
            humanoid.StateType != HumanoidStateType.Jumping &&
            humanoid.StateType != HumanoidStateType.Prone &&
            isGrounded;

        canProne = humanoid.CanProne &&
            humanoid.IsAlive &&
            !humanoid.PlatformStanding &&
            humanoid.StateType != HumanoidStateType.Airborne &&
            humanoid.StateType != HumanoidStateType.FreeFalling &&
            humanoid.StateType != HumanoidStateType.Jumping &&
            humanoid.StateType != HumanoidStateType.Crouch &&
            isGrounded;
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

        _idleStayingTimer = Time.time;
    }

    private void FixedUpdate()
    {
        if (!motorEnabled || !humanoid.IsAlive)
            return;
        
        CheckGround();
        CheckHead();

        UpdatePermissionRuntime();

        HandleJump();
        HandleMovement();
        HandleMass();

        HandleFallTracking();

        HandleSlopeSliding();
        HandleRotation();
        HandleCrouching();

        UpdateHumanoidRuntime();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(headCheck.position + Vector3.down * (headRadius * headSkin), headRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(headCheck.position + Vector3.down * headSkin, headRadius * 0.95f);
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
