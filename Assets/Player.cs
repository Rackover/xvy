﻿using UnityEngine;
using System.Collections;
using System;

public class Player : MonoBehaviour
{
    public event Action OnKilled;

    public event Action OnKilledByCollision;
    public event Action OnKilledByEnemy;
    public event Action<Vector3, Vector3> OnBirthed;

    [SerializeField]
    private Transform playerMovement;

    [SerializeField]
    private PlayerWeapon weapon;

    [SerializeField]
    private PlayerCollisions collisions;

    [SerializeField]
    private PlayerCamera playerCamera;

    [SerializeField]
    private ParticleSystem deathShuriken;

    [SerializeField]
    private Transform visualsTransform;

    public int Index { get { return index; } }

    public bool IsReady { get; private set; }

    public bool IsSpawned { get; private set; }

    public bool IsBeingHomedTo { get { return Game.i.Level.GetPlayerWeapon(1 - Index).HomingMissileAlive; } }

    public bool IsAlive { get; private set; }

    public bool IsBoosting { get; private set; }

    public bool IsAiming { get; private set; }

    public bool IsShooting { get; private set; }

    public Vector2 StickDirection { get; private set; }

    public Transform Transform { get { return playerMovement; } }

    public PlayerWeapon Weapon { get { return weapon; } }

    public Camera Camera { get { return playerCamera.Camera; } }

    private PlayerInput input;

    private int index;

    public void Initialize(int index)
    {
        if (input != null)
        {
            input.Dispose();
        }

        // Crashes dashboard
        //        input = new XInputDLLPlayerInput();

        //        input = new MockInput();
        input = new NativeUnityInput();

        input.SetPlayerIndex(index);

        if (!input.GamepadPresent() && Game.i.EmulateP2)
        {
            input.Dispose();
            input = new MockInput();
            input.SetPlayerIndex(index);
        }

        this.index = index;
        name = "PLAYER #" + index;

        IsReady = Game.i.AlwaysReady;
        IsSpawned = false;

        IsBoosting = false;
        IsShooting = false;
        StickDirection = new Vector2();
    }

    public bool GamepadConnected() { return input.GamepadPresent() || Game.i.AlwaysReady; }

    public void RumbleLight() { input.RumbleLightOnce(); }
    public void RumbleHeavy() { input.RumbleHeavyOnce(); }
    public void RumbleForSeconds(float seconds) { input.RumbleForSeconds(false, seconds); }

    public bool AnyKey() { return input.AnyKey(); }

    public void Birth(Transform spawner)
    {
        Debug.Log("Spawning " + name + " on " + spawner);

        IsSpawned = true;
        IsAlive = true;
        visualsTransform.gameObject.SetActive(true);

        if (OnBirthed != null)
        {
            OnBirthed.Invoke(spawner.position, spawner.forward);
        }
    }

    public void NotifyKilled()
    {
        IsAlive = false;
        PlayDeathAnimation();

        if (OnKilledByCollision != null)
        {
            OnKilledByEnemy.Invoke();
        }

        if (OnKilled != null)
        {
            OnKilled.Invoke();
        }
    }

    public void DeSpawn()
    {
        IsSpawned = false;
    }

    public void PlayDeathAnimation()
    {
        deathShuriken.Play();
        visualsTransform.gameObject.SetActive(false);
        RumbleForSeconds(0.4f);
    }

    private void Awake()
    {
        collisions.OnCollide += Collisions_OnCollide;
        visualsTransform.gameObject.SetActive(false);
    }

    public void NotifyOobDeath()
    {
        Collisions_OnCollide(null);
    }

    private void Collisions_OnCollide(Collision _)
    {
        if (IsAlive)
        {
            IsAlive = false;
            PlayDeathAnimation();

            if (OnKilledByCollision != null)
            {
                OnKilledByCollision.Invoke();
            }

            if (OnKilled != null)
            {
                OnKilled.Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        if (input != null)
        {
            input.Dispose();
        }
    }

    private void Update()
    {
        if (input == null)
        {
            return;
        }

        input.Refresh();

        if (Game.i.InGame)
        {

            if (IsSpawned)
            {
                if (IsAlive)
                {
                    float gasAxis = input.RightTrigger();
                    float antiGasAxis = input.LeftTrigger();
                    bool shoot = input.AButton();
                    {
                        Vector2 dir = input.GetDirection();

                        if (float.IsNaN(dir.x) || float.IsInfinity(dir.x))
                        {
                            dir.x = 0f;
                        }

                        if (float.IsNaN(dir.y) || float.IsInfinity(dir.y))
                        {
                            dir.y = 0f;
                        }

                        StickDirection = dir;
                    }


                    if (gasAxis > 0f)
                    {
                        IsBoosting = true;
                    }
                    else
                    {
                        IsBoosting = false;
                    }

                    if (antiGasAxis > 0f)
                    {
                        IsAiming = true;
                    }
                    else
                    {
                        IsAiming = false;
                    }

                    if (shoot)
                    {
                        IsShooting = true;
                    }
                    else
                    {
                        IsShooting = false;
                    }
                }
            }
            else
            {
                if (!IsReady)
                {
                    IsReady = input.IsPressingStart();

                    if (IsReady)
                    {
                        input.RumbleLightOnce();
                    }
                }
            }
        }
    }
}
