using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem.LowLevel;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using NewTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using NewTouchPhase = UnityEngine.InputSystem.TouchPhase;
using MouseControl = UnityEngine.InputSystem.Controls.ButtonControl;

using InputKey = UnityEngine.InputSystem.Key;
using GyroScope = UnityEngine.InputSystem.Gyroscope;
#elif ENABLE_LEGACY_INPUT_MANAGER
using InputKey = UnityEngine.KeyCode;
#endif

public class HICAxesTable
{
    public Dictionary<string, List<HICInputAxisInfo>> AxisBindsTable;
    public Dictionary<string, HICAxisParams> AxisParamsTable;
    public Dictionary<string, HICAxisState> AxisStatesTable;
    public Dictionary<string, HICInputAxis2Info> Axis2DInfoTable;

    public HICAxesTable()
    {
        AxisBindsTable = new Dictionary<string, List<HICInputAxisInfo>>(32);
        AxisParamsTable = new Dictionary<string, HICAxisParams>(32);
        AxisStatesTable = new Dictionary<string, HICAxisState>(32);
        Axis2DInfoTable = new Dictionary<string, HICInputAxis2Info>(16);
    }
};

public class HICActionsTable
{
    public Dictionary<string, HICInputActionEntry> ActionEntries;

    public HICActionsTable()
    {
        ActionEntries = new Dictionary<string, HICInputActionEntry>(64);
    }  
};

// coming soon
// public class HICInputControlScheme
// {
//     public string SchemeName;

//     public HICAxesTable AxesTable;
//     public HICActionsTable ActionsTable;

//     public HICInputControlScheme(string __schemeName)
//     {
//         SchemeName = __schemeName;
//         AxesTable = new HICAxesTable();
//         ActionsTable = new HICActionsTable();
//     }   
// };

/// <summary>
/// Enumerations of Input stage for HumanoidInputController
/// </summary>
public enum HICInputActionStage
{
    /// <summary>
    /// Input phase where there is no input happened
    /// </summary>
    None,
    /// <summary>
    /// Input phase where happened when input is beginning by clicked or pressed down
    /// </summary>
    Begin,
    /// <summary>
    /// Input phase where happened when input successfully performed and do the task
    /// </summary>
    Performed,
    /// <summary>
    /// Input phase where the input is still staying to be pressed or held
    /// </summary>
    Holding,
    /// <summary>
    /// Input phase where the input is being interrupted by unexpected task
    /// </summary>
    Cancelled,
    /// <summary>
    /// Input phase where happened when the key is released up and input ended
    /// </summary>
    Released
};

/// <summary>
/// Enumerations of Input's action type for HumanoidInputController
/// </summary>
public enum HICInputTriggerType
{
    /// <summary>
    /// Input action type where the key is pressed down
    /// </summary>
    Down,
    /// <summary>
    /// Input action type where the key is getting released or unpressed up
    /// </summary>
    Up,
    /// <summary>
    /// Input action type where the key is still pressing and not released yet
    /// </summary>
    Hold,
    /// <summary>
    /// Input action type where the screen is tapped by finger
    /// </summary>
    Tap,
    /// <summary>
    /// Input action type where the screen is getting tapped two times by finger
    /// </summary>
    DoubleTap,
    /// <summary>
    /// Input action type where the screen is scrolled by finger
    /// </summary>
    Toggle,
    /// <summary>
    /// Input action type where the input is pressing so long and don't leave
    /// </summary>
    LongPressing,
    /// <summary>
    /// Input action type where the input is need to be pressed by other inputs.
    /// </summary>
    Combo,
    /// <summary>
    /// Input action type where the input is for specific reason or task
    /// </summary>
    Special
};

/// <summary>
/// Enumerations of device type of the input for HumanoidInputController
/// </summary>
public enum HICInputDeviceType
{
    /// <summary>
    /// Input device where happened by keyboard
    /// </summary>
    Keyboard,
    /// <summary>
    /// Input device where happened by mouse
    /// </summary>
    Mouse,
    /// <summary>
    /// Inpput device where happened by gamepad
    /// </summary>
    Gamepad,
    /// <summary>
    /// Input device where happened by screen toucing
    /// </summary>
    Touch,
    /// <summary>
    /// Input device where happened by UI/GUI handlers
    /// </summary>
    UI,
    /// <summary>
    /// Input device where happened by VR stick-controllers
    /// </summary>
    VR,
    /// <summary>
    /// There is no conceptual input device
    /// </summary>
    Unknown
};

/// <summary>
/// Enumerations of Input conflict type for HumanoidInputController
/// </summary>
public enum HICInputConflictType
{
    /// <summary>
    /// Input conflict type where two or more keys are pressed together, but none of them do action
    /// </summary>
    Neutralized,
    /// <summary>
    /// Input conflict type where two or more keys are pressed together, but last key that pressed do action
    /// </summary>
    LastPressed,
    PositiveWins,
    NegativeWins,
};

/// <summary>
/// HumanoidInputController's settings for input axis
/// </summary>
public struct HICAxisParams
{
    /// <summary>
    /// The speed for axis returning to 0 after the input unpressed
    /// </summary>
    public float Gravity;
    /// <summary>
    /// The speed for pro-smoothed axis returning to 0 after the input unpressed
    /// </summary>
    public float ProGravity;
    /// <summary>
    /// The size of joysticks center deadzone where input is ignored
    /// </summary>
    public float DeadZone;
    /// <summary>
    /// The speed for how axis become value or -value when input pressed or happened
    /// </summary>
    public float Sensitivity;
    /// <summary>
    /// The speed for how pro-smoothed axis become value or -value when input pressed or happened
    /// </summary>
    public float ProSensitivity;
    /// <summary>
    /// Sets axis to 0 immediately if an opposing button is hit
    /// </summary>
    public bool Snap;
    /// <summary>
    /// Delay/Threshold time before the axis sets to 0
    /// </summary>
    public float SnapThreshold;
    /// <summary>
    /// Inverting axis from the input, like value to -value and -value to value
    /// </summary>
    public bool InvertAxis;
    /// <summary>
    /// Sets how input axis will handle inputs conflict
    /// </summary>
    public HICInputConflictType ConflictType;

    public HICAxisParams(float __gravity, float __proGravity, float __deadZone, float __sensitivity, float __proSensitivity, bool __snapping = true, float __snapThreshold = 0.01f, HICInputConflictType __conflictType = HICInputConflictType.Neutralized, bool __inverted = false)
    {
        Gravity = __gravity;
        ProGravity = __proGravity;
        DeadZone = __deadZone;
        Sensitivity = __sensitivity;
        ProSensitivity = __proSensitivity;
        Snap = __snapping;
        SnapThreshold = __snapThreshold;
        ConflictType = __conflictType;
        InvertAxis = __inverted;
    }

    public static HICAxisParams Default()
    {
        return new HICAxisParams
        {
            Gravity = 18.0f,
            ProGravity = 18.0f,
            DeadZone = 0.04f,
            Sensitivity = 12.0f,
            ProSensitivity = 12.0f,
            Snap = true,
            SnapThreshold = 0.01f,
            InvertAxis = false,
            ConflictType = HICInputConflictType.Neutralized
        };
    }
}

/// <summary>
/// HumanoidInputController's context for Action
/// </summary>
public struct HICInputActionContext
{
    public string ActionName;

    public InputKey Key;
    public int MouseButton;
    public int TouchFingersIndex;
    public int GamepadButtonIndex;

    public HICInputDeviceType DeviceType;
    public HICInputActionStage InputStage;
    public HICInputTriggerType TriggerType;

    public Vector2 ScreenInputPosition;
    public Vector3 WorldInputPosition;

    public double LastTimePressed;
    public float HoldingTime;

    public bool Consumed;

    public HICInputActionContext(string __actionName, HICInputDeviceType __deviceType, HICInputActionStage __phase)
    {
        ActionName = __actionName;
        DeviceType = __deviceType;
        InputStage = __phase;

        Key = InputKey.None;
        MouseButton = -1;
        TouchFingersIndex = -1;
        GamepadButtonIndex = -1;

        TriggerType = HICInputTriggerType.Down;

        ScreenInputPosition = Vector2.zero;
        WorldInputPosition = Vector2.zero;

        LastTimePressed = Time.time;
        HoldingTime = 0f;

        Consumed = false;
    }
};

/// <summary>
/// HumanoidInputController's pointer projection information
/// </summary>
public struct HICPointerToWorldInfo
{
    public Vector3 WorldPosition;
    public Vector2 ScreenPosition;
    public Ray Ray;
    public RaycastHit Hit;
    public bool HasHit;
    public float Distance;
};

/// <summary>
/// axis infos of HumanoidInputController
/// </summary>
public struct HICInputAxisInfo
{
    public string AxisName;
    public InputKey Key;
    public float Value;
    public HICInputDeviceType DeviceType;

    public int GamepadIndex;
    public int GamepadAxisIndex;
    public bool IsAnalog;

    public HICInputAxisInfo(string __axisName, InputKey __key, float __value, HICInputDeviceType __deviceType = HICInputDeviceType.Keyboard)
    {
        AxisName = __axisName;
        Key = __key;
        Value = __value;
        DeviceType = __deviceType;

        GamepadIndex = -1;
        GamepadAxisIndex = -1;
        IsAnalog = false;
    }
};

/// <summary>
/// Struct of axis state for HumanoidInputController
/// </summary>
public struct HICAxisState
{
    public float RawValue;
    public float SmoothValue;
    public float ProValue;
    public float TargetValue;

    public float LastRawValue;
    public float LastSmoothValue;
    public float LastProValue;

    public InputKey LastPressedKey;
    public float LastChangedTime;
    public bool ChangedThisFrame;

    public void Reset() {
        RawValue = 0; SmoothValue = 0; ProValue = 0; TargetValue = 0;
        LastRawValue = 0; LastSmoothValue = 0; LastProValue = 0;
        LastPressedKey = InputKey.None; LastChangedTime = 0; ChangedThisFrame = false;
    }  
};

/// <summary>
/// HumanoidInputController's context or info for BindAction
/// </summary>
public struct HICInputActionParams
{
    public HICInputDeviceType DeviceType;
    public HICInputTriggerType TriggerType;

    public InputKey Key;

    public int GamepadButtonIndex;
    public int MouseButton;
    public int TouchFingersIndex;

    public float HoldingTime;
    public float TapMaxTime;
    public float DoubleTapMaxDelay;

    public InputKey KeyCombo;
    public InputKey[] SequenceKeys;

    public static HICInputActionParams Keyboard(InputKey __key, HICInputTriggerType __triggerType = HICInputTriggerType.Down, float __holdTime = 1.0f)
    {
        return new HICInputActionParams
        {
            DeviceType = HICInputDeviceType.Keyboard,
            TriggerType = __triggerType,

            Key = __key,

            GamepadButtonIndex = -1,
            MouseButton = -1,
            TouchFingersIndex = -1,

            HoldingTime = __holdTime,
            TapMaxTime = 0.25f,
            DoubleTapMaxDelay = 0.3f,

            KeyCombo = InputKey.None,
            SequenceKeys = null
        };
    }

    public static HICInputActionParams Mouse(int __mouseButton, HICInputTriggerType __triggerType = HICInputTriggerType.Down, float __holdTime = 1.0f)
    {
        return new HICInputActionParams
        {
            DeviceType = HICInputDeviceType.Mouse,
            TriggerType = __triggerType,

            Key = InputKey.None,

            GamepadButtonIndex = -1,
            MouseButton = __mouseButton,
            TouchFingersIndex = -1,

            HoldingTime = __holdTime,
            TapMaxTime = 0.25f,
            DoubleTapMaxDelay = 0.3f,

            KeyCombo = InputKey.None,
            SequenceKeys = null
        };
    }

