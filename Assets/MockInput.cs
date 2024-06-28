using UnityEngine;
using System.Collections;

public class MockInput : IPlayerInput {

    int playerIndex = 0;

    bool IPlayerInput.GamepadPresent()
    {
        return Time.time > 2.5f + playerIndex;
    }

    bool IPlayerInput.AnyKey()
    {
        return Time.time > 3f + playerIndex;
    }

    void IPlayerInput.SetPlayerIndex(int index)
    {
        playerIndex = index;
    }

    float IPlayerInput.LeftTrigger()
    {
        return 0f;
    }

    float IPlayerInput.RightTrigger()
    {
        return 0f;
    }

    bool IPlayerInput.IsPressingStart()
    {
        return Time.time > 5f + playerIndex;
    }

    void IPlayerInput.Refresh()
    {

    }

    Vector2 IPlayerInput.GetDirection()
    {
        return new Vector2(Mathf.Sin(Time.time + playerIndex), Mathf.Cos(Time.time + playerIndex));
    }

    void System.IDisposable.Dispose()
    {

    }
}
