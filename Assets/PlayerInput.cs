using UnityEngine;
using System.Collections;
using System;
using System.Text;

public abstract class PlayerInput : IDisposable
{
    private Coroutine currentRumbleRoutine = null;

    public abstract bool GamepadPresent();

    public abstract bool AnyKey();

    public abstract void SetPlayerIndex(int index);

    public abstract float LeftTrigger();

    public abstract float RightTrigger();

    public abstract bool AButton();

    public abstract bool IsPressingStart();

    public abstract bool IsPressingSelect();

    public abstract void Refresh();

    public abstract Vector2 GetDirection();

    public virtual Vector2 GetDPad() { return Vector2.zero; }

    protected virtual void SetVibration(float leftValue, float rightValue) { }

    public static PlayerInput MakeForPlatform()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return new XInputKernel();
#else
        return new NativeUnityInput();
#endif
    }

    public string Dump()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Connected: " + GamepadPresent());
        builder.AppendLine("Start: " + IsPressingStart());
        builder.AppendLine("Select: " + IsPressingSelect());
        builder.AppendLine("LStick: " + GetDirection());
        builder.AppendLine("DPad: " + GetDPad());
        builder.AppendLine("A: " + AButton());
        builder.AppendLine("LT: " + LeftTrigger().ToString("n0"));
        builder.AppendLine("RT: " + RightTrigger().ToString("n0"));
        builder.AppendLine("AnyKey: " + AnyKey());

        return builder.ToString();
    }

    public virtual void Dispose()
    {

    }

    public void RumbleHeavyOnce()
    {
        RumbleForSecondsRoutine(isLight: false, seconds: 0.2f);
    }

    public void RumbleLightOnce()
    {
        RumbleForSeconds(isLight: true, seconds: 0.1f);
    }

    public void RumbleForSeconds(bool isLight, float seconds)
    {
        if (currentRumbleRoutine != null)
        {
            Game.i.StopCoroutine(currentRumbleRoutine);
        }

        currentRumbleRoutine = Game.i.StartCoroutine(RumbleForSecondsRoutine(isLight, seconds));
    }

    private IEnumerator RumbleForSecondsRoutine(bool isLight, float seconds)
    {
        const float LIGHT = 0.3f;
        const float HEAVY = 1f;

        SetVibration(isLight ? LIGHT : HEAVY, isLight ? LIGHT : HEAVY);
        yield return new WaitForSeconds(seconds);
        SetVibration(0f, 0F);
    }
}
