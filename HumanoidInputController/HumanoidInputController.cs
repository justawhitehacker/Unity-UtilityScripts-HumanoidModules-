// UNFINISHED

/*
Proposal
** HumanoidInputController **

Objection:
HumanoidInputController input; 
    |--> Independent component from other Humanoid's components

* Methods *

Setter:

(One-time, Start/Awake/etc)
input.SetAxesTable(axisTable : AxesTableInfo);

(One-time, Start/Awake/etc)
input.SetActionsTable(actionTable : InputActionsTableInfo)

Axes:

(One-time, Start/Awake/etc)
input.BindAxis(axisName : string, keycode : KeyCode, value : float)
    	|--> Inserting new axis into default/internal axes table, or external
    	|--> table that setted.

(Running)
input.GetAxis(axisName : string)
    	|-> Returning float of value, where between [-value, 0, value]. More
    	|-> smooth and scalable than Input.GetAxis() from Unity.

(Running)
input.GetProAxis(axisName : string)
    	|-> Do same as GetAxis() but having more smooth and better rising or 
    	|-> decreasing value, still same [-value, 0, value].

(One-time, Start/Awake/etc)
input.UnbindAxis(axisName : string)
    	|-> Removing the axis info from the axes table of default/internal axes 
    	|-> table or external table that setted.

Action:

(One-time, Start/Awake/etc)
input.AddAction(actionName : string, function : (), uiButton : Button, 	keyInfo : 	InputActionInfo...)
	|-> creating action where happened by input, the keyInfos are args, so 	
	|-> that can be setted that input like from gamepad, mobile, console, 	
	|-> VR, etc.
	|-> table can be from outside or default.
	|-> uiButton is where a Button UI can trigger this action too, but
	|-> you can set it to `null` to make no Button triggers action.

(One-time, Start/Awake/etc)
input.RemoveAction(actionName : string)
	|-> removing the action from the table

(Running)
input.InputAction(key : KeyCode, func_whenKeyDown : (), func_whenKeyUp : ())
	|-> input action where the keycode will call the function based of the key pressed
	|-> when key just pressed down, func_whenKeyDown will be called. But when the key is unpressed
	|-> func_whenKeyUp will be called instead.

Projection:

(Running)
input.GetMousePositionInWorld()
	|-> returning Vector3 in World-Space of mouse position from screen in world, max distance of
	|-> mouse from transform can be changed.

input.GetMousePositionInScreen()
	|-> returning Vector2 in screen of mouse position

dll...

Class-Helpers:

(struct)
InputActionInfo: berisi informasi KeyCode dan tipe key press nya seperti Down, Up, Holding, dll. Jika Holding, maka metode struct yaitu holdTime harus diisi (default 1 detik). 

dll...

Events:

TouchStarted<Vector2 : touch_pos> -> mobile touching screen begin.
TouchHold<Vector2 : touch_pos> -> mobile touching the screen in hold.
TouchPan<Vector2 : touch_pos, float : fingers_count> -> mobile panning the screen with more than one finger.
TouchEnded<Vector2 : touch_pos> -> mobile touching screen ended.
TouchInterrupted -> when touching session is interrupted by pop, or something.
TouchStationary<Vector2 : touch_pos> -> when touching session is still holding but the position haven't changed.
dll...

MouseDownButton<int : button_type, Vector2 : mouse_clickedPos> -> when button of mouse pressed down and begin.
MouseScrolling -> when wheel button is scrolling or hovering.
MouseHolding -> when button of mouse staying to be pressed.
MouseUpButton<int : button_type, Vector2 : mouse_pos> -> when button of mouse unpressed and ended.
dll...

InputKeyDown<key : KeyCode> -> happened when any key is pressed down and begin.
InputKeyStaying<key : KeyCode> -> happened when any key is staying to press the key.
InputKeyUp<key : KeyCode> -> happened when any key is unpressed and ended.
dll...

ThumbstickPressDown<int : thumbstickIndex> -> happened when one of the thumbstick is pressed down and begin.
ThumbstickPressStaying<int : thumbstickIndex> -> happened when one of the thumbsticks is staying to press.
ThumbstickPressUp<int : thumbstickIndex> -> happened when one of the thumbsticks is unpressed and ended.
LeftThumbstickRotating -> happened when left thumbstick is rotating after 360* rotation of the thumbstick.
RightThumbstickRotating -> happened when right thumbstick is rotating after 360* rotation of the thumbstick.
dll...
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

public class HICInputControlScheme
{
    public string SchemeName;

    public HICAxesTable AxesTable;
    public HICActionsTable ActionsTable;

    public HICInputControlScheme(string __schemeName)
    {
        SchemeName = __schemeName;
        AxesTable = new HICAxesTable();
        ActionsTable = new HICActionsTable();
    }   
};
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
            DeadZone = 0.04f,
            Sensitivity = 12.0f,
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

    public KeyCode Key;
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

        Key = KeyCode.None;
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
    public KeyCode Key;
    public float Value;
    public HICInputDeviceType DeviceType;

    public int GamepadIndex;
    public int GamepadAxisIndex;
    public bool IsAnalog;

    public HICInputAxisInfo(string __axisName, KeyCode __key, float __value, HICInputDeviceType __deviceType = HICInputDeviceType.Keyboard)
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

    public KeyCode LastPressedKey;
    public float LastChangedTime;
    public bool ChangedThisFrame;

    public void Reset() {
        RawValue = 0; SmoothValue = 0; ProValue = 0; TargetValue = 0;
        LastRawValue = 0; LastSmoothValue = 0; LastProValue = 0;
        LastPressedKey = KeyCode.None; LastChangedTime = 0; ChangedThisFrame = false;
    }  
};

/// <summary>
/// HumanoidInputController's context or info for BindAction
/// </summary>
public struct HICInputActionParams
{
    public HICInputDeviceType DeviceType;
    public HICInputTriggerType TriggerType;

    public KeyCode Key;

    public int GamepadButtonIndex;
    public int MouseButton;
    public int TouchFingersIndex;

    public float HoldingTime;
    public float TapMaxTime;
    public float DoubleTapMaxDelay;

    public KeyCode KeyCombo;
    public KeyCode[] SequenceKeys;

    public HICInputActionParams Keyboard(KeyCode __key, HICInputTriggerType __triggerType = HICInputTriggerType.Down, float __holdTime = 1.0f)
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

            KeyCombo = KeyCode.None,
            SequenceKeys = null
        };
    }

    public HICInputActionParams Mouse(int __mouseButton, HICInputTriggerType __triggerType = HICInputTriggerType.Down, float __holdTime = 1.0f)
    {
        return new HICInputActionParams
        {
            DeviceType = HICInputDeviceType.Mouse,
            TriggerType = __triggerType,

            Key = KeyCode.None,

            GamepadButtonIndex = -1,
            MouseButton = __mouseButton,
            TouchFingersIndex = -1,

            HoldingTime = __holdTime,
            TapMaxTime = 0.25f,
            DoubleTapMaxDelay = 0.3f,

            KeyCombo = KeyCode.None,
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
    public KeyCode LastKey;

    public void ResetFrameState()
    {
        IsPressedDown = false;
        IsHolding = false;
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
        LastKey = KeyCode.None;
    }
};

public class HICInputActionEntry
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

public struct HICAxisBindingInterface
{
    public string AxisName;
    public KeyCode Key;
    public float Value;  
};

public class HumanoidInputController : MonoBehaviour
{
    #region SerializedPreferences

    [Header("Projection")]
    [SerializeField] private Camera currentCamera;
    [SerializeField] private LayerMask projectionLayer;
    [SerializeField] private float projectionMaxDistance = 100.0f;

    [Header("Touch")]
    [SerializeField] private float touchHoldingThreshold = 2.0f;
    [SerializeField] private float touchTappingMaxTime = 0.35f;
    [SerializeField] private float touchDoubleTapMaxDelay = 0.35f;
    [SerializeField] private float touchSwipeMinDistance = 50.0f;

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

    /* Private helpers*/
    private bool _isUsingExternalAxesTable;
    private bool _isUsingExternalActionsTable;
    #endregion

    #region InputScheme

    private Dictionary<string, HICInputControlScheme> _schemes;
    private string _currentSchemeName;

    #endregion

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

    private int _touchFingerCounts;
    private int _touchBeginFrame = -1;
    private int _touchEndedFrame = -1;

    private bool _touchLongPressFired;
    private bool _touchDoubleTapCandidate;
    private float _lastTouchTapTime;

    #endregion

    #region GamepadCache

    private Vector2 _leftStick;
    private Vector2 _rightStick;
    private Vector2 _previousLeftStick;
    private Vector2 _previousRightStick;

    private float _leftTrigger;
    private float _rightTrigger;

    #endregion

    #region ButtonsCache

    private List<KeyCode> _watchedButtons;

    private Dictionary<KeyCode, bool> _buttonsDown;
    private Dictionary<KeyCode, bool> _buttonsHolding;
    private Dictionary<KeyCode, bool> _buttonsUp;
    private Dictionary<KeyCode, float> _buttonsPressedTime;

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

    public event Action<KeyCode> OnInputKeyDown;
    public event Action<KeyCode> OnInputKeyStaying;
    public event Action<KeyCode> OnInputKeyUp;

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
    public event Action<Vector2, int> OnTouchPan;

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

    #endregion

    #region Methods

    #region TableSetters

    public void SetHICAxesTable(HICAxesTable __table)
    {
        if (__table == null) return;

        _axesTable = __table;
        _isUsingExternalAxesTable = true;
    }

    public void SetHICActionsTable(HICActionsTable __table)
    {
        if (__table == null) return;

        _actionsTable = __table;
        _isUsingExternalActionsTable = true;

        RefreshWatchedKeys();
    }

    #endregion

    #region AxisBinds

    public void BindAxis(string __axisName, KeyCode __key, float __value)
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

    public void UnbindAxis(string __axisName, KeyCode __key)
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

        return __entry.State.ReleasedThisFrame && !__entry.State.Consumed;
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

                Key = KeyCode.None,

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

    private void AddWatchedKey(KeyCode __key)
    {
        if (__key == KeyCode.None)
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
        {
            HICAxisState __param = __pair.Value;
            __param.ChangedThisFrame = false;
            _axesTable.AxisStatesTable[__pair.Key] = __param;
        }
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
            __state.ProValue = ProAxisValueResult(__state.ProValue, __target__, __param.ProGravity, __param.ProSensitivity, __snap_th__);

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
        if (!IsActionBindingDown(__param)) return;

        ReadyPerformAction(__entry, __param);
        PerformingAction(__entry, __param);
    }

    private void Input_HandleAction_Hold(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (IsActionBindingDown(__param))
            ReadyPerformAction(__entry, __param);

        if (IsActionBindingHolding(__param))
        {
            __entry.State.IsHolding = true;
            __entry.State.HoldingDuration = Time.time - __entry.State.BeginTime;

            if (__entry.State.HoldingDuration >= __param.HoldingTime)
                PerformingAction(__entry, __param);
        }

        if (IsActionBindingUp(__param))
            ReleasePerformedAction(__entry, __param);
    }

    private void Input_HandleAction_Up(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (!IsActionBindingUp(__param)) return;

        ReleasePerformedAction(__entry, __param);
        PerformingAction(__entry, __param);
    }

    private void Input_HandleAction_Tap(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (IsActionBindingDown(__param))
            ReadyPerformAction(__entry, __param);

        if (IsActionBindingUp(__param))
        {
            if (Time.time - __entry.State.BeginTime <= __param.TapMaxTime)
                PerformingAction(__entry, __param);

            ReleasePerformedAction(__entry, __param);
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
            ReadyPerformAction(__entry, __param);

        if (IsActionBindingHolding(__param))
        {
            __entry.State.IsHolding = true;
            __entry.State.HoldingDuration = Time.time - __entry.State.BeginTime;

            if (__entry.State.HoldingDuration >= __param.HoldingTime && !__entry.State.PerformedThisFrame)
                PerformingAction(__entry, __param);
        }

        if (IsActionBindingUp(__param))
            ReleasePerformedAction(__entry, __param);
    }

    private void Input_HandleAction_Combo(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        bool __sequenceKeyPressed = __param.KeyCombo == KeyCode.None || IsInputHolding(__param.KeyCombo);

        if (!__sequenceKeyPressed) return;

        if (IsActionBindingDown(__param))
        {
            ReadyPerformAction(__entry, __param);
            PerformingAction(__entry, __param);
        }
    }

    private void Input_HandleAction_Toggle(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (!IsActionBindingDown(__param)) return;

        __entry.State.Toggled = !__entry.State.Toggled;

        ReadyPerformAction(__entry, __param);
        PerformingAction(__entry, __param);
    }

    private void Input_HandleAction_Special(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (__param.SequenceKeys == null || __param.SequenceKeys.Length <= 0)
            return;

        int __index = __entry.State.SequenceIndex;
        KeyCode __expectedKey = __param.SequenceKeys[__index];

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
            KeyCode __susKey = _watchedButtons[__i];

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
                return !_touchActive;

            default:
                return false;
        }
    }

    #endregion

    #region ActionPerforms

    private HICInputActionContext CreateActionContext(HICInputActionEntry __entry, HICInputActionParams __param, HICInputActionStage __stage)
    {
        HICInputActionContext __context = new HICInputActionContext(__entry.ActionName, __param.DeviceType, __stage);
        
        __context.TriggerType = __param.TriggerType;

        __context.Key = __param.Key;
        __context.MouseButton = __param.MouseButton;
        __context.GamepadButtonIndex = __param.GamepadButtonIndex;

        __context.WorldInputPosition = GetMousePositionInWorld();
        __context.ScreenInputPosition = GetMousePositionInScreen();
        
        __context.LastTimePressed = Time.time;
        __context.HoldingTime = __entry.State.HoldingDuration;
        __context.Consumed = __entry.State.Consumed;

        return __context;
    }

    private void ReadyPerformAction(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        __entry.State.IsPressedDown = true;
        __entry.State.IsHolding = true;
        __entry.State.BeginTime = Time.time;
        __entry.State.Stage = HICInputActionStage.Begin;
        __entry.State.LastDeviceType = __param.DeviceType;
        __entry.State.LastKey = __param.Key;

        HICInputActionContext __context = CreateActionContext(__entry, __param, HICInputActionStage.Begin);

        OnActionBegin?.Invoke(__context);

        SetCurrentDevice(__param.DeviceType);
    }

    private void PerformingAction(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        if (__entry.State.Consumed) return;

        __entry.State.PerformedThisFrame = true;
        __entry.State.LastPerformedTime = Time.time;
        __entry.State.Buffered = true;
        __entry.State.Stage = HICInputActionStage.Performed;

        HICInputActionContext __context = CreateActionContext(__entry, __param, HICInputActionStage.Performed);

        __entry.Callbacks?.Invoke(__context);
        OnActionPerformed?.Invoke(__context);

        if (__entry.ConsumeOnPerformed)
            __entry.State.Consumed = true;
    }

    private void ReleasePerformedAction(HICInputActionEntry __entry, HICInputActionParams __param)
    {
        __entry.State.IsHolding = false;
        __entry.State.IsReleasedUp = true;
        __entry.State.ReleasedThisFrame = true;
        __entry.State.ReleasedTime = Time.time;
        __entry.State.HoldingDuration = Time.time - __entry.State.BeginTime;
        __entry.State.Stage = HICInputActionStage.Released;

        HICInputActionContext __context = CreateActionContext(__entry, __param, HICInputActionStage.Released);

        OnActionReleased?.Invoke(__context);
    }

    private void CancelPerformedAction(HICInputActionEntry __entry)
    {
        if (!__entry.State.IsHolding) return;

        __entry.State.IsHolding = false;
        __entry.State.Stage = HICInputActionStage.Cancelled;

        HICInputActionContext __context = new HICInputActionContext(__entry.ActionName, __entry.State.LastDeviceType, HICInputActionStage.Cancelled);

        OnActionCancelled?.Invoke(__context);
    }

    private void UpdateActionHoldingEvent(HICInputActionEntry __entry)
    {
        if (__entry.State.IsHolding) return;

        __entry.State.HoldingDuration = Time.time - __entry.State.BeginTime;
        __entry.State.Stage = HICInputActionStage.Holding;

        HICInputActionContext __context = new HICInputActionContext(__entry.ActionName, __entry.State.LastDeviceType, HICInputActionStage.Holding);

        __context.Key = __entry.State.LastKey;
        __context.HoldingTime = __entry.State.HoldingDuration;
        __context.LastTimePressed = Time.time;

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

        KeyCode __lastPressedKey = __state.LastPressedKey;

        for (int i = 0; i < __bindings.Count; i++)
        {
            HICInputAxisInfo __info = __bindings[i];

            bool __isPressing = IsBindingPressed(__info);

            if (!__isPressing)
                return 0f;

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
            KeyCode __current_key = _watchedButtons[i];

            bool __isKeyDown = Input.GetKeyDown(__current_key);
            bool __isKeyHolding = Input.GetKey(__current_key);
            bool __isKeyUp = Input.GetKeyUp(__current_key);

            _buttonsDown[__current_key] = __isKeyDown;
            _buttonsHolding[__current_key] = __isKeyHolding;
            _buttonsUp[__current_key] = __isKeyUp;

            if (__isKeyDown)
            {
                _buttonsPressedTime[__current_key] = Time.time;
                SetCurrentDevice(GetKeyOrGPDevice(__current_key));
                OnInputKeyDown?.Invoke(__current_key);
            }

            if (__isKeyHolding)
            {
                SetCurrentDevice(GetKeyOrGPDevice(__current_key));
                OnInputKeyStaying?.Invoke(__current_key);
            }

            if (__isKeyUp)
            {
                SetCurrentDevice(GetKeyOrGPDevice(__current_key));
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

    private bool IsBindingUp(HICInputAxisInfo __info)
    {
        if (IsInputUp(__info.Key))
            return true;

        return false;
    }

    private bool IsInputHolding(KeyCode __key)
    {
        bool __isInputValid = __key != KeyCode.None && _buttonsHolding.TryGetValue(__key, out bool __result) && __result;

        return __isInputValid;
    }

    private bool IsInputDown(KeyCode __key)
    {
        bool __isInputValid = __key != KeyCode.None && _buttonsDown.TryGetValue(__key, out bool __result) && __result;

        return __isInputValid;
    }   

    private bool IsInputUp(KeyCode __key)
    {
        bool __isInputValid = __key != KeyCode.None && _buttonsUp.TryGetValue(__key, out bool __result) && __result;

        return __isInputValid;
    }

    private bool IsGamepadDown(int __gamepadIndex)
    {
        KeyCode __index = KeyCode.JoystickButton0 + __gamepadIndex;
        return IsInputDown(__index);
    }

    private bool IsGamepadHolding(int __gamepadIndex)
    {
        KeyCode __index = KeyCode.JoystickButton0 + __gamepadIndex;
        return IsInputHolding(__index);
    }

    private bool IsGamepadUp(int __gamepadIndex)
    {
        KeyCode __index = KeyCode.JoystickButton0 + __gamepadIndex;
        return IsInputUp(__index);
    }

    private HICInputDeviceType GetKeyOrGPDevice(KeyCode __key)
    {
        if (__key >= KeyCode.JoystickButton0 && __key <= KeyCode.JoystickButton19)
            return HICInputDeviceType.Gamepad;

        return HICInputDeviceType.Keyboard;
    }   

    private float ResolveCurrentPressedAxis(List<HICInputAxisInfo> __info, KeyCode __key)
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
    
    private void UpdateMouseCache()
    {
        _previous_mousePos_screen = _mousePos_screen;
        _mousePos_screen = Input.mousePosition;
        _mouseDelta = _mousePos_screen - _previous_mousePos_screen;

        _scroll_mouseDelta = Input.mouseScrollDelta.y;

        for (int __i = 0; __i < 3; __i++)
        {
            _mouseDown[__i] = Input.GetMouseButtonDown(__i);
            _mouseHolding[__i] = Input.GetMouseButton(__i);
            _mouseUp[__i] = Input.GetMouseButtonUp(__i);

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
        HICPointerToWorldInfo __pointerInfo = new HICPointerToWorldInfo();

        __pointerInfo.ScreenPosition = _mousePos_screen;
        __pointerInfo.Ray = GetMouseRay();

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

    private int GetBeginTouchFrame()
    {
        return _touchBeginFrame;
    }

    private void UpdateTouchCache()
    {
        if (Input.touchCount <= 0)
        {
            if (_touchActive)
                Debug.Log("CONTINUE HERE...");

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
                break;

            case TouchPhase.Moved:
                break;

            case TouchPhase.Canceled:
                break;

            case TouchPhase.Stationary:
                break;

            case TouchPhase.Ended:
                break;
        }

        if (Input.touchCount >= 2)
            Debug.Log("CONTINUE HERE....");
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

    #endregion

    #region UnityHelpers

    private void Awake()
    {
        if (_axesTable == null)
            _axesTable = new HICAxesTable();

        if (_actionsTable == null)
            _actionsTable = new HICActionsTable();

        _schemes = new Dictionary<string, HICInputControlScheme>(10);

        _inputLocks = new HashSet<string>();
        _inputContexts = new Stack<string>();

        _lastVector2Axis2D = new Dictionary<string, Vector2>(16);

        _buttonCallbacks = new Dictionary<Button, UnityAction>(64);

        _usedDevice = HICInputDeviceType.Unknown;
        _previousUsedDevice = HICInputDeviceType.Unknown;

        currentCamera = currentCamera != null ? currentCamera : GetComponent<Camera>();
        
        _mouseDown = new bool[3];
        _mouseHolding = new bool[3];
        _mouseUp = new bool[3];

        _watchedButtons = new List<KeyCode>(128);
        _buttonsDown = new Dictionary<KeyCode, bool>(128);
        _buttonsHolding = new Dictionary<KeyCode, bool>(128);
        _buttonsUp = new Dictionary<KeyCode, bool>(128);
        _buttonsPressedTime = new Dictionary<KeyCode, float>(128);
    }

    private void Update()
    {
        FrameBegin();

        UpdateMouseCache();
        UpdateKeyCache();
        UpdateTouchCache();
        
        UpdateActions();
        HandleAxes();

        UpdateMouseProjection();

        FrameEnd();
    }


    #endregion

}
