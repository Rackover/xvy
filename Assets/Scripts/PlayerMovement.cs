﻿using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    Player player;

    [SerializeField] private float minBoost = 9f;
    [SerializeField] private float maxBoost = 40f;
    [SerializeField] private float kickBoostBonus = 10f;
    [SerializeField] private float boostAcceleration = 5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float verticalStickMultiplier = -1f;

    [SerializeField]
    private float angleCap = 75f;

    [SerializeField]
    private float rollLerpSpeed = 7f;

    [SerializeField]
    private float pitchLerpSpeed = 4f;

    [SerializeField]
    private float kickBoostTime = 0.5f;

    [SerializeField]
    private float lethalGees = 3f;

    [SerializeField]
    private float noEffectUntilGees = 1f;

    [SerializeField]
    private float minimumDeltaToGetGeesDegPerSec = 60f;

    public float GeesAmount01 { get { return (gees - noEffectUntilGees) / (lethalGees - noEffectUntilGees); } }

    public float MaxBoostAdjusted { get { return maxBoost * Game.i.Level.SpeedMultiplier; } }

    public float MinBoostAdjusted { get { return minBoost * Game.i.Level.SpeedMultiplier; } }

    public float KickBoostBonusAdjusted { get { return kickBoostBonus * Game.i.Level.SpeedMultiplier; } }

    public float Speed { get { return speed; } }

    public float KickAmount { get { return startedBoostingAtTime.HasValue ? Mathf.Clamp01(1f - (Time.time - startedBoostingAtTime.Value) / kickBoostTime) : 0f; } }

    public float BoostAmount { get { return (speed - MinBoostAdjusted) / (MaxBoostAdjusted - MinBoostAdjusted); } }

    public float SpeedAmount { get { return speed / MaxBoostAdjusted; } }

    public Vector2 VirtualJoystick { get { return virtualStick; } }

    public float MaxDownwardsAngle { get { return angleCap; } }

    private float speed;
    private float gasPedal;
    private Vector2 virtualStick;
    private Vector2 realStick;
    private float? startedBoostingAtTime = 0f;
    private Quaternion lastRotation;

#if UNITY_EDITOR
    [SerializeField]
#endif
    private float gees = 0f;

    void Awake()
    {
        player.OnBirthed += Player_OnSpawn;
    }

    private void Player_OnSpawn(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        transform.forward = direction;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        if (player.IsAlive)
        {
            GrabInput();

            lastRotation = transform.rotation;
            ApplyDirection();
            ApplyThrust();
            ApplyGees();
        }
        else
        {
            gees = 0f;
            virtualStick = Vector2.zero;
            realStick = Vector2.zero;
            gasPedal = 0f;
        }
    }

    void GrabInput()
    {
        if (startedBoostingAtTime.HasValue && !player.IsBoosting)
        {
            startedBoostingAtTime = null;
        }
        else if (!startedBoostingAtTime.HasValue && player.IsBoosting)
        {
            startedBoostingAtTime = Time.time;
        }

        float gasAxis = player.IsBoosting ? 1f : 0f;


        gasPedal = Mathf.Lerp(gasPedal, gasAxis, boostAcceleration * Time.deltaTime);


        realStick.x = player.StickDirection.x;
        realStick.y = verticalStickMultiplier * player.StickDirection.y;

        virtualStick.x = Mathf.Lerp(virtualStick.x, realStick.x, rollLerpSpeed * Time.deltaTime);
        virtualStick.y = Mathf.Lerp(virtualStick.y, realStick.y, pitchLerpSpeed * Time.deltaTime);

        if (realStick.y > virtualStick.y && player.IsBoosting && transform.forward.y < -0.3f)
        {
            // giving them a hand
            virtualStick.y = Mathf.Lerp(virtualStick.y, realStick.y, pitchLerpSpeed * Time.deltaTime * 0.3f);
        }

    }

    void ApplyThrust()
    {
        float kickSpeedBonus = 0f;
        if (startedBoostingAtTime.HasValue)
        {
            kickSpeedBonus = KickBoostBonusAdjusted * Mathf.Clamp01(1f - (Time.time - startedBoostingAtTime.Value) / kickBoostTime);
        }

        speed = MinBoostAdjusted + (MaxBoostAdjusted - MinBoostAdjusted) * gasPedal;
        speed += kickSpeedBonus;

        transform.position += speed * Time.deltaTime * transform.forward;
    }

    void ApplyGees()
    {
        if (Time.deltaTime <= 0f)
        {
            return;
        }

        Vector3 forward = transform.forward;
        Vector3 lastForward = lastRotation * Vector3.forward;

        forward.y = 0f;
        forward.Normalize();

        lastForward.y = 0f;
        lastForward.Normalize();

        float angle = Vector3.Angle(forward, lastForward);
        float delta = angle / Time.deltaTime;

        if (delta > minimumDeltaToGetGeesDegPerSec)
        {
            gees += Time.deltaTime * (delta / minimumDeltaToGetGeesDegPerSec);
        }
        else
        {
            gees -= Time.deltaTime * (1f - delta / minimumDeltaToGetGeesDegPerSec);
        }

        gees = Mathf.Clamp(gees, 0f, lethalGees);
    }

    void ApplyDirection()
    {
        if (virtualStick.sqrMagnitude > 0f)
        {
            float localRot = Mathf.Repeat(transform.eulerAngles.x + 180f, 360) - 180f;
            float yStick = virtualStick.y;
            if (localRot > angleCap)
            {
                // We're going down
                yStick = Mathf.Max(virtualStick.y, 0f);
            }
            else if (localRot < -angleCap)
            {
                yStick = Mathf.Min(virtualStick.y, 0f);
            }

            Vector2 fixedStick = new Vector2(
                virtualStick.x * (1f - Mathf.Abs(localRot) / 90f),
                yStick
            );

            transform.forward = Vector3.Lerp(transform.forward, transform.TransformDirection(fixedStick), rotationSpeed * Time.deltaTime);
        }

        //transform.Rotate(Vector3.forward, -virtualStick.x * rotationSpeed * Time.deltaTime);
        //transform.Rotate(Vector3.right, virtualStick.y * rotationSpeed * Time.deltaTime);
    }
}
