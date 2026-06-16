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

public interface IInputAxes
{
    float GetAxis(string axisName);
    void AddAxis(string axisName, float value);
    void RemoveAxis(string axisName);
    bool IsAxisExisted(string axisName);
    void ClearAxes();
};

public sealed class DefaultInputAxesStorage : IInputAxes
{
    private readonly Dictionary<string, float> __axeses = new Dictionary<string, float>();

    public float GetAxis(string axisName)
    {
        return __axeses.TryGetValue(axisName, out float value) ? value : 0f;
    }

    public void AddAxis(string axisName, float value)
    {
        value = Mathf.Clamp(value, -1.0f, 1.0f);
        __axeses.Add(axisName, value);
    }

    public void RemoveAxis(string axisName)
    {
        __axeses.Remove(axisName);
    }

    public bool IsAxisExisted(string axisName)
    {
        return __axeses.ContainsKey(axisName);
    }

    public void ClearAxes()
    {
        __axeses.Clear();
    }
}

public class HumanoidInputController : MonoBehaviour
{

    private IInputAxes _axisStorage;
    public void SetAxisStorage(IInputAxes __axesStorage)
    {
        _axisStorage = __axesStorage ?? new DefaultInputAxesStorage();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
