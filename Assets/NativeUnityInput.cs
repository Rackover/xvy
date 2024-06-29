using UnityEngine;
using System.Collections;

public class NativeUnityInput : PlayerInput {

    int playerIndex = 0;

    public override bool GamepadPresent()
    {

        return Input.GetJoystickNames().Length > playerIndex;
    }

    public override bool AnyKey()
    {
        return (this as PlayerInput).IsPressingStart();
    }

    public override void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }

    public override bool AButton()
    {
        return Input.GetButton("Fire" + playerIndex);
    }

    public override float LeftTrigger()
    {
        return Input.GetAxis("LeftTrigger"+playerIndex);
    }

    public override float RightTrigger()
    {
        return Input.GetAxis("RightTrigger" + playerIndex);
    }

    public override bool IsPressingStart()
    {
        return Input.GetButton("Start" + playerIndex);
    }

    public override void Refresh()
    {
    }

    public override UnityEngine.Vector2 GetDirection()
    {
        return new Vector2(Input.GetAxis("Horizontal" + playerIndex), Input.GetAxis("Vertical" +playerIndex));
    }
}
