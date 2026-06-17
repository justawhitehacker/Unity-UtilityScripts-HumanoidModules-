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
using UnityEngine.UI;

public class HICAxesTable
{
    public Dictionary<string, List<HICInputAxisInfo>> AxisBindsTable;
    public Dictionary<string, HICAxisParams> AxisSettingsTable;
    public Dictionary<string, HICAxisState> AxisStatesTable;
    public Dictionary<string, HICInputAxis2Info> Axis2DInfoTable;

    public HICAxesTable()
    {
        AxisBindsTable = new Dictionary<string, List<HICInputAxisInfo>>(32);
        AxisSettingsTable = new Dictionary<string, HICAxisParams>(32);
        AxisStatesTable = new Dictionary<string, HICAxisState>(32);
        Axis2DInfoTable = new Dictionary<string, HICInputAxis2Info>(16);
    }
};

public class HICActionsTable
{
    public Dictionary<string, HICInputActionEntry> ActionEntries;

    public HICActionsTable()
    {
        ActionEntries = new Dictionary<string, HICInputActionEntry>(32);
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
    /// The size of joysticks center deadzone where input is ignored
    /// </summary>
    public float DeadZone;
    /// <summary>
    /// The speed for how axis become 1 or -1 when input pressed or happened
    /// </summary>
    public float Sensitivity;
    /// <summary>
    /// Sets axis to 0 immediately if an opposing button is hit
    /// </summary>
    public bool Snap;
    /// <summary>
    /// Delay/Threshold time before the axis sets to 0
    /// </summary>
    public float SnapThreshold;
    /// <summary>
    /// Inverting axis from the input, like 1 to -1 and -1 to 1
    /// </summary>
    public bool InvertAxis;
    /// <summary>
    /// Sets how input axis will handle inputs conflict
    /// </summary>
    public HICInputConflictType ConflictType;

    public HICAxisParams(float __gravity, float __deadZone, float __sensitivity, bool __snapping, float __snapThreshold, HICInputConflictType __conflictType, bool __inverted = false)
    {
        Gravity = __gravity;
        DeadZone = __deadZone;
        Sensitivity = __sensitivity;
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
    public float TargetValue;

    public float LastRawValue;
    public float LastSmoothValue;

    public KeyCode LastPressedKey;
    public float LastChangedTime;
    public bool ChangedThisFrame;

    public void Reset() {
        RawValue = 0; SmoothValue = 0; TargetValue = 0;
        LastRawValue = 0; LastSmoothValue = 0;
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

    #endregion

    #region PrivateMembers

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

    #region MouseCache

    private HICPointerToWorldInfo _projectionInfoCache;

    private Vector2 _mousePos_screen;
    private Vector2 _previous_mousePos_screen;
    private Vector2 _mouseDelta;

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

    #region KeysCache

    private List<KeyCode> _watchedKeys;

    private Dictionary<KeyCode, bool> _keyDown;
    private Dictionary<KeyCode, bool> _keyHolding;
    private Dictionary<KeyCode, bool> _keyUp;
    private Dictionary<KeyCode, float> _keyPressedTime;

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

    #endregion

    #region Axis

    public event Action<string, float, float> OnAxisChanged;
    public event Action<string, Vector2, Vector2> OnAxis2Changed;

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
    }

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
        DoubleCheckAxisSettings(__binding.AxisName);
        DoubleCheckAxisState(__binding.AxisName);

        AddWatchedKey(__binding.Key);
    }

    #endregion

    #endregion

    #region Handlers

    #region KeysCache

    private void AddWatchedKey(KeyCode __key)
    {
        if (__key == KeyCode.None)
            return;

        if (_keyHolding.ContainsKey(__key))
            return;

        _watchedKeys.Add(__key);

        _keyDown[__key] = false;
        _keyHolding[__key] = false;
        _keyUp[__key] = false;
        _keyPressedTime[__key] = 0f;
    }

    #endregion

    #region DoubleCheckHandlers

    public void DoubleCheckAxisSettings(string __axisName)
    {
        if (!_axesTable.AxisSettingsTable.ContainsKey(__axisName))
            _axesTable.AxisSettingsTable.Add(__axisName, HICAxisParams.Default());
    }

    public void DoubleCheckAxisState(string __axisName)
    {
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
            __entry.State.Reset();
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

        _usedDevice = HICInputDeviceType.Unknown;
        _previousUsedDevice = HICInputDeviceType.Unknown;

        currentCamera = currentCamera != null ? currentCamera : GetComponent<Camera>();
        
        _mouseDown = new bool[3];
        _mouseHolding = new bool[3];
        _mouseUp = new bool[3];

        _watchedKeys = new List<KeyCode>(72);
        _keyDown = new Dictionary<KeyCode, bool>(72);
        _keyHolding = new Dictionary<KeyCode, bool>(72);
        _keyUp = new Dictionary<KeyCode, bool>(72);
        _keyPressedTime = new Dictionary<KeyCode, float>(72);
    }

    private void Update()
    {
        FrameBegin();

        FrameEnd();
    }


    #endregion

}
