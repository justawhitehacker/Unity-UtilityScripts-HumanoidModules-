/***************************************************************************************************************************
 * HumanoidMotor, module for Humanoid movement system of character with utilize physics, Unity. Works for Unity >6.        *
 * Created by Raihan, May 28 - 3 June - 15 June, 2026. v1.0.0                                                              *
 * Free usage, MIT License in GitHub repository.                                                                           *
 *                                                                                                                         *
 * Structures:                                                                                                             *
 * Humanoid                                                                                                                *
 *     \_ HumanoidMotor                                                                                                    *
 *     \_ HumanoidAnimator                                                                                                 *
 *     \_ HumanoidCombat                                                                                                   *
 *     \_ HumanoidInputController                                                                                          *
 *     \_ AIHumanoidHandler                                                                                                *
 *                                                                                                                         *
 * If you have some problems of my modules or giving recommendation, chat:                                                 *
 * @raihanaufal_77 in Instagram.                                                                                           * 
 *                                                                                                                         *
 * But, I'm not too active in media socials. Therefore, you need some times for waiting my responses.                      *
 ***************************************************************************************************************************/
using System;
using UnityEngine;

[Flags]
/// <summary>
/// Locomotion type of Motor of Humanoid
/// </summary>
public enum MotorLocomotion
{
    /// <summary>
    /// Locomotion type where Humanoid is moving by the direction of vector
    /// </summary>
    Move = 1,
    /// <summary>
    /// Locomotion type where Humanoid is moving upwards that affected by physics
    /// </summary>
    Jump = 2,
    /// <summary>
    /// Locomotion type where Humanoid's collider is in half of its collider/standing size
    /// </summary>
    Crouch = 4,
    /// <summary>
    /// Locomotion type where Humanoid's collider is in half of its crouch size
    /// </summary>
    Prone = 8,
    /// <summary>
    /// Locomotion type where Humanoid is moving quickly in ease by the direction of vector
    /// </summary>
    Dash = 16,
    /// <summary>
    /// Locomotion type where Humanoid is able to stepping up on the obstacles
    /// </summary>
    SteppingUp = 32,
    /// <summary>
    /// Locomotion type for default and perfect system for simpel character movement, where Humanoid is able to move and jump
    /// </summary>
    Controller = Move | Jump | SteppingUp,
    /// <summary>
    /// All of locomotion types of Humanoid
    /// </summary>
    Everything = Move | Jump | Crouch | Prone | Dash | SteppingUp
};

