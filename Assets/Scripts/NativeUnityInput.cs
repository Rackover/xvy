﻿using UnityEngine;
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

    public override bool IsPressingRS()
    {
#if !UNITY_XBOX360
        return Input.GetKey(playerIndex == 0 ? KeyCode.Joystick1Button9 : KeyCode.Joystick2Button9);
#else
        return Input.GetKey(playerIndex == 0 ? KeyCode.JoystickButton9 : KeyCode.Joystick1Button9);
#endif
    }

    protected override void SetVibration(float leftValue, float rightValue)
    {
#if UNITY_EDITOR

#elif UNITY_XENON
        X360Core.SetControllerVibration((uint)playerIndex, leftValue, rightValue);
#elif UNITY_PS3
        PS3Pad.SetVibration(playerIndex, leftValue > 0f, rightValue);
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
#if UNITY_EDITOR
        return Input.GetJoystickNames().Length > playerIndex;
#elif UNITY_PS3
        return PS3Pad.IsReady(playerIndex);
#elif UNITY_XENON
        return X360Core.GetTotalSignedInUsers() > playerIndex;
        return X360Core.GetUserLocalPlayerId((uint)playerIndex).IsValid;
#else
        return Input.GetJoystickNames().Length > playerIndex;
#endif
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

    public override bool IsPressingSelect()
    {
#if !UNITY_XBOX360
        return Input.GetKey(playerIndex == 0 ? KeyCode.Joystick1Button6 : KeyCode.Joystick2Button6);
#else
        return Input.GetKey(playerIndex == 0 ? KeyCode.JoystickButton6 : KeyCode.Joystick1Button6);
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
