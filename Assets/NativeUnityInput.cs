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

    public override bool GamepadPresent()
    {
        return Input.GetJoystickNames().Length > playerIndex;
    }

    public override bool AnyKey()
    {
        foreach(var val in inputStates.Values)
        {
            if (val != 0f)
            {
                return true;
            }
        }

        return false;
    }

    public override void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }

    public override bool AButton()
    {
        return inputStates[NativeUnityInputMappings.GamepadInput.A] > 0f;
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
        return inputStates[NativeUnityInputMappings.GamepadInput.Start] > 0f;
    }

    public override void Refresh()
    {
        for (int i = 0; i < NativeUnityInputMappings.axes.Length; i++)
        {
            inputStates[NativeUnityInputMappings.axes[i].name] = Input.GetAxis(NativeUnityInputMappings.GetVirtualInputName(NativeUnityInputMappings.axes[i].name, playerIndex));
        }

        for (int i = 0; i < NativeUnityInputMappings.buttons.Length; i++)
        {
            inputStates[NativeUnityInputMappings.buttons[i].name] = 
                Input.GetButton(NativeUnityInputMappings.GetVirtualInputName(NativeUnityInputMappings.buttons[i].name, playerIndex)) ? 1f : 0f;
        }
    }

    public override UnityEngine.Vector2 GetDirection()
    {
        return new Vector2(inputStates[NativeUnityInputMappings.GamepadInput.ThumbLX], inputStates[NativeUnityInputMappings.GamepadInput.ThumbLY]);
    }
}