/// <summary>
/// HumanoidMotor's module script, as the locomotive structure of this Humanoid or transform based with physics
/// </summary>
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
    [SerializeField] private float floatingVelocity = 2.5f;
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
    [SerializeField] private bool jumpAffectsFall = true;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchWalkingSpeed = 5.0f;
    [SerializeField] private float crouchTransitionSpeed = 6.0f;
    [SerializeField] private float uncrouchTransitionSpeed = 7.0f;
    [SerializeField] private float crouchFuzzyEquivalence = 0.1f;
    [SerializeField] private bool cantUncrouchOverCeiling = false;
    [SerializeField] private float autoScaleCrouchMultiplier = 0.2f;

    [Header("Prone")]
    [SerializeField] private float proneHeight = 0.5f;
    [SerializeField] private float proneWalkingSpeed = 3.0f;
    [SerializeField] private float proneTransitionSpeed = 6.0f;
    [SerializeField] private float unproneTransitionSpeed = 7.0f;
    [SerializeField] private float proneFuzzyEquivalence = 0.1f;
    [SerializeField] private bool cantUnproneOverCeiling = false;
    [SerializeField] private float autoScaleProneMultiplier = 0.2f;

    [Header("Dash")]
    [SerializeField] private bool dashUseCast = true;
    [SerializeField] private bool dashOnlyOnGrounded = true;
    [SerializeField] private bool dashStopMovement = true;
    [SerializeField] private bool dashLinearDashing = true;

    [SerializeField] private LayerMask dashCastMask;

    [SerializeField] private float dashSpeed = 32.0f;
    [SerializeField] private float dashCooldown = 0.8f;
    [SerializeField] private float dashCheckSkin = 0.05f;
    [SerializeField] private float dashDuration = 0.65f;
    [SerializeField] private float dashMinDistance = 0.2f;

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
    [SerializeField] private LayerMask bodyLayer;
    [SerializeField] private float stepHeight = 0.4f;
    [SerializeField] private float stepCheckDistance = 0.45f;
    [SerializeField] private float stepSmoothness = 12.0f;
    [SerializeField] private float lowerGroundHeight = 0.075f;
    [SerializeField] private float stepCheckRadiusMultiplier = 0.3f;
    [SerializeField] private float stepForwardOffset = 0.05f;
    [SerializeField] private float stepTopExtraHeight = 0.1f;
    [SerializeField] private float minStepHeight = 0.03f;

    #endregion

    #region UnserializedReferences

    /// <summary>
    /// Stance Intent include Prone - Crouch - and Standing, LLM
    /// </summary>
    private enum StanceIntent
    {
        Prone,
        Crouch,
        Standing
    };

    private Vector3 _moveInput;
    private Vector3 _targetMoveDirection;
    private Vector3 groundNormal;
    private Vector3 moveDirection;
    private Vector3 lastMoveDirection;
    private Vector3 _dashDirection;

    private Vector3 _thisGravity;

    private Vector3 _standingCenter;

    private PhysicsMaterial floorMaterial;
    private Collider floorCollider;

    private bool _runReady;
    private bool _jumpRequested;
    private bool _isDashing;

    private bool _setCrouching;
    private bool _setProning;

    private bool jumpHolding;
    private bool motorEnabled = true;

    private bool _crouched;
    private bool _proned;

    private bool _cantUncrouch;
    private bool _cantUnprone;

    private bool _standCenterObtained;

    private bool isGrounded;
    private bool isCeilingAbove;
    private bool isOnSlope;
    private bool isSliding;

    private float fallStartY;
    private float fallDistance;

    private float _lastGroundedTime;
    private float _lastJumpRequestTime;
    private float _lastJumpTime;
    private float _dashEndTime;
    private float _lastDashed = -999f;

    private float _staminaUsedTimer;
    private float _groundIgnoreTimer;
    private float _idleStayingTimer;

    private float _idleStayingCounter;

    private int _movementLockCount;
    private int _jumpLockCount;

    private bool canMove;
    private bool canJump;
    private bool canCrouch;
    private bool canProne;
    private bool canDash;
    private bool canStepUp;

    private MotorLocomotion _allowedLocomotions;

    #endregion

    #region Events

    #region StateEvents

    /// <summary>
    /// Called when motor becomes grounded.
    /// </summary>
    public event Action Grounded;

    /// <summary>
    /// Called after landing from free fall.
    /// </summary>
    public event Action Landed;

    /// <summary>
    /// Called while sliding on a steep slope.
    /// </summary>
    public event Action Sliding;

    /// <summary>
    /// Called when crouch transition completes.
    /// </summary>
    public event Action OnCrouchBegin;

    /// <summary>
    /// Called when prone transition completes.
    /// </summary>
    public event Action OnProneBegin;

    /// <summary>
    /// Called when airborne state begins.
    /// </summary>
    public event Action OnAirborneBegin;

    /// <summary>
    /// Called when free fall begins.
    /// </summary>
    public event Action OnFreeFallingBegin;

    /// <summary>
    /// Called when dash starts.
    /// </summary>
    public event Action OnDashBegin;

    /// <summary>
    /// Called when ceiling detection starts.
    /// </summary>
    public event Action CeilingAboveHeadEnter;

    /// <summary>
    /// Called while motor is airborne.
    /// </summary>
    public event Action OnAirborne;

    /// <summary>
    /// Called while motor is free falling.
    /// </summary>
    public event Action OnFreeFalling;

    /// <summary>
    /// Called when jump is performed.
    /// </summary>
    public event Action OnJumping;

    /// <summary>
    /// Called while dash is active.
    /// </summary>
    public event Action OnDashing;

    /// <summary>
    /// Called while prone stance is active.
    /// </summary>
    public event Action Proning;

    /// <summary>
    /// Called while crouch stance is active.
    /// </summary>
    public event Action Crouching;

    /// <summary>
    /// Called while a ceiling is detected.
    /// </summary>
    public event Action CeilingAboveHead;

    /// <summary>
    /// Called when dash ends.
    /// </summary>
    public event Action OnDashEnded;

    /// <summary>
    /// Called when crouch returns to standing.
    /// </summary>
    public event Action UnCrouched;

    /// <summary>
    /// Called when prone returns to standing.
    /// </summary>
    public event Action UnProned;

    /// <summary>
    /// Called when ceiling detection ends.
    /// </summary>
    public event Action CeilingAboveHeadExit;

    /// <summary>
    /// Called while walking with direction.
    /// </summary>
    public event Action<Vector3> OnWalking;

    /// <summary>
    /// Called while running with direction.
    /// </summary>
    public event Action<Vector3> OnRunning;

    /// <summary>
    /// Called while rotating with direction.
    /// </summary>
    public event Action<Vector3> OnRotating;

    /// <summary>
    /// Called while idling with idle counter.
    /// </summary>
    public event Action<float> Idle;

    #endregion

    #region SetterEvents

    #region CoreChangedEvents

    /// <summary>
    /// Called when humanoid reference changes. Params: old value, new value.
    /// </summary>
    public event Action<Humanoid, Humanoid> OnHumanoidChanged;

    #endregion

    #region ReferenceChangedEvents

    /// <summary>
    /// Called when RigidBody changes. Params: old value, new value.
    /// </summary>
    public event Action<Rigidbody, Rigidbody> OnRigidBodyChanged;

    /// <summary>
    /// Called when RootPart changes. Params: old value, new value.
    /// </summary>
    public event Action<Transform, Transform> OnRootPartChanged;

    /// <summary>
    /// Called when BodyCollider changes. Params: old value, new value.
    /// </summary>
    public event Action<Collider, Collider> OnBodyColliderChanged;

    /// <summary>
    /// Called when GroundCheck changes. Params: old value, new value.
    /// </summary>
    public event Action<Transform, Transform> OnGroundCheckChanged;

    /// <summary>
    /// Called when HeadCheck changes. Params: old value, new value.
    /// </summary>
    public event Action<Transform, Transform> OnHeadCheckChanged;

    /// <summary>
    /// Called when BodyHeight changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnBodyHeightChanged;

    #endregion

    #region MovementChangedEvents

    /// <summary>
    /// Called when Acceleration changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnAccelerationChanged;

    /// <summary>
    /// Called when Deceleration changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnDecelerationChanged;

    /// <summary>
    /// Called when AirAcceleration changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnAirAccelerationChanged;

    /// <summary>
    /// Called when AirDeceleration changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnAirDecelerationChanged;

    /// <summary>
    /// Called when MovementStrength changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnMovementStrengthChanged;

    /// <summary>
    /// Called when MomentumOnAir changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnMomentumOnAirChanged;

    #endregion

    #region RotatorChangedEvents

    /// <summary>
    /// Called when AutoRotate changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnAutoRotateChanged;

    /// <summary>
    /// Called when RotationSpeed changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnRotationSpeedChanged;

    /// <summary>
    /// Called when OnlyRotateByMoving changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnOnlyRotateByMovingChanged;

    #endregion

    #region FeetChangedEvents

    /// <summary>
    /// Called when FeetLayer changes. Params: old value, new value.
    /// </summary>
    public event Action<LayerMask, LayerMask> OnFeetLayerChanged;

    /// <summary>
    /// Called when FloatingVelocity changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnFloatingVelocityChanged;

    /// <summary>
    /// Called when MaxSlopeAngle changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnMaxSlopeAngleChanged;

    /// <summary>
    /// Called when CheckRadius changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnCheckRadiusChanged;

    /// <summary>
    /// Called when CheckDistance changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnCheckDistanceChanged;

    /// <summary>
    /// Called when GroundedStickForce changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnGroundedStickForceChanged;

    /// <summary>
    /// Called when SlopeSlideAcceleration changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnSlopeSlideAccelerationChanged;

    /// <summary>
    /// Called when IgnoreGroundAfterJump changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnIgnoreGroundAfterJumpChanged;

    /// <summary>
    /// Called when FeetSkin changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnFeetSkinChanged;

    #endregion

    #region HeadChangedEvents

    /// <summary>
    /// Called when HeadLayer changes. Params: old value, new value.
    /// </summary>
    public event Action<LayerMask, LayerMask> OnHeadLayerChanged;

    /// <summary>
    /// Called when HeadRadius changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnHeadRadiusChanged;

    /// <summary>
    /// Called when HeadMaxDistance changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnHeadMaxDistanceChanged;

    /// <summary>
    /// Called when HeadSkin changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnHeadSkinChanged;

    #endregion

    #region JumpChangedEvents

    /// <summary>
    /// Called when JumpHeight changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnJumpHeightChanged;

    /// <summary>
    /// Called when JumpCooldown changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnJumpCooldownChanged;

    /// <summary>
    /// Called when CoyoteTime changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnCoyoteTimeChanged;

    /// <summary>
    /// Called when JumpBufferTime changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnJumpBufferTimeChanged;

    /// <summary>
    /// Called when JumpCutMultiplier changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnJumpCutMultiplierChanged;

    /// <summary>
    /// Called when JumpAffectsFall changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnJumpAffectsFallChanged;

    #endregion

    #region CrouchChangedEvents

    /// <summary>
    /// Called when CrouchHeight changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnCrouchHeightChanged;

    /// <summary>
    /// Called when CrouchWalkingSpeed changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnCrouchWalkingSpeedChanged;

    /// <summary>
    /// Called when CrouchTransitionSpeed changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnCrouchTransitionSpeedChanged;

    /// <summary>
    /// Called when UncrouchTransitionSpeed changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnUncrouchTransitionSpeedChanged;

    /// <summary>
    /// Called when CrouchFuzzyEquivalence changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnCrouchFuzzyEquivalenceChanged;

    /// <summary>
    /// Called when CantUncrouchOverCeiling changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnCantUncrouchOverCeilingChanged;

    /// <summary>
    /// Called when AutoScaleCrouchMultiplier changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnAutoScaleCrouchMultiplierChanged;

    #endregion

    #region ProneChangedEvents

    /// <summary>
    /// Called when ProneHeight changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnProneHeightChanged;

    /// <summary>
    /// Called when ProneWalkingSpeed changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnProneWalkingSpeedChanged;

    /// <summary>
    /// Called when ProneTransitionSpeed changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnProneTransitionSpeedChanged;

    /// <summary>
    /// Called when UnproneTransitionSpeed changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnUnproneTransitionSpeedChanged;

    /// <summary>
    /// Called when ProneFuzzyEquivalence changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnProneFuzzyEquivalenceChanged;

    /// <summary>
    /// Called when CantUnproneOverCeiling changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnCantUnproneOverCeilingChanged;

    /// <summary>
    /// Called when AutoScaleProneMultiplier changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnAutoScaleProneMultiplierChanged;

    #endregion

    #region DashChangedEvents

    /// <summary>
    /// Called when DashUseCast changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnDashUseCastChanged;

    /// <summary>
    /// Called when DashOnlyOnGrounded changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnDashOnlyOnGroundedChanged;

    /// <summary>
    /// Called when DashStopMovement changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnDashStopMovementChanged;

    /// <summary>
    /// Called when DashLinearDashing changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnDashLinearDashingChanged;

    /// <summary>
    /// Called when DashCastMask changes. Params: old value, new value.
    /// </summary>
    public event Action<LayerMask, LayerMask> OnDashCastMaskChanged;

    /// <summary>
    /// Called when DashSpeed changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnDashSpeedChanged;

    /// <summary>
    /// Called when DashCooldown changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnDashCooldownChanged;

    /// <summary>
    /// Called when DashCheckSkin changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnDashCheckSkinChanged;

    /// <summary>
    /// Called when DashDuration changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnDashDurationChanged;

    /// <summary>
    /// Called when DashMinDistance changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnDashMinDistanceChanged;

    #endregion

    #region MassChangedEvents

    /// <summary>
    /// Called when GravityScale changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnGravityScaleChanged;

    /// <summary>
    /// Called when FallingGravityMultiplier changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnFallingGravityMultiplierChanged;

    /// <summary>
    /// Called when LowJumpGravityMultiplier changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnLowJumpGravityMultiplierChanged;

    /// <summary>
    /// Called when MaxFallingSpeed changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnMaxFallingSpeedChanged;

    /// <summary>
    /// Called when AirResistance changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnAirResistanceChanged;

    /// <summary>
    /// Called when WindVelocity changes. Params: old value, new value.
    /// </summary>
    public event Action<Vector3, Vector3> OnWindVelocityChanged;

    /// <summary>
    /// Called when WindInfluence changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnWindInfluenceChanged;

    #endregion

    #region ObstacleChangedEvents

    /// <summary>
    /// Called when EnableStepUp changes. Params: old value, new value.
    /// </summary>
    public event Action<bool, bool> OnEnableStepUpChanged;

    /// <summary>
    /// Called when BodyLayer changes. Params: old value, new value.
    /// </summary>
    public event Action<LayerMask, LayerMask> OnBodyLayerChanged;

    /// <summary>
    /// Called when StepHeight changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnStepHeightChanged;

    /// <summary>
    /// Called when StepCheckDistance changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnStepCheckDistanceChanged;

    /// <summary>
    /// Called when StepSmoothness changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnStepSmoothnessChanged;

    /// <summary>
    /// Called when LowerGroundHeight changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnLowerGroundHeightChanged;

    /// <summary>
    /// Called when StepCheckRadiusMultiplier changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnStepCheckRadiusMultiplierChanged;

    /// <summary>
    /// Called when StepForwardOffset changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnStepForwardOffsetChanged;

    /// <summary>
    /// Called when StepTopExtraHeight changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnStepTopExtraHeightChanged;

    /// <summary>
    /// Called when MinStepHeight changes. Params: old value, new value.
    /// </summary>
    public event Action<float, float> OnMinStepHeightChanged;

    #endregion

    #endregion

    #endregion

    #region Tables

    private readonly Collider[] _overlappedColliders = new Collider[16];
    private readonly RaycastHit[] _dash_hits = new RaycastHit[16];

    #endregion

    #region ReadReferences

    #region SerializedReadReferences

    #region HumanoidReferences

    /// <summary>
    /// Core Humanoid reference used by this motor.
    /// </summary>
    public Humanoid Core => humanoid;

    #endregion

    #region ReferencesReferences

    /// <summary>
    /// Rigidbody driven by this motor.
    /// </summary>
    public Rigidbody RigidBody => rigidBody;

    /// <summary>
    /// Root transform used as movement origin.
    /// </summary>
    public Transform RootPart => rootPart;

    /// <summary>
    /// Main body collider controlled by this motor.
    /// </summary>
    public Collider BodyCollider => bodyCollider;

    /// <summary>
    /// Optional ground check transform.
    /// </summary>
    public Transform GroundCheck => groundCheck;

    /// <summary>
    /// Optional head check transform.
    /// </summary>
    public Transform HeadCheck => headCheck;

    /// <summary>
    /// Standing body height used by collider resizing.
    /// </summary>
    public float BodyHeight => bodyHeight;

    #endregion

    #region MovementReferences

    /// <summary>
    /// Ground acceleration while moving.
    /// </summary>
    public float Acceleration => acceleration;

    /// <summary>
    /// Ground deceleration while stopping.
    /// </summary>
    public float Deceleration => deceleration;

    /// <summary>
    /// Air acceleration while moving.
    /// </summary>
    public float AirAcceleration => airAcceleration;

    /// <summary>
    /// Air deceleration while stopping in air.
    /// </summary>
    public float AirDeceleration => airDeceleration;

    /// <summary>
    /// Movement strength value for motor tuning.
    /// </summary>
    public float MovementStrength => movementStrength;

    /// <summary>
    /// Whether horizontal momentum is kept while airborne.
    /// </summary>
    public bool MomentumOnAir => momentumOnAir;

    #endregion

    #region RotatorReferences

    /// <summary>
    /// Whether motor rotates automatically toward movement.
    /// </summary>
    public bool AutoRotate => autoRotate;

    /// <summary>
    /// Rotation interpolation speed.
    /// </summary>
    public float RotationSpeed => rotationSpeed;

    /// <summary>
    /// Whether rotation only happens with movement input.
    /// </summary>
    public bool OnlyRotateByMoving => onlyRotateByMoving;

    #endregion

    #region FeetReferences

    /// <summary>
    /// Layer mask used for ground checks.
    /// </summary>
    public LayerMask FeetLayer => feetLayer;

    /// <summary>
    /// Vertical threshold for airborne and falling states.
    /// </summary>
    public float FloatingVelocity => floatingVelocity;

    /// <summary>
    /// Maximum walkable slope angle.
    /// </summary>
    public float MaxSlopeAngle => maxSlopeAngle;

    /// <summary>
    /// Radius used by feet sphere checks.
    /// </summary>
    public float CheckRadius => checkRadius;

    /// <summary>
    /// Distance used by feet ground checks.
    /// </summary>
    public float CheckDistance => checkDistance;

    /// <summary>
    /// Downward force used to keep grounded contact.
    /// </summary>
    public float GroundedStickForce => groundedStickForce;

    /// <summary>
    /// Acceleration applied while sliding on steep slopes.
    /// </summary>
    public float SlopeSlideAcceleration => slopeSlideAcceleration;

    /// <summary>
    /// Ground detection ignore time after jumping.
    /// </summary>
    public float IgnoreGroundAfterJump => ignoreGroundAfterJump;

    /// <summary>
    /// Small skin offset for feet checks.
    /// </summary>
    public float FeetSkin => feetSkin;

    #endregion

    #region HeadReferences

    /// <summary>
    /// Layer mask used for ceiling checks.
    /// </summary>
    public LayerMask HeadLayer => headLayer;

    /// <summary>
    /// Radius used by head sphere checks.
    /// </summary>
    public float HeadRadius => headRadius;

    /// <summary>
    /// Distance used by head ceiling checks.
    /// </summary>
    public float HeadMaxDistance => headMaxDistance;

    /// <summary>
    /// Small skin offset for head checks.
    /// </summary>
    public float HeadSkin => headSkin;

    #endregion

    #region JumpReferences

    /// <summary>
    /// Target jump height.
    /// </summary>
    public float JumpHeight => jumpHeight;

    /// <summary>
    /// Minimum delay between jumps.
    /// </summary>
    public float JumpCooldown => jumpCooldown;

    /// <summary>
    /// Grace time after leaving ground.
    /// </summary>
    public float CoyoteTime => coyoteTime;

    /// <summary>
    /// Grace time for queued jump input.
    /// </summary>
    public float JumpBufferTime => jumpBufferTime;

    /// <summary>
    /// Multiplier used when jump is released early.
    /// </summary>
    public float JumpCutMultiplier => jumpCutMultiplier;

    /// <summary>
    /// Whether jump state affects fall tracking.
    /// </summary>
    public bool JumpAffectsFall => jumpAffectsFall;

    #endregion

    #region CrouchReferences

    /// <summary>
    /// Collider height while crouching.
    /// </summary>
    public float CrouchHeight => crouchHeight;

    /// <summary>
    /// Movement speed while crouching.
    /// </summary>
    public float CrouchWalkingSpeed => crouchWalkingSpeed;

    /// <summary>
    /// Speed for entering crouch.
    /// </summary>
    public float CrouchTransitionSpeed => crouchTransitionSpeed;

    /// <summary>
    /// Speed for leaving crouch.
    /// </summary>
    public float UncrouchTransitionSpeed => uncrouchTransitionSpeed;

    /// <summary>
    /// Tolerance used to finish crouch resizing.
    /// </summary>
    public float CrouchFuzzyEquivalence => crouchFuzzyEquivalence;

    /// <summary>
    /// Whether uncrouch is blocked by ceiling.
    /// </summary>
    public bool CantUncrouchOverCeiling => cantUncrouchOverCeiling;

    /// <summary>
    /// Smoothing multiplier for crouch resizing.
    /// </summary>
    public float AutoScaleCrouchMultiplier => autoScaleCrouchMultiplier;

    #endregion

    #region ProneReferences

    /// <summary>
    /// Collider height while proning.
    /// </summary>
    public float ProneHeight => proneHeight;

    /// <summary>
    /// Movement speed while proning.
    /// </summary>
    public float ProneWalkingSpeed => proneWalkingSpeed;

    /// <summary>
    /// Speed for entering prone.
    /// </summary>
    public float ProneTransitionSpeed => proneTransitionSpeed;

    /// <summary>
    /// Speed for leaving prone.
    /// </summary>
    public float UnproneTransitionSpeed => unproneTransitionSpeed;

    /// <summary>
    /// Tolerance used to finish prone resizing.
    /// </summary>
    public float ProneFuzzyEquivalence => proneFuzzyEquivalence;

    /// <summary>
    /// Whether unprone is blocked by ceiling.
    /// </summary>
    public bool CantUnproneOverCeiling => cantUnproneOverCeiling;

    /// <summary>
    /// Smoothing multiplier for prone resizing.
    /// </summary>
    public float AutoScaleProneMultiplier => autoScaleProneMultiplier;

    #endregion

    #region DashReferences

    /// <summary>
    /// Whether dash checks obstacles with casts.
    /// </summary>
    public bool DashUseCast => dashUseCast;

    /// <summary>
    /// Whether dash is allowed only on ground.
    /// </summary>
    public bool DashOnlyOnGrounded => dashOnlyOnGrounded;

    /// <summary>
    /// Whether dash cancels normal movement.
    /// </summary>
    public bool DashStopMovement => dashStopMovement;

    /// <summary>
    /// Whether dash keeps vertical velocity behavior.
    /// </summary>
    public bool DashLinearDashing => dashLinearDashing;

    /// <summary>
    /// Layer mask used for dash obstacle checks.
    /// </summary>
    public LayerMask DashCastMask => dashCastMask;

    /// <summary>
    /// Horizontal dash speed.
    /// </summary>
    public float DashSpeed => dashSpeed;

    /// <summary>
    /// Minimum delay between dashes.
    /// </summary>
    public float DashCooldown => dashCooldown;

    /// <summary>
    /// Safety skin offset for dash casts.
    /// </summary>
    public float DashCheckSkin => dashCheckSkin;

    /// <summary>
    /// Maximum dash duration.
    /// </summary>
    public float DashDuration => dashDuration;

    /// <summary>
    /// Minimum safe dash distance.
    /// </summary>
    public float DashMinDistance => dashMinDistance;

    #endregion

    #region MassReferences

    /// <summary>
    /// Multiplier applied to physics gravity.
    /// </summary>
    public float GravityScale => gravityScale;

    /// <summary>
    /// Extra gravity multiplier while falling.
    /// </summary>
    public float FallingGravityMultiplier => fallingGravityMultiplier;

    /// <summary>
    /// Extra gravity multiplier for low jumps.
    /// </summary>
    public float LowJumpGravityMultiplier => lowJumpGravityMultiplier;

    /// <summary>
    /// Maximum downward falling speed.
    /// </summary>
    public float MaxFallingSpeed => maxFallingSpeed;

    /// <summary>
    /// Horizontal air resistance while airborne.
    /// </summary>
    public float AirResistance => airResistance;

    /// <summary>
    /// Wind velocity applied while airborne.
    /// </summary>
    public Vector3 WindVelocity => windVelocity;

    /// <summary>
    /// Multiplier for wind influence.
    /// </summary>
    public float WindInfluence => windInfluence;

    #endregion

    #region ObstacleReferences

    /// <summary>
    /// Whether step-up helper is enabled.
    /// </summary>
    public bool EnableStepUp => enableStepUp;

    /// <summary>
    /// Layer mask used for body overlap checks.
    /// </summary>
    public LayerMask BodyLayer => bodyLayer;

    /// <summary>
    /// Maximum height allowed for stepping up.
    /// </summary>
    public float StepHeight => stepHeight;

    /// <summary>
    /// Forward distance for step-up checks.
    /// </summary>
    public float StepCheckDistance => stepCheckDistance;

    /// <summary>
    /// Smoothing speed for step-up movement.
    /// </summary>
    public float StepSmoothness => stepSmoothness;

    /// <summary>
    /// Lower ray height offset for step checks.
    /// </summary>
    public float LowerGroundHeight => lowerGroundHeight;

    /// <summary>
    /// Multiplier for step check radius.
    /// </summary>
    public float StepCheckRadiusMultiplier => stepCheckRadiusMultiplier;

    /// <summary>
    /// Forward offset for step top probing.
    /// </summary>
    public float StepForwardOffset => stepForwardOffset;

    /// <summary>
    /// Extra height used to probe step top.
    /// </summary>
    public float StepTopExtraHeight => stepTopExtraHeight;

    /// <summary>
    /// Minimum height considered as a step.
    /// </summary>
    public float MinStepHeight => minStepHeight;

    #endregion

    #endregion

    #region RuntimeReadReferences

    /// <summary>
    /// Normal vector of the current ground.
    /// </summary>
    public Vector3 GroundNormal => groundNormal;

    /// <summary>
    /// Current normalized move direction.
    /// </summary>
    public Vector3 MoveDirection => moveDirection;

    /// <summary>
    /// Last valid normalized move direction.
    /// </summary>
    public Vector3 LastMoveDirection => lastMoveDirection;

    /// <summary>
    /// Physics material of the current floor.
    /// </summary>
    public PhysicsMaterial FloorMaterial => floorMaterial;

    /// <summary>
    /// Collider of the current floor.
    /// </summary>
    public Collider FloorCollider => floorCollider;

    /// <summary>
    /// Whether jump input is currently held.
    /// </summary>
    public bool JumpHolding => jumpHolding;

    /// <summary>
    /// Whether this motor is currently enabled.
    /// </summary>
    public bool MotorEnabled => motorEnabled;

    /// <summary>
    /// Whether the motor is currently grounded.
    /// </summary>
    public bool IsGrounded => isGrounded;

    /// <summary>
    /// Whether the current ground is a slope.
    /// </summary>
    public bool IsOnSlope => isOnSlope;

    /// <summary>
    /// Whether the motor is sliding on steep ground.
    /// </summary>
    public bool IsSliding => isSliding;

    /// <summary>
    /// Whether a ceiling is detected above the head.
    /// </summary>
    public bool IsCeilingAbove => isCeilingAbove;

    /// <summary>
    /// Runtime permission for movement input.
    /// </summary>
    public bool CanMove => canMove;

    /// <summary>
    /// Runtime permission for jumping.
    /// </summary>
    public bool CanJump => canJump;

    /// <summary>
    /// Runtime permission for crouching.
    /// </summary>
    public bool CanCrouch => canCrouch;

    /// <summary>
    /// Runtime permission for proning.
    /// </summary>
    public bool CanProne => canProne;

    /// <summary>
    /// Runtime permission for dashing.
    /// </summary>
    public bool CanDash => canDash;

    /// <summary>
    /// Runtime permission for stepping up.
    /// </summary>
    public bool CanStepUp => canStepUp;

    /// <summary>
    /// Highest Y position recorded before falling.
    /// </summary>
    public float FallStartY => fallStartY;

    /// <summary>
    /// Current tracked fall distance.
    /// </summary>
    public float FallDistance => fallDistance;

    #endregion

    #endregion

    #region APIs

    /// <summary>
    /// Enabling the locomotion(s) type of Motor
    /// </summary>
    /// <param name="Locomotions"></param>
    public void EnableLocomotions(MotorLocomotion Locomotions)
    {
        _allowedLocomotions |= Locomotions;
    }

    /// <summary>
    /// Disabling the locomotion(s) type of Motor
    /// </summary>
    /// <param name="Locomotions"></param>
    public void DisableLocomotions(MotorLocomotion Locomotions)
    {
        _allowedLocomotions &= ~Locomotions;
    }

    /// <summary>
    /// Comparing locomotion if the locomotion was allowed by the HumanoidMotor
    /// </summary>
    /// <param name="__locomotion">MotorLocomotions</param>
    /// <returns>True/False</returns>
    public bool IsLocomotionAllowed(MotorLocomotion __locomotion)
    {
        return (_allowedLocomotions & __locomotion) != 0;
    }

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

        bool hasInput = dir.sqrMagnitude > 0.05f;

        if (hasInput)
            dir.Normalize();
        else
            dir = Vector3.zero;

       bool canRun = hasInput && 
            Running &&
            humanoid.StateType != HumanoidStateType.Crouch &&
            humanoid.StateType != HumanoidStateType.Prone &&
            humanoid.StateType != HumanoidStateType.Sliding &&
            humanoid.Stamina > humanoid.StaminaDecrementAmount;

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
        if (!motorEnabled || !canCrouch || _isDashing)
            return;

        SetCurrentStanceIntent(StanceIntent.Crouch);
    }

    /// <summary>
    /// Stopping the crouch session of Humanoid
    /// </summary>
    public void UnCrouch()
    {
        if (cantUncrouchOverCeiling && isCeilingAbove) return;
        if (_cantUncrouch) return;

        SetCurrentStanceIntent(StanceIntent.Standing);
    }

    /// <summary>
    /// Setting Humanoid's collider to relative proning height
    /// </summary>
    public void Prone()
    {
        if (!motorEnabled || !canProne || _isDashing)
            return;


        SetCurrentStanceIntent(StanceIntent.Prone);
    }

    /// <summary>
    /// Stopping proning session of Humanoid
    /// </summary>
    public void UnProne()
    {
        if (cantUnproneOverCeiling && isCeilingAbove) return;
        if (_cantUnprone) return;
        
        SetCurrentStanceIntent(StanceIntent.Standing);
    }

    /// <summary>
    /// Starts a dash toward a direction.
    /// </summary>
    public void Dash(Vector3 Direction)
    {
        if (!motorEnabled || !canDash)
            return;

        if (dashOnlyOnGrounded && !isGrounded)
            return;

        if (Time.time - _lastDashed <= dashCooldown)
            return;

        Vector3 _direction = Direction;
        _direction.y = 0f;

        if (_direction.sqrMagnitude <= 0.001f)
            _direction = lastMoveDirection.sqrMagnitude > 0.001f ? lastMoveDirection : humanoid.FacingDirection;

        _direction.y = 0f;

        if (_direction.sqrMagnitude <= 0.001f)
            return;

        _direction.Normalize();

        if (isGrounded && isOnSlope && !isSliding)
            _direction = Vector3.ProjectOnPlane(_direction, groundNormal).normalized;

        float expDistance = dashSpeed * dashDuration;

        if (dashUseCast)
        {
            float nearestSafeDistance = TestObstacleForDash(_direction, expDistance);

            if (nearestSafeDistance < dashMinDistance)
                return;

            float allowedDuration = nearestSafeDistance / dashSpeed;
            _dashEndTime = Time.time + Mathf.Min(dashDuration, allowedDuration);
        }
        else
        {
            _dashEndTime = Time.time + dashDuration;
        }

        _dashDirection = _direction;
        _lastDashed = Time.time;
        _isDashing = true;
        
        humanoid.ChangeState(HumanoidStateType.Lunging);
        OnDashBegin?.Invoke();

        if (dashStopMovement)
            StopMove();
    }

    /// <summary>
    /// Stops the current dash immediately.
    /// </summary>
    public void StopDash()
    {
        if (!_isDashing)
            return;

        _isDashing = false;
        _dashDirection = Vector3.zero;

        Vector3 velocity = rigidBody.linearVelocity;

        velocity.x = 0f;
        velocity.z = 0f;

        rigidBody.linearVelocity = velocity;

        humanoid.ChangeState(HumanoidStateType.Idle);
        OnDashEnded?.Invoke();
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
    /// Translating transform of Humanoid position from relative position with current position to desired location matrix
    /// </summary>
    /// <param name="Matrix"></param>
    public void Translate(Vector3 Matrix)
    {
        if (!humanoid.IsAlive)
            return;
        
        rigidBody.position = rigidBody.position + Matrix;
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

    #region methods

    #region SetterMethods

    #region CoreSetters

    /// <summary>
    /// Sets the motor's humanoid.
    /// </summary>
    /// <param name="amount">New humanoid value.</param>
    public void SetMotorHumanoid(Humanoid amount)
    {
        Humanoid old = humanoid;
        humanoid = amount;

        if (old != humanoid)
            OnHumanoidChanged?.Invoke(old, humanoid);
    }

    #endregion

    #region ReferenceSetters

    /// <summary>
    /// Sets the motor's rigid body.
    /// </summary>
    /// <param name="amount">New rigid body value.</param>
    public void SetMotorRigidBody(Rigidbody amount)
    {
        Rigidbody old = rigidBody;
        rigidBody = amount;

        if (old != rigidBody)
            OnRigidBodyChanged?.Invoke(old, rigidBody);
    }

    /// <summary>
    /// Sets the motor's root part.
    /// </summary>
    /// <param name="amount">New root part value.</param>
    public void SetMotorRootPart(Transform amount)
    {
        Transform old = rootPart;
        rootPart = amount;

        if (old != rootPart)
            OnRootPartChanged?.Invoke(old, rootPart);
    }

    /// <summary>
    /// Sets the motor's body collider.
    /// </summary>
    /// <param name="amount">New body collider value.</param>
    public void SetMotorBodyCollider(Collider amount)
    {
        Collider old = bodyCollider;
        bodyCollider = amount;

        if (old != bodyCollider)
            OnBodyColliderChanged?.Invoke(old, bodyCollider);
    }

    /// <summary>
    /// Sets the motor's ground check.
    /// </summary>
    /// <param name="amount">New ground check value.</param>
    public void SetMotorGroundCheck(Transform amount)
    {
        Transform old = groundCheck;
        groundCheck = amount;

        if (old != groundCheck)
            OnGroundCheckChanged?.Invoke(old, groundCheck);
    }

    /// <summary>
    /// Sets the motor's head check.
    /// </summary>
    /// <param name="amount">New head check value.</param>
    public void SetMotorHeadCheck(Transform amount)
    {
        Transform old = headCheck;
        headCheck = amount;

        if (old != headCheck)
            OnHeadCheckChanged?.Invoke(old, headCheck);
    }

    /// <summary>
    /// Sets the motor's body height.
    /// </summary>
    /// <param name="amount">New body height value.</param>
    public void SetMotorBodyHeight(float amount)
    {
        float old = bodyHeight;
        bodyHeight = Mathf.Max(0f, amount);

        if (old != bodyHeight)
            OnBodyHeightChanged?.Invoke(old, bodyHeight);
    }

    #endregion

    #region MovementSetters

    /// <summary>
    /// Sets the motor's acceleration.
    /// </summary>
    /// <param name="amount">New acceleration value.</param>
    public void SetMotorAcceleration(float amount)
    {
        float old = acceleration;
        acceleration = Mathf.Max(0f, amount);

        if (old != acceleration)
            OnAccelerationChanged?.Invoke(old, acceleration);
    }

    /// <summary>
    /// Sets the motor's deceleration.
    /// </summary>
    /// <param name="amount">New deceleration value.</param>
    public void SetMotorDeceleration(float amount)
    {
        float old = deceleration;
        deceleration = Mathf.Max(0f, amount);

        if (old != deceleration)
            OnDecelerationChanged?.Invoke(old, deceleration);
    }

    /// <summary>
    /// Sets the motor's air acceleration.
    /// </summary>
    /// <param name="amount">New air acceleration value.</param>
    public void SetMotorAirAcceleration(float amount)
    {
        float old = airAcceleration;
        airAcceleration = Mathf.Max(0f, amount);

        if (old != airAcceleration)
            OnAirAccelerationChanged?.Invoke(old, airAcceleration);
    }

    /// <summary>
    /// Sets the motor's air deceleration.
    /// </summary>
    /// <param name="amount">New air deceleration value.</param>
    public void SetMotorAirDeceleration(float amount)
    {
        float old = airDeceleration;
        airDeceleration = Mathf.Max(0f, amount);

        if (old != airDeceleration)
            OnAirDecelerationChanged?.Invoke(old, airDeceleration);
    }

    /// <summary>
    /// Sets the motor's movement strength.
    /// </summary>
    /// <param name="amount">New movement strength value.</param>
    public void SetMotorMovementStrength(float amount)
    {
        float old = movementStrength;
        movementStrength = Mathf.Max(0f, amount);

        if (old != movementStrength)
            OnMovementStrengthChanged?.Invoke(old, movementStrength);
    }

    /// <summary>
    /// Sets the motor's momentum on air.
    /// </summary>
    /// <param name="amount">New momentum on air value.</param>
    public void SetMotorMomentumOnAir(bool amount)
    {
        bool old = momentumOnAir;
        momentumOnAir = amount;

        if (old != momentumOnAir)
            OnMomentumOnAirChanged?.Invoke(old, momentumOnAir);
    }

    #endregion

    #region RotatorSetters

    /// <summary>
    /// Sets the motor's auto rotate.
    /// </summary>
    /// <param name="amount">New auto rotate value.</param>
    public void SetMotorAutoRotate(bool amount)
    {
        bool old = autoRotate;
        autoRotate = amount;

        if (old != autoRotate)
            OnAutoRotateChanged?.Invoke(old, autoRotate);
    }

    /// <summary>
    /// Sets the motor's rotation speed.
    /// </summary>
    /// <param name="amount">New rotation speed value.</param>
    public void SetMotorRotationSpeed(float amount)
    {
        float old = rotationSpeed;
        rotationSpeed = Mathf.Max(0f, amount);

        if (old != rotationSpeed)
            OnRotationSpeedChanged?.Invoke(old, rotationSpeed);
    }

    /// <summary>
    /// Sets the motor's only rotate by moving.
    /// </summary>
    /// <param name="amount">New only rotate by moving value.</param>
    public void SetMotorOnlyRotateByMoving(bool amount)
    {
        bool old = onlyRotateByMoving;
        onlyRotateByMoving = amount;

        if (old != onlyRotateByMoving)
            OnOnlyRotateByMovingChanged?.Invoke(old, onlyRotateByMoving);
    }

    #endregion

    #region FeetSetters

    /// <summary>
    /// Sets the motor's feet layer.
    /// </summary>
    /// <param name="amount">New feet layer value.</param>
    public void SetMotorFeetLayer(LayerMask amount)
    {
        LayerMask old = feetLayer;
        feetLayer = amount;

        if (old.value != feetLayer.value)
            OnFeetLayerChanged?.Invoke(old, feetLayer);
    }

    /// <summary>
    /// Sets the motor's floating velocity.
    /// </summary>
    /// <param name="amount">New floating velocity value.</param>
    public void SetMotorFloatingVelocity(float amount)
    {
        float old = floatingVelocity;
        floatingVelocity = Mathf.Max(0f, amount);

        if (old != floatingVelocity)
            OnFloatingVelocityChanged?.Invoke(old, floatingVelocity);
    }

    /// <summary>
    /// Sets the motor's max slope angle.
    /// </summary>
    /// <param name="amount">New max slope angle value.</param>
    public void SetMotorMaxSlopeAngle(float amount)
    {
        float old = maxSlopeAngle;
        maxSlopeAngle = Mathf.Max(0f, amount);

        if (old != maxSlopeAngle)
            OnMaxSlopeAngleChanged?.Invoke(old, maxSlopeAngle);
    }

    /// <summary>
    /// Sets the motor's check radius.
    /// </summary>
    /// <param name="amount">New check radius value.</param>
    public void SetMotorCheckRadius(float amount)
    {
        float old = checkRadius;
        checkRadius = Mathf.Max(0.01f, amount);

        if (old != checkRadius)
            OnCheckRadiusChanged?.Invoke(old, checkRadius);
    }

    /// <summary>
    /// Sets the motor's check distance.
    /// </summary>
    /// <param name="amount">New check distance value.</param>
    public void SetMotorCheckDistance(float amount)
    {
        float old = checkDistance;
        checkDistance = Mathf.Max(0.01f, amount);

        if (old != checkDistance)
            OnCheckDistanceChanged?.Invoke(old, checkDistance);
    }

    /// <summary>
    /// Sets the motor's grounded stick force.
    /// </summary>
    /// <param name="amount">New grounded stick force value.</param>
    public void SetMotorGroundedStickForce(float amount)
    {
        float old = groundedStickForce;
        groundedStickForce = Mathf.Max(0f, amount);

        if (old != groundedStickForce)
            OnGroundedStickForceChanged?.Invoke(old, groundedStickForce);
    }

    /// <summary>
    /// Sets the motor's slope slide acceleration.
    /// </summary>
    /// <param name="amount">New slope slide acceleration value.</param>
    public void SetMotorSlopeSlideAcceleration(float amount)
    {
        float old = slopeSlideAcceleration;
        slopeSlideAcceleration = Mathf.Max(0f, amount);

        if (old != slopeSlideAcceleration)
            OnSlopeSlideAccelerationChanged?.Invoke(old, slopeSlideAcceleration);
    }

    /// <summary>
    /// Sets the motor's ignore ground after jump.
    /// </summary>
    /// <param name="amount">New ignore ground after jump value.</param>
    public void SetMotorIgnoreGroundAfterJump(float amount)
    {
        float old = ignoreGroundAfterJump;
        ignoreGroundAfterJump = Mathf.Max(0f, amount);

        if (old != ignoreGroundAfterJump)
            OnIgnoreGroundAfterJumpChanged?.Invoke(old, ignoreGroundAfterJump);
    }

    /// <summary>
    /// Sets the motor's feet skin.
    /// </summary>
    /// <param name="amount">New feet skin value.</param>
    public void SetMotorFeetSkin(float amount)
    {
        float old = feetSkin;
        feetSkin = Mathf.Max(0f, amount);

        if (old != feetSkin)
            OnFeetSkinChanged?.Invoke(old, feetSkin);
    }

    #endregion

    #region HeadSetters

    /// <summary>
    /// Sets the motor's head layer.
    /// </summary>
    /// <param name="amount">New head layer value.</param>
    public void SetMotorHeadLayer(LayerMask amount)
    {
        LayerMask old = headLayer;
        headLayer = amount;

        if (old.value != headLayer.value)
            OnHeadLayerChanged?.Invoke(old, headLayer);
    }

    /// <summary>
    /// Sets the motor's head radius.
    /// </summary>
    /// <param name="amount">New head radius value.</param>
    public void SetMotorHeadRadius(float amount)
    {
        float old = headRadius;
        headRadius = Mathf.Max(0.01f, amount);

        if (old != headRadius)
            OnHeadRadiusChanged?.Invoke(old, headRadius);
    }

    /// <summary>
    /// Sets the motor's head max distance.
    /// </summary>
    /// <param name="amount">New head max distance value.</param>
    public void SetMotorHeadMaxDistance(float amount)
    {
        float old = headMaxDistance;
        headMaxDistance = Mathf.Max(0.01f, amount);

        if (old != headMaxDistance)
            OnHeadMaxDistanceChanged?.Invoke(old, headMaxDistance);
    }

    /// <summary>
    /// Sets the motor's head skin.
    /// </summary>
    /// <param name="amount">New head skin value.</param>
    public void SetMotorHeadSkin(float amount)
    {
        float old = headSkin;
        headSkin = Mathf.Max(0f, amount);

        if (old != headSkin)
            OnHeadSkinChanged?.Invoke(old, headSkin);
    }

    #endregion

    #region JumpSetters

    /// <summary>
    /// Sets the motor's jump height.
    /// </summary>
    /// <param name="amount">New jump height value.</param>
    public void SetMotorJumpHeight(float amount)
    {
        float old = jumpHeight;
        jumpHeight = Mathf.Max(0f, amount);

        if (old != jumpHeight)
            OnJumpHeightChanged?.Invoke(old, jumpHeight);
    }

    /// <summary>
    /// Sets the motor's jump cooldown.
    /// </summary>
    /// <param name="amount">New jump cooldown value.</param>
    public void SetMotorJumpCooldown(float amount)
    {
        float old = jumpCooldown;
        jumpCooldown = Mathf.Max(0f, amount);

        if (old != jumpCooldown)
            OnJumpCooldownChanged?.Invoke(old, jumpCooldown);
    }

    /// <summary>
    /// Sets the motor's coyote time.
    /// </summary>
    /// <param name="amount">New coyote time value.</param>
    public void SetMotorCoyoteTime(float amount)
    {
        float old = coyoteTime;
        coyoteTime = Mathf.Max(0f, amount);

        if (old != coyoteTime)
            OnCoyoteTimeChanged?.Invoke(old, coyoteTime);
    }

    /// <summary>
    /// Sets the motor's jump buffer time.
    /// </summary>
    /// <param name="amount">New jump buffer time value.</param>
    public void SetMotorJumpBufferTime(float amount)
    {
        float old = jumpBufferTime;
        jumpBufferTime = Mathf.Max(0f, amount);

        if (old != jumpBufferTime)
            OnJumpBufferTimeChanged?.Invoke(old, jumpBufferTime);
    }

    /// <summary>
    /// Sets the motor's jump cut multiplier.
    /// </summary>
    /// <param name="amount">New jump cut multiplier value.</param>
    public void SetMotorJumpCutMultiplier(float amount)
    {
        float old = jumpCutMultiplier;
        jumpCutMultiplier = Mathf.Clamp01(amount);

        if (old != jumpCutMultiplier)
            OnJumpCutMultiplierChanged?.Invoke(old, jumpCutMultiplier);
    }

    /// <summary>
    /// Sets the motor's jump affects fall.
    /// </summary>
    /// <param name="amount">New jump affects fall value.</param>
    public void SetMotorJumpAffectsFall(bool amount)
    {
        bool old = jumpAffectsFall;
        jumpAffectsFall = amount;

        if (old != jumpAffectsFall)
            OnJumpAffectsFallChanged?.Invoke(old, jumpAffectsFall);
    }

    #endregion

    #region CrouchSetters

    /// <summary>
    /// Sets the motor's crouch height.
    /// </summary>
    /// <param name="amount">New crouch height value.</param>
    public void SetMotorCrouchHeight(float amount)
    {
        float old = crouchHeight;
        crouchHeight = Mathf.Clamp(amount, 0.1f, bodyHeight);

        if (old != crouchHeight)
            OnCrouchHeightChanged?.Invoke(old, crouchHeight);
    }

    /// <summary>
    /// Sets the motor's crouch walking speed.
    /// </summary>
    /// <param name="amount">New crouch walking speed value.</param>
    public void SetMotorCrouchWalkingSpeed(float amount)
    {
        float old = crouchWalkingSpeed;
        crouchWalkingSpeed = Mathf.Max(0f, amount);

        if (old != crouchWalkingSpeed)
            OnCrouchWalkingSpeedChanged?.Invoke(old, crouchWalkingSpeed);
    }

    /// <summary>
    /// Sets the motor's crouch transition speed.
    /// </summary>
    /// <param name="amount">New crouch transition speed value.</param>
    public void SetMotorCrouchTransitionSpeed(float amount)
    {
        float old = crouchTransitionSpeed;
        crouchTransitionSpeed = Mathf.Max(0f, amount);

        if (old != crouchTransitionSpeed)
            OnCrouchTransitionSpeedChanged?.Invoke(old, crouchTransitionSpeed);
    }

    /// <summary>
    /// Sets the motor's uncrouch transition speed.
    /// </summary>
    /// <param name="amount">New uncrouch transition speed value.</param>
    public void SetMotorUncrouchTransitionSpeed(float amount)
    {
        float old = uncrouchTransitionSpeed;
        uncrouchTransitionSpeed = Mathf.Max(0f, amount);

        if (old != uncrouchTransitionSpeed)
            OnUncrouchTransitionSpeedChanged?.Invoke(old, uncrouchTransitionSpeed);
    }

    /// <summary>
    /// Sets the motor's crouch fuzzy equivalence.
    /// </summary>
    /// <param name="amount">New crouch fuzzy equivalence value.</param>
    public void SetMotorCrouchFuzzyEquivalence(float amount)
    {
        float old = crouchFuzzyEquivalence;
        crouchFuzzyEquivalence = Mathf.Max(0f, amount);

        if (old != crouchFuzzyEquivalence)
            OnCrouchFuzzyEquivalenceChanged?.Invoke(old, crouchFuzzyEquivalence);
    }

    /// <summary>
    /// Sets the motor's cant uncrouch over ceiling.
    /// </summary>
    /// <param name="amount">New cant uncrouch over ceiling value.</param>
    public void SetMotorCantUncrouchOverCeiling(bool amount)
    {
        bool old = cantUncrouchOverCeiling;
        cantUncrouchOverCeiling = amount;

        if (old != cantUncrouchOverCeiling)
            OnCantUncrouchOverCeilingChanged?.Invoke(old, cantUncrouchOverCeiling);
    }

    /// <summary>
    /// Sets the motor's auto scale crouch multiplier.
    /// </summary>
    /// <param name="amount">New auto scale crouch multiplier value.</param>
    public void SetMotorAutoScaleCrouchMultiplier(float amount)
    {
        float old = autoScaleCrouchMultiplier;
        autoScaleCrouchMultiplier = Mathf.Max(0f, amount);

        if (old != autoScaleCrouchMultiplier)
            OnAutoScaleCrouchMultiplierChanged?.Invoke(old, autoScaleCrouchMultiplier);
    }

    #endregion

    #region ProneSetters

    /// <summary>
    /// Sets the motor's prone height.
    /// </summary>
    /// <param name="amount">New prone height value.</param>
    public void SetMotorProneHeight(float amount)
    {
        float old = proneHeight;
        proneHeight = Mathf.Clamp(amount, 0.1f, crouchHeight);

        if (old != proneHeight)
            OnProneHeightChanged?.Invoke(old, proneHeight);
    }

    /// <summary>
    /// Sets the motor's prone walking speed.
    /// </summary>
    /// <param name="amount">New prone walking speed value.</param>
    public void SetMotorProneWalkingSpeed(float amount)
    {
        float old = proneWalkingSpeed;
        proneWalkingSpeed = Mathf.Max(0f, amount);

        if (old != proneWalkingSpeed)
            OnProneWalkingSpeedChanged?.Invoke(old, proneWalkingSpeed);
    }

    /// <summary>
    /// Sets the motor's prone transition speed.
    /// </summary>
    /// <param name="amount">New prone transition speed value.</param>
    public void SetMotorProneTransitionSpeed(float amount)
    {
        float old = proneTransitionSpeed;
        proneTransitionSpeed = Mathf.Max(0f, amount);

        if (old != proneTransitionSpeed)
            OnProneTransitionSpeedChanged?.Invoke(old, proneTransitionSpeed);
    }

    /// <summary>
    /// Sets the motor's unprone transition speed.
    /// </summary>
    /// <param name="amount">New unprone transition speed value.</param>
    public void SetMotorUnproneTransitionSpeed(float amount)
    {
        float old = unproneTransitionSpeed;
        unproneTransitionSpeed = Mathf.Max(0f, amount);

        if (old != unproneTransitionSpeed)
            OnUnproneTransitionSpeedChanged?.Invoke(old, unproneTransitionSpeed);
    }

    /// <summary>
    /// Sets the motor's prone fuzzy equivalence.
    /// </summary>
    /// <param name="amount">New prone fuzzy equivalence value.</param>
    public void SetMotorProneFuzzyEquivalence(float amount)
    {
        float old = proneFuzzyEquivalence;
        proneFuzzyEquivalence = Mathf.Max(0f, amount);

        if (old != proneFuzzyEquivalence)
            OnProneFuzzyEquivalenceChanged?.Invoke(old, proneFuzzyEquivalence);
    }

    /// <summary>
    /// Sets the motor's cant unprone over ceiling.
    /// </summary>
    /// <param name="amount">New cant unprone over ceiling value.</param>
    public void SetMotorCantUnproneOverCeiling(bool amount)
    {
        bool old = cantUnproneOverCeiling;
        cantUnproneOverCeiling = amount;

        if (old != cantUnproneOverCeiling)
            OnCantUnproneOverCeilingChanged?.Invoke(old, cantUnproneOverCeiling);
    }

    /// <summary>
    /// Sets the motor's auto scale prone multiplier.
    /// </summary>
    /// <param name="amount">New auto scale prone multiplier value.</param>
    public void SetMotorAutoScaleProneMultiplier(float amount)
    {
        float old = autoScaleProneMultiplier;
        autoScaleProneMultiplier = Mathf.Max(0f, amount);

        if (old != autoScaleProneMultiplier)
            OnAutoScaleProneMultiplierChanged?.Invoke(old, autoScaleProneMultiplier);
    }

    #endregion

    #region DashSetters

    /// <summary>
    /// Sets the motor's dash use cast.
    /// </summary>
    /// <param name="amount">New dash use cast value.</param>
    public void SetMotorDashUseCast(bool amount)
    {
        bool old = dashUseCast;
        dashUseCast = amount;

        if (old != dashUseCast)
            OnDashUseCastChanged?.Invoke(old, dashUseCast);
    }

    /// <summary>
    /// Sets the motor's dash only on grounded.
    /// </summary>
    /// <param name="amount">New dash only on grounded value.</param>
    public void SetMotorDashOnlyOnGrounded(bool amount)
    {
        bool old = dashOnlyOnGrounded;
        dashOnlyOnGrounded = amount;

        if (old != dashOnlyOnGrounded)
            OnDashOnlyOnGroundedChanged?.Invoke(old, dashOnlyOnGrounded);
    }

    /// <summary>
    /// Sets the motor's dash stop movement.
    /// </summary>
    /// <param name="amount">New dash stop movement value.</param>
    public void SetMotorDashStopMovement(bool amount)
    {
        bool old = dashStopMovement;
        dashStopMovement = amount;

        if (old != dashStopMovement)
            OnDashStopMovementChanged?.Invoke(old, dashStopMovement);
    }

    /// <summary>
    /// Sets the motor's dash linear dashing.
    /// </summary>
    /// <param name="amount">New dash linear dashing value.</param>
    public void SetMotorDashLinearDashing(bool amount)
    {
        bool old = dashLinearDashing;
        dashLinearDashing = amount;

        if (old != dashLinearDashing)
            OnDashLinearDashingChanged?.Invoke(old, dashLinearDashing);
    }

    /// <summary>
    /// Sets the motor's dash cast mask.
    /// </summary>
    /// <param name="amount">New dash cast mask value.</param>
    public void SetMotorDashCastMask(LayerMask amount)
    {
        LayerMask old = dashCastMask;
        dashCastMask = amount;

        if (old.value != dashCastMask.value)
            OnDashCastMaskChanged?.Invoke(old, dashCastMask);
    }

    /// <summary>
    /// Sets the motor's dash speed.
    /// </summary>
    /// <param name="amount">New dash speed value.</param>
    public void SetMotorDashSpeed(float amount)
    {
        float old = dashSpeed;
        dashSpeed = Mathf.Max(0f, amount);

        if (old != dashSpeed)
            OnDashSpeedChanged?.Invoke(old, dashSpeed);
    }

    /// <summary>
    /// Sets the motor's dash cooldown.
    /// </summary>
    /// <param name="amount">New dash cooldown value.</param>
    public void SetMotorDashCooldown(float amount)
    {
        float old = dashCooldown;
        dashCooldown = Mathf.Max(0f, amount);

        if (old != dashCooldown)
            OnDashCooldownChanged?.Invoke(old, dashCooldown);
    }

    /// <summary>
    /// Sets the motor's dash check skin.
    /// </summary>
    /// <param name="amount">New dash check skin value.</param>
    public void SetMotorDashCheckSkin(float amount)
    {
        float old = dashCheckSkin;
        dashCheckSkin = Mathf.Max(0f, amount);

        if (old != dashCheckSkin)
            OnDashCheckSkinChanged?.Invoke(old, dashCheckSkin);
    }

    /// <summary>
    /// Sets the motor's dash duration.
    /// </summary>
    /// <param name="amount">New dash duration value.</param>
    public void SetMotorDashDuration(float amount)
    {
        float old = dashDuration;
        dashDuration = Mathf.Max(0f, amount);

        if (old != dashDuration)
            OnDashDurationChanged?.Invoke(old, dashDuration);
    }

    /// <summary>
    /// Sets the motor's dash min distance.
    /// </summary>
    /// <param name="amount">New dash min distance value.</param>
    public void SetMotorDashMinDistance(float amount)
    {
        float old = dashMinDistance;
        dashMinDistance = Mathf.Max(0f, amount);

        if (old != dashMinDistance)
            OnDashMinDistanceChanged?.Invoke(old, dashMinDistance);
    }

    #endregion

    #region MassSetters

    /// <summary>
    /// Sets the motor's gravity scale.
    /// </summary>
    /// <param name="amount">New gravity scale value.</param>
    public void SetMotorGravityScale(float amount)
    {
        float old = gravityScale;
        gravityScale = Mathf.Max(0f, amount);

        if (old != gravityScale)
            OnGravityScaleChanged?.Invoke(old, gravityScale);
    }

    /// <summary>
    /// Sets the motor's falling gravity multiplier.
    /// </summary>
    /// <param name="amount">New falling gravity multiplier value.</param>
    public void SetMotorFallingGravityMultiplier(float amount)
    {
        float old = fallingGravityMultiplier;
        fallingGravityMultiplier = Mathf.Max(0f, amount);

        if (old != fallingGravityMultiplier)
            OnFallingGravityMultiplierChanged?.Invoke(old, fallingGravityMultiplier);
    }

    /// <summary>
    /// Sets the motor's low jump gravity multiplier.
    /// </summary>
    /// <param name="amount">New low jump gravity multiplier value.</param>
    public void SetMotorLowJumpGravityMultiplier(float amount)
    {
        float old = lowJumpGravityMultiplier;
        lowJumpGravityMultiplier = Mathf.Max(0f, amount);

        if (old != lowJumpGravityMultiplier)
            OnLowJumpGravityMultiplierChanged?.Invoke(old, lowJumpGravityMultiplier);
    }

    /// <summary>
    /// Sets the motor's max falling speed.
    /// </summary>
    /// <param name="amount">New max falling speed value.</param>
    public void SetMotorMaxFallingSpeed(float amount)
    {
        float old = maxFallingSpeed;
        maxFallingSpeed = Mathf.Max(0f, amount);

        if (old != maxFallingSpeed)
            OnMaxFallingSpeedChanged?.Invoke(old, maxFallingSpeed);
    }

    /// <summary>
    /// Sets the motor's air resistance.
    /// </summary>
    /// <param name="amount">New air resistance value.</param>
    public void SetMotorAirResistance(float amount)
    {
        float old = airResistance;
        airResistance = Mathf.Max(0f, amount);

        if (old != airResistance)
            OnAirResistanceChanged?.Invoke(old, airResistance);
    }

    /// <summary>
    /// Sets the motor's wind velocity.
    /// </summary>
    /// <param name="amount">New wind velocity value.</param>
    public void SetMotorWindVelocity(Vector3 amount)
    {
        Vector3 old = windVelocity;
        windVelocity = amount;

        if (old != windVelocity)
            OnWindVelocityChanged?.Invoke(old, windVelocity);
    }

    /// <summary>
    /// Sets the motor's wind influence.
    /// </summary>
    /// <param name="amount">New wind influence value.</param>
    public void SetMotorWindInfluence(float amount)
    {
        float old = windInfluence;
        windInfluence = Mathf.Max(0f, amount);

        if (old != windInfluence)
            OnWindInfluenceChanged?.Invoke(old, windInfluence);
    }

    #endregion

    #region ObstacleSetters

    /// <summary>
    /// Sets the motor's enable step up.
    /// </summary>
    /// <param name="amount">New enable step up value.</param>
    public void SetMotorEnableStepUp(bool amount)
    {
        bool old = enableStepUp;
        enableStepUp = amount;

        if (old != enableStepUp)
            OnEnableStepUpChanged?.Invoke(old, enableStepUp);
    }

    /// <summary>
    /// Sets the motor's body layer.
    /// </summary>
    /// <param name="amount">New body layer value.</param>
    public void SetMotorBodyLayer(LayerMask amount)
    {
        LayerMask old = bodyLayer;
        bodyLayer = amount;

        if (old.value != bodyLayer.value)
            OnBodyLayerChanged?.Invoke(old, bodyLayer);
    }

    /// <summary>
    /// Sets the motor's step height.
    /// </summary>
    /// <param name="amount">New step height value.</param>
    public void SetMotorStepHeight(float amount)
    {
        float old = stepHeight;
        stepHeight = Mathf.Max(0f, amount);

        if (old != stepHeight)
            OnStepHeightChanged?.Invoke(old, stepHeight);
    }

    /// <summary>
    /// Sets the motor's step check distance.
    /// </summary>
    /// <param name="amount">New step check distance value.</param>
    public void SetMotorStepCheckDistance(float amount)
    {
        float old = stepCheckDistance;
        stepCheckDistance = Mathf.Max(0f, amount);

        if (old != stepCheckDistance)
            OnStepCheckDistanceChanged?.Invoke(old, stepCheckDistance);
    }

    /// <summary>
    /// Sets the motor's step smoothness.
    /// </summary>
    /// <param name="amount">New step smoothness value.</param>
    public void SetMotorStepSmoothness(float amount)
    {
        float old = stepSmoothness;
        stepSmoothness = Mathf.Max(0f, amount);

        if (old != stepSmoothness)
            OnStepSmoothnessChanged?.Invoke(old, stepSmoothness);
    }

    /// <summary>
    /// Sets the motor's lower ground height.
    /// </summary>
    /// <param name="amount">New lower ground height value.</param>
    public void SetMotorLowerGroundHeight(float amount)
    {
        float old = lowerGroundHeight;
        lowerGroundHeight = Mathf.Max(0f, amount);

        if (old != lowerGroundHeight)
            OnLowerGroundHeightChanged?.Invoke(old, lowerGroundHeight);
    }

    /// <summary>
    /// Sets the motor's step check radius multiplier.
    /// </summary>
    /// <param name="amount">New step check radius multiplier value.</param>
    public void SetMotorStepCheckRadiusMultiplier(float amount)
    {
        float old = stepCheckRadiusMultiplier;
        stepCheckRadiusMultiplier = Mathf.Max(0f, amount);

        if (old != stepCheckRadiusMultiplier)
            OnStepCheckRadiusMultiplierChanged?.Invoke(old, stepCheckRadiusMultiplier);
    }

    /// <summary>
    /// Sets the motor's step forward offset.
    /// </summary>
    /// <param name="amount">New step forward offset value.</param>
    public void SetMotorStepForwardOffset(float amount)
    {
        float old = stepForwardOffset;
        stepForwardOffset = Mathf.Max(0f, amount);

        if (old != stepForwardOffset)
            OnStepForwardOffsetChanged?.Invoke(old, stepForwardOffset);
    }

    /// <summary>
    /// Sets the motor's step top extra height.
    /// </summary>
    /// <param name="amount">New step top extra height value.</param>
    public void SetMotorStepTopExtraHeight(float amount)
    {
        float old = stepTopExtraHeight;
        stepTopExtraHeight = Mathf.Max(0f, amount);

        if (old != stepTopExtraHeight)
            OnStepTopExtraHeightChanged?.Invoke(old, stepTopExtraHeight);
    }

    /// <summary>
    /// Sets the motor's min step height.
    /// </summary>
    /// <param name="amount">New min step height value.</param>
    public void SetMotorMinStepHeight(float amount)
    {
        float old = minStepHeight;
        minStepHeight = Mathf.Max(0f, amount);

        if (old != minStepHeight)
            OnMinStepHeightChanged?.Invoke(old, minStepHeight);
    }

    #endregion

    #endregion

    #endregion

    #region InternalHelpers

    /// <summary>
    /// Returns exponential smoothing factor for fixed update transitions.
    /// </summary>
    private float GetT(float speed, float multiplier)
    {
        multiplier = Mathf.Max(0.01f, multiplier);
        return 1.0f - Mathf.Exp(-speed * multiplier * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Checks whether a collider belongs to this motor.
    /// </summary>
    private bool IsSelfHitCollider(Collider __this_collider)
    {
        if (__this_collider == bodyCollider || __this_collider.attachedRigidbody == rigidBody || __this_collider.transform.IsChildOf(rootPart))
            return true;

        return false;
    }

    /// <summary>
    /// Checks whether a raycast hit belongs to this motor.
    /// </summary>
    private bool IsSelfHitRay(RaycastHit __this_hit)
    {
        Collider __this_colider = __this_hit.collider;
        if (__this_colider == null || __this_colider == bodyCollider || __this_hit.rigidbody == rigidBody || __this_hit.transform.IsChildOf(transform)) 
            return true;

        return false;
    }

    /// <summary>
    /// Checks whether stance input can be accepted.
    /// </summary>
    private bool CanUsePoseInput()
    {
        return motorEnabled &&
            humanoid.IsAlive &&
            isGrounded &&
            !humanoid.PlatformStanding &&
            humanoid.StateType != HumanoidStateType.Airborne &&
            humanoid.StateType != HumanoidStateType.FreeFalling &&
            humanoid.StateType != HumanoidStateType.Jumping &&
            humanoid.StateType != HumanoidStateType.Lunging;
    }

    /// <summary>
    /// Updates desired stance intent for collider handling.
    /// </summary>
    private void SetCurrentStanceIntent(StanceIntent intent)
    {
        switch (intent)
        {
            case StanceIntent.Standing:
            {
                _setCrouching = false;
                _setProning = false;

                break;        
            }

            case StanceIntent.Crouch:
            {
                _setCrouching = true;
                _setProning = false;

                break;        
            }

            case StanceIntent.Prone:
            {
                _setProning = true;
                _setCrouching = false;

                break;        
            }
        }
    }

    /// <summary>
    /// Returns the safe dash distance before an obstacle.
    /// </summary>
    private float TestObstacleForDash(Vector3 direction, float targetDistance)
    {
        if (bodyCollider == null) return targetDistance;

        float mostNearestDistance = targetDistance;
        if (bodyCollider is CapsuleCollider capsule)
        {
            Vector3 __world_center = rootPart.TransformPoint(capsule.center);
            float __half = capsule.height / 2.0f - capsule.radius;

            Vector3 __pA__ = __world_center + Vector3.up * __half;
            Vector3 __pB__ = __world_center - Vector3.up * __half;

            int __counts = Physics.CapsuleCastNonAlloc(
                __pA__,
                __pB__,
                capsule.radius,
                direction,
                _dash_hits,
                targetDistance,
                dashCastMask,
                QueryTriggerInteraction.Ignore
            );

            for (int __i = 0; __i < __counts; __i++)
            {
                RaycastHit __this_hit = _dash_hits[__i];

                if (IsSelfHitRay(__this_hit))
                    continue;

                float __perfect_distance = Mathf.Max(0f, __this_hit.distance - dashCheckSkin);
                if (mostNearestDistance > __perfect_distance)
                    mostNearestDistance = __perfect_distance;
            }
        }
        else if (bodyCollider is BoxCollider box)
        {
            Vector3 __world_center = rootPart.TransformPoint(box.center);
            Vector3 __half_size = box.size / 2.0f;

            int __counts = Physics.BoxCastNonAlloc(__world_center, 
                __half_size, 
                direction, 
                _dash_hits, 
                rigidBody.rotation, 
                targetDistance,
                dashCastMask,
                QueryTriggerInteraction.Ignore
            );

            for (int __i = 0; __i < __counts; __i++)
            {
                RaycastHit __this_hit = _dash_hits[__i];

                if (IsSelfHitRay(__this_hit))
                    continue;

                float __perfect_distance = Mathf.Max(0f, __this_hit.distance - dashCheckSkin);
                if (mostNearestDistance > __perfect_distance)
                    mostNearestDistance = __perfect_distance;
            }
        }
        else if (bodyCollider is SphereCollider sphere)
        {
            Vector3 __world_center = rootPart.TransformPoint(sphere.center);
            float __half_radius = sphere.radius / 2.0f;

            int __counts = Physics.SphereCastNonAlloc(
                __world_center,
                __half_radius,
                direction,
                _dash_hits,
                targetDistance,
                dashCastMask,
                QueryTriggerInteraction.Ignore
            );

            for (int __i = 0; __i < __counts; __i++)
            {
                RaycastHit __this_hit = _dash_hits[__i];

                if (IsSelfHitRay(__this_hit))
                    continue;

                float __perfect_distance = Mathf.Max(0f, __this_hit.distance - dashCheckSkin);
                if (mostNearestDistance > __perfect_distance)
                    mostNearestDistance = __perfect_distance;
            }
        }
        else
        {
            return targetDistance;
        }

        return mostNearestDistance;
    }

    /// <summary>
    /// Applies ceiling-blocked runtime state.
    /// </summary>
    private void CeilingIsAboveHelper()
    {
        if (cantUncrouchOverCeiling)
            _cantUncrouch = true;
        
        if (cantUnproneOverCeiling)
            _cantUnprone = true;

        CeilingAboveHead?.Invoke();
        isCeilingAbove = true;
    }

    /// <summary>
    /// Clears ceiling-blocked runtime state.
    /// </summary>
    private void CeilingIsntAboveHelper()
    {
        _cantUncrouch = false;
        _cantUnprone = false;

        if (isCeilingAbove)
            CeilingAboveHeadExit?.Invoke();
        
        isCeilingAbove = false;
    }

    /// <summary>
    /// Moves the rigidbody upward for step-up handling.
    /// </summary>
    private void ApplySteppingUp(float deltaSteppy)
    {
        Vector3 __velocity = rigidBody.position;

        if (__velocity.y > 0.1f)
            return;
        
        Vector3 __target_vel = __velocity + Vector3.up * deltaSteppy;

        float __t = 1.0f - Mathf.Exp(-stepSmoothness * Time.fixedDeltaTime);

        Vector3 __new_velocity = Vector3.Lerp(
            __velocity,
            __target_vel,
            __t
        );

        rigidBody.MovePosition(__new_velocity);
    }

    /// <summary>
    /// Checks whether step-up prediction overlaps obstacles.
    /// </summary>
    private bool CheckColliderOverlap(float __steppy_amount)
    {
        if (bodyCollider == null) return false;

        if (bodyCollider is CapsuleCollider capsule)
        {
            Vector3 __world_center = rootPart.TransformPoint(capsule.center) + Vector3.up * __steppy_amount;
            float __half_height = capsule.height / 2.0f - capsule.radius;

            Vector3 __pA__ = __world_center + Vector3.up * __half_height;
            Vector3 __pB__ = __world_center - Vector3.up * __half_height;

            int __counts = Physics.OverlapCapsuleNonAlloc(__pA__, __pB__, capsule.radius, _overlappedColliders, bodyLayer);

            for (int __i = 0; __i < __counts; __i++)
            {
                Collider __this_collider = _overlappedColliders[__i];

                if (IsSelfHitCollider(__this_collider) || __this_collider.isTrigger)
                    continue;

                return true;
            }
        }
        else if (bodyCollider is BoxCollider box)
        {
            Vector3 __world_center = rootPart.TransformPoint(box.center) + Vector3.up * __steppy_amount;
            Vector3 __half_size = box.size / 2.0f;

            int __counts = Physics.OverlapBoxNonAlloc(__world_center, __half_size, _overlappedColliders, rigidBody.rotation, bodyLayer);

            for (int __i = 0; __i < __counts; __i++)
            {
                Collider __this_collider = _overlappedColliders[__i];

                if (IsSelfHitCollider(__this_collider) || __this_collider.isTrigger)
                    continue;

                return true;
            }
        }
        else if (bodyCollider is SphereCollider sphere)
        {
            Vector3 __world_center = rootPart.TransformPoint(sphere.center) + Vector3.up * __steppy_amount;
            float __half_radius = sphere.radius / 2.0f;

            int __counts = Physics.OverlapSphereNonAlloc(__world_center, __half_radius, _overlappedColliders, bodyLayer);

            for (int __i = 0; __i < __counts; __i++)
            {
                Collider __this_collider = _overlappedColliders[__i];

                if (IsSelfHitCollider(__this_collider) || __this_collider.isTrigger)
                    continue;

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the world bottom point of a collider.
    /// </summary>
    private Vector3 GetGroundBottomOnWorld(Collider collider)
    {
        switch (collider)
        {
            case CapsuleCollider capsule:
            {
                Vector3 center = rootPart.TransformPoint(capsule.center);
                float halfHeight = capsule.height / 2.0f;
                return center - rootPart.up * halfHeight;
            }

            case BoxCollider box:
            {
                Vector3 center = rootPart.TransformPoint(box.center);
                return center - rootPart.up * (box.size.y / 2.0f);
            }

            case SphereCollider sphere:
            {
                Vector3 center = rootPart.TransformPoint(sphere.center);
                return center - rootPart.up * sphere.radius;
            }
            default:
            {
                Bounds bounds = collider.bounds;
                return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            }
        }
    }

    /// <summary>
    /// Returns the world top point of a collider.
    /// </summary>
    private Vector3 GetHeadTopOnWorld(Collider collider)
    {
        switch(collider)
        {
            case CapsuleCollider capsule:
            {
                Vector3 center = rootPart.TransformPoint(capsule.center);
                return center + rootPart.up * (capsule.height / 2.0f);
            }

            case BoxCollider box:
            {
                Vector3 center = rootPart.TransformPoint(box.center);
                return center + rootPart.up * (box.size.y / 2.0f);
            }

            case SphereCollider sphere:
            {
                Vector3 center = rootPart.TransformPoint(sphere.center);
                return center + rootPart.up * sphere.radius;
            }

            default:
            {
                Bounds bounds = collider.bounds;
                return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            }
        }
    }

    /// <summary>
    /// Shrinks supported collider types toward a target height.
    /// </summary>
    private bool TryShrinkCollider(Collider collider, float standingHeight, float toHeight, float transitionSpeed, float autoScaleMultiplier, float fuzzyeq)
    {
        switch (collider)
        {
            case CapsuleCollider capsule:
            {
                float targetHeight = toHeight;
                if (!_standCenterObtained)
                {
                    _standingCenter = capsule.center;
                    _standCenterObtained = true;
                }

                float standingBottom = standingHeight - targetHeight;
                float targetY = _standingCenter.y - (standingBottom / 2);

                float t = GetT(transitionSpeed, autoScaleMultiplier);
                Vector3 targetCenter = new Vector3(_standingCenter.x, targetY, _standingCenter.z);

                float lerpHeight = Mathf.Lerp(capsule.height, targetHeight, t);
                Vector3 lerpCenter = Vector3.Lerp(capsule.center, targetCenter, t);

                capsule.height = lerpHeight;
                capsule.center = lerpCenter;

                bool fuzzyEq = 
                    Mathf.Abs(capsule.height - targetHeight) < fuzzyeq &&
                    Vector3.Distance(capsule.center, targetCenter) < fuzzyeq;
                
                if (fuzzyEq)
                {
                    capsule.height = targetHeight;
                    capsule.center = targetCenter;        
                }
                return fuzzyEq;
            }
            
            case BoxCollider box:
            {
                float targetHeight = toHeight;

                if (!_standCenterObtained)
                {
                    _standingCenter = box.center;
                    _standCenterObtained = true;        
                }
                float standingBottom = standingHeight - targetHeight;
                float targetY = _standingCenter.y - (standingBottom / 2);

                float t = GetT(transitionSpeed, autoScaleMultiplier);
                Vector3 targetCenter = new Vector3(_standingCenter.x, targetY, _standingCenter.z);
                
                float lerpHeight = Mathf.Lerp(box.size.y, targetHeight, t);
                Vector3 lerpCenter = Vector3.Lerp(box.center, targetCenter, t);

                Vector3 lerpSize = box.size;
                lerpSize.y = lerpHeight;

                box.size = lerpSize;
                box.center = lerpCenter;

                bool fuzzyEq = 
                    Mathf.Abs(box.size.y - targetHeight) < fuzzyeq &&
                    Vector3.Distance(box.center, targetCenter) < fuzzyeq;
                
                if (fuzzyEq)
                {
                    box.size = new Vector3(box.size.x, targetHeight, box.size.z);
                    box.center = targetCenter;        
                }

                
                return fuzzyEq;
            }

            case SphereCollider sphere:
            {
                float targetHeight = toHeight;
                float targetRadius = targetHeight / 2f;

                if (!_standCenterObtained)
                {
                    _standingCenter = sphere.center;
                    _standCenterObtained = true;
                }
                float standingBottom = standingHeight / 2;
                float radius_Delta = standingBottom - targetRadius;

                float targetY = _standingCenter.y - radius_Delta;

                float t = GetT(transitionSpeed, autoScaleMultiplier);
                Vector3 targetCenter = new Vector3(_standingCenter.x, targetY, _standingCenter.z);

                float lerpHeight = Mathf.Lerp(sphere.radius, targetRadius, t);
                Vector3 lerpCenter = Vector3.Lerp(sphere.center, targetCenter, t);

                sphere.radius = lerpHeight;
                sphere.center = lerpCenter;

                bool fuzzyEq =
                    Mathf.Abs(sphere.radius - targetRadius) < fuzzyeq &&
                    Vector3.Distance(sphere.center, targetCenter) < fuzzyeq;

                if (fuzzyEq)
                {
                    sphere.radius = targetRadius;
                    sphere.center = targetCenter;        
                }

                return fuzzyEq;
            }

            default:
                return false;
        }
    }

    /// <summary>
    /// Stretches supported collider types back to standing height.
    /// </summary>
    private bool TryStretchCollider(Collider collider, float standingHeight, float unTransitionSpeed, float autoScaleMultiplier, float fuzzyeq)
    {
        switch (collider)
        {
            case CapsuleCollider capsule:
            {
                float targetHeight = standingHeight;
                Vector3 targetCenter = _standingCenter;
                
                float t = GetT(unTransitionSpeed, autoScaleMultiplier);

                float lerpHeight = Mathf.Lerp(capsule.height, targetHeight, t);
                Vector3 lerpCenter = Vector3.Lerp(capsule.center, targetCenter, t);

                capsule.height = lerpHeight;
                capsule.center = lerpCenter;

                bool fuzzyEq = 
                    Mathf.Abs(capsule.height - targetHeight) < fuzzyeq &&
                    Vector3.Distance(capsule.center, targetCenter) < fuzzyeq;
                
                if (fuzzyEq)
                {
                    capsule.height = standingHeight;
                    capsule.center = targetCenter;

                    _standCenterObtained = false;        
                }
                return fuzzyEq;
            }
            
            case BoxCollider box:
            {
                float targetHeight = standingHeight;
                Vector3 targetCenter = _standingCenter;

                float t = GetT(unTransitionSpeed, autoScaleMultiplier);

                float lerpHeight = Mathf.Lerp(box.size.y, targetHeight, t);
                Vector3 lerpCenter = Vector3.Lerp(box.center, targetCenter, t);

                Vector3 lerpSize = box.size;
                lerpSize.y = lerpHeight;

                box.size = lerpSize;
                box.center = lerpCenter;

                bool fuzzyEq =
                    Mathf.Abs(box.size.y - targetHeight) < fuzzyeq &&
                    Vector3.Distance(box.center, targetCenter) < fuzzyeq;
                
                if (fuzzyEq)
                {
                    box.size = new Vector3(box.size.x, targetHeight, box.size.z);
                    box.center = targetCenter;

                    _standCenterObtained = false;        
                }

                return fuzzyEq;
            }

            case SphereCollider sphere:
            {
                float targetHeight = standingHeight;
                float targetRadius = targetHeight / 2.0f;
                Vector3 targetCenter = _standingCenter;

                float t = GetT(unTransitionSpeed, autoScaleMultiplier);

                float lerpHeight = Mathf.Lerp(sphere.radius, targetRadius, t);
                Vector3 lerpCenter = Vector3.Lerp(sphere.center, targetCenter, t);

                sphere.radius = lerpHeight;
                sphere.center = lerpCenter;

                bool fuzzyEq =
                    Mathf.Abs(sphere.radius - targetHeight) < fuzzyeq &&
                    Vector3.Distance(sphere.center, targetCenter) < fuzzyeq;
                
                if (fuzzyEq)
                {
                    sphere.radius = targetRadius;
                    sphere.center = targetCenter;

                    _standCenterObtained = false;        
                }

                return fuzzyEq;
            }

            default:
                return false;
        }
    }

    /// <summary>
    /// Applies grounded or ungrounded state data.
    /// </summary>
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

    /// <summary>
    /// Applies full ground information from a raycast hit.
    /// </summary>
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

    /// <summary>
    /// Applies simplified grounded information.
    /// </summary>
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

    /// <summary>
    /// Applies fall damage based on tracked fall height.
    /// </summary>
    private void ApplyFallDamage()
    {
        if (!humanoid.CanApplyFallDamage || !humanoid.IsAlive)
            return;
        
        if (humanoid.SafeFromFallDistance >= fallStartY)
            return;

        float __h = Mathf.Max(0f, fallStartY - humanoid.SafeFromFallDistance);
        if (__h <= 0) return;

        if (_thisGravity.y <= 0.01f)
            _thisGravity.y = 9.81f;

        float __m = Mathf.Max(0.01f, rigidBody.mass);
        float __energy = __m * _thisGravity.y * __h;
        __energy = Mathf.Abs(__energy);

        float __total_damage = __energy * humanoid.FallDamageMultiplier;
        humanoid.TakeDamage(__total_damage);
    }

    /// <summary>
    /// Handles crouch, prone, and standing collider transitions.
    /// </summary>
    private void HandleColliderExtension()
    {
        bool proneReady = _setProning;
        bool crouchReady = !proneReady && _setCrouching;

        if (proneReady)
        {
            bool to = TryShrinkCollider(
                bodyCollider,
                bodyHeight,
                proneHeight,
                proneTransitionSpeed,
                autoScaleProneMultiplier,
                proneFuzzyEquivalence
            );

            if (to && !_proned)
                OnProneBegin?.Invoke();

            _proned = true;
            _crouched = false;
            Proning?.Invoke();

            humanoid.ChangeState(HumanoidStateType.Prone);
            humanoid.SetHumanoidIsCrouching(false);
            humanoid.SetHumanoidIsProning(true);

            return;
        }

        if (crouchReady)
        {
            bool to = TryShrinkCollider(
                bodyCollider,
                bodyHeight,
                crouchHeight,
                crouchTransitionSpeed,
                autoScaleCrouchMultiplier,
                crouchFuzzyEquivalence
            );

            if (to && !_crouched)
                OnCrouchBegin?.Invoke();
            
            _proned = false;
            _crouched = true;
            Crouching?.Invoke();

            humanoid.ChangeState(HumanoidStateType.Crouch);
            humanoid.SetHumanoidIsProning(false);
            humanoid.SetHumanoidIsCrouching(true);

            return;
        }

        float stretchSpeed = _crouched ? uncrouchTransitionSpeed : unproneTransitionSpeed;
        float stretchMultiplier = _crouched ? autoScaleCrouchMultiplier : autoScaleProneMultiplier;
        float stretchFuzzyEq = _crouched ? crouchFuzzyEquivalence : proneFuzzyEquivalence;

        bool back = TryStretchCollider(
            bodyCollider,
            bodyHeight,
            stretchSpeed,
            stretchMultiplier,
            stretchFuzzyEq
        );

        if (back)
        {
            if (_crouched)
                UnCrouched?.Invoke();
            
            if (_proned)
                UnProned?.Invoke();
            
            _crouched = false;
            _proned = false;

            humanoid.SetHumanoidIsCrouching(false);
            humanoid.SetHumanoidIsProning(false);

            if (humanoid.StateType == HumanoidStateType.Crouch || humanoid.StateType == HumanoidStateType.Prone)
                humanoid.ChangeState(HumanoidStateType.Idle);
        }
    }

    /// <summary>
    /// Tracks airborne, falling, landing, and idle states.
    /// </summary>
    private void HandleFallTracking()
    {                    
        if (!isGrounded)
        {
            if (!jumpAffectsFall && humanoid.StateType == HumanoidStateType.Jumping) return;

            if (rigidBody.linearVelocity.y > floatingVelocity)
            {
                if (humanoid.StateType != HumanoidStateType.Airborne && humanoid.StateType != HumanoidStateType.FreeFalling)
                {
                    fallStartY = rootPart.position.y;
                    if (!(isSliding && humanoid.StateType == HumanoidStateType.Sliding))
                        humanoid.ChangeState(HumanoidStateType.Airborne);

                    isGrounded = false;
                    OnAirborneBegin?.Invoke();
                }

                fallStartY = Mathf.Max(fallStartY, rootPart.position.y);
                OnAirborne?.Invoke();
            }
            else if (rigidBody.linearVelocity.y < -floatingVelocity)
            {

                if (humanoid.StateType != HumanoidStateType.FreeFalling)
                {
                    humanoid.ChangeState(HumanoidStateType.FreeFalling);
                    OnFreeFallingBegin?.Invoke();
                }

                fallDistance = Mathf.Max(0f, fallStartY - rootPart.position.y);
                OnFreeFalling?.Invoke();
            }
        }
        else
        {
            if (humanoid.StateType == HumanoidStateType.FreeFalling)
            {
                ApplyFallDamage();
                if (humanoid.IsAlive || humanoid.StateType != HumanoidStateType.Died)
                    humanoid.ChangeState(HumanoidStateType.Grounded);

                isGrounded = true;

                Landed?.Invoke();
            }
            else
            {
                if (humanoid.StateType != HumanoidStateType.Airborne &&
                    humanoid.StateType != HumanoidStateType.Crouch && 
                    humanoid.StateType != HumanoidStateType.Prone && 
                    humanoid.StateType != HumanoidStateType.Lunging)
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

    /// <summary>
    /// Updates ground detection using sphere checks.
    /// </summary>
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
        
        Vector3 worldBottom = GetGroundBottomOnWorld(bodyCollider);
        preOrigin = humanoid.StateType == HumanoidStateType.Crouch || humanoid.StateType == HumanoidStateType.Prone ?
            worldBottom : preOrigin;

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

    /// <summary>
    /// Updates ceiling detection using sphere checks.
    /// </summary>
    private void CheckHead()
    {
        bool oldCeilingAbove = isCeilingAbove;

        Vector3 multiplier = Vector3.down * (headRadius + headSkin);
        Vector3 preOrigin = headCheck != null ?
            headCheck.position : rootPart.position;

        Vector3 worldTop = GetHeadTopOnWorld(bodyCollider);
        preOrigin = humanoid.StateType == HumanoidStateType.Prone ?
            worldTop : preOrigin;

        Vector3 origin = multiplier + preOrigin;

        bool isChecked = Physics.SphereCast(
            origin,
            headRadius,
            Vector3.up,
            out _,
            headMaxDistance,
            headLayer,
            QueryTriggerInteraction.Ignore
        );

        if (isChecked)
        {
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
                bool ray = Physics.Raycast(origin, Vector3.up, out _, headMaxDistance + headRadius + headSkin, headLayer, QueryTriggerInteraction.Ignore);

                if (ray)
                {
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

    /// <summary>
    /// Handles automatic step-up movement over small obstacles.
    /// </summary>
    private void HandleStepUp()
    {
        if (!canStepUp)
            return;

        Vector3 __direction = _targetMoveDirection;
        __direction.y = 0f;

        if (__direction.sqrMagnitude < 0.001f)
            return;

        __direction.Normalize();
        
        float __effective_step_height = stepHeight;
        if (humanoid.IsCrouching)
            __effective_step_height = __effective_step_height * 0.5f;

        Vector3 __bottom = GetGroundBottomOnWorld(bodyCollider);
        Vector3 __top = GetHeadTopOnWorld(bodyCollider);

        Vector3 __lower_origin = __bottom + Vector3.up * lowerGroundHeight;
        Vector3 __upper_origin = __top + Vector3.up * __effective_step_height;

        float __perfect_check_radius = checkRadius * stepCheckRadiusMultiplier;

        RaycastHit __rhHit_lower;
        bool __bLowerHit = Physics.SphereCast(
            __lower_origin,
            __perfect_check_radius,
            __direction,
            out __rhHit_lower,
            stepCheckDistance,
            feetLayer,
            QueryTriggerInteraction.Ignore
        );
        if (!__bLowerHit) return;

        if (__rhHit_lower.collider == bodyCollider || __rhHit_lower.collider.isTrigger || __rhHit_lower.rigidbody == rigidBody)
            return;

        bool __bUpperHit = Physics.SphereCast(
            __upper_origin,
            __perfect_check_radius,
            __direction,
            out _,
            stepCheckDistance,
            feetLayer,
            QueryTriggerInteraction.Ignore
        );
        if (__bUpperHit) return;

        Vector3 __step_top_origin = __rhHit_lower.point;
        __step_top_origin += (__direction * stepForwardOffset) + (Vector3.up * (__effective_step_height + stepTopExtraHeight));

        RaycastHit __rhHit_upper;
        bool __bTopHit = Physics.Raycast(
            __step_top_origin,
            Vector3.down,
            out __rhHit_upper,
            __effective_step_height + stepTopExtraHeight,
            feetLayer,
            QueryTriggerInteraction.Ignore
        );
        if (!__bTopHit) return;

        float __steppy_amount = __rhHit_upper.point.y - __rhHit_lower.point.y;

        float __steppy_angle = Vector3.Angle(__rhHit_upper.normal, Vector3.up);
        if (__steppy_amount <= 0 || __steppy_amount > stepHeight || __steppy_angle > maxSlopeAngle || isCeilingAbove)
            return;

        Vector3 __head_center_top = GetHeadTopOnWorld(bodyCollider);
        Vector3 __predicted_head_top = __head_center_top + Vector3.up * __steppy_amount;

        Vector3 __predicition_check_origin = __predicted_head_top - Vector3.up * (headRadius + headSkin);

        bool __bHeadBlocked = Physics.SphereCast(
            __predicition_check_origin,
            headRadius,
            Vector3.up,
            out _,
            headRadius + headSkin,
            headLayer,
            QueryTriggerInteraction.Ignore
        );

        if (__bHeadBlocked || CheckColliderOverlap(__steppy_amount))
            return;
        
        ApplySteppingUp(__steppy_amount);
    }   

    /// <summary>
    /// Applies horizontal movement velocity.
    /// </summary>
    private void HandleMovement()
    {
        if (_isDashing && dashStopMovement)
            return;
        
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
        if (humanoid.StateType == HumanoidStateType.Crouch)
            speed = crouchWalkingSpeed;
        else if (humanoid.StateType == HumanoidStateType.Prone)
            speed = proneWalkingSpeed;

        Vector3 targetHorizontalVelocity = onInput ? _targetMoveDirection * speed : Vector3.zero;

        float accel;
        
        if (isGrounded)
            accel = onInput ? acceleration : deceleration;
        else
            accel = onInput ? airAcceleration : airDeceleration;

        if (!isGrounded && momentumOnAir && !onInput)
        {
            targetHorizontalVelocity = horizontalVelocity;
            accel = 0f;
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
        {
            lastMoveDirection = _targetMoveDirection.normalized;

            if (humanoid.StateType != HumanoidStateType.Airborne && 
                humanoid.StateType != HumanoidStateType.FreeFalling && 
                humanoid.StateType != HumanoidStateType.Crouch &&
                humanoid.StateType != HumanoidStateType.Sliding &&
                humanoid.StateType != HumanoidStateType.Prone &&
                humanoid.StateType != HumanoidStateType.Lunging &&
                isGrounded)
                humanoid.ChangeState(_runReady ? HumanoidStateType.Running : HumanoidStateType.Walking);

            if (_runReady)
            {
                _staminaUsedTimer += Time.fixedDeltaTime;

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
    }

    /// <summary>
    /// Applies buffered jump logic and jump velocity.
    /// </summary>
    private void HandleJump()
    {
        bool jumpBuffered = _jumpRequested && Time.time - _lastJumpRequestTime <= jumpBufferTime;
        bool canCoyote = isGrounded || Time.time - _lastGroundedTime <= coyoteTime;
        bool cooldownReady = Time.time - _lastJumpTime >= jumpCooldown;

        if (!cooldownReady || !canCoyote || !jumpBuffered)
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

    /// <summary>
    /// Applies dash movement while dash is active.
    /// </summary>
    private void HandleDashing()
    {
        if (!_isDashing)
            return;

        if (Time.time >= _dashEndTime)
        {
            StopDash();
            return;
        }
        
        if (dashUseCast)
        {
            float stepDistance = dashSpeed * Time.fixedDeltaTime;
            float targetDistance = TestObstacleForDash(_dashDirection, stepDistance);

            if (targetDistance < dashMinDistance)
            {
                StopDash();
                return;
            }
        }

        Vector3 velocity = rigidBody.linearVelocity;
        Vector3 dashVelocity = _dashDirection * dashSpeed;

        velocity.x = dashVelocity.x;
        velocity.z = dashVelocity.z;

        if (!dashLinearDashing)
            velocity.y = 0f;

        rigidBody.linearVelocity = velocity;

        humanoid.ChangeState(HumanoidStateType.Lunging);
        OnDashing?.Invoke();
    }

    /// <summary>
    /// Applies custom gravity, wind, and air resistance.
    /// </summary>
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

        _thisGravity = gravity;

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

    /// <summary>
    /// Applies sliding force on steep slopes.
    /// </summary>
    private void HandleSlopeSliding()
    {
        if (!isGrounded || !isSliding)
            return;

        Vector3 slidingDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

        rigidBody.AddForce(slidingDirection * slopeSlideAcceleration, ForceMode.Acceleration);

        humanoid.ChangeState(HumanoidStateType.Sliding);
        Sliding?.Invoke();
    }

    /// <summary>
    /// Handles automatic rotation when enabled.
    /// </summary>
    private void HandleRotation()
    {
        if (!autoRotate)
            return;

        FaceDirection(_targetMoveDirection);
    }

    /// <summary>
    /// Pushes runtime velocity data into Humanoid.
    /// </summary>
    private void UpdateHumanoidRuntime()
    {
        humanoid.SetHumanoidLinearVelocity(rigidBody.linearVelocity);
        humanoid.SetHumanoidAngularVelocity(rigidBody.angularVelocity);
    }

    /// <summary>
    /// Updates runtime locomotion permissions.
    /// </summary>
    private void UpdatePermissionRuntime()
    {
        canMove = IsLocomotionAllowed(MotorLocomotion.Move) && 
            humanoid.IsAlive &&
            !humanoid.PlatformStanding && 
            _movementLockCount <= 0;
        
        canJump = IsLocomotionAllowed(MotorLocomotion.Jump) &&
            humanoid.IsAlive &&
            !humanoid.PlatformStanding &&
            humanoid.StateType != HumanoidStateType.Crouch &&
            humanoid.StateType != HumanoidStateType.Prone &&
            humanoid.StateType != HumanoidStateType.Sliding &&
            humanoid.StateType != HumanoidStateType.Lunging &&
            _jumpLockCount <= 0;

        canCrouch = IsLocomotionAllowed(MotorLocomotion.Crouch) && humanoid.StateType != HumanoidStateType.Crouch && CanUsePoseInput();

        canProne = IsLocomotionAllowed(MotorLocomotion.Prone) && humanoid.StateType != HumanoidStateType.Prone && CanUsePoseInput();

        canDash = IsLocomotionAllowed(MotorLocomotion.Dash) &&
            humanoid.IsAlive &&
            !humanoid.PlatformStanding &&
            humanoid.StateType != HumanoidStateType.Crouch &&
            humanoid.StateType != HumanoidStateType.Prone;

        canStepUp = IsLocomotionAllowed(MotorLocomotion.SteppingUp) &&
             motorEnabled &&
            enableStepUp &&
            humanoid.IsAlive &&
            isGrounded &&
            !isSliding &&
            !_isDashing &&
            humanoid.StateType != HumanoidStateType.Jumping &&
            humanoid.StateType != HumanoidStateType.Sliding &&
            humanoid.StateType != HumanoidStateType.Prone &&
            humanoid.StateType != HumanoidStateType.Airborne &&
            humanoid.StateType != HumanoidStateType.FreeFalling &&
            _targetMoveDirection.sqrMagnitude > 0.001f;
    }

    #endregion

    #region UnityHelpers

    /// <summary>
    /// Initializes references and runtime defaults.
    /// </summary>
    private void Awake()
    { 
        humanoid = humanoid == null ? GetComponent<Humanoid>() : humanoid;
        rigidBody = rigidBody == null ? GetComponent<Rigidbody>() : rigidBody;
        bodyCollider = bodyCollider == null ? GetComponent<Collider>() : bodyCollider;
        rootPart = rootPart == null ? transform : rootPart;

        rigidBody.freezeRotation = true;
        rigidBody.useGravity = false;
        rigidBody.interpolation = RigidbodyInterpolation.Interpolate;

        _setCrouching = false;
        _setProning = false;
        _crouched = false;
        _proned = false;
        _cantUncrouch = false;
        _cantUnprone = false;
        _jumpRequested = false;
        _standCenterObtained = false;
        _isDashing = false;
        isCeilingAbove = false;
        isGrounded = false;

        _allowedLocomotions = MotorLocomotion.Move;

        _idleStayingTimer = Time.time;
    }

    /// <summary>
    /// Runs motor physics update loop.
    /// </summary>
    private void FixedUpdate()
    {
        if (!motorEnabled || !humanoid.IsAlive)
        {   
            StopMove();
            return;
        }
        
        CheckGround();
        CheckHead();

        HandleFallTracking();
        HandleSlopeSliding();

        UpdatePermissionRuntime();

        HandleJump();
        HandleMovement();
        HandleStepUp();
        HandleMass();
        HandleDashing();
        HandleRotation();
        HandleColliderExtension();

        UpdateHumanoidRuntime();
    }

    /// <summary>
    /// Assigns default component references in the editor.
    /// </summary>
    private void Reset()
    {
        humanoid = GetComponent<Humanoid>();
        rigidBody = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<Collider>();
        rootPart = transform;
    }

    /// <summary>
    /// Clamps serialized values in the editor.
    /// </summary>
    private void OnValidate()
    {
        acceleration = Mathf.Max(0f, acceleration);
        deceleration = Mathf.Max(0f, deceleration);
        airAcceleration = Mathf.Max(0f, airAcceleration);
        airDeceleration = Mathf.Max(0f, airDeceleration);

        bodyHeight = Mathf.Max(0.1f, bodyHeight);
        crouchHeight = Mathf.Clamp(crouchHeight, 0.1f, bodyHeight);
        proneHeight = Mathf.Clamp(proneHeight, 0.1f, crouchHeight);

        jumpHeight = Mathf.Max(0f, jumpHeight);
        jumpCooldown = Mathf.Max(0f, jumpCooldown);
        coyoteTime = Mathf.Max(0f, coyoteTime);
        jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
        jumpCutMultiplier = Mathf.Clamp01(jumpCutMultiplier);

        dashSpeed = Mathf.Max(0f, dashSpeed);
        dashCooldown = Mathf.Max(0f, dashCooldown);
        dashDuration = Mathf.Max(0f, dashDuration);
        dashMinDistance = Mathf.Max(0f, dashMinDistance);

        checkRadius = Mathf.Max(0.01f, checkRadius);
        checkDistance = Mathf.Max(0.01f, checkDistance);
        headRadius = Mathf.Max(0.01f, headRadius);
        headMaxDistance = Mathf.Max(0.01f, headMaxDistance);


    }

    /// <summary>
    /// Clears event references on destroy.
    /// </summary>
    private void OnDestroy()
    {
        Grounded = null;
        Landed = null;
        Sliding = null;
        OnCrouchBegin = null;
        OnProneBegin = null;
        OnAirborneBegin = null;
        OnFreeFallingBegin = null;
        OnDashBegin = null;
        CeilingAboveHeadEnter = null;
        OnAirborne = null;
        OnFreeFalling = null;
        OnJumping = null;
        OnDashing = null;
        Proning = null;
        Crouching = null;
        CeilingAboveHead = null;
        OnDashEnded = null;
        UnCrouched = null;
        UnProned = null;
        CeilingAboveHeadExit = null;
        OnWalking = null;
        OnRunning = null;
        OnRotating = null;
        Idle = null;
        OnHumanoidChanged = null;
        OnRigidBodyChanged = null;
        OnRootPartChanged = null;
        OnBodyColliderChanged = null;
        OnGroundCheckChanged = null;
        OnHeadCheckChanged = null;
        OnBodyHeightChanged = null;
        OnAccelerationChanged = null;
        OnDecelerationChanged = null;
        OnAirAccelerationChanged = null;
        OnAirDecelerationChanged = null;
        OnMovementStrengthChanged = null;
        OnMomentumOnAirChanged = null;
        OnAutoRotateChanged = null;
        OnRotationSpeedChanged = null;
        OnOnlyRotateByMovingChanged = null;
        OnFeetLayerChanged = null;
        OnFloatingVelocityChanged = null;
        OnMaxSlopeAngleChanged = null;
        OnCheckRadiusChanged = null;
        OnCheckDistanceChanged = null;
        OnGroundedStickForceChanged = null;
        OnSlopeSlideAccelerationChanged = null;
        OnIgnoreGroundAfterJumpChanged = null;
        OnFeetSkinChanged = null;
        OnHeadLayerChanged = null;
        OnHeadRadiusChanged = null;
        OnHeadMaxDistanceChanged = null;
        OnHeadSkinChanged = null;
        OnJumpHeightChanged = null;
        OnJumpCooldownChanged = null;
        OnCoyoteTimeChanged = null;
        OnJumpBufferTimeChanged = null;
        OnJumpCutMultiplierChanged = null;
        OnJumpAffectsFallChanged = null;
        OnCrouchHeightChanged = null;
        OnCrouchWalkingSpeedChanged = null;
        OnCrouchTransitionSpeedChanged = null;
        OnUncrouchTransitionSpeedChanged = null;
        OnCrouchFuzzyEquivalenceChanged = null;
        OnCantUncrouchOverCeilingChanged = null;
        OnAutoScaleCrouchMultiplierChanged = null;
        OnProneHeightChanged = null;
        OnProneWalkingSpeedChanged = null;
        OnProneTransitionSpeedChanged = null;
        OnUnproneTransitionSpeedChanged = null;
        OnProneFuzzyEquivalenceChanged = null;
        OnCantUnproneOverCeilingChanged = null;
        OnAutoScaleProneMultiplierChanged = null;
        OnDashUseCastChanged = null;
        OnDashOnlyOnGroundedChanged = null;
        OnDashStopMovementChanged = null;
        OnDashLinearDashingChanged = null;
        OnDashCastMaskChanged = null;
        OnDashSpeedChanged = null;
        OnDashCooldownChanged = null;
        OnDashCheckSkinChanged = null;
        OnDashDurationChanged = null;
        OnDashMinDistanceChanged = null;
        OnGravityScaleChanged = null;
        OnFallingGravityMultiplierChanged = null;
        OnLowJumpGravityMultiplierChanged = null;
        OnMaxFallingSpeedChanged = null;
        OnAirResistanceChanged = null;
        OnWindVelocityChanged = null;
        OnWindInfluenceChanged = null;
        OnEnableStepUpChanged = null;
        OnBodyLayerChanged = null;
        OnStepHeightChanged = null;
        OnStepCheckDistanceChanged = null;
        OnStepSmoothnessChanged = null;
        OnLowerGroundHeightChanged = null;
        OnStepCheckRadiusMultiplierChanged = null;
        OnStepForwardOffsetChanged = null;
        OnStepTopExtraHeightChanged = null;
        OnMinStepHeightChanged = null;
    }

    #endregion
};
