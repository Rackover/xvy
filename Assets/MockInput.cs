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
        return Time.time > 3f + playerIndex;
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
        return Mathf.Sin(Time.time) > 0.5f && Random.value > 0.1f;
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
        return new Vector2(Mathf.Sin(Time.time + playerIndex), Mathf.Cos(Time.time + playerIndex));
    }    
}