    public static HICInputActionParams Touch(int __fingerIndex, HICInputTriggerType __triggerType = HICInputTriggerType.Down, float __holdTime = 1.0f)
    {
        return new HICInputActionParams
        {
            DeviceType = HICInputDeviceType.Touch,
            TriggerType = __triggerType,

            Key = InputKey.None,

            GamepadButtonIndex = -1,
            MouseButton = -1,
            TouchFingersIndex = __fingerIndex,

            HoldingTime = __holdTime,
            TapMaxTime = 0.25f,
            DoubleTapMaxDelay = 0.3f,

            KeyCombo = InputKey.None,
            SequenceKeys = null
        };
    }

    public static HICInputActionParams Gamepad(int __gamepadButtonIndex, HICInputTriggerType __triggerType = HICInputTriggerType.Down, float __holdTime = 1.0f)
    {
        return new HICInputActionParams
        {
            DeviceType = HICInputDeviceType.Gamepad,
            TriggerType = __triggerType,

            Key = InputKey.None,

            GamepadButtonIndex = __gamepadButtonIndex,
            MouseButton = -1,
            TouchFingersIndex = -1,

            HoldingTime = __holdTime,
            TapMaxTime = 1.0f,
            DoubleTapMaxDelay = 0.3f,

            KeyCombo = InputKey.None,
            SequenceKeys = null   
        };
    }
};

public struct HICInputActionState
{
    public bool IsPressedDown;
    public bool IsHolding;
    public bool IsReleasedUp;

    public bool PerformedThisFrame;
    public bool ReleasedThisFrame;
    public bool Consumed;

    public bool Toggled;
    public bool Buffered;

    public float BeginTime;
    public float ReleasedTime;
    public float LastPerformedTime;
    public float LastTapTime;
    public float HoldingDuration;

    public int TapCounts;
    public int SequenceIndex;

    public HICInputActionStage Stage;
    public HICInputDeviceType LastDeviceType;
    public InputKey LastKey;

    public void ResetFrameState()
    {
        IsPressedDown = false;
        IsReleasedUp = false;

        PerformedThisFrame = false;
        ReleasedThisFrame = false;

        Stage = HICInputActionStage.None;
    }

    public void Reset()
    {
        IsPressedDown = false;
        IsHolding = false;
        IsReleasedUp = false;

        PerformedThisFrame = false;
        ReleasedThisFrame = false;
        Consumed = false;

        Toggled = false;
        Buffered = false;
        
        BeginTime = 0.0f;
        ReleasedTime = 0.0f;
        LastPerformedTime = -999f;
        LastTapTime = -999f;
        HoldingDuration = 0.0f;
        
        Stage = HICInputActionStage.None;
        LastDeviceType = HICInputDeviceType.Unknown;
        LastKey = InputKey.None;
    }
};

public struct HICInputActionEntry
{
    public string ActionName;
    public float BufferTime;
    public bool ConsumeOnPerformed;
    public bool Enabled;

    public List<HICInputActionParams> Bindings;
    public Action<HICInputActionContext> Callbacks;

    public Button UIButton;
    public HICInputActionState State;

    public HICInputActionEntry(string __actionName)
    {
        ActionName = __actionName;
        Bindings = new List<HICInputActionParams>(4);
        Callbacks = null;
        UIButton = null;

        State = new HICInputActionState();
        State.Reset();

        BufferTime = 0.15f;
        ConsumeOnPerformed = false;
        Enabled = true;
    }
};

public struct HICInputAxis2Info
{
    public string AxisName;
    public string XAxisName;
    public string YAxisName;

    public HICInputAxis2Info(string __axisName, string __x_axisName, string __y_axisName)
    {
        AxisName = __axisName;
        XAxisName = __x_axisName;
        YAxisName = __y_axisName;
    }
};

public struct HICFingersIdentity
{
    public int Index;
    public Vector2 Position;  

    public HICFingersIdentity(int __index, Vector2 __position)
    {
        Index = __index;
        Position = __position;
    }
};

[Flags]
public enum HICGyroAxis
{
    X = 0,
    Y = 1,
    Z = 1 << 1,
    W = 1 << 2
};

public enum HICGyroType
{
    /// <summary>
    /// When the rotation absolutely relaying by how the rotation of device (as Attitude rotation)
    /// </summary>
    Absolute,
    /// <summary>
    /// When the rotation is relative by the speed of device's rotation (as AngularVelocity rotation)
    /// </summary>
    Relative,
    /// <summary>
    /// When the rotation combined with Absolute and Relative
    /// </summary>
    Hybird  
};

public class HICGyroService
{
    public Camera CurrentCamera;
    public ushort CurrentMinusAxis;
    public HICGyroType CurrentType;

    public Quaternion StartAttitude;
    public Vector3 StartAngularVelocity;

    public bool Started => __started;

    public float GyroSensitivity;
    public float GyroDeadZone;
    public float GyroSmoothScale;
    public float GyroMinRotationAngle;
    public float GyroMaxRotationAngle;
    public float GyroDriftCorrection;
    public float GyroAxisMultiplier;
    public float GyroCalibration;
    public float GyroAccelerationCurve;
    public float GyroMaxAngularSpeed;

    private bool __started = false;

    public HICGyroService(HICGyroType __gyroType = HICGyroType.Relative, ushort __minusAxis = (ushort) HICGyroAxis.Z | (ushort) HICGyroAxis.W)
    {
        CurrentType = __gyroType;
        CurrentMinusAxis = __minusAxis;
    }

    public void Start()
    {
        __started = true;

        #if ENABLE_INPUT_SYSTEM
        StartAttitude = AttitudeSensor.current.attitude.ReadValue();
        StartAngularVelocity = GyroScope.current.angularVelocity.ReadValue();
        #elif ENABLE_LEGACY_INPUT_MANAGER
        StartAttitude = Input.gyro.attitude;
        StartAngularVelocity = Input.gyro.rotationRate;
        #endif
    }

    public void Stop()
    {
        __started = false;

        StartAttitude = Quaternion.identity;
        StartAngularVelocity = Vector3.zero;
    }
};

public class HumanoidInputController : MonoBehaviour
{
    #region SerializedPreferences

    [Header("Projection")]
    [SerializeField] private Camera currentCamera;
    [SerializeField] private LayerMask projectionLayer;
    [SerializeField] private float projectionMaxDistance = 100.0f;

    [Header("Touch")]
    [SerializeField] private float touchHoldingDurations = 2.0f;
    [SerializeField] private float touchDoubleTapMaxDelay = 0.35f;
    [SerializeField] private float touchSwipeMinDistance = 50.0f;
    [SerializeField] private float touchSwipeDuration = 0.75f;
    [SerializeField] private int touchMaximalOnScreen = 10;

    [Header("Gamepad")]
    [SerializeField] private float joystickRotationThreshold = 360.0f;
    [SerializeField] private string leftJoystickAxisName = "JoystickLeft";
    [SerializeField] private string rightJoystickAxisName = "JoystickRight";

    [Header("Gyroscope")]
    [SerializeField] private bool enableGyroscope = false;
    [SerializeField] private float gyroSensitivity = 5.0f;
    [SerializeField] private float gyroDeadZone = 0.01f;
    [SerializeField] private float gyroSmoothScale = 5.0f;
    [SerializeField] private float gyroMinRotation = -50.0f;
    [SerializeField] private float gyroMaxRotation = 50.0f;
    [SerializeField] private float gyroDriftCorrection = 0.1f;
    [SerializeField] private float gyroAxisMultiplier = 0.2f;
    [SerializeField] private float gyroCalibration = 0.3f;
    [SerializeField] private float gyroAccelerationCurve = 8.0f;
    [SerializeField] private float gyroMaxAngularSpeed = 20.0f;

    [Header("Details")]
    [SerializeField] private float EPSILON = 0.0001f;

    #endregion

    #region PrivateMembers

    #region Refs

    private float ___proAxis_smooth_references;

    #endregion

    #region Tables
    /* Tables*/
    private HICAxesTable _axesTable;
    private HICActionsTable _actionsTable;

    // /* Private helpers*/
    // private bool _isUsingExternalAxesTable;
    // private bool _isUsingExternalActionsTable;
    #endregion

    // #region InputScheme

    // private Dictionary<string, HICInputControlScheme> _schemes;
    // private string _currentSchemeName;

    // #endregion

    #region InputLocks

    private HashSet<string> _inputLocks;
    private Stack<string> _inputContexts;

    #endregion

    #region DeviceStates

    private HICInputDeviceType _usedDevice;
    private HICInputDeviceType _previousUsedDevice;

    #endregion

    #region AxisCache

    private Dictionary<string, Vector2> _lastVector2Axis2D;

    #endregion

    #region ActionCache

    private Dictionary<Button, UnityAction> _buttonCallbacks;

    #endregion

    #region MouseCache

    private HICPointerToWorldInfo _projectionInfoCache;

    private Vector2 _mousePos_screen;
    private Vector2 _previous_mousePos_screen;
    private Vector2 _mouseDelta;
    private float _scroll_mouseDelta;

    private bool[] _mouseDown;
    private bool[] _mouseHolding;
    private bool[] _mouseUp;

    #endregion

    #region TouchCache

    private bool _touchActive;

    private Vector2 _touchBeginPos;
    private Vector2 _touchCurrentPos;
    private Vector2 _touchPreviousPos;
    private Vector2 _touchDelta;

    private float _touchBeginTime;
    private float _previousPitchDistance;
    private float _previousTouchAngle;

    private int _touchFingerCounts;
    private int _touchBeginFrame = -1;
    private int _touchEndedFrame = -1;

    private bool _touchLongPressFired;
    private bool _touchDoubleTapCandidate;
    private float _lastTouchTapTime;

    private List<HICFingersIdentity> _fingersIdentity;

    #endregion

    #region GamepadCache

    private Vector2 _leftStick;
    private Vector2 _rightStick;
    private Vector2 _previousLeftStick;
    private Vector2 _previousRightStick;

    private bool _leftStickPressed;
    private bool _rightStickPressed;
    private bool _previousLeftStickPressed;
    private bool _previousRightStickPressed;

    private float _leftAccumulatedRotationAngle;
    private float _rightAccumulatedRotationAngle;

    private float _previousLeftAngle;
    private float _previousRightAngle;

    #endregion

    #region ButtonsCache

    private List<InputKey> _watchedButtons;

    private Dictionary<InputKey, bool> _buttonsDown;
    private Dictionary<InputKey, bool> _buttonsHolding;
    private Dictionary<InputKey, bool> _buttonsUp;
    private Dictionary<InputKey, float> _buttonsPressedTime;

    #endregion

    #region AxisCaches

    private Dictionary<string, HICAxisState> _axisTrackDumps;
    private Dictionary<string, List<HICInputAxisInfo>> _axisBindTrackDumps;

    #endregion

    #endregion

    #region Events

    #region DebugHelpers

    public event Action<HICInputActionContext> OnActionBegin;
    public event Action<HICInputActionContext> OnActionPerformed;
    public event Action<HICInputActionContext> OnActionHolding;
    public event Action<HICInputActionContext> OnActionReleased;
    public event Action<HICInputActionContext> OnActionCancelled;

    public event Action<HICInputDeviceType, HICInputDeviceType> OnDeviceTypeChanged;

