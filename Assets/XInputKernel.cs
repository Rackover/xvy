using UnityEngine;
using System.Collections;
using XInputBindings;
using System;


public class XInputKernel : PlayerInput
{
    private uint index = 0;

    public override bool AnyKey()
    {
        return XInputKernelBindings.GetButton(index, XInputButton.A);
    }

    public override bool GamepadPresent()
    {
        return XInputKernelBindings.IsControllerConnected(index);
    }

    public override UnityEngine.Vector2 GetDPad()
    {
        return new UnityEngine.Vector2(
            XInputKernelBindings.GetButton(index, XInputButton.DPadRight) ? 1f : (XInputKernelBindings.GetButton(index, XInputButton.DPadLeft) ? -1f : 0f),
            XInputKernelBindings.GetButton(index, XInputButton.DPadUp) ? 1f : (XInputKernelBindings.GetButton(index, XInputButton.DPadLeft) ? -1f : 0f)
        );
    }

    public override bool AButton()
    {
        return XInputKernelBindings.GetButton(index, XInputButton.A);
    }

    public override UnityEngine.Vector2 GetDirection()
    {
        XInputBindings.Vector2 input = XInputKernelBindings.GetThumbStickLeft(index);
        return new UnityEngine.Vector2(input.x, input.y);
    }

    public override bool IsPressingStart()
    {
        return XInputKernelBindings.GetButton(index, XInputButton.Start);
    }

    public override float LeftTrigger()
    {
        return XInputKernelBindings.GetTriggerLeft(index);
    }

    protected override void SetVibration(float leftValue, float rightValue)
    {
        XInputKernelBindings.SetVibration(index, leftValue, rightValue);
    }

    public override void Refresh()
    {

    }

    public override float RightTrigger()
    {
        return XInputKernelBindings.GetTriggerRight(index);
    }

    public override void SetPlayerIndex(int index)
    {
        this.index = (uint)index;
    }
}
