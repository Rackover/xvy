using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NativeUnityInput : PlayerInput {

    int playerIndex = 0;

    private readonly Dictionary<NativeUnityInputMappings.GamepadInput, float> inputStates = new Dictionary<NativeUnityInputMappings.GamepadInput, float>();

    public NativeUnityInput()
    {
        for (int i = 0; i < Enum.GetValues(typeof(NativeUnityInputMappings.GamepadInput)).Length; i++)
        {
            inputStates[(NativeUnityInputMappings.GamepadInput)i] = 0f;
        }
    }

    protected override void SetVibration(float leftValue, float rightValue)
    {
#if X360
        X360Core.SetControllerVibration((uint)playerIndex, leftValue, rightValue);
#else

        base.SetVibration(leftValue, rightValue);
#endif
    }

    public override Vector2 GetDPad()
    {
        return new Vector2(inputStates[NativeUnityInputMappings.GamepadInput.DPadX], inputStates[NativeUnityInputMappings.GamepadInput.DPadY]);
    }

    public override bool GamepadPresent()
    {
        return Input.GetJoystickNames().Length > playerIndex;
    }

    public override bool AnyKey()
    {
        foreach(var val in inputStates.Values)
        {
            if (Mathf.Abs(val) > 0.5f)
            {
                return true;
            }
        }

        return AButton() || IsPressingStart();
    }

    public override void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }

    public override bool AButton()
    {
#if !UNITY_XBOX360
        return Input.GetKey(playerIndex == 0 ? KeyCode.Joystick1Button0 : KeyCode.Joystick2Button0);
#else
        return Input.GetKey(playerIndex == 0 ? KeyCode.JoystickButton0 : KeyCode.Joystick1Button0);
#endif
    }

    public override float LeftTrigger()
    {
        return inputStates[NativeUnityInputMappings.GamepadInput.LeftTrigger];
    }

    public override float RightTrigger()
    {
        return inputStates[NativeUnityInputMappings.GamepadInput.RightTrigger];
    }

    public override bool IsPressingStart()
    {
#if !UNITY_XBOX360
        return Input.GetKey(playerIndex == 0 ? KeyCode.Joystick1Button7 : KeyCode.Joystick2Button7);
#else
        return Input.GetKey(playerIndex == 0 ? KeyCode.JoystickButton7 : KeyCode.Joystick1Button7);
#endif
    }

    public override void Refresh()
    {
        for (int i = 0; i < NativeUnityInputMappings.axes.Length; i++)
        {
            string name = NativeUnityInputMappings.GetVirtualInputName(NativeUnityInputMappings.axes[i].name, playerIndex);
            inputStates[NativeUnityInputMappings.axes[i].name] = Input.GetAxis(name);
        }

        for (int i = 0; i < NativeUnityInputMappings.buttons.Length; i++)
        {
            string name = NativeUnityInputMappings.GetVirtualInputName(NativeUnityInputMappings.buttons[i].name, playerIndex);

            inputStates[NativeUnityInputMappings.buttons[i].name] = 
                Input.GetButton(name) ? 1f : 0f;
        }
    }

    public override UnityEngine.Vector2 GetDirection()
    {
        return new Vector2(inputStates[NativeUnityInputMappings.GamepadInput.ThumbLX], inputStates[NativeUnityInputMappings.GamepadInput.ThumbLY]);
    }
}
