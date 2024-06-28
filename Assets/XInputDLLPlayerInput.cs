using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class XInputDLLPlayerInput : IPlayerInput
{
    PlayerIndex index = 0;

    GamePadState state;

    public bool GamepadPresent()
    {
        return state.IsConnected;
    }

    public bool AnyKey()
    {
        return (state.Buttons.A | state.Buttons.B | state.Buttons.X | state.Buttons.Y | state.Buttons.Start | state.Buttons.Back) != 0;
    }

    public void Refresh()
    {
        state = GamePad.GetState(index);
    }

    public Vector2 GetDirection()
    {
        return new Vector2(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y);
    }

    public bool IsPressingStart()
    {
        return state.Buttons.Start == ButtonState.Pressed;
    }

    public float LeftTrigger()
    {
        return state.Triggers.Left;
    }

    public float RightTrigger()
    {
        return state.Triggers.Right;
    }

    public void SetPlayerIndex(int index)
    {
        this.index = (PlayerIndex)index;
    }

    public void Dispose()
    {
        GamePad.SetVibration(index, 0f, 0F);
    }
}
