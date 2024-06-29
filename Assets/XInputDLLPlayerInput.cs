using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class XInputDLLPlayerInput : PlayerInput
{
    PlayerIndex index = 0;

    GamePadState state;

    public override bool GamepadPresent()
    {
        return state.IsConnected;
    }

    public override bool AnyKey()
    {
        return (state.Buttons.A | state.Buttons.B | state.Buttons.X | state.Buttons.Y | state.Buttons.Start | state.Buttons.Back) != 0;
    }

    public override bool AButton()
    {
        return state.Buttons.A == ButtonState.Pressed;
    }

    public override void Refresh()
    {
        state = GamePad.GetState(index);
    }

    public override Vector2 GetDirection()
    {
        return new Vector2(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y);
    }

    public override bool IsPressingStart()
    {
        return state.Buttons.Start == ButtonState.Pressed;
    }

    public override float LeftTrigger()
    {
        return state.Triggers.Left;
    }

    public override float RightTrigger()
    {
        return state.Triggers.Right;
    }

    public override void SetPlayerIndex(int index)
    {
        this.index = (PlayerIndex)index;
    }

    public override void Dispose()
    {
        GamePad.SetVibration(index, 0f, 0F);
        base.Dispose();
    }
}
