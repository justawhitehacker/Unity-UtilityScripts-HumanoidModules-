  ___ ___                                    .__    .___ _____          __                
 /   |   \ __ __  _____ _____    ____   ____ |__| __| _//     \   _____/  |_  ___________ 
/    ~    \  |  \/     \\__  \  /    \ /  _ \|  |/ __ |/  \ /  \ /  _ \   __\/  _ \_  __ \
\    Y    /  |  /  Y Y  \/ __ \|   |  (  <_> )  / /_/ /    Y    (  <_> )  | (  <_> )  | \/
 \___|_  /|____/|__|_|  (____  /___|  /\____/|__\____ \____|__  /\____/|__|  \____/|__|   
       \/             \/     \/     \/               \/       \/                          

       
Copyright (c) 2026 justawhitehacker (aka Raihan)

# humanoid motor

`HumanoidMotor` is a physics-based locomotion module for unity humanoid characters. it is designed as a lightweight movement layer for a `humanoid` object, as manual locomotion controls.

this module handles movement from direction, jumping, crouching, proning, dashing, slope response, step-up, custom gravity, wind influence, and runtime movement events.

## status

- version: `v1.0.0`
- engine target: unity `>6`
- license: MIT
- main script: `HumanoidMotor.cs`

## info
I wrote this script's features and API manually by my hand, and AI helped me by created its setters, documentations, and README.
Setters that AI wrote, exclusively followed my "golden rule" of the syntaxes.

## module structure

```structure
Humanoid
 ├─ HumanoidMotor
 ├─ HumanoidAnimator
 ├─ HumanoidCombat
 ├─ HumanoidInputController
 └─ AIHumanoidHandler
```

`HumanoidMotor` should be placed with/after Humanoid in the same GameObject

## requirements

attach `HumanoidMotor` to the same gameobject that has:

- `Humanoid`
- `Rigidbody`
- `Collider`

recommended collider types:

- `CapsuleCollider`
- `BoxCollider`
- `SphereCollider`

excellently, `CapsuleCollider` for best control and standard for game systems.

on awake, the motor auto-fills missing references from the same gameobject when possible. it also disables default rigidbody gravity because gravity is handled manually by the motor.

## setup

1. add `HumanoidMotor.cs` into your unity project.
2. attach `Humanoid`, `Rigidbody`, and a `Collider` to the character root.
3. attach `HumanoidMotor` to the same object.
4. assign `feetLayer`, `headLayer`, `bodyLayer`, and `dashCastMask` with layer masks.
5. optionally assign `groundCheck` and `headCheck`; if empty, the motor falls back to collider/root calculations.
6. tune movement, jump, crouch, prone, dash, gravity, and step-up fields in the inspector.

## locomotions

`HumanoidMotor` has only moving locomotions as default, to use other features, you should enable it. (FEATURES BELOW)

## quick example

```csharp
using UnityEngine;

public class PlayerMotorInput : MonoBehaviour
{
    [SerializeField] private HumanoidMotor motor;
    [SerializeField] private Transform cameraRoot;

    private void Start()
    {
        // turn on other locomotions that we needed than only move locomotion
        motor.EnableLocomotions( MotorLocomotion.Everything );
    }

    private void Update()
    {
        Vector3 input = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        Vector3 _cameraRoot = cameraRoot;
        _cameraRoot.Normalize();

        Vector3 direction = _cameraRoot.forward * input.z + _cameraRoot.right * input.x;
        direction.y = 0f;

        bool running = Input.GetKey(KeyCode.LeftShift);
        motor.Move(direction, running);

        if (Input.GetKeyDown(KeyCode.Space))
            motor.Jump();

        if (Input.GetKeyUp(KeyCode.Space))
            motor.StopJumping();

        if (Input.GetKeyDown(KeyCode.LeftControl))
            motor.Crouch();

        if (Input.GetKeyUp(KeyCode.LeftControl))
            motor.UnCrouch();

        if (Input.GetKeyDown(KeyCode.Z))
            motor.Prone();

        if (Input.GetKeyUp(KeyCode.Z))
            motor.UnProne();

        if (Input.GetKeyDown(KeyCode.Q))
            motor.Dash(direction);
    }
}
```

## ai / move-to example

```csharp
using UnityEngine;

public class SimpleHumanoidAI : MonoBehaviour
{
    [SerializeField] private HumanoidMotor motor;
    [SerializeField] private Transform target;
    [SerializeField] private bool running;

    private void FixedUpdate()
    {
        if (target == null)
        {
            motor.StopMove();
            return;
        }

        motor.MoveTo(target.position, running);
    }
}
```

## simple crouch/prone agent input

```csharp
using UnityEngine;

public class SimpleStanceAgent : MonoBehaviour
{
    private enum StanceIntent { Crouch, Prone, Idle };

    [SerializeField] private HumanoidMotor motor;
    
    private StanceIntent _currentStance;
    private void Start()
    {
        _currentStance = StanceIntent.Idle;

        // must enable crouch and prone feature
        // in default, moving, jumping, and stepping-up enabled
        motor.EnableLocomotions( MotorLocomotion.Crouch | MotorLocomotion.Prone );
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            _currentStance = StanceIntent.Crouch;
        
        if (Input.GetKeyUp(KeyCode.LeftShift))
            _currentStance = StanceIntent.Idle;

        if (Input.GetKeyDown(KeyCode.C))
            _currentStance = StanceIntent.Prone;
        
        if (Input.GetKeyUp(KeyCode.C))
            _currentStance = StanceIntent.Idle;

        switch (_currentStance)
        {
            case StanceIntent.Idle:
                motor.UnProne();
                motor.UnCrouch();

            case StanceIntent.Crouch:
                motor.Crouch();

            case StanceIntent.Prone:
                motor.Prone();
        }
    }
};
```

## api reference

### locomotions

[ MotorLocomotions ]
| enumerations | description |
| --- | --- |
| `Move` | where humanoid is able to do movement, such as walking or running. |
| `Jump` | where humanoid is able to do jump locomotion. |
| `Crouch` | where humanoid is able to do crouch system. |
| `Prone` | where humanoid is able to do prone system. |
| `Dash` | where humanoid is able to dash, different from move that moves in smooth, dash is moving humanoid in high-speed. |
| `SteppingUp` | where humanoid is able to move and stepping up on the small obstacles like stairs, small stone/platform, etc. |
| `Controller` | the good choice for simple motor system, where humanoid is able to move, jump, and moving through small obstacles.F Following what `CharacterController` of Unity can do. |
| `Everything` | where humanoid can do any locomotion motives. |

| method | description |
| --- | --- |
| `EnableLocomotions(MotorLocomotion Locomotions)` | enabling the locomotions of Motor, when move locomotion is just only the default of HumanoidMotor. |
| `DisableLocomotions(MotorLocomotion Locomotions)` | disabling the locomotions that didn't be needed. |
| `IsLocomotionAllowed(MotorLocomotion __locomotion)` | comparing if the locomotion was allowed or available to use by the Motor. |

### movement

| method | description |
|---|---|
| `Move(Vector3 Direction, bool Running)` | moves the humanoid toward a world-space direction. y is ignored, then the direction is normalized. `Running` enables running only when stamina and state rules allow it. |
| `StopMove()` | clears movement input and disables running intent. |
| `MoveTo(Vector3 Location)` | moves the humanoid toward a world-space location without running. returns `false` only when movement is not allowed. |
| `MoveTo(Vector3 Location, bool Running)` | moves the humanoid toward a world-space location with optional running. it also stores the target point in `Humanoid`. |
| `FaceDirection(Vector3 Direction)` | rotates the rigidbody toward a world-space direction and updates humanoid facing direction. |

> note: the actual method is `Move(Vector3 Direction, bool Running)`. if you want `Move(direction)` style, call `motor.Move(direction, false)` or add your own wrapper.

### jumping

| method | description |
|---|---|
| `Jump()` | requests a physics jump. the motor supports jump buffering, coyote time, cooldown, and custom gravity scaling. |
| `StopJumping()` | releases jump hold. this allows shorter jump behavior through jump cut and low-jump gravity settings. |

### stance

| method | description |
|---|---|
| `Crouch()` | requests crouch stance. the collider shrinks toward `crouchHeight` during fixed update. |
| `UnCrouch()` | requests standing stance from crouch. can be blocked when `cantUncrouchOverCeiling` is enabled and a ceiling is detected. |
| `Prone()` | requests prone stance. the collider shrinks toward `proneHeight` during fixed update. |
| `UnProne()` | requests standing stance from prone. can be blocked when `cantUnproneOverCeiling` is enabled and a ceiling is detected. |

stance changes are processed as intent. this means calling `Crouch()` or `Prone()` sets the desired stance, while the collider transition happens over time in the motor update.

### dash

| method | description |
|---|---|
| `Dash(Vector3 Direction)` | starts a dash in the given direction. if the direction is almost zero, the motor tries `LastMoveDirection`, then `Humanoid.FacingDirection`. |
| `StopDash()` | stops current dash, clears dash direction, resets horizontal velocity, and invokes dash-ended behavior. |

when `dashUseCast` is enabled, the motor tests obstacles before and during dash. supported cast behavior depends on the active collider type.

### force and transform helpers

| method | description |
|---|---|
| `AddImpulse(Vector3 Direction)` | adds impulse force to the humanoid through `Humanoid.AddForce`. |
| `AddForce(Vector3 Direction)` | adds continuous force to the humanoid through `Humanoid.AddForce`. |
| `Teleport(Vector3 Location)` | moves the rigidbody directly to a world-space location and clears velocity. only works while the humanoid is alive. |
| `Translate(Vector3 Matrix)` | moves the rigidbody by a relative vector and clears velocity. only works while the humanoid is alive. |

### locking

| method | description |
|---|---|
| `LockMovement()` | adds one movement lock. movement stays locked until all locks are removed. |
| `UnlockMovement()` | removes one movement lock, clamped at zero. |
| `LockJump()` | adds one jump lock. jumping stays locked until all locks are removed. |
| `UnlockJump()` | removes one jump lock, clamped at zero. |

lock counters are useful when cutscenes, attacks, stun states, dialogues, or scripted interactions need temporary control over the motor.

## runtime properties

### references

| property | description |
|---|---|
| `Core` | returns the used `Humanoid` reference. |
| `RigidBody` | returns the used `Rigidbody`. |
| `RootPart` | returns the movement root transform. |
| `BodyCollider` | returns the active body collider. |
| `GroundCheck` | returns the assigned ground checker transform. |
| `FloorMaterial` | returns current floor physics material. |
| `FloorCollider` | returns current floor collider. |
| `FeetLayer` | returns ground detection layer mask. |
| `HeadLayer` | returns ceiling detection layer mask. |

### movement data

| property | description |
|---|---|
| `GroundNormal` | current ground normal. |
| `MoveDirection` | current normalized move direction. |
| `LastMoveDirection` | last non-zero move direction. |
| `WindVelocity` | current wind velocity. |
| `FallStartY` | y position where the current fall started. |
| `FallDistance` | tracked fall distance from fall start to current position. |

### state checks

| property | description |
|---|---|
| `JumpHolding` | true while jump input is being held. |
| `MotorEnabled` | true when motor runtime is enabled. |
| `IsGrounded` | true when the motor detects ground. |
| `IsOnSlope` | true when standing on a slope. |
| `IsSliding` | true when the slope angle is above the maximum allowed slope angle. |
| `IsCeilingAbove` | true when ceiling detection finds an obstacle above the humanoid. |

### tuning readbacks

| property | description |
|---|---|
| `Acceleration` | ground acceleration. |
| `Deceleration` | ground deceleration. |
| `AirAcceleration` | air acceleration. |
| `AirDeceleration` | air deceleration. |
| `MovementStrength` | base movement strength value. |
| `RotationSpeed` | rotation interpolation speed. |
| `MaxSlopeAngle` | maximum stable slope angle before sliding. |
| `CheckRadius` | ground check radius. |
| `CheckDistance` | ground check distance. |
| `GroundedStickForce` | downward stick force while grounded. |
| `SlopeSlideAcceleration` | acceleration applied while sliding. |
| `IgnoreGroundAfterJump` | small time window where ground is ignored after jump. |
| `JumpHeight` | configured jump height. |
| `JumpCooldown` | jump cooldown duration. |
| `CoyoteTime` | time after leaving ground where jump is still accepted. |
| `JumpBufferTime` | time window for buffered jump input. |
| `JumpCutMultiplier` | vertical velocity multiplier used for shorter jump behavior. |
| `GravityScale` | custom gravity scale. |
| `FallingGravityMultiplier` | extra gravity multiplier while falling. |
| `LowJumpGravityMultiplier` | extra gravity multiplier when jump is released early. |
| `MaxFallingSpeed` | maximum downward speed. |
| `AirResistance` | horizontal air resistance. |
| `WindInfluence` | wind influence multiplier. |
| `StepHeight` | maximum step height. |
| `StepCheckDistance` | forward step detection distance. |
| `StepSmoothness` | step-up smoothing speed. |
| `MomentumOnAir` | true when momentum should remain in air without movement input. |
| `AutoRotate` | true when motor should automatically rotate toward movement direction. |
| `OnlyRotateByMoving` | true when rotation only happens while direction has input. |
| `EnableStepUp` | true when step-up logic is enabled. |

## events

### ground and slope

| event | description |
|---|---|
| `Grounded` | invoked when the motor becomes grounded. |
| `Landed` | invoked when landing from freefall. |
| `Sliding` | invoked while sliding on a slope that is too steep. |

### movement

| event | description |
|---|---|
| `OnWalking(Vector3 direction)` | invoked while walking with movement input. |
| `OnRunning(Vector3 direction)` | invoked while running with movement input. |
| `OnRotating(Vector3 direction)` | invoked when the motor rotates toward a direction. |
| `Idle(float idleCounter)` | invoked while idle, with an idle counter value. |

### jump and air

| event | description |
|---|---|
| `OnJumping` | invoked when jump is executed. |
| `OnAirborneBegin` | invoked when airborne state begins. |
| `OnAirborne` | invoked while airborne. |
| `OnFreeFallingBegin` | invoked when freefall begins. |
| `OnFreeFalling` | invoked while freefalling. |

### stance

| event | description |
|---|---|
| `OnCrouchBegin` | invoked when crouch transition reaches crouch target. |
| `Crouching` | invoked while crouch intent is active. |
| `UnCrouched` | invoked when collider returns to standing from crouch. |
| `OnProneBegin` | invoked when prone transition reaches prone target. |
| `Proning` | invoked while prone intent is active. |
| `UnProned` | invoked when collider returns to standing from prone. |

### dash

| event | description |
|---|---|
| `OnDashBegin` | invoked when dash starts. |
| `OnDashing` | invoked while dash is active. |
| `OnDashEnded` | invoked when dash stops. |

### ceiling

| event | description |
|---|---|
| `CeilingAboveHeadEnter` | invoked when ceiling detection first detects an obstacle. |
| `CeilingAboveHead` | invoked while a ceiling is detected. |
| `CeilingAboveHeadExit` | invoked when ceiling detection stops detecting an obstacle. |

## event example

```csharp
using UnityEngine;

public class MotorEventExample : MonoBehaviour
{
    [SerializeField] private HumanoidMotor motor;

    private void OnEnable()
    {
        motor.Landed += OnLanded;
        motor.OnDashBegin += OnDashBegin;
        motor.OnWalking += OnWalking;
    }

    private void OnDisable()
    {
        motor.Landed -= OnLanded;
        motor.OnDashBegin -= OnDashBegin;
        motor.OnWalking -= OnWalking;
    }

    private void OnLanded()
    {
        Debug.Log("landed");
    }

    private void OnDashBegin()
    {
        Debug.Log("dash started");
    }

    private void OnWalking(Vector3 direction)
    {
        Debug.Log($"walking: {direction}");
    }
}
```

## inspector overview

### references

| field | purpose |
|---|---|
| `humanoid` | core humanoid reference. |
| `rigidBody` | rigidbody used for physics movement. |
| `rootPart` | root transform used for movement and rotation. |
| `bodyCollider` | collider used for stance, ground, dash, and obstacle checks. |
| `groundCheck` | optional transform used as ground origin. |
| `headCheck` | optional transform used as ceiling origin. |
| `bodyHeight` | standing height reference for collider resizing. |

### movement

| field | purpose |
|---|---|
| `acceleration` | how fast grounded velocity reaches target speed. |
| `deceleration` | how fast grounded velocity stops. |
| `airAcceleration` | how fast air velocity responds to input. |
| `airDeceleration` | how fast air velocity slows down. |
| `movementStrength` | general movement strength value. |
| `momentumOnAir` | keeps horizontal momentum while airborne and without input. |

### rotation

| field | purpose |
|---|---|
| `autoRotate` | automatically faces movement direction. |
| `rotationSpeed` | rotation interpolation speed. |
| `onlyRotateByMoving` | prevents rotation when direction is almost zero. |

### ground and slope

| field | purpose |
|---|---|
| `feetLayer` | layer mask used for ground detection. |
| `floatingVelocity` | velocity threshold used for airborne/freefall handling. |
| `maxSlopeAngle` | slope angle limit before sliding. |
| `checkRadius` | sphere radius for ground check. |
| `checkDistance` | ground check distance. |
| `groundedStickForce` | downward force used to keep grounded body stable. |
| `slopeSlideAcceleration` | acceleration used when sliding. |
| `ignoreGroundAfterJump` | short ground ignore window after jumping. |
| `feetSkin` | small offset for ground checking. |

### ceiling

| field | purpose |
|---|---|
| `headLayer` | layer mask used for ceiling detection. |
| `headRadius` | sphere radius for ceiling check. |
| `headMaxDistance` | ceiling check distance. |
| `headSkin` | small offset for ceiling checking. |

### jump

| field | purpose |
|---|---|
| `jumpHeight` | target jump height. |
| `jumpCooldown` | delay between jump executions. |
| `coyoteTime` | grace window after leaving ground. |
| `jumpBufferTime` | grace window for jump input before landing. |
| `jumpCutMultiplier` | vertical velocity multiplier for short jump behavior. |
| `jumpAffectsFall` | controls how jump state interacts with airborne/freefall tracking. |

### crouch

| field | purpose |
|---|---|
| `crouchHeight` | target collider height while crouching. |
| `crouchWalkingSpeed` | movement speed while crouching. |
| `crouchTransitionSpeed` | transition speed into crouch. |
| `uncrouchTransitionSpeed` | transition speed back to standing. |
| `crouchFuzzyEquivalence` | tolerance used to snap crouch transition into final value. |
| `cantUncrouchOverCeiling` | blocks uncrouch while ceiling is detected. |
| `autoScaleCrouchMultiplier` | smoothing multiplier for crouch resize. |

### prone

| field | purpose |
|---|---|
| `proneHeight` | target collider height while proning. |
| `proneWalkingSpeed` | movement speed while proning. |
| `proneTransitionSpeed` | transition speed into prone. |
| `unproneTransitionSpeed` | transition speed back to standing. |
| `proneFuzzyEquivalence` | tolerance used to snap prone transition into final value. |
| `cantUnproneOverCeiling` | blocks unprone while ceiling is detected. |
| `autoScaleProneMultiplier` | smoothing multiplier for prone resize. |

### dash

| field | purpose |
|---|---|
| `dashUseCast` | checks obstacles before and during dash. |
| `dashOnlyOnGrounded` | allows dash only while grounded. |
| `dashStopMovement` | stops normal movement during dash. |
| `dashLinearDashing` | controls whether dash preserves vertical velocity. |
| `dashCastMask` | layer mask used for dash obstacle checks. |
| `dashSpeed` | dash horizontal speed. |
| `dashCooldown` | delay between dash executions. |
| `dashCheckSkin` | safe distance offset from dash collision. |
| `dashDuration` | maximum dash duration. |
| `dashMinDistance` | minimum allowed dash distance. |

### gravity and external force

| field | purpose |
|---|---|
| `gravityScale` | multiplier for custom gravity. |
| `fallingGravityMultiplier` | stronger gravity while falling. |
| `lowJumpGravityMultiplier` | stronger gravity when jump is released early. |
| `maxFallingSpeed` | clamps maximum falling velocity. |
| `airResistance` | reduces horizontal air velocity. |
| `windVelocity` | wind velocity applied while airborne. |
| `windInfluence` | wind influence multiplier. |

### step-up

| field | purpose |
|---|---|
| `enableStepUp` | enables step-up logic. |
| `bodyLayer` | layer mask used for body overlap checks. |
| `stepHeight` | maximum step height. |
| `stepCheckDistance` | forward distance for detecting steps. |
| `stepSmoothness` | smoothing amount for step movement. |
| `lowerGroundHeight` | lower origin height for step check. |
| `stepCheckRadiusMultiplier` | radius multiplier for step casts. |
| `stepForwardOffset` | forward offset used to check step top. |
| `stepTopExtraHeight` | extra height used for top-down step detection. |
| `minStepHeight` | minimum step height reference. |

## behavior notes

- movement is applied through `Rigidbody.linearVelocity` inside fixed update.
- unity default gravity is disabled on the rigidbody; the motor applies custom gravity manually.
- ground detection uses sphere cast, check sphere, and raycast fallback.
- ceiling detection uses sphere cast, check sphere, and raycast fallback.
- slope movement projects direction onto the ground normal.
- steep slopes trigger sliding when the slope angle is greater than `maxSlopeAngle`.
- crouch and prone modify collider height/center over time instead of instantly snapping every frame.
- dash can use collider casts to reduce wall clipping or obstacle tunneling.
- step-up is skipped while airborne, sliding, dashing, proning, or not moving.

## recommended usage pattern

keep the motor as a locomotion backend:

```txt
input / ai / cutscene / ability system
        ↓
humanoidmotor api
        ↓
rigidbody + collider + humanoid state
```

example responsibilities:

| system | should do |
|---|---|
| input script | read keyboard, controller, or touch, then call `Move`, `Jump`, `Crouch`, `Dash`, etc. |
| ai script | calculate path/waypoints, then call `MoveTo` or `Move`. |
| animator script | listen to motor/humanoid states and play animations. |
| combat script | lock movement/jump during attacks, knockback using `AddImpulse`, or force dash using `Dash`. |
| humanoidmotor | only handle locomotion physics and movement state. |

## common examples

### walk without running

```csharp
motor.Move(direction, false);
```

### run when allowed

```csharp
motor.Move(direction, true);
```

### stop all movement input

```csharp
motor.StopMove();
```

### move toward a target

```csharp
bool accepted = motor.MoveTo(targetPosition, true);
```

### jump with release

```csharp
motor.Jump();

// when the button is released
motor.StopJumping();
```

### crouch hold

```csharp
if (Input.GetKeyDown(KeyCode.LeftControl))
    motor.Crouch();

if (Input.GetKeyUp(KeyCode.LeftControl))
    motor.UnCrouch();
```

### prone toggle

```csharp
private bool proning;

private void ToggleProne()
{
    proning = !proning;

    if (proning)
        motor.Prone();
    else
        motor.UnProne();
}
```

### dash forward

```csharp
motor.Dash(transform.forward);
```

### temporary stun

```csharp
motor.LockMovement();
motor.LockJump();

// later
motor.UnlockMovement();
motor.UnlockJump();
```

### knockback

```csharp
Vector3 knockback = -transform.forward * 12f + Vector3.up * 3f;
motor.AddImpulse(knockback);
```

## contact

for problems or recommendations, contact:

```txt
instagram: @raihanaufal_77
```

responses may take time.
