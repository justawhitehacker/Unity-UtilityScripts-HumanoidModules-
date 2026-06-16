  ___ ___                                    .__    .___
 /   |   \ __ __  _____ _____    ____   ____ |__| __| _/
/    ~    \  |  \/     \\__  \  /    \ /  _ \|  |/ __ | 
\    Y    /  |  /  Y Y  \/ __ \|   |  (  <_> )  / /_/ | 
 \___|_  /|____/|__|_|  (____  /___|  /\____/|__\____ | 
       \/             \/     \/     \/               \/ 

Copyright (c) 2026 by justawhitehacker (aka Raihan Naufal Azmi)
# humanoid

`Humanoid` is the core character module for a unity humanoid-style character system.

this component stores and manages the character's main runtime data, such as health, stamina, movement attributes, state type, owner type, physics info, fall damage settings, status effects, and simple helper methods.

basically:

```txt
Humanoid        -> character core data, state, health, stamina, status
HumanoidMotor   -> physics-based movement and locomotion
HumanoidAnimator -> animation handling
HumanoidCombat  -> combat / damage extension
AIHumanoidHandler -> ai-side controller
```

---

## features

- health and max health system
- stamina and max stamina system
- walk speed, running speed, and jump power attributes
- health regeneration
- stamina regeneration
- fall damage configuration
- state management using `HumanoidStateType`
- owner management using `HumanoidOwnerType`
- permanent status system
- temporary status system
- target point storage for `MoveTo` behavior
- physics information cache
- event-based callbacks for attribute changes
- helper methods for direction, facing, movement amount, and attached motor access

---

## requirements

attach `Humanoid` to a unity gameobject `Rigidbody`

the script uses:

```csharp
[RequireComponent(typeof(Rigidbody))]
```

so unity will expect those components to exist on the same object.

recommended setup:

```txt
player
├── humanoid
├── humanoidmotor
├── humanoidanimator
└── humanoidcombat
```

or directly:

```txt
character root
├── Rigidbody
├── Humanoid
└── HumanoidMotor
```

---

## installation

1. put `Humanoid.cs` inside your unity project.
2. attach it to your character root object.
3. make sure the object has a `Rigidbody`
4. configure the attributes in the inspector.
5. use other modules like `HumanoidMotor` to control movement.

example:

```csharp
using UnityEngine;

public class PlayerBootstrap : MonoBehaviour
{
    [SerializeField] private Humanoid humanoid;

    private void Awake()
    {
        if (humanoid == null)
            humanoid = GetComponent<Humanoid>();

        humanoid.ChangeOwner(HumanoidOwnerType.Player);
    }
}
```

---

## humanoid state type

`HumanoidStateType` represents the current conceptual state of the character.

```csharp
public enum HumanoidStateType
{
    Running,
    Walking,
    Jumping,
    Idle,
    Died,
    Airborne,
    FreeFalling,
    Grounded,
    Sliding,
    Crouch,
    Prone,
    Lunging,
    Neutral
}
```

| state | meaning |
|---|---|
| `Running` | character is moving with running enabled |
| `Walking` | character is moving normally |
| `Jumping` | character has started a jump |
| `Idle` | character is alive but not doing active movement |
| `Died` | character health reached zero |
| `Airborne` | character is in the air |
| `FreeFalling` | character is falling downward |
| `Grounded` | character has landed or touched ground |
| `Sliding` | character is sliding on a steep slope |
| `Crouch` | character is crouching |
| `Prone` | character is proning |
| `Lunging` | character is dashing or lunging |
| `Neutral` | no specific state is assigned |

---

## humanoid owner type

`HumanoidOwnerType` defines who controls the character.

```csharp
public enum HumanoidOwnerType
{
    Player,
    AI,
    Neutral
}
```

| owner | meaning |
|---|---|
| `Player` | controlled by player input |
| `AI` | controlled by script or ai behavior |
| `Neutral` | no controller owner assigned |

---

## inspector fields

### references

| field | description |
|---|---|
| `rigidBody` | rigidbody used by the humanoid |
| `rootPart` | root transform of the character |

### owner

| field | description |
|---|---|
| `ownerType` | defines whether the humanoid is controlled by player, ai, or neutral logic |

### simple physics informations

| field | description |
|---|---|
| `linearVelocity` | cached linear velocity |
| `angularVelocity` | cached angular velocity |
| `lastMoveDirection` | last movement direction |
| `facingDirection` | current facing direction |

### details

| field | description |
|---|---|
| `canApplyFallDamage` | allows or blocks fall damage |
| `safeFromFallDistance` | fall distance threshold before damage can happen |
| `fallDamageMultiplier` | damage multiplier used by fall damage logic |
| `walkToStoppingDistance` | stopping distance used by `MoveTo` logic |
| `cameraOffset` | camera offset value for camera-related modules |
| `platformStanding` | disables movement-like actions when enabled |

### max attributes

| field | description |
|---|---|
| `maxHealth` | maximum health |
| `maxWalkSpeed` | maximum walk speed |
| `maxRunningSpeed` | maximum running speed |
| `maxJumpPower` | maximum jump power |
| `maxStamina` | maximum stamina |

### regeneration

| field | description |
|---|---|
| `healthRegenerationEnabled` | enables health regeneration |
| `healthRegenerationAmount` | health gained per regen tick |
| `healthRegenerationTick` | delay between health regen ticks |
| `healthRegenerationDelay` | delay after damage before regen starts |
| `staminaRegenerationEnabled` | enables stamina regeneration |
| `staminaRegenerationAmount` | stamina gained per regen tick |
| `staminaRegenerationTick` | delay between stamina regen ticks |
| `staminaRegenerationDelay` | delay after stamina use before regen starts |

### runtime attributes

| field | description |
|---|---|
| `health` | current health |
| `walkSpeed` | current walk speed |
| `runningSpeed` | current running speed |
| `jumpPower` | current jump power |
| `stamina` | current stamina |

---

## public properties

### max attributes

| property | type | description |
|---|---|---|
| `MaxHealth` | `float` | max health value |
| `MaxWalkSpeed` | `float` | max walk speed value |
| `MaxRunningSpeed` | `float` | max running speed value |
| `MaxJumpPower` | `float` | max jump power value |
| `MaxStamina` | `float` | max stamina value |

### state and owner

| property | type | description |
|---|---|---|
| `StateType` | `HumanoidStateType` | current humanoid state |
| `OwnerType` | `HumanoidOwnerType` | current humanoid owner type |

### physics

| property | type | description |
|---|---|---|
| `LinearVelocity` | `Vector3` | cached linear velocity |
| `AngularVelocity` | `Vector3` | cached angular velocity |
| `VerticalVelocity` | `float` | y axis velocity |
| `HorizontalSpeed` | `float` | horizontal movement speed |
| `MoveDirection` | `Vector3` | current normalized movement direction |
| `LastMoveDirection` | `Vector3` | last normalized movement direction |
| `FacingDirection` | `Vector3` | current facing direction |
| `RigidBody` | `Rigidbody` | attached rigidbody |
| `RootPart` | `Transform` | character root transform |

### attributes

| property | type | description |
|---|---|---|
| `Health` | `float` | current health |
| `WalkSpeed` | `float` | current walk speed |
| `RunningSpeed` | `float` | current running speed |
| `JumpPower` | `float` | current jump power |
| `Stamina` | `float` | current stamina |
| `WalkToStoppingDistance` | `float` | minimum distance used by move-to behavior |

### fall damage

| property | type | description |
|---|---|---|
| `CanApplyFallDamage` | `bool` | whether fall damage can be applied |
| `SafeFromFallDistance` | `float` | safe fall distance threshold |
| `FallDamageMultiplier` | `float` | fall damage multiplier |

### regeneration

| property | type | description |
|---|---|---|
| `HealthRegenerationEnabled` | `bool` | whether health regen is enabled |
| `StaminaRegenerationEnabled` | `bool` | whether stamina regen is enabled |
| `HealthRegenerationAmount` | `float` | health gained per tick |
| `HealthRegenerationTick` | `float` | health regen tick duration |
| `HealthRegenerationDelay` | `float` | delay before health regen starts |
| `StaminaRegenerationAmount` | `float` | stamina gained per tick |
| `StaminaRegenerationTick` | `float` | stamina regen tick duration |
| `StaminaRegenerationDelay` | `float` | delay before stamina regen starts |

### booleans

| property | type | description |
|---|---|---|
| `IsAlive` | `bool` | true if the humanoid is alive |
| `IsMoving` | `bool` | true if the humanoid is moving |
| `IsJumping` | `bool` | true if the humanoid is jumping |
| `IsCrouching` | `bool` | true if the humanoid is crouching |
| `IsProning` | `bool` | true if the humanoid is proning |
| `PlatformStanding` | `bool` | true if movement-style actions should be blocked |

### vectors
| `HasTargetPoint` | `bool` | true if humanoid has a target point |
| `TargetPoint` | `Vector3` | current target point |

---

## api reference

### health

#### `TakeDamage(float amount)`

decreases health by the given damage amount.

```csharp
humanoid.TakeDamage(25f);
```

if health reaches zero, the humanoid will be killed and changed to `HumanoidStateType.Died`.

#### `Heal(float amount)`

increases health by the given amount.

```csharp
humanoid.Heal(10f);
```

health is clamped by `MaxHealth`.

#### `Kill()`

kills the humanoid immediately.

```csharp
humanoid.Kill();
```

this sets health to `0`, changes state to `Died`, and invokes `Died`.

#### `Revive(float reviveHealthAmount)`

revives the humanoid if it is dead.

```csharp
humanoid.Revive(50f);
```

revive health is clamped between `1` and `MaxHealth`.

---

### stamina

#### `TryUsingStamina(float amount)`

tries to consume stamina.

```csharp
if (humanoid.TryUsingStamina(10f))
{
    // stamina was consumed
}
```

returns `true` if stamina is enough.  
returns `false` if stamina is too low.

this is useful for running, dashing, skills, attacks, or any other action that should drain stamina. because apparently infinite stamina is how you accidentally make a god instead of a character.

---

### movement data

#### `StopMovement()`

stops horizontal movement and changes state to `Idle`.

```csharp
humanoid.StopMovement();
```

this does not fully replace motor movement logic. it is mostly a core helper used by systems like `HumanoidMotor`.

#### `SetHumanoidTargetPoint(Vector3 point)`

sets the current target point.

```csharp
humanoid.SetHumanoidTargetPoint(destination);
```

this is commonly used by `HumanoidMotor.MoveTo()` or ai movement.

#### `ClearHumanoidTargetPoint()`

clears the current target point flag.

```csharp
humanoid.ClearHumanoidTargetPoint();
```

---

### state

#### `ChangeState(HumanoidStateType newType)`

changes the current humanoid state.

```csharp
humanoid.ChangeState(HumanoidStateType.Walking);
```

the state will not change if that state is disabled by `SetStateEnabled`.

#### `IsInState(HumanoidStateType type)`

checks whether the humanoid is currently in the given state.

```csharp
if (humanoid.IsInState(HumanoidStateType.FreeFalling))
{
    // falling logic
}
```

#### `SetStateEnabled(HumanoidStateType state, bool enable)`

enables or disables a state.

```csharp
humanoid.SetStateEnabled(HumanoidStateType.Jumping, false);
```

when a state is disabled, `ChangeState` will ignore it.

#### `IsStateEnabled(HumanoidStateType state)`

checks whether a state is currently enabled.

```csharp
bool canEnterJumpingState = humanoid.IsStateEnabled(HumanoidStateType.Jumping);
```

---

### owner

#### `ChangeOwner(HumanoidOwnerType newType)`

changes who controls the humanoid.

```csharp
humanoid.ChangeOwner(HumanoidOwnerType.Player);
```

or for ai:

```csharp
humanoid.ChangeOwner(HumanoidOwnerType.AI);
```

---

### force

#### `AddForce(Vector3 direction)`

adds force to the humanoid rigidbody.

```csharp
humanoid.AddForce(Vector3.forward * 10f);
```

#### `AddForce(Vector3 direction, ForceMode forceMode)`

adds force with a specific `ForceMode`.

```csharp
humanoid.AddForce(Vector3.up * 8f, ForceMode.Impulse);
```

this is a wrapper around rigidbody force usage, with an alive check.

---

### facing

#### `FaceDirection(Vector3 direction, bool rotate)`

sets the humanoid facing direction.

```csharp
humanoid.FaceDirection(transform.forward, true);
```

if `rotate` is true, the rigidbody rotation is updated.  
if `rotate` is false, only the facing direction data is updated.

---

### attribute setters

#### current attributes

```csharp
humanoid.SetHumanoidHealth(100f);
humanoid.SetHumanoidWalkSpeed(8f);
humanoid.SetHumanoidRunningSpeed(14f);
humanoid.SetHumanoidJumpPower(10f);
humanoid.SetHumanoidStamina(150f);
```

these values are clamped by their max attributes.

#### max attributes

```csharp
humanoid.SetHumanoidMaxHealth(150f);
humanoid.SetHumanoidMaxWalkSpeed(12f);
humanoid.SetHumanoidMaxRunningSpeed(20f);
humanoid.SetHumanoidMaxJumpPower(18f);
humanoid.SetHumanoidMaxStamina(250f);
```

changing max attributes also clamps the current attributes.

---

### stamina drain settings

```csharp
humanoid.SetHumanoidStaminaDecrementAmount(2f);
humanoid.SetHumanoidStaminaDecrementTick(0.2f);
```

these are useful for systems like running stamina drain.

---

### regeneration settings

#### health regeneration

```csharp
humanoid.SetHumanoidHealthRegenerationEnabled(true);
humanoid.SetHumanoidHealthRegenerationAmount(1f);
humanoid.SetHumanoidHealthRegenerationTick(0.8f);
humanoid.SetHumanoidHealthRegenerationDelay(3f);
```

health regeneration starts after the humanoid has not taken damage for the configured delay.

#### stamina regeneration

```csharp
humanoid.SetHumanoidStaminaRegenerationEnabled(true);
humanoid.SetHumanoidStaminaRegenerationAmount(5f);
humanoid.SetHumanoidStaminaRegenerationTick(0.75f);
humanoid.SetHumanoidStaminaRegenerationDelay(2f);
```

stamina regeneration starts after stamina has not been used for the configured delay.

---

### fall damage settings

```csharp
humanoid.SetHumanoidCanApplyFallDamage(true);
humanoid.SetHumanoidSafeFromFallDistance(3.5f);
humanoid.SetHumanoidFallDamageMultiplier(10f);
```

`Humanoid` stores the fall damage configuration.  
the actual fall tracking and damage application can be handled by movement systems such as `HumanoidMotor`.

---

### platform standing

#### `SetHumanoidPlatformStanding(bool enable)`

sets whether the humanoid is platform-standing.

```csharp
humanoid.SetHumanoidPlatformStanding(true);
```

when enabled, movement modules can treat the humanoid as unable to move or jump.

---

### camera offset

#### `SetHumanoidCameraOffset(Vector3 offset)`

sets camera offset data.

```csharp
humanoid.SetHumanoidCameraOffset(new Vector3(0f, 1.6f, 0f));
```

this does not move the camera by itself. it only stores the data for camera-related modules.

---

### runtime flags

these methods are mostly intended for internal module syncing.

```csharp
humanoid.SetHumanoidIsMoving(true);
humanoid.SetHumanoidIsJumping(true);
humanoid.SetHumanoidIsCrouching(true);
humanoid.SetHumanoidIsProning(true);
```

for example, `HumanoidMotor` can update these when movement, jumping, crouching, or proning happens.

---

## direction helpers

### `GetLocalMoveDirection()`

returns the current move direction in local space.

```csharp
Vector3 localMove = humanoid.GetLocalMoveDirection();
```

### `GetGlobalMoveDirection()`

returns the current move direction in world space.

```csharp
Vector3 worldMove = humanoid.GetGlobalMoveDirection();
```

### `GetFacingDirection()`

returns the current facing direction.

```csharp
Vector3 facing = humanoid.GetFacingDirection();
```

### `GetForwardAmount()`

returns how much the humanoid is moving forward or backward relative to its transform.

```csharp
float forward = humanoid.GetForwardAmount();
```

value range is roughly:

```txt
1   -> moving forward
0   -> moving sideways
-1  -> moving backward
```

### `GetRightAmount()`

returns how much the humanoid is moving right or left relative to its transform.

```csharp
float right = humanoid.GetRightAmount();
```

value range is roughly:

```txt
1   -> moving right
0   -> moving forward/backward
-1  -> moving left
```

### `GetMotor()`

returns the attached `HumanoidMotor`.

```csharp
HumanoidMotor motor = humanoid.GetMotor();
```

if no motor exists, it logs an error and returns `null`.

---

## status system

`Humanoid` supports two types of statuses:

```txt
permanent status  -> stays until removed manually
temporary status  -> expires after timeout
```

### permanent status

#### `AddStatus(string statusName)`

adds a permanent status.

```csharp
humanoid.AddStatus("stunned");
```

#### `HasStatus(string statusName)`

checks whether a permanent status exists.

```csharp
if (humanoid.HasStatus("stunned"))
{
    humanoid.StopMovement();
}
```

#### `RemoveStatus(string statusName)`

removes a permanent status.

```csharp
humanoid.RemoveStatus("stunned");
```

#### `ChangeStatus(string fromStatus, string toStatus)`

replaces one status with another.

```csharp
humanoid.ChangeStatus("burning", "scorched");
```

#### `GetStatuses()`

returns all permanent statuses.

```csharp
List<string> statuses = humanoid.GetStatuses();
```

---

### temporary status

#### `AddTemporaryStatus(string statusName, float timeout)`

adds or updates a temporary status.

```csharp
humanoid.AddTemporaryStatus("slowed", 3f);
```

the status expires after `timeout` seconds.

#### `HasTemporaryStatus(string statusName)`

checks whether a temporary status is still active.

```csharp
if (humanoid.HasTemporaryStatus("slowed"))
{
    humanoid.SetHumanoidWalkSpeed(3f);
}
```

#### `RemoveTemporaryStatus(string statusName)`

removes a temporary status manually.

```csharp
humanoid.RemoveTemporaryStatus("slowed");
```

#### `GetTemporaryStatuses()`

returns all temporary status names.

```csharp
List<string> temporary = humanoid.GetTemporaryStatuses();
```

---

## events

### life events

| event | description |
|---|---|
| `Died` | invoked when humanoid dies |
| `Revived` | invoked when humanoid is revived |
| `Damaged(float amount)` | invoked when damage is applied |
| `Healed(float amount)` | invoked when healing is applied |

### regeneration events

| event | description |
|---|---|
| `OnHealthRegenerationEnabledChanged` | invoked when health regen toggle changes |
| `OnStaminaRegenerationEnabledChanged` | invoked when stamina regen toggle changes |

### status events

| event | description |
|---|---|
| `OnStatusAdded(string status)` | invoked when permanent status is added |
| `OnStatusRemoved(string status)` | invoked when permanent status is removed |
| `OnStatusChanged(string from, string to)` | invoked when permanent status changes |
| `OnTemporaryStatusAddedOrChanged(string status, float timeout)` | invoked when temporary status is added or refreshed |
| `OnTemporaryStatusExpired(string status)` | invoked when temporary status expires |
| `OnTemporaryStatusRemoved(string status)` | invoked when temporary status is manually removed |

### attribute events

| event | description |
|---|---|
| `OnHealthChanged(float oldValue, float newValue)` | invoked when health changes |
| `OnStaminaChanged(float oldValue, float newValue)` | invoked when stamina changes |
| `OnWalkSpeedChanged(float oldValue, float newValue)` | invoked when walk speed changes |
| `OnRunningSpeedChanged(float oldValue, float newValue)` | invoked when running speed changes |
| `OnJumpPowerChanged(float oldValue, float newValue)` | invoked when jump power changes |
| `OnMaxHealthChanged(float oldValue, float newValue)` | invoked when max health changes |
| `OnMaxStaminaChanged(float oldValue, float newValue)` | invoked when max stamina changes |
| `OnMaxWalkSpeedChanged(float oldValue, float newValue)` | invoked when max walk speed changes |
| `OnMaxRunningSpeedChanged(float oldValue, float newValue)` | invoked when max running speed changes |
| `OnMaxJumpPowerChanged(float oldValue, float newValue)` | invoked when max jump power changes |

### config events

| event | description |
|---|---|
| `OnStaminaDecrementAmountChanged(float oldValue, float newValue)` | invoked when stamina drain amount changes |
| `OnStaminaDecrementTickChanged(float oldValue, float newValue)` | invoked when stamina drain tick changes |
| `OnHealthRegenerationAmountChanged(float oldValue, float newValue)` | invoked when health regen amount changes |
| `OnHealthRegenerationTickChanged(float oldValue, float newValue)` | invoked when health regen tick changes |
| `OnHealthRegenerationDelayChanged(float oldValue, float newValue)` | invoked when health regen delay changes |
| `OnStaminaRegenerationAmountChanged(float oldValue, float newValue)` | invoked when stamina regen amount changes |
| `OnStaminaRegenerationTickChanged(float oldValue, float newValue)` | invoked when stamina regen tick changes |
| `OnStaminaRegenerationDelayChanged(float oldValue, float newValue)` | invoked when stamina regen delay changes |
| `OnFallDamageMultiplierChanged(float oldValue, float newValue)` | invoked when fall damage multiplier changes |
| `OnSafeFromFallDistanceChanged(float oldValue, float newValue)` | invoked when safe fall distance changes |
| `OnWalkToStoppingDistanceChanged(float oldValue, float newValue)` | invoked when move-to stopping distance changes |

### runtime events

| event | description |
|---|---|
| `OnLinearVelocityChanged(Vector3 oldValue, Vector3 newValue)` | invoked when cached linear velocity changes |
| `OnAngularVelocityChanged(Vector3 oldValue, Vector3 newValue)` | invoked when cached angular velocity changes |
| `OnStateChanged(HumanoidStateType oldValue, HumanoidStateType newValue)` | invoked when state changes |
| `OnOwnerChanged(HumanoidOwnerType oldValue, HumanoidOwnerType newValue)` | invoked when owner type changes |
| `OnCanApplyFallDamageChanged(bool oldValue, bool newValue)` | invoked when fall damage access changes |
| `OnPlatformStandingChanged(bool oldValue, bool newValue)` | invoked when platform standing changes |

---

## examples

### basic health usage

```csharp
public class DamageTester : MonoBehaviour
{
    [SerializeField] private Humanoid humanoid;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            humanoid.TakeDamage(20f);

        if (Input.GetKeyDown(KeyCode.J))
            humanoid.Heal(10f);

        if (Input.GetKeyDown(KeyCode.K))
            humanoid.Kill();
    }
}
```

---

### listening to health changes

```csharp
public class HumanoidHealthUI : MonoBehaviour
{
    [SerializeField] private Humanoid humanoid;

    private void OnEnable()
    {
        humanoid.OnHealthChanged += HandleHealthChanged;
        humanoid.Died += HandleDied;
    }

    private void OnDisable()
    {
        humanoid.OnHealthChanged -= HandleHealthChanged;
        humanoid.Died -= HandleDied;
    }

    private void HandleHealthChanged(float oldHealth, float newHealth)
    {
        Debug.Log($"health changed: {oldHealth} -> {newHealth}");
    }

    private void HandleDied()
    {
        Debug.Log("humanoid died");
    }
}
```

---

### stamina use

```csharp
public class SkillCaster : MonoBehaviour
{
    [SerializeField] private Humanoid humanoid;

    public void CastSkill()
    {
        if (!humanoid.TryUsingStamina(25f))
            return;

        Debug.Log("skill casted");
    }
}
```

---

### temporary stun status

```csharp
public class StunExample : MonoBehaviour
{
    [SerializeField] private Humanoid humanoid;

    public void ApplyStun()
    {
        humanoid.AddTemporaryStatus("stunned", 2f);
    }

    private void Update()
    {
        if (humanoid.HasTemporaryStatus("stunned"))
        {
            humanoid.StopMovement();
        }
    }
}
```

---

### changing owner

```csharp
public class OwnershipExample : MonoBehaviour
{
    [SerializeField] private Humanoid humanoid;

    public void SetAsPlayer()
    {
        humanoid.ChangeOwner(HumanoidOwnerType.Player);
    }

    public void SetAsAI()
    {
        humanoid.ChangeOwner(HumanoidOwnerType.AI);
    }
}
```

---

### using with humanoidmotor

```csharp
public class SimpleController : MonoBehaviour
{
    [SerializeField] private Humanoid humanoid;
    [SerializeField] private HumanoidMotor motor;

    private void Awake()
    {
        if (humanoid == null)
            humanoid = GetComponent<Humanoid>();

        if (motor == null)
            motor = humanoid.GetMotor();
    }

    private void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        bool running = Input.GetKey(KeyCode.LeftShift);

        motor.Move(input, running);

        if (Input.GetKeyDown(KeyCode.Space))
            motor.Jump();
    }
}
```

---

## internal behavior

### awake

on `Awake`, the component:

- auto-assigns `Rigidbody`
- auto-assigns `Transform`
- sets current health to `maxHealth`
- sets current walk speed to `maxWalkSpeed`
- sets current running speed to `maxRunningSpeed`
- sets current jump power to `maxJumpPower`
- sets current stamina to `maxStamina`
- sets alive state based on health

### update

on `Update`, the component handles:

- health regeneration
- stamina regeneration
- temporary status expiration

### onvalidate

on `OnValidate`, the component clamps max attributes and runtime values to safer ranges.

because apparently users, inspectors, and sleep-deprived devs all love typing impossible numbers into fields.

---

## design notes

`Humanoid` is designed to be the core data layer.

it should know:

- what the character is
- what state the character is in
- how much health and stamina it has
- whether the character is alive
- what statuses are active
- what attributes are available

it should not become:

- a full input controller
- a full ai controller
- a full animation system
- a full combat system
- a physics motor
- a networking authority system
- a cursed god object with 3000 lines and emotional damage

keep this module as the center of truth, then let other modules operate around it.

---

## recommended module responsibility

| module | responsibility |
|---|---|
| `Humanoid` | core character attributes, state, status, events |
| `HumanoidMotor` | physics movement and locomotion |
| `HumanoidAnimator` | animation state and animation parameter sync |
| `HumanoidCombat` | damage, hit, attack, combat handling |
| `HumanoidInputController` | player input mapping |
| `AIHumanoidHandler` | ai decision and motor calls |

---

## authorship

`Humanoid` was designed and implemented manually by the author.  
ai assistance was used only to help draft and structure this documentation.

---

## license

free usage under the mit license.

---

## author

created by raihan.  
for problems or recommendations, contact:

```txt
@raihanaufal_77
```

responses may take time.
