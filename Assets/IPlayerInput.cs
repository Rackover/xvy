using UnityEngine;
using System.Collections;
using System;

public interface IPlayerInput : IDisposable
{
    bool GamepadPresent();

    bool AnyKey();

    void SetPlayerIndex(int index);

    float LeftTrigger();

    float RightTrigger();

    bool IsPressingStart();

    void Refresh();

    Vector2 GetDirection();
}
