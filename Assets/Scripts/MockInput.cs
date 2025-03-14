using UnityEngine;
using System.Collections;


public class MockInput : PlayerInput {

    int playerIndex = 0;

    public override bool GamepadPresent()
    {
        return Time.time > 2.5f + playerIndex;
    }

    public override bool AnyKey()
    {
        return Time.time > 6f + playerIndex;
    }

    public override void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }

    public override float LeftTrigger()
    {
        return 0f;
    }

    public override bool AButton()
    {
        return false;
    }

    public override float RightTrigger()
    {
        return 0f;
    }

    public override bool IsPressingStart()
    {
        return Time.time > 5f + playerIndex;
    }

    public override void Refresh()
    {

    }

    public override UnityEngine.Vector2 GetDirection()
    {
        return new Vector2(Mathf.Sin(Time.time + playerIndex), Mathf.Clamp01(Mathf.Cos(Time.time + playerIndex) + 0.4f));
    }

    public override bool IsPressingSelect()
    {
        return false;
    }
}