    public event Action<string> OnInputLocked;
    public event Action<string> OnInputUnlocked;

    #endregion

    #region Axis

    public event Action<string, float, float> OnAxisChanged;
    public event Action<string, Vector2, Vector2> OnAxis2DChanged;

    #endregion

    #region Keys

    public event Action<InputKey> OnInputKeyDown;
    public event Action<InputKey> OnInputKeyStaying;
    public event Action<InputKey> OnInputKeyUp;

    #endregion

    #region Mouse

    public event Action<int, Vector2> OnMouseDown;
    public event Action<int, Vector2> OnMouseHolding;
    public event Action<int, Vector2> OnMouseUp;
    public event Action<float> OnMouseScrolling;

    #endregion

    #region Touch

    public event Action OnTouchInterrupted;

    public event Action<Vector2> OnTouchHappened;

    public event Action<Vector2, float> OnTouchHolding;
    public event Action<int, HICFingersIdentity[]> OnTouchPan;

    public event Action<Vector2> OnTouchReleased;
    public event Action<Vector2> OnTouchStationary;

    public event Action<Vector2, Vector2, Vector2> OnTouchSwipe;

    public event Action<Vector2> OnTouchDoubleTapping;
    public event Action<Vector2, float> OnTouchLongHolding;

    public event Action<float, float> OnTouchPinch;
    public event Action<float> OnTouchRotate;

    #endregion

    #region Gamepad

    public event Action<int> OnThumbstickPressDown;
    public event Action<int> OnThumbstickPressHolding;
    public event Action<int> OnThumbstickPressUp;

    public event Action OnLeftThumbstickRotating;
    public event Action OnRightThumbstickRotating;

    public event Action<Vector2> OnGamepadLeftStickChanged;
    public event Action<Vector2> OnGamepadRightStickChanged;
    public event Action<int> OnGamepadConnected;
    public event Action<int> OnGamepadDisconnected;

    #endregion

    #region Gyroscope

    private HICGyroService _gyroService;

    private Quaternion _calibrationQuat = Quaternion.identity;
    private Quaternion _lastRotatedQuat = Quaternion.identity;

    private Vector3 _calibrationVector3;
    private Vector3 _lastRotatedVector3;

    #endregion

    #endregion

    #region Methods

    #region TableSetters

    public void SetHICAxesTable(HICAxesTable __table)
    {
        if (__table == null) return;

        _axesTable = __table;
        // _isUsingExternalAxesTable = true;
    }

    public void SetHICActionsTable(HICActionsTable __table)
    {
        if (__table == null) return;

        _actionsTable = __table;
        // _isUsingExternalActionsTable = true;

        RefreshWatchedKeys();
    }

    #endregion

    #region AxisBinds

    public void BindAxis(string __axisName, InputKey __key, float __value)
    {
        if (string.IsNullOrEmpty(__axisName)) return;

        HICInputAxisInfo __binding = new HICInputAxisInfo(
            __axisName,
            __key,
            __value
        );

        BindAxis(__binding);
    }

    public void BindAxis(HICInputAxisInfo __binding)
    {
        if (string.IsNullOrEmpty(__binding.AxisName)) return;
        if (!float.IsFinite(__binding.Value)) return;

        if (!_axesTable.AxisBindsTable.TryGetValue(__binding.AxisName, out var __interfaces))
        {
            __interfaces = new List<HICInputAxisInfo>(4);
            _axesTable.AxisBindsTable.Add(__binding.AxisName, __interfaces);
        }

        for (int i = 0; i < __interfaces.Count; i++)
        {
            if (__interfaces[i].Key == __binding.Key && __interfaces[i].DeviceType == __binding.DeviceType)
            {
                __interfaces[i] = __binding;

                AddWatchedKey(__binding.Key);
                DoubleCheckAxisState(__binding.AxisName);
                return;
            }
        }

        __interfaces.Add(__binding);
        DoubleCheckAxisParams(__binding.AxisName);
        DoubleCheckAxisState(__binding.AxisName);

        AddWatchedKey(__binding.Key);
    }

    public void UnbindAxis(string __axisName)
    {
        if (string.IsNullOrEmpty(__axisName)) return;

        _axesTable.AxisBindsTable.Remove(__axisName);
        _axesTable.AxisParamsTable.Remove(__axisName);
        _axesTable.AxisStatesTable.Remove(__axisName);
    }

    public void UnbindAxis(string __axisName, InputKey __key)
    {
        if (!_axesTable.AxisBindsTable.TryGetValue(__axisName, out var list))
            return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            HICInputAxisInfo __inputAxis = list[i];

            if (__inputAxis.Key == __key)
                list.RemoveAt(i);
        }

