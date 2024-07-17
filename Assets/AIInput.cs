using UnityEngine;

public class AIInput : PlayerInput
{
    private int OtherPlayer { get { return 1 - playerIndex; } }

    private int playerIndex = 0;


    public override bool GamepadPresent()
    {
        return true;
    }

    public override bool AnyKey()
    {
        return Game.i && Game.i.InGame;
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
        Vector2 alignment = GetHomingInputs();

        if (alignment.sqrMagnitude < 0.2f)
        {
            PlayerWeapon weap = Game.i.Level.GetPlayerWeapon(playerIndex);
            if (weap)
            {
                if (weap.TargetAcquired)
                {
                    return true;
                }
                else if (weap.IsAcquiring && weap.Acquisition01 > 0.2f)
                {
                    return false;
                }
            }

            return false;
        }

        return false;
    }

    public override float RightTrigger()
    {
        return Game.i && Game.i.Level.IsPlayerBeingHomedTo(playerIndex) ? 1f : 0f;
    }

    public override bool IsPressingStart()
    {
        return Game.i && !Game.i.Playing;
    }

    public override void Refresh()
    {
        
    }

    public override UnityEngine.Vector2 GetDirection()
    {
        if (Game.i && Game.i.Playing)
        {
            return GetHomingInputs();
        }

        return Vector2.zero;
    }

    private Vector2 GetHomingInputs()
    {
        if (Game.i.Level)
        {
            Transform myTransform = Game.i.Level.GetPlayerTransform(playerIndex);

            Transform theirTransform = Game.i.Level.GetPlayerTransform(OtherPlayer);

            if (myTransform && theirTransform)
            {
                Vector3 directionVector = (theirTransform.position - myTransform.position).normalized;

                float x, y;
                {
                   Quaternion rot =  Quaternion.FromToRotation(directionVector, myTransform.forward);

                    float flatAngle = rot.eulerAngles.y;

                    if (flatAngle > 180f)
                    {
                        flatAngle = -(180f - flatAngle % 180f);
                    }

                    x = Mathf.Clamp(- flatAngle / 90f, -1f, 1f);
                }
                {
                    y = Mathf.Clamp01(directionVector.y - myTransform.forward.y);
                }

                return new Vector2(x, y);
            }
        }

        return Vector2.zero;
    }

    public override bool IsPressingSelect()
    {
        return false;
    }
}
