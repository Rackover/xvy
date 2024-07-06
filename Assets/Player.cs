using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking.Types;

public class Player : MonoBehaviour
{
    public event Action OnKilled;

    public event Action OnKilledByCollision;
    public event Action OnKilledByEnemy;
    public event Action<Vector3, Vector3> OnBirthed;

    [SerializeField]
    private PlayerMovement playerMovement;

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


    [Header("Sounds")]
    [SerializeField]
    private AudioSource source;

    [SerializeField]
    private AudioClip birthClip;

    [SerializeField]
    private AudioClip readyClip;

    [SerializeField]
    private AudioClip deathClip;

    public int Index { get { return index; } }

    public bool IsReady { get; private set; }

    public bool IsSpawned { get; private set; }

    public bool IsBeingHomedTo { get { return Game.i.Level.GetPlayerWeapon(1 - Index).HomingMissileAlive; } }

    public bool IsAlive { get; private set; }

    public bool IsBoosting { get; private set; }

    public bool IsAiming { get; private set; }

    public bool IsShooting { get; private set; }

    public AudioSource LocalSource { get { return source; } }

    public Vector2 StickDirection { get; private set; }

    public float Speed { get { return playerMovement.Speed; } }

    public Transform Transform { get { return playerMovement.transform; } }

    public PlayerWeapon Weapon { get { return weapon; } }

    public Camera Camera { get { return playerCamera.Camera; } }

    public string DebugDump { get { return "PLAYER #"+index+"\n"+ lastDeath + "\n" + input.Dump(); } }

    private PlayerInput input;

    private float lifetime = 0f;

    private string lastDeath = string.Empty;

    private int index;

    // Debugging
    public static Player[] players = new Player[Level.PLAYERS];

    public static void SetDeathString(int index, string death)
    {
        if (players[index])
        {
            players[index].lastDeath = death;
        }
    }

    // \Debugging

    public void Initialize(int index)
    {
        if (input != null)
        {
            input.Dispose();
        }

        // Crashes dashboard
        //        input = new XInputDLLPlayerInput();

        //        input = new MockInput();
        input = PlayerInput.MakeForPlatform();

        input.SetPlayerIndex(index);

        if (!input.GamepadPresent() && Game.i.EmulateP2)
        {
            input.Dispose();
            input = new MockInput();
            input.SetPlayerIndex(index);

            Console.WriteLine("Created mock input for player " + index + "");
        }

        this.index = index;
        name = "PLAYER #" + index;
        players[index] = this;

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
        lifetime = 0f;

        source.PlayOneShot(birthClip);

        if (OnBirthed != null)
        {
            OnBirthed.Invoke(spawner.position, spawner.forward);
        }
    }

    public void NotifyKilled()
    {
        if (lastDeath == string.Empty)
        {
            lastDeath = "Notify killed!";
        }

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
        source.PlayOneShot(deathClip);
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
        lastDeath = "Out of bounds death";
        Collisions_OnCollide(null);
    }

    private void Collisions_OnCollide(Collision obj)
    {
        if (lifetime < 1f)
        {
            // Cannot instant die of collision
            return;
        }

        if (IsAlive)
        {
            if (obj != null)
            {
                lastDeath = "(died colliding " + obj.gameObject.name + ")";
            }
            
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
                    lifetime += Time.deltaTime;

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
                        source.PlayOneShot(readyClip);
                        input.RumbleLightOnce();
                    }
                }
            }
        }
    }
}
