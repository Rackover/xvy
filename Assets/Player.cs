using UnityEngine;
using System.Collections;
using System;

public class Player : MonoBehaviour
{

    public event Action<Vector3, Vector3> OnSpawn;

    [SerializeField]
    private Transform playerMovement;

    [SerializeField]
    private PlayerWeapon weapon;

    [SerializeField]
    private PlayerCamera playerCamera;

    public int Index { get { return index; } }

    public bool IsReady { get; private set; }

    public bool IsSpawned { get; private set; }

    public bool IsBoosting { get; private set; }

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

    public bool AnyKey() { return input.AnyKey(); }

    public void Spawn(Transform spawner)
    {
        Debug.Log("Spawning " + name + " on " + spawner);

        if (OnSpawn != null)
        {
            OnSpawn.Invoke(spawner.position, spawner.forward);
        }

        IsSpawned = true;
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

                float aimAxis = input.RightTrigger();
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


                if (aimAxis > 0f)
                {
                    IsBoosting = true;
                }
                else
                {
                    IsBoosting = false;
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