        if (list.Count <= 0)
            UnbindAxis(__axisName);
    }

    #endregion

    #region AxisSetters

    public void SetAxisParams(string __axisName, HICAxisParams __params)
    {
        if (string.IsNullOrEmpty(__axisName)) return;

        _axesTable.AxisParamsTable[__axisName] = __params;
        DoubleCheckAxisState(__axisName);
    }

    public HICAxisParams GetAxisParams(string __axisName)
    {
        DoubleCheckAxisParams(__axisName);
        return _axesTable.AxisParamsTable[__axisName];
    }

    #endregion

    #region AxisGetters

    public float GetAxis(string __axisName, bool __raw = false)
    {
        if (string.IsNullOrEmpty(__axisName)) return 0f;

        if (_axesTable.AxisStatesTable.TryGetValue(__axisName, out var __state))
        {
            float __axis = __raw ? __state.RawValue : __state.SmoothValue;

            return __axis;
        }

        return 0f;
    }

    public float GetAxisRaw(string __axisName)
    {
        if (string.IsNullOrEmpty(__axisName)) return 0f;

        if (_axesTable.AxisStatesTable.TryGetValue(__axisName, out var __state))
            return __state.RawValue;

        return 0f;
    }

    #endregion

    #region Axis2DBinds

    public void BindAxis2D(string __vectorName, string __x_axisName, string __y_axisName)
    {
        if (string.IsNullOrEmpty(__vectorName) || string.IsNullOrEmpty(__x_axisName) || string.IsNullOrEmpty(__y_axisName)) return;

        _axesTable.Axis2DInfoTable[__vectorName] = new HICInputAxis2Info(
            __vectorName,
            __x_axisName,
            __y_axisName
        );
    }

    public void UnbindAxis2D(string __vectorName)
    {
        if (string.IsNullOrEmpty(__vectorName)) return;

        if (!_axesTable.Axis2DInfoTable.ContainsKey(__vectorName))
            return;

        _axesTable.Axis2DInfoTable.Remove(__vectorName);
    }

    #endregion

    #region Axis2DGetters

    public Vector2 GetAxis2D(string __vectorName, bool __raw = false)
    {
        if (string.IsNullOrEmpty(__vectorName)) return Vector2.zero;

        if (_axesTable.Axis2DInfoTable.TryGetValue(__vectorName, out var __value))
            return new Vector2(GetAxis(__value.XAxisName, __raw), GetAxis(__value.YAxisName, __raw));
        
        return Vector2.zero;
    }

    public Vector2 GetAxis2DRaw(string __vectorName)
    {
        if (string.IsNullOrEmpty(__vectorName)) return Vector2.zero;

        if (_axesTable.Axis2DInfoTable.TryGetValue(__vectorName, out var __value))
            return new Vector2(GetAxisRaw(__value.XAxisName), GetAxisRaw(__value.YAxisName));

        return Vector2.zero;
    }

    #endregion

    #region DeviceGetters

    public HICInputDeviceType GetDeviceType()
    {
        return _usedDevice;
    }

    #endregion

    #region ProjectionGetters

    public Vector2 GetMousePositionInScreen()
    {
        return _mousePos_screen;
    }

    public Vector2 GetMouseDelta()
    {
        return _mouseDelta;
    }

    public float GetMouseScrollDelta()
    {
        return _scroll_mouseDelta;
    }

    public Ray GetMouseRay()
    {
        if (currentCamera == null)
            return new Ray(Vector3.zero, Vector3.forward);

        return currentCamera.ScreenPointToRay(_mousePos_screen);
    }

    public bool GetMouseHitInWorld(out RaycastHit __hit)
    {
        Ray __ray = GetMouseRay();

        return Physics.Raycast(
            __ray,
            out __hit,
            projectionMaxDistance,
            projectionLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    public HICPointerToWorldInfo GetPointerToWorldInfo()
    {
        return _projectionInfoCache;
    }

    public Vector3 GetMousePositionInWorld()
    {
        return GetPointerToWorldInfo().WorldPosition;
    }

    public Vector3 GetMouseInWorldPoint(float __distanceFromCamera)
    {
        if (currentCamera == null)
            return Vector3.zero;

        Ray __ray = GetMouseRay();
        return __ray.GetPoint(__distanceFromCamera);
    }

    public Vector3 GetMouseWorldPointOnPlane(Plane __plane)
    {
        Ray __ray = GetMouseRay();

        if (__plane.Raycast(__ray, out float __dist))
            return __ray.GetPoint(__dist);

        return Vector3.zero;
    }

    #endregion

    #region Touches

    public bool IsTouchBegin()
    {
        return _touchActive && Time.frameCount == GetBeginTouchFrame();
    }

    public bool IsTouchHolding()
    {
        return _touchActive;
    }

    public bool IsTouchReleased()
    {
        return !_touchActive && Time.frameCount == GetEndedTouchFrame();
    }
    
    public int GetBeginTouchFrame()
    {
        return _touchBeginFrame;
    }

    public int GetEndedTouchFrame()
    {
        return _touchEndedFrame;
    }

    public Vector2 GetTouchDelta()
    {
        return _touchDelta;
    }

    #endregion

    #region Actions

    public void BindAction(string __actionName, Action<HICInputActionContext> __callback, Button __uiButton, params HICInputActionParams[] __params)
    {
        if (string.IsNullOrEmpty(__actionName)) return;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
        {
            __entry = new HICInputActionEntry(__actionName);
            _actionsTable.ActionEntries.Add(__actionName, __entry);
        }

        __entry.Callbacks = __callback;
        __entry.UIButton = __uiButton;

        __entry.Bindings.Clear();

        if (__params != null)
        {
            foreach (var __param in __params)
            {
                __entry.Bindings.Add(__param);
                AddWatchedActionKey(__param);
            }
        }

        BindUIButtonCallback(__entry);
    }

    public bool RebindAction(string __actionName, HICInputActionParams __oldParam, HICInputActionParams __newParam)
    {
        if (string.IsNullOrEmpty(__actionName)) return false;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return false;

        for (int i = 0; i < __entry.Bindings.Count; i++)
        {
            if (IsSameActionParam(__entry.Bindings[i], __oldParam))
            {
                __entry.Bindings[i] = __newParam;
                AddWatchedActionKey(__newParam);
                return true;
            }
        }

        return false;
    }

    public static bool IsSameActionParam(HICInputActionParams __currParam, HICInputActionParams __comparerParam)
    {
        return (
            __currParam.Key == __comparerParam.Key &&
            __currParam.KeyCombo == __comparerParam.KeyCombo &&
            __currParam.TapMaxTime == __comparerParam.TapMaxTime &&
            __currParam.TouchFingersIndex == __comparerParam.TouchFingersIndex &&
            __currParam.GamepadButtonIndex == __comparerParam.GamepadButtonIndex &&
            __currParam.MouseButton == __comparerParam.MouseButton &&
            __currParam.TriggerType == __comparerParam.TriggerType &&
            __currParam.DeviceType == __comparerParam.DeviceType &&
            __currParam.DoubleTapMaxDelay == __comparerParam.DoubleTapMaxDelay &&
            __currParam.HoldingTime == __comparerParam.HoldingTime &&
            __currParam.TapMaxTime == __comparerParam.TapMaxTime &&
            __currParam.SequenceKeys.SequenceEqual(__comparerParam.SequenceKeys)
        );
    }

    public void UnbindAction(string __actionName)
    {
        if (string.IsNullOrEmpty(__actionName)) return;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return;

        UnbindUIButtonCallback(__entry);

        __entry.Bindings.Clear();
        _actionsTable.ActionEntries.Remove(__actionName);

        RefreshWatchedKeys();
    }

    public void ClearActionParams(string __actionName, bool __unbindButton = false)
    {
        if (string.IsNullOrEmpty(__actionName)) return;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return;

        if (__unbindButton)
            UnbindUIButtonCallback(__entry);

        __entry.Bindings.Clear();
        RefreshWatchedKeys();
    }

    public bool GetActionDown(string __actionName)
    {
        if (string.IsNullOrEmpty(__actionName)) return false;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return false;

        return __entry.State.PerformedThisFrame && !__entry.State.Consumed;
    }

    public bool GetAction(string __actionName)
    {
        if (string.IsNullOrEmpty(__actionName)) return false;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return false;

        return __entry.State.IsHolding && !__entry.State.Consumed;
    }

    public bool GetActionUp(string __actionName)
    {
        if (string.IsNullOrEmpty(__actionName)) return false;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return false;

        return __entry.State.ReleasedThisFrame;
    }

    public float GetActionHoldingDuration(string __actionName)
    {
        if (string.IsNullOrEmpty(__actionName)) return 0f;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return 0f;

        return __entry.State.HoldingDuration; 
    }  

    #endregion

    #region Locker

    public void LockInput(string __reason = "DeathCoroutine")
    {
        if (string.IsNullOrEmpty(__reason)) return;

        if (_inputLocks.Add(__reason))
            OnInputLocked?.Invoke(__reason);
    }

    public void UnlockInput(string __reason = "RevivedCoroutine")
    {
        if (string.IsNullOrEmpty(__reason)) return;

        if (_inputLocks.Remove(__reason))
            OnInputUnlocked?.Invoke(__reason);
    }

    public void ClearLocks()
    {
        _inputLocks.Clear();
    }

    public bool IsInputLocked()
    {
        return _inputLocks.Count > 0;
    }
    
    public bool IsInputLockedWithReason(string __reason)
    {
        if (string.IsNullOrEmpty(__reason)) return false;

        return _inputLocks.Contains(__reason);
    }

    #endregion

    #region Buffers

    public void SetActionBuffer(string __actionName, float __bufferTime)
    {
        if (string.IsNullOrEmpty(__actionName)) return;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return;

        __entry.BufferTime = Mathf.Max(0f, __bufferTime);
    }

    public bool WasActionBuffered(string __actionName)
    {
        if (string.IsNullOrEmpty(__actionName)) return false;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return false;

        if (!__entry.State.Buffered)
            return false;

        float __elapsed = Time.time - __entry.State.LastPerformedTime;
        return __elapsed <= __entry.BufferTime;
    }

    public void ConsumeActionBuffer(string __actionName)
    {
        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return;

        __entry.State.Buffered = false;
    }

    #endregion

    #region ActionConsumes

    public void ConsumeAction(string __actionName)
    {
        if (string.IsNullOrEmpty(__actionName)) return;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return;

        __entry.State.Consumed = true;
    }

    public void UnConsumeAction(string __actionName)
    {
        if (string.IsNullOrEmpty(__actionName)) return;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return;

        __entry.State.Consumed = false;
    }

    public bool IsActionConsumed(string __actionName)
    {
        if (string.IsNullOrEmpty(__actionName)) return false;

        if (!_actionsTable.ActionEntries.TryGetValue(__actionName, out var __entry))
            return false;

        return __entry.State.Consumed;
    }

    public void ClearAllConsumedActions()
    {
        foreach (var __pair in _actionsTable.ActionEntries)
        {
            HICInputActionState __state = __pair.Value.State;
            __state.Consumed = false;
        }
    }

    #endregion

    #region InputContexts

    public void PushInputContext(string __inputContext)
    {
        if (string.IsNullOrEmpty(__inputContext)) return;

        _inputContexts.Push(__inputContext);
    }

    public void PopInputContext(string __inputContext)
    {
        if (string.IsNullOrEmpty(__inputContext)) return;

        if (_inputContexts.Count <= 0) return;

        _inputContexts.Pop();
    }

    public bool IsThisCurrentInputContext(string __inputContext)
    {
        if (string.IsNullOrEmpty(__inputContext)) return false;

        if (_inputContexts.Count <= 0) return false;

        return _inputContexts.Peek() == __inputContext;
    }

    public string GetCurrentInputContext()
    {
        if (_inputContexts.Count <= 0) return string.Empty;

        return _inputContexts.Peek();
    }

    #endregion

    #region Gyro

    public HICGyroService GyroscopeService(HICGyroType __gyroType = HICGyroType.Relative, ushort __minusAxis = (ushort) HICGyroAxis.Z | (ushort) HICGyroAxis.W)
    {
        #if ENABLE_INPUT_SYSTEM
        if (GyroScope.current == null || AttitudeSensor.current == null) return null;
        #elif ENABLE_LEGACY_INPUT_MANAGER
        if (!SystemInfo.supportsGyroscope) return null;
        #endif

        _gyroService = new HICGyroService(__gyroType, __minusAxis)
        {
            CurrentCamera = currentCamera,
            GyroSensitivity = gyroSensitivity,
            GyroDeadZone = gyroDeadZone,
            GyroMinRotationAngle = gyroMinRotation,
            GyroMaxRotationAngle = gyroMaxRotation,
            GyroDriftCorrection = gyroDriftCorrection,
            GyroAxisMultiplier = gyroAxisMultiplier,
            GyroCalibration = gyroCalibration,
            GyroAccelerationCurve = gyroAccelerationCurve,
            GyroMaxAngularSpeed = gyroMaxAngularSpeed,
        };

        return _gyroService;
    }

    #endregion

    #endregion

    #region Helpers

    #region KeysCache

    private void UnbindUIButtonCallback(HICInputActionEntry __entry)
    {
        if (__entry.UIButton == null) return;

        if (!_buttonCallbacks.TryGetValue(__entry.UIButton, out var __action))
            return;

        __entry.UIButton.onClick.RemoveListener(__action);
        _buttonCallbacks.Remove(__entry.UIButton);
    }

    private void BindUIButtonCallback(HICInputActionEntry __entry)
    {
        if (__entry.UIButton == null) return;

        UnbindUIButtonCallback(__entry);

        UnityAction __action = () =>
        {
            if (IsInputLocked())
                return;

            HICInputActionParams __params = new HICInputActionParams
            {
                DeviceType = HICInputDeviceType.UI,
                TriggerType = HICInputTriggerType.Down,

                Key = InputKey.None,

                GamepadButtonIndex = -1,
                MouseButton = -1,
                TouchFingersIndex = -1,

                HoldingTime = 0f,
            };


        };

        __entry.UIButton.onClick.AddListener(__action);
        _buttonCallbacks.Add(__entry.UIButton, __action);
    }

    private void AddWatchedActionKey(HICInputActionParams __param)
    {
        AddWatchedKey(__param.Key);
        AddWatchedKey(__param.KeyCombo);

        if (__param.SequenceKeys != null)
        {
            for (int i = 0; i < __param.SequenceKeys.Length; i++)
                AddWatchedKey(__param.SequenceKeys[i]); 
        }
    }

    private void AddWatchedKey(InputKey __key)
    {
        if (__key == InputKey.None)
            return;

        if (_buttonsHolding.ContainsKey(__key))
            return;

        _watchedButtons.Add(__key);

        _buttonsDown[__key] = false;
        _buttonsHolding[__key] = false;
        _buttonsUp[__key] = false;
        _buttonsPressedTime[__key] = 0f;
    }

    private void RefreshWatchedKeys()
    {
        _buttonsDown.Clear();
        _buttonsHolding.Clear();
        _buttonsUp.Clear();
        _buttonsPressedTime.Clear();

        _watchedButtons.Clear();

        foreach (var __axis in _axesTable.AxisBindsTable)
        {
            List<HICInputAxisInfo> __info = __axis.Value;

            for (int i = 0; i < __info.Count; i++)
                AddWatchedKey(__info[i].Key);
        }

        foreach (var __act in _actionsTable.ActionEntries)
        {
            HICInputActionEntry __entry = __act.Value;

            for (int i = 0; i < __entry.Bindings.Count; i++)
            {
                HICInputActionParams __param = __entry.Bindings[i];

                AddWatchedKey(__param.Key);
                AddWatchedKey(__param.KeyCombo);

                if (__param.SequenceKeys != null)
                {
                    for (int j = 0; j < __param.SequenceKeys.Length; j++)
                        AddWatchedKey(__param.SequenceKeys[j]);
                }
            }
        }
    }

    #endregion

    #region DoubleCheckHandlers

    private void DoubleCheckAxisParams(string __axisName)
    {
        if (string.IsNullOrEmpty(__axisName)) return;

        if (!_axesTable.AxisParamsTable.ContainsKey(__axisName))
            _axesTable.AxisParamsTable.Add(__axisName, HICAxisParams.Default());
    }

    private void DoubleCheckAxisState(string __axisName)
    {
        if (string.IsNullOrEmpty(__axisName)) return;

        if (!_axesTable.AxisStatesTable.ContainsKey(__axisName))
        {
            HICAxisState __state = new HICAxisState();

            __state.Reset();
            _axesTable.AxisStatesTable.Add(__axisName, __state);
        }
    }

    #endregion

    #region FrameHandlers
    private void FrameBegin()
    {
        for (int i = 0; i < 3; i++)
        {
            _mouseDown[i] = false;
            _mouseUp[i] = false;
        }

        foreach (var __action in _actionsTable.ActionEntries)
        {
            HICInputActionEntry __entry = __action.Value;
            __entry.State.ResetFrameState();
        }

        foreach (var __pair in _axesTable.AxisStatesTable)
            _axisTrackDumps.Add(__pair.Key, __pair.Value);

        foreach (var __trackPair in _axisTrackDumps)
        {
            HICAxisState __state = __trackPair.Value;
            __state.ChangedThisFrame = false;
            _axesTable.AxisStatesTable[__trackPair.Key] = __state;
        }

        _axisTrackDumps.Clear();
    }

    private void FrameEnd()
    {
        if (_usedDevice != _previousUsedDevice)
        {
            OnDeviceTypeChanged?.Invoke(_previousUsedDevice, _usedDevice);
            _previousUsedDevice = _usedDevice;
        }
    }

    #region Axes

    private void HandleAxes()
    {
        foreach (var __axis in _axesTable.AxisBindsTable)
        {
            string __axisName = __axis.Key;
            List<HICInputAxisInfo> __binds = __axis.Value;

            DoubleCheckAxisParams(__axisName);
            DoubleCheckAxisState(__axisName);

            HICAxisParams __param = _axesTable.AxisParamsTable[__axisName];
            HICAxisState __state = _axesTable.AxisStatesTable[__axisName];

            float __oldSmooth = __state.SmoothValue;
            float __oldPro = __state.ProValue;
            float __oldRaw = __state.RawValue;

            float __target__ = CalculateAxisTarget(__binds, __param, ref __state);

            if (__param.InvertAxis)
                __target__ = -__target__;

            __target__ = DeadZoneResult(__target__, __param.DeadZone);

            __state.TargetValue = __target__;
            __state.RawValue = __target__;

            float __snap_th__ = __param.Snap ? __param.SnapThreshold : 0f;

            __state.SmoothValue = AxisValueResult(__state.SmoothValue, __target__, __param.Sensitivity, __param.Gravity);
            __state.ProValue = ProAxisValueResult(__state.ProValue, __target__, __param.ProSensitivity, __param.ProGravity, __snap_th__);

            // Debug.Log($"Axis Name: {__axisName} RawValue: {__state.RawValue} Value: {__state.SmoothValue} ProValue: {__state.ProValue}");

            __state.ChangedThisFrame =
            (
                !FuzzyEq(__oldSmooth, __state.SmoothValue) ||
                !FuzzyEq(__oldPro, __state.ProValue) ||
                !FuzzyEq(__oldRaw, __state.RawValue)
            );

            if (__state.ChangedThisFrame)
            {
                __state.LastChangedTime = Time.time;
                OnAxisChanged?.Invoke(__axisName, __oldSmooth, __state.SmoothValue);
            }

            // update
            _axesTable.AxisStatesTable[__axisName] = __state;
        }

        Internal_UpdateAxis2D();
    }

    private void Internal_UpdateAxis2D()
    {
        foreach (var __axis2d in _axesTable.Axis2DInfoTable)
        {
            string __vectorName = __axis2d.Key;
            Vector2 __current = GetAxis2D(__vectorName);

            if (!_lastVector2Axis2D.TryGetValue(__vectorName, out Vector2 __old))
            {
                _lastVector2Axis2D.Add(__vectorName, __current);
                continue;
            }

            if ((__current - __old).sqrMagnitude > EPSILON)
            {
                OnAxis2DChanged?.Invoke(__vectorName, __old, __current);
                _lastVector2Axis2D[__vectorName] = __current;
            }
        }
    }

    #endregion

    #region Actions

    private void UpdateActions()
    {
        if (_actionsTable.ActionEntries.Count <= 0) return;

        foreach (var __pair in _actionsTable.ActionEntries)
        {
            HICInputActionEntry __entry = __pair.Value;

            if (!__entry.Enabled)
                continue;

            if (IsInputLocked())
            {
                CancelPerformedAction(__entry);
                continue;
            }

            UpdateSingleAction(__entry);
        }
    }

    private void UpdateSingleAction(HICInputActionEntry __entry)
    {
        for (int i = 0; i < __entry.Bindings.Count; i++)
        {
            HICInputActionParams __param = __entry.Bindings[i];

            HandleActionTrigger(__entry, __param);

            if (__entry.State.PerformedThisFrame)
                break;
        }

        UpdateActionHoldingEvent(__entry);
    }

    private void HandleActionTrigger(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        switch (__param.TriggerType)
        {
            case HICInputTriggerType.Down:
                Input_HandleAction_Down(__entry, __param);
                break;
                
            case HICInputTriggerType.Hold:
                Input_HandleAction_Hold(__entry, __param);
                break;
            
            case HICInputTriggerType.Up:
                Input_HandleAction_Up(__entry, __param);
                break;

            case HICInputTriggerType.Tap:
                Input_HandleAction_Tap(__entry, __param);
                break;

            case HICInputTriggerType.DoubleTap:
                Input_HandleAction_DoubleTap(__entry, __param);
                break;

            case HICInputTriggerType.LongPressing:
                Input_HandleAction_LongPressing(__entry, __param);
                break;

            case HICInputTriggerType.Combo:
                Input_HandleAction_Combo(__entry, __param);
                break;

            case HICInputTriggerType.Toggle:
                Input_HandleAction_Toggle(__entry, __param);
                break;

            case HICInputTriggerType.Special:
                Input_HandleAction_Special(__entry, __param);
                break;
        }
    }

    private void Input_HandleAction_Down(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (IsActionBindingDown(__param))
        {
            ReadyPerformAction(__entry, __param);
            return;
        }

        if (IsActionBindingHolding(__param))
        {
            __entry.State.IsHolding = true;
            __entry.State.HoldingDuration = Time.time - __entry.State.BeginTime;
            return;
        }

        if (IsActionBindingUp(__param))
        {
            ReleasePerformedAction(__entry, __param, false);
            return;
        }
    }

    private void Input_HandleAction_Hold(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (IsActionBindingDown(__param))
            ReadyPerformAction(__entry, __param, false);

        if (IsActionBindingHolding(__param))
        {
            __entry.State.IsHolding = true;
            __entry.State.HoldingDuration = Time.time - __entry.State.BeginTime;

            if (__entry.State.HoldingDuration >= __param.HoldingTime)
                PerformingAction(__entry, __param);
        }

        if (IsActionBindingUp(__param))
            ReleasePerformedAction(__entry, __param, false);
    }

    private void Input_HandleAction_Up(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (IsActionBindingUp(__param))
            ReleasePerformedAction(__entry, __param);
    }

    private void Input_HandleAction_Tap(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (IsActionBindingDown(__param))
            ReadyPerformAction(__entry, __param, false);

        if (IsActionBindingUp(__param))
        {
            if (Time.time - __entry.State.BeginTime <= __param.TapMaxTime)
                PerformingAction(__entry, __param);

            ReleasePerformedAction(__entry, __param, false);
        }
    }

    private void Input_HandleAction_DoubleTap(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (!IsActionBindingDown(__param)) return;

        float __now = Time.time;
        float __delta = __now - __entry.State.LastTapTime;

        if (__param.DoubleTapMaxDelay >= __delta)
        {
            __entry.State.TapCounts++;

            if (__entry.State.TapCounts >= 2)
            {
                PerformingAction(__entry, __param);
                __entry.State.TapCounts = 0;
            }
        }
        else
        {
            __entry.State.TapCounts = 1;
        }

        __entry.State.LastTapTime = __now;
    }

    private void Input_HandleAction_LongPressing(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (IsActionBindingDown(__param))
            ReadyPerformAction(__entry, __param, false);

        if (IsActionBindingHolding(__param))
        {
            __entry.State.IsHolding = true;
            __entry.State.HoldingDuration = Time.time - __entry.State.BeginTime;

            if (__entry.State.HoldingDuration >= __param.HoldingTime && !__entry.State.PerformedThisFrame)
                PerformingAction(__entry, __param);
        }

        if (IsActionBindingUp(__param))
            ReleasePerformedAction(__entry, __param, false);
    }

    private void Input_HandleAction_Combo(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        bool __sequenceKeyPressed = __param.KeyCombo == InputKey.None || IsInputHolding(__param.KeyCombo);

        if (!__sequenceKeyPressed) return;

        if (IsActionBindingDown(__param))
            ReadyPerformAction(__entry, __param);
    }

    private void Input_HandleAction_Toggle(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (!IsActionBindingDown(__param)) return;

        __entry.State.Toggled = !__entry.State.Toggled;

        ReadyPerformAction(__entry, __param);
    }

    private void Input_HandleAction_Special(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (__param.SequenceKeys == null || __param.SequenceKeys.Length <= 0)
            return;

        int __index = __entry.State.SequenceIndex;
        InputKey __expectedKey = __param.SequenceKeys[__index];

        if (IsInputDown(__expectedKey))
        {
            __entry.State.SequenceIndex++;

            if (__entry.State.SequenceIndex >= __param.SequenceKeys.Length)
            {
                PerformingAction(__entry, __param);
                __entry.State.SequenceIndex = 0;
            }

            return;
        }

        // happened when a key accidentally/in-purpose clicked, resetting the sequence (special) keys
        for (int __i = 0; __i < _watchedButtons.Count; __i++)
        {
            InputKey __susKey = _watchedButtons[__i];

            if (__susKey != __expectedKey && IsInputDown(__susKey))
            {
                __entry.State.SequenceIndex = 0;
                return;
            }
        }
    }

    #region ActionHelpers

    private bool IsActionBindingDown(HICInputActionParams __param)
    {
        switch (__param.DeviceType)
        {
            case HICInputDeviceType.Keyboard:
                return IsInputDown(__param.Key);

            case HICInputDeviceType.Mouse:
                return __param.MouseButton >= 0 && __param.MouseButton < _mouseDown.Length && _mouseDown[__param.MouseButton];

            case HICInputDeviceType.Gamepad:
                return IsGamepadDown(__param.GamepadButtonIndex);

            case HICInputDeviceType.Touch:
                return _touchActive && Time.frameCount == GetBeginTouchFrame();

            default:
                return false;
        }
    }

    private bool IsActionBindingHolding(HICInputActionParams __param)
    {
        switch (__param.DeviceType)
        {
            case HICInputDeviceType.Keyboard:
                return IsInputHolding(__param.Key);
            
            case HICInputDeviceType.Mouse:
                return __param.MouseButton >= 0 && __param.MouseButton < _mouseHolding.Length && _mouseHolding[__param.MouseButton];

            case HICInputDeviceType.Gamepad:
                return IsGamepadHolding(__param.GamepadButtonIndex);

            case HICInputDeviceType.Touch:
                return _touchActive;

            default:
                return false;
        }
    }

    private bool IsActionBindingUp(HICInputActionParams __param)
    {
        switch (__param.DeviceType)
        {
            case HICInputDeviceType.Keyboard:
                return IsInputUp(__param.Key);

            case HICInputDeviceType.Mouse:
                return __param.MouseButton >= 0 && __param.MouseButton < _mouseUp.Length && _mouseUp[__param.MouseButton];

            case HICInputDeviceType.Gamepad:
                return IsGamepadUp(__param.GamepadButtonIndex);

            case HICInputDeviceType.Touch:
                return !_touchActive && Time.frameCount == GetEndedTouchFrame();
            default:
                return false;
        }
    }

    #endregion

    #region ActionPerforms

    private HICInputActionContext CreateActionContext(HICInputActionEntry __entry, HICInputActionParams __param, HICInputActionStage __stage)
    {
        HICInputActionContext __context = new HICInputActionContext(__entry.ActionName, __param.DeviceType, __stage)
        {
            TriggerType = __param.TriggerType,

            Key = __param.Key,
            MouseButton = __param.MouseButton,
            GamepadButtonIndex = __param.GamepadButtonIndex,

            WorldInputPosition = GetMousePositionInWorld(),
            ScreenInputPosition = GetMousePositionInScreen(),

            LastTimePressed = Time.time,
            HoldingTime = __entry.State.HoldingDuration,
            Consumed = __entry.State.Consumed
        };

        return __context;
    }

    private void ReadyPerformAction(HICInputActionEntry __entry, HICInputActionParams __param, bool __call = true)
    {
        __entry.State.IsPressedDown = true;
        __entry.State.IsHolding = true;
        __entry.State.BeginTime = Time.time;
        __entry.State.Stage = HICInputActionStage.Begin;
        __entry.State.LastDeviceType = __param.DeviceType;
        __entry.State.LastKey = __param.Key;

        HICInputActionContext __context = CreateActionContext(__entry, __param, HICInputActionStage.Begin);

        if (__call)
            __entry.Callbacks?.Invoke(__context);
        OnActionBegin?.Invoke(__context);

        SetCurrentDevice(__param.DeviceType);
    }

    private void PerformingAction(HICInputActionEntry __entry, HICInputActionParams __param, bool __call = true)
    {
        if (__entry.State.Consumed) return;

        __entry.State.PerformedThisFrame = true;
        __entry.State.LastPerformedTime = Time.time;
        __entry.State.Buffered = true;
        __entry.State.Stage = HICInputActionStage.Performed;

        HICInputActionContext __context = CreateActionContext(__entry, __param, HICInputActionStage.Performed);

        if (__call)
            __entry.Callbacks?.Invoke(__context);
        OnActionPerformed?.Invoke(__context);

        if (__entry.ConsumeOnPerformed)
            __entry.State.Consumed = true;
    }

    private void ReleasePerformedAction(HICInputActionEntry __entry, HICInputActionParams __param, bool __call = true)
    {
        __entry.State.IsHolding = false;
        __entry.State.IsReleasedUp = true;
        __entry.State.ReleasedThisFrame = true;
        __entry.State.ReleasedTime = Time.time;
        __entry.State.HoldingDuration = Time.time - __entry.State.BeginTime;
        __entry.State.Stage = HICInputActionStage.Released;

        HICInputActionContext __context = CreateActionContext(__entry, __param, HICInputActionStage.Released);

        if (__call)
            __entry.Callbacks?.Invoke(__context);
        OnActionReleased?.Invoke(__context);
    }

    private void CancelPerformedAction(HICInputActionEntry __entry)
    {
        if (!__entry.State.IsHolding) return;

        __entry.State.IsHolding = false;
        __entry.State.Stage = HICInputActionStage.Cancelled;

        HICInputActionContext __context = new HICInputActionContext(__entry.ActionName, __entry.State.LastDeviceType, HICInputActionStage.Cancelled);

        __entry.Callbacks?.Invoke(__context);
        OnActionCancelled?.Invoke(__context);
    }

    private void UpdateActionHoldingEvent(HICInputActionEntry __entry)
    {
        if (!__entry.State.IsHolding) return;

        if (__entry.State.ReleasedThisFrame) 
            return;

        __entry.State.HoldingDuration = Time.time - __entry.State.BeginTime;
        __entry.State.Stage = HICInputActionStage.Holding;

        HICInputActionContext __context = new HICInputActionContext(__entry.ActionName, __entry.State.LastDeviceType, HICInputActionStage.Holding)
        {
            Key = __entry.State.LastKey,
            HoldingTime = __entry.State.HoldingDuration,
            LastTimePressed = Time.time
        };

        OnActionHolding?.Invoke(__context);
    }

    #endregion

    #endregion

    #endregion

    #region Calculation

    private float CalculateAxisTarget(List<HICInputAxisInfo> __bindings, HICAxisParams __param, ref HICAxisState __state)
    {
        float __positive = 0;
        float __negative = 0;

        bool __hasPressedKey = false;

        InputKey __lastPressedKey = __state.LastPressedKey;

        for (int i = 0; i < __bindings.Count; i++)
        {
            HICInputAxisInfo __info = __bindings[i];

            bool __isPressing = IsBindingPressed(__info);

            if (!__isPressing)
                continue; // from "return 0f"

            __hasPressedKey = true;

            float __value = __info.Value;

            if (__value > 0f)
                __positive = Mathf.Max(__positive, __value);
            else if (__value < 0f)
                __negative = Mathf.Min(__negative, __value);

            if (IsBindingDown(__info))
            {
                __lastPressedKey = __info.Key;
                __state.LastPressedKey = __info.Key;
            }

            SetCurrentDevice(__info.DeviceType);
        }

        if (!__hasPressedKey)
            return 0f;

        bool __hasPositive = __positive != 0f;
        bool __hasNegative = __negative != 0f;

        if (__hasPositive && __hasNegative)
        {
            switch (__param.ConflictType)
            {
                case HICInputConflictType.Neutralized:
                    return 0f;

                case HICInputConflictType.PositiveWins:
                    return __positive;

                case HICInputConflictType.NegativeWins:
                    return __negative;

                case HICInputConflictType.LastPressed:
                    return ResolveCurrentPressedAxis(__bindings, __lastPressedKey);
            }
        }

        if (__hasPositive)
            return __positive;

        if (__hasNegative)
            return __negative;

        return 0f;
    }

    private float AxisValueResult(float __v_current, float __v_target, float __sensitivity, float __gravity)
    {
        float __speed = Mathf.Abs(__v_target) > Mathf.Abs(__v_current) ?
            __sensitivity : __gravity;

        return Mathf.MoveTowards(__v_current, __v_target, __speed * Time.deltaTime);
    }

    private float ProAxisValueResult(float __v_current, float __v_target, float __sensitivity, float __gravity, float __snapThreshold)
    {
        if (Mathf.Sign(__v_current) != Mathf.Sign(__v_target) && Mathf.Abs(__v_current) > __snapThreshold && Mathf.Abs(__v_target) > __snapThreshold)
            __v_current = 0;

        float __speed = Mathf.Abs(__v_target) > Mathf.Abs(__v_current) ?
            __sensitivity : __gravity;

        return Mathf.SmoothDamp(__v_current, __v_target, ref ___proAxis_smooth_references, __speed * Time.deltaTime);
    }

    private float DeadZoneResult(float __value, float __deadZone)
    {
        return Mathf.Abs(__value) >= __deadZone ? __value : 0f;
    }

    private bool FuzzyEq(float __a, float __b)
    {
        return Mathf.Abs(__a - __b) <= EPSILON;
    }

    #endregion

    #region InputCheck

    private void UpdateKeyCache()
    {
        for (int i = 0; i < _watchedButtons.Count; i++)
        {
            bool __isKeyDown;
            bool __isKeyHolding;
            bool __isKeyUp;

            #if ENABLE_INPUT_SYSTEM

            InputKey __current_key = _watchedButtons[i];
            
            __isKeyDown = Keyboard.current[__current_key].wasPressedThisFrame;
            __isKeyHolding = Keyboard.current[__current_key].isPressed;
            __isKeyUp = Keyboard.current[__current_key].wasReleasedThisFrame;

            #elif ENABLE_LEGACY_INPUT_MANAGER

            InputKey __current_key = _watchedButtons[i];

            __isKeyDown = Input.GetKeyDown(__current_key);
            __isKeyHolding = Input.GetKey(__current_key);
            __isKeyUp = Input.GetKeyUp(__current_key);

            #endif

            _buttonsDown[__current_key] = __isKeyDown;
            _buttonsHolding[__current_key] = __isKeyHolding;
            _buttonsUp[__current_key] = __isKeyUp;

            if (__isKeyDown)
            {
                _buttonsPressedTime[__current_key] = Time.time;
                SetCurrentDevice(HICInputDeviceType.Keyboard);
                OnInputKeyDown?.Invoke(__current_key);
            }

            if (__isKeyHolding)
            {
                SetCurrentDevice(HICInputDeviceType.Keyboard);
                OnInputKeyStaying?.Invoke(__current_key);
            }

            if (__isKeyUp)
            {
                SetCurrentDevice(HICInputDeviceType.Keyboard);
                OnInputKeyUp?.Invoke(__current_key);
            }
        }
    }

    private bool IsBindingPressed(HICInputAxisInfo __info)
    {
        if (IsInputHolding(__info.Key))
            return true;

        return false;
    }

    private bool IsBindingDown(HICInputAxisInfo __info)
    {
        if (IsInputDown(__info.Key))
            return true;

        return false;
    }

    private bool IsInputHolding(InputKey __key)
    {
        bool __isInputValid = __key != InputKey.None && _buttonsHolding.TryGetValue(__key, out bool __result) && __result;

        return __isInputValid;
    }

    private bool IsInputDown(InputKey __key)
    {
        bool __isInputValid = __key != InputKey.None && _buttonsDown.TryGetValue(__key, out bool __result) && __result;

        return __isInputValid;
    }   

    private bool IsInputUp(InputKey __key)
    {
        bool __isInputValid = __key != InputKey.None && _buttonsUp.TryGetValue(__key, out bool __result) && __result;

        return __isInputValid;
    }

    private bool IsGamepadDown(int __gamepadIndex)
    {
        #if ENABLE_INPUT_SYSTEM
        if (Gamepad.current == null) return false;
        GamepadButton __button = (GamepadButton) 0 + __gamepadIndex;
        
        bool __down = Gamepad.current[__button].wasPressedThisFrame;

        #elif ENABLE_LEGACY_INPUT_MANAGER
        InputKey __index = InputKey.JoystickButton0 + __gamepadIndex;

        bool __down = Input.GetKeyDown(__index);
        #endif

        SetCurrentDevice(HICInputDeviceType.Gamepad);

        return __down;
    }

    private bool IsGamepadHolding(int __gamepadIndex)
    {
        #if ENABLE_INPUT_SYSTEM
        if (Gamepad.current == null) return false;
        GamepadButton __button = (GamepadButton) 0 + __gamepadIndex;
        
        bool __down = Gamepad.current[__button].isPressed;

        #elif ENABLE_LEGACY_INPUT_MANAGER
        InputKey __index = InputKey.JoystickButton0 + __gamepadIndex;

        bool __down = Input.GetKey(__index);
        #endif

        SetCurrentDevice(HICInputDeviceType.Gamepad);

        return __down;
    }

    private bool IsGamepadUp(int __gamepadIndex)
    {
        #if ENABLE_INPUT_SYSTEM
        if (Gamepad.current == null) return false;
        GamepadButton __button = (GamepadButton) 0 + __gamepadIndex;
        
        bool __down = Gamepad.current[__button].wasReleasedThisFrame;

        #elif ENABLE_LEGACY_INPUT_MANAGER
        InputKey __index = InputKey.JoystickButton0 + __gamepadIndex;

        bool __down = Input.GetKeyUp(__index);
        #endif

        SetCurrentDevice(HICInputDeviceType.Gamepad);

        return __down;
    }

    private float ResolveCurrentPressedAxis(List<HICInputAxisInfo> __info, InputKey __key)
    {
        for (int __i = 0; __i < __info.Count; __i++)
        {
            HICInputAxisInfo __bind = __info[__i];
            if (__bind.Key == __key)
                return __bind.Value;
        }

        return 0f;
    }

    #endregion

    #region MouseCaches

    private MouseControl NewSystem_Internal_GetMouse(int __index)
    {
        return __index switch
        {
            0 => Mouse.current.leftButton,
            1 => Mouse.current.rightButton,
            2 => Mouse.current.middleButton,
            3 => Mouse.current.forwardButton,
            4 => Mouse.current.backButton,
            _ => null
        };
    }
    
    private void UpdateMouseCache()
    {
        _previous_mousePos_screen = _mousePos_screen;
        #if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return;

            _mousePos_screen = Mouse.current.position.ReadValue();
            _scroll_mouseDelta = Mouse.current.delta.y.ReadValue();
        #elif ENABLE_LEGACY_INPUT_MANAGER
            _mousePos_screen = Input.mousePosition;
             _scroll_mouseDelta = Input.mouseScrollDelta.y;
        #endif
        _mouseDelta = _mousePos_screen - _previous_mousePos_screen;


        for (int __i = 0; __i < 5; __i++)
        {
            #if ENABLE_INPUT_SYSTEM

            _mouseDown[__i] = NewSystem_Internal_GetMouse(__i).wasPressedThisFrame;
            _mouseHolding[__i] = NewSystem_Internal_GetMouse(__i).isPressed;
            _mouseUp[__i] = NewSystem_Internal_GetMouse(__i).wasReleasedThisFrame;

            #elif ENABLE_LEGACY_INPUT_MANAGER

            _mouseDown[__i] = Input.GetMouseButtonDown(__i);
            _mouseHolding[__i] = Input.GetMouseButton(__i);
            _mouseUp[__i] = Input.GetMouseButtonUp(__i);

            #endif

            if (_mouseDown[__i])
            {
                SetCurrentDevice(HICInputDeviceType.Mouse);
                OnMouseDown?.Invoke(__i, _mousePos_screen);
            }

            if (_mouseHolding[__i])
            {
                SetCurrentDevice(HICInputDeviceType.Mouse);
                OnMouseHolding?.Invoke(__i, _mousePos_screen);
            }

            if (_mouseUp[__i])
            {
                SetCurrentDevice(HICInputDeviceType.Mouse);
                OnMouseUp?.Invoke(__i, _mousePos_screen);
            }
        }

        if (Mathf.Abs(_scroll_mouseDelta) > 0.001f)
        {
            SetCurrentDevice(HICInputDeviceType.Mouse);
            OnMouseScrolling?.Invoke(_scroll_mouseDelta);
        }
    }

    public void UpdateMouseProjection()
    {
        HICPointerToWorldInfo __pointerInfo = new HICPointerToWorldInfo
        {
            ScreenPosition = _mousePos_screen,
            Ray = GetMouseRay()
        };

        bool __bPointerRay = Physics.Raycast(__pointerInfo.Ray, out RaycastHit __hit, projectionMaxDistance, projectionLayer, QueryTriggerInteraction.Ignore);

        if (__bPointerRay)
        {
            __pointerInfo.HasHit = true;
            __pointerInfo.Hit = __hit;
            __pointerInfo.WorldPosition = __hit.point;
            __pointerInfo.Distance = __hit.distance;
        }
        else
        {
            __pointerInfo.HasHit = false;
            __pointerInfo.WorldPosition = __pointerInfo.Ray.GetPoint(projectionMaxDistance);
            __pointerInfo.Distance = projectionMaxDistance;
        }

        _projectionInfoCache = __pointerInfo;
    }

    #endregion

    #region TouchCaches

    private void UpdateTouchCache()
    {
        #if ENABLE_INPUT_SYSTEM

        if (NewTouch.activeTouches.Count <= 0)
        {
            if (_touchActive)
                Input_EndedTouch();
            
            _touchFingerCounts = 0;
            return;
        }

        NewTouch __touch = NewTouch.activeTouches[0];

        _touchFingerCounts = NewTouch.activeTouches.Count;
        _touchPreviousPos = _touchCurrentPos;
        _touchCurrentPos = __touch.screenPosition;
        _touchDelta = _touchCurrentPos - _touchPreviousPos;

        SetCurrentDevice(HICInputDeviceType.Touch);

        switch (__touch.phase)
        {
            case NewTouchPhase.Began:
                Input_BeginTouch(__touch.screenPosition);
                break;
            
            case NewTouchPhase.Moved:
                Input_MovedTouch(__touch.screenPosition);
                break;

            case NewTouchPhase.Canceled:
                Input_CancelledTouch();
                break;

            case NewTouchPhase.Stationary:
                Input_StationaryTouch(__touch.screenPosition);
                break;

            case NewTouchPhase.Ended:
                Input_EndedTouch();
                break;
        }

        if (NewTouch.activeTouches.Count >= 2)
            CheckMultiFingers();

        #elif ENABLE_LEGACY_INPUT_MANAGER

        if (Input.touchCount <= 0)
        {
            if (_touchActive)
                Input_EndedTouch();

            _touchFingerCounts = 0;
            return;
        }

        Touch touch = Input.GetTouch(0);

        _touchFingerCounts = Input.touchCount;
        _touchPreviousPos = _touchCurrentPos;
        _touchCurrentPos = touch.position;
        _touchDelta = _touchCurrentPos - _touchPreviousPos;

        SetCurrentDevice(HICInputDeviceType.Touch);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                Input_BeginTouch(touch.position);
                break;

            case TouchPhase.Moved:
                Input_MovedTouch(touch.position);
                break;

            case TouchPhase.Canceled:
                Input_CancelledTouch();
                break;

            case TouchPhase.Stationary:
                Input_StationaryTouch(touch.position);
                break;

            case TouchPhase.Ended:
                Input_EndedTouch();
                break;
        }

        if (Input.touchCount >= 2)
            CheckMultiFingers();

        #endif
    }

    private void Input_BeginTouch(Vector2 __position)
    {
        _touchActive = true;

        _touchBeginPos = __position;
        _touchBeginTime = Time.time;
        _touchCurrentPos = __position;
        _touchPreviousPos = __position;

        _touchDelta = Vector2.zero;
        _touchLongPressFired = false;

        _touchBeginFrame = Time.frameCount;

        OnTouchHappened?.Invoke(__position);

        float __tapSinceBegin = Time.time - _lastTouchTapTime;

        if (touchDoubleTapMaxDelay >= __tapSinceBegin)
        {
            OnTouchDoubleTapping?.Invoke(__position);
            _touchDoubleTapCandidate = true;
        }
        else
        {
            _touchDoubleTapCandidate = false;
        }

        _lastTouchTapTime = Time.time;
    }

    private void Input_MovedTouch(Vector2 __position)
    {
        float __holdingDuration = Time.time - _touchBeginTime;

        OnTouchHolding?.Invoke(__position, __holdingDuration);

        #if ENABLE_INPUT_SYSTEM

        if (_touchFingerCounts > 1)
        {
            foreach (var __touch in NewTouch.activeTouches)
                _fingersIdentity.Add(new HICFingersIdentity(__touch.touchId, __touch.screenPosition));

            OnTouchPan?.Invoke(_touchFingerCounts, _fingersIdentity.ToArray());
            _fingersIdentity.Clear();
        }

        #elif ENABLE_LEGACY_INPUT_MANAGER

        if (_touchFingerCounts > 1)
        {
            for (int i = 0; i < _touchFingerCounts; i++)
            {
                Touch __indexTouch = Input.GetTouch(i);
                _fingersIdentity.Add(new HICFingersIdentity(__indexTouch.fingerId, __indexTouch.position));
            }
            
            OnTouchPan?.Invoke(_touchFingerCounts, _fingersIdentity.ToArray());
            _fingersIdentity.Clear();
        }

        #endif

        CheckLongPressingTouch(__position, __holdingDuration);
    }

    private void Input_StationaryTouch(Vector2 __position)
    {
        float __holdingDuration = Time.time - _touchBeginTime;

        OnTouchStationary?.Invoke(__position);
        OnTouchHolding?.Invoke(__position, __holdingDuration);

        CheckLongPressingTouch(__position, __holdingDuration);
    }

    private void Input_EndedTouch()
    {
        if (!_touchActive) return;

        _touchActive = false;
        _touchEndedFrame = Time.frameCount;

        Vector2 __currentPos = _touchCurrentPos;
        Vector2 __deltaPos = __currentPos - _touchBeginPos;

        float __swipeDuration = Time.time - _touchBeginTime;

        OnTouchReleased?.Invoke(__currentPos);

        if (__deltaPos.magnitude >= touchSwipeMinDistance && touchSwipeDuration >= __swipeDuration)
        {
            Vector2 __direction = __deltaPos.normalized;
            OnTouchSwipe?.Invoke(_touchBeginPos, __currentPos, __direction);
        }
    }

    private void Input_CancelledTouch()
    {
        if (!_touchActive) return;

        _touchActive = false;
        OnTouchInterrupted?.Invoke();
    }

    private void CheckLongPressingTouch(Vector2 __position, float __holdingDuration)
    {
        if (_touchLongPressFired) return;

        if (__holdingDuration < touchHoldingDurations)
            return;

        _touchLongPressFired = true;
        OnTouchLongHolding?.Invoke(__position, __holdingDuration);
    }

    private void CheckMultiFingers()
    {
        if (!_touchActive) return;

        #if ENABLE_INPUT_SYSTEM

        if (NewTouch.activeTouches.Count < 2)
            return;

        NewTouch __firstFinger = NewTouch.activeTouches[0];
        NewTouch __secondFinger = NewTouch.activeTouches[1];

        float __currDistance = Vector2.Distance(__firstFinger.screenPosition, __secondFinger.screenPosition);

        float __zoomDiff = __currDistance - _previousPitchDistance;
        __zoomDiff = Mathf.Abs(__zoomDiff);

        if (_previousPitchDistance > 0f && __zoomDiff > 0.001f)
            OnTouchPinch?.Invoke(__zoomDiff, __currDistance);

        _previousPitchDistance = __currDistance;
        
        Vector2 __dirr = __secondFinger.screenPosition - __firstFinger.screenPosition;

        #elif ENABLE_LEGACY_INPUT_MANAGER

        if (Input.touchCount < 2)
            return;

        Touch __firstFinger = Input.GetTouch(0);
        Touch __secondFinger = Input.GetTouch(1);

        float __currDistance = Vector2.Distance(__firstFinger.position, __secondFinger.position);

        float __zoomDiff = __currDistance - _previousPitchDistance;
        __zoomDiff = Mathf.Abs(__zoomDiff);

        if (_previousPitchDistance > 0f && __zoomDiff > 0.001f)
            OnTouchPinch?.Invoke(__zoomDiff, __currDistance);

        _previousPitchDistance = __currDistance;
        
        Vector2 __dirr = __secondFinger.position - __firstFinger.position;

        #endif

        float __currAngle = Mathf.Atan2(__dirr.y, __dirr.x) * Mathf.Rad2Deg;
        float __angleDelta = Mathf.DeltaAngle(_previousTouchAngle, __currAngle);
        __angleDelta = Mathf.Abs(__angleDelta);

        if (__angleDelta > 0.001f)
            OnTouchRotate?.Invoke(__angleDelta);

        _previousTouchAngle = __currAngle;
    }

    #endregion

    #region Gamepads

    private void UpdateGamepadCache()
    {
        _previousLeftStick = _leftStick;
        _previousRightStick = _rightStick;

        #if ENABLE_INPUT_SYSTEM

        if (Gamepad.current == null) return;

        _leftStick = Gamepad.current.leftStick.ReadValue();
        _rightStick = Gamepad.current.rightStick.ReadValue();

        #elif ENABLE_LEGACY_INPUT_MANAGER

        _leftStick = new Vector2(
            Input.GetAxisRaw(leftJoystickAxisName + "X"),
            Input.GetAxisRaw(leftJoystickAxisName + "Y")
        );

        _rightStick = new Vector2(
            Input.GetAxisRaw(rightJoystickAxisName + "X"),
            Input.GetAxisRaw(rightJoystickAxisName + "Y")
        );

        #endif

        if ((_leftStick - _previousLeftStick).sqrMagnitude > EPSILON)
        {
            SetCurrentDevice(HICInputDeviceType.Gamepad);
            OnGamepadLeftStickChanged?.Invoke(_leftStick);
        }

        if ((_rightStick - _previousRightStick).sqrMagnitude > EPSILON)
        {
            SetCurrentDevice(HICInputDeviceType.Gamepad);
            OnGamepadRightStickChanged?.Invoke(_rightStick);
        }

        Internal_HandleGamepadPressings();
        Internal_HandleGamepadRotation();
    }

    private void Internal_HandleGamepadPressings()
    {
        _previousLeftStickPressed = _leftStickPressed;
        _previousRightStickPressed = _rightStickPressed;

        #if ENABLE_INPUT_SYSTEM

        _leftStickPressed = Gamepad.current.leftStickButton.wasPressedThisFrame;
        _rightStickPressed = Gamepad.current.rightStickButton.wasPressedThisFrame;

        #elif ENABLE_LEGACY_INPUT_MANAGER

        _leftStickPressed = Input.GetKey(KeyCode.JoystickButton8);
        _rightStickPressed = Input.GetKey(KeyCode.JoystickButton9);

        #endif

        HandleJoysticksPress(0, _previousLeftStickPressed, _leftStickPressed);
        HandleJoysticksPress(1, _previousRightStickPressed, _rightStickPressed);
    }

    private void HandleJoysticksPress(int __index, bool __prev, bool __cur)
    {
        if (!__prev && __cur)
            OnThumbstickPressDown?.Invoke(__index);

        if (__cur)
            OnThumbstickPressHolding?.Invoke(__index);

        if (__prev && !__cur)
            OnThumbstickPressUp?.Invoke(__index);
    }

    private void Internal_HandleGamepadRotation()
    {
        HandleJoystickRotation(_leftStick, ref _previousLeftAngle, ref _leftAccumulatedRotationAngle, OnLeftThumbstickRotating);

        HandleJoystickRotation(_rightStick, ref _previousRightAngle, ref _rightAccumulatedRotationAngle, OnRightThumbstickRotating);
    }

    private void HandleJoystickRotation(Vector2 __stick, ref float __prevAngle, ref float __currAngle, Action __action)
    {
        if (__stick.sqrMagnitude < 0.5f * 0.5f)
            return;

        float __angle = Mathf.Atan2(__stick.y, __stick.x) * Mathf.Rad2Deg;
        float __angleDelta = Mathf.DeltaAngle(__prevAngle, __angle);
        __angleDelta = Mathf.Abs(__angleDelta);

        __currAngle += __angleDelta;
        __prevAngle = __angle;

        if (__currAngle >= joystickRotationThreshold)
        {
            __currAngle = 0f;
            __action?.Invoke();
        }
    }

    #endregion

    #region Devices

    private void SetCurrentDevice(HICInputDeviceType __deviceType)
    {
        if (__deviceType == HICInputDeviceType.Unknown)
            return;

        _usedDevice = __deviceType;
    }

    #endregion

    #region GyroCaches

    private Quaternion ConvertRotationResult(Quaternion __currentQuat)
    {
        Quaternion __now = __currentQuat;
        if ((_gyroService.CurrentMinusAxis & (ushort) HICGyroAxis.X) != 0)
            __now.x = -__now.x;

        if ((_gyroService.CurrentMinusAxis & (ushort) HICGyroAxis.Y) != 0)
            __now.y = -__now.y;

        if ((_gyroService.CurrentMinusAxis & (ushort) HICGyroAxis.Z) != 0)
            __now.z = -__now.z;

        if ((_gyroService.CurrentMinusAxis & (ushort) HICGyroAxis.W) != 0)
            __now.w = -__now.w;

        return __now;
    }

    private Vector3 ConvertRotationResult(Vector3 __currentVector)
    {
        Vector3 __now = __currentVector;
        if ((_gyroService.CurrentMinusAxis & (ushort) HICGyroAxis.X) != 0)
            __now.x = -__now.x;

        if ((_gyroService.CurrentMinusAxis & (ushort) HICGyroAxis.Y) != 0)
            __now.y = -__now.y;

        if ((_gyroService.CurrentMinusAxis & (ushort) HICGyroAxis.Z) != 0)
            __now.z = -__now.z;

        return __now;
    }

    private void SetAttitudeCallibration(Quaternion __raw)
    {
        __raw = ConvertRotationResult(__raw);
        _calibrationQuat = __raw;
    }

    private void SetAngularCalibration(Vector3 __raw)
    {
        __raw = ConvertRotationResult(__raw);
        _calibrationVector3 = __raw;
    }

    private Quaternion ApplyAttitude(Quaternion __raw)
    {
        // inverted/minus axis
        __raw = ConvertRotationResult(__raw);

        // calibration
        __raw = _calibrationQuat * __raw;

        // angle-axis out
        __raw.ToAngleAxis(out float __angle, out Vector3 __axis);

        __angle = __angle > 180.0f ? (__angle - 360.0f) : __angle;
        float __absAngle = Mathf.Abs(__angle);

        // dead-zone
        if (__absAngle < _gyroService.GyroDeadZone)
            return Quaternion.identity;

        // min rotation angle against quat angle
        if (__absAngle < _gyroService.GyroMinRotationAngle)
            return Quaternion.identity;
        
        float __sign = Mathf.Sign(__angle);

        // sensitivity, accel curve, and max rotation angle against quat angle
        __angle = Mathf.Pow(__absAngle * _gyroService.GyroSensitivity, _gyroService.GyroAccelerationCurve) * __sign;
        __angle = Mathf.Clamp(__angle, 0f, _gyroService.GyroMaxRotationAngle);

        // axis multiplier
        __axis = __axis * _gyroService.GyroAxisMultiplier;

        if (__axis.sqrMagnitude <= EPSILON)
            return Quaternion.identity;
        __axis = __axis.normalized;

        // return to quat
        __raw = Quaternion.AngleAxis(__angle, __axis);

        // slerp checker, smoothscale and driftcorrection
        __raw = Quaternion.Slerp(_lastRotatedQuat, __raw, _gyroService.GyroSmoothScale);
        __raw = Quaternion.Slerp(__raw, Quaternion.identity, _gyroService.GyroDriftCorrection);

        // quat attitude cache
        _lastRotatedQuat = __raw;

        return __raw;
    }

    private Quaternion ApplyAngularVelocity(Vector3 __raw)
    {
        // invert/minus axis and calibration cache
        __raw = ConvertRotationResult(__raw);

        // calibration
        __raw = __raw - _calibrationVector3;

        // dead zone
        __raw = __raw.magnitude >= _gyroService.GyroDeadZone ? __raw :  Vector3.zero;
        
        // axis sensitivity and multiplier
        __raw *= _gyroService.GyroSensitivity * _gyroService.GyroAxisMultiplier;
        float __speed = __raw.magnitude;

        // accel curve
        if (__speed > 0)
        {
            Vector3 __direction = __raw.normalized;
            float __curveSpeed = Mathf.Pow(__speed, _gyroService.GyroAccelerationCurve);
            __raw = __direction * __curveSpeed;
        }

        // angular speed
        if (__raw.magnitude > _gyroService.GyroMaxAngularSpeed)
            __raw = __raw.normalized * _gyroService.GyroMaxAngularSpeed;

        // smooth
        __raw = Vector3.Lerp(_lastRotatedVector3, __raw, _gyroService.GyroSmoothScale);
        // last rotated cache
        _lastRotatedVector3 = __raw;

        // angle
        float __angle = __raw.magnitude * Time.deltaTime * Mathf.Rad2Deg;

        // min angle
        if (__angle < _gyroService.GyroMinRotationAngle)
            return Quaternion.identity;

        // max angle
        __angle = Mathf.Clamp(__angle, 0f, _gyroService.GyroMaxRotationAngle);
        // axis
        Vector3 __axis = __raw.normalized;

        return Quaternion.AngleAxis(__angle, __axis);
    }

    private void UpdateGyroscope()
    {
        if (_gyroService == null || !_gyroService.Started) return;

        SetAttitudeCallibration(_gyroService.StartAttitude);
        SetAngularCalibration(_gyroService.StartAngularVelocity);

        Quaternion __rotationDelta_Attitude;        
        Quaternion __rotationDelta_angularVelocity;

        Vector3 __angVel;

        #if ENABLE_INPUT_SYSTEM

        __rotationDelta_Attitude = AttitudeSensor.current != null ? AttitudeSensor.current.attitude.ReadValue() : Quaternion.identity;
        __angVel = GyroScope.current != null ? GyroScope.current.angularVelocity.ReadValue() : Vector3.zero;

        #elif ENABLE_LEGACY_INPUT_MANAGER

        __rotationDelta_Attitude = Input.gyro.attitude;
        __angVel = Input.gyro.rotationRate;

        #endif

        __rotationDelta_Attitude = ApplyAttitude(__rotationDelta_Attitude);
        __rotationDelta_angularVelocity = ApplyAngularVelocity(__angVel);

        Quaternion __final = Quaternion.identity;
        Transform __cam = _gyroService.CurrentCamera.transform;
        switch (_gyroService.CurrentType)
        {
            case HICGyroType.Absolute:
                __final = __rotationDelta_Attitude;
                break;        

            case HICGyroType.Relative:
                __final = __cam.localRotation * __rotationDelta_angularVelocity;
                break;        

            case HICGyroType.Hybird:
                __final = __rotationDelta_Attitude * __rotationDelta_angularVelocity;
                break;        
        }

        __cam.localRotation = __final;
    }

    #endregion

    #endregion

    #region UnityHelpers

    private void OnEnable()
    {
        #if ENABLE_INPUT_SYSTEM
        
        EnhancedTouchSupport.Enable();

        #endif
    }

    private void OnDisable()
    {
        #if ENABLE_INPUT_SYSTEM
        
        EnhancedTouchSupport.Disable();

        #endif
    }

    private void Awake()
    {
        if (_axesTable == null)
            _axesTable = new HICAxesTable();

        if (_actionsTable == null)
            _actionsTable = new HICActionsTable();

        // _schemes = new Dictionary<string, HICInputControlScheme>(10);

        _inputLocks = new HashSet<string>();
        _inputContexts = new Stack<string>();

        _lastVector2Axis2D = new Dictionary<string, Vector2>(16);

        _buttonCallbacks = new Dictionary<Button, UnityAction>(64);

        _usedDevice = HICInputDeviceType.Unknown;
        _previousUsedDevice = HICInputDeviceType.Unknown;

        currentCamera = currentCamera != null ? currentCamera : GetComponent<Camera>();
        
        _mouseDown = new bool[5];
        _mouseHolding = new bool[5];
        _mouseUp = new bool[5];

        _fingersIdentity = new List<HICFingersIdentity>(touchMaximalOnScreen);

        _watchedButtons = new List<InputKey>(128);
        _buttonsDown = new Dictionary<InputKey, bool>(128);
        _buttonsHolding = new Dictionary<InputKey, bool>(128);
        _buttonsUp = new Dictionary<InputKey, bool>(128);
        _buttonsPressedTime = new Dictionary<InputKey, float>(128);

        _axisTrackDumps = new Dictionary<string, HICAxisState>(64);
        _axisBindTrackDumps = new Dictionary<string, List<HICInputAxisInfo>>(64);
    }

    private void Start()
    {
        #if ENABLE_INPUT_SYSTEM 

        if (enableGyroscope && GyroScope.current != null)
        {
            InputSystem.EnableDevice(GyroScope.current);

            if (_gyroService.CurrentType == HICGyroType.Absolute || _gyroService.CurrentType == HICGyroType.Hybird && AttitudeSensor.current != null)
                InputSystem.EnableDevice(AttitudeSensor.current);
        }
        else
        {
            InputSystem.DisableDevice(GyroScope.current);
            InputSystem.DisableDevice(AttitudeSensor.current);
        }
        
        #elif ENABLE_LEGACY_INPUT_MANAGER
        
        Input.gyro.enabled = enableGyroscope;

        #endif
    }

    private void Update()
    {
        FrameBegin();

        UpdateMouseCache();
        UpdateKeyCache();
        UpdateTouchCache();
        UpdateGamepadCache();
        UpdateGyroscope();
        
        UpdateActions();

        HandleAxes();
        UpdateMouseProjection();

        FrameEnd();
    }


    #endregion

}
