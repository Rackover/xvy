using UnityEngine;
using System.Collections;
using LouveSystems;
using System.Collections.Generic;
using System.Reflection;

public class PlayerWeapon : MonoBehaviour
{
    const byte HOMING_MISSILE_POOL = 1;
    const byte STRAY_MISSILE_POOL = 0;

    [SerializeField]
    private float acquisitionDistance = 800;

    [SerializeField]
    private Player player;

    [SerializeField]
    private Projectile missilePrefab;

    [SerializeField]
    private Projectile homingMissilePrefab;

    [SerializeField]
    [Range(0f, 1f)]
    private float precisionIncreaseOverTime = 0.05f;

    [SerializeField]
    [Range(0f, 1f)]
    private float precisionDecreasePerShot = 0.25f;

    [SerializeField]
    [Range(0f, 1f)]
    private float maxImprecisionAmount = 0.3f;

    [SerializeField]
    private float fireRate = 0.2f;

    [SerializeField]
    private int burstSize = 3;

    [SerializeField]
    private float reloadTime = 1f;

    [SerializeField]
    private float offCenterMultiplier = 0.5f;

    [SerializeField]
    private float acquisitionSpeed = 0.33f;

    [SerializeField]
    private float aimingAmplitude = 150f;

    [SerializeField]
    private AudioSource generalSource;

    [SerializeField]
    private AudioClip homingConfirmedClip;

    public float AcquisitionDistanceMeters { get { return acquisitionDistance * Game.i.Level.SpeedMultiplier; } }

    public bool HasEnemyInAcquisitionSights { set; private get; }

    public bool IsAcquiring { get { return acquisitionAmount > 0f; } }

    public float Imprecision01 { get { return currentImprecision / maxImprecisionAmount; } }

    public float AimingAmplitude { get { return aimingAmplitude; } }

    public Vector2 AimingPosition { get { return player.StickDirection; } }

    public float Acquisition01 { get { return acquisitionAmount; } }

    public bool TargetAcquired { get { return acquisitionAmount >= 1f || homingMissileAlive; } }

    public bool HasLineOfSightOnEnemy { get; private set; }

    public HomingMissile HomingMissileAlive { get { return homingMissileAlive; } }

    private float reloadTimeRemaining = 0f;

    private bool shootLeft = false;

    private int burstIndex = 0;

#if UNITY_EDITOR
    [SerializeField]
#endif
    private float acquisitionAmount = 0f;

    private readonly Projectile[] activeMissiles = new Projectile[100];

    private HomingMissile homingMissileAlive = null;

    private float acquisitionDistanceSquared;

#if UNITY_EDITOR
    [SerializeField]
#endif

    private float currentImprecision = 0f;

    private int losMask = 0;

    void Awake()
    {
        losMask = LayerMask.GetMask("Level");
        missilePrefab.gameObject.SetActive(false);
        homingMissilePrefab.gameObject.SetActive(false);
    }

    private void RefreshLineOfSight()
    {
        HasLineOfSightOnEnemy = false;
        if (Game.i.InGame)
        {
            if (player.IsReady)
            {
                if (player.IsAlive && player.IsSpawned)
                {
                    int otherPlayer = 1 - player.Index;

                    if (Game.i.Level.IsPlayerAlive(otherPlayer))
                    {
                        Vector3 position = Game.i.Level.GetPlayerPosition(player.Index);
                        Vector3 enemyPosition = Game.i.Level.GetPlayerPosition(otherPlayer);

                        Ray ray = new Ray(position, enemyPosition - position);


                        if (Physics.Raycast(ray, float.MaxValue, losMask))
                        {
                            HasLineOfSightOnEnemy = false;
                        }
                        else
                        {
                            HasLineOfSightOnEnemy = true;
                        }
                    }
                }
            }
        }
    }
    void Update()
    {
        acquisitionDistanceSquared = AcquisitionDistanceMeters * AcquisitionDistanceMeters;

        RefreshLineOfSight();

        if (player.IsAlive)
        {
            if (player.IsShooting || burstIndex != 0)
            {
                if (reloadTimeRemaining <= 0f && !homingMissileAlive)
                {
                    if (TargetAcquired || Game.i.AlwaysHoming)
                    {
                        ShootHomingMissile();
                        reloadTimeRemaining = reloadTime;
                        acquisitionAmount = 0f;
                    }
                    else
                    {
                        Shoot();
                        burstIndex++;

                        if (burstIndex >= burstSize)
                        {
                            burstIndex = 0;
                            reloadTimeRemaining = reloadTime;
                        }
                        else
                        {
                            reloadTimeRemaining = fireRate;
                        }

                        acquisitionAmount = 0f;
                    }
                }
            }
            else
            {
                int otherPlayer = 1 - player.Index;
                if (Game.i.Level.IsPlayerAlive(otherPlayer) && !homingMissileAlive)
                {
                    if (player.IsBoosting)
                    {
                        acquisitionAmount -= Time.deltaTime;
                    }
                    else
                    {
                        Vector3 enemyPosition = Game.i.Level.GetPlayerPosition(otherPlayer);
                        float distanceSquared = (enemyPosition - transform.position).sqrMagnitude;

                        if (distanceSquared < acquisitionDistanceSquared && HasEnemyInAcquisitionSights)
                        {
                            acquisitionAmount += Time.deltaTime * acquisitionSpeed;
                        }
                        else
                        {
                            acquisitionAmount -= Time.deltaTime * acquisitionSpeed;
                        }

                        acquisitionAmount = Mathf.Clamp01(acquisitionAmount);
                    }
                }
                else
                {
                    acquisitionAmount = 0f;
                }

                currentImprecision = Mathf.Clamp01(currentImprecision - Time.deltaTime * precisionIncreaseOverTime);
            }
        }
        else
        {
            currentImprecision = 0f;
            acquisitionAmount = 0f;
        }

        reloadTimeRemaining -= Time.deltaTime;
        reloadTimeRemaining = Mathf.Max(0f, reloadTimeRemaining);

        UpdateMissiles();
    }

    void ShootHomingMissile()
    {
        if (homingMissileAlive)
        {
            // Should not happen anyway
            return;
        }

        bool shot = false;

        for (int i = 0; i < activeMissiles.Length; i++)
        {
            if (activeMissiles[i] == null)
            {
                HomingMissile homing = Pooler.DePool(this, homingMissilePrefab, HOMING_MISSILE_POOL) as HomingMissile;

                Transform otherPlayerTransform = Game.i.Level.GetPlayerTransform(1 - player.Index);
                Vector3 fwd = otherPlayerTransform ? (otherPlayerTransform.position - player.Transform.position).normalized : GetShootDirection();

                homing.SetOwner(player.Index);
                homing.ClearTrails();
                homing.SetHomingTarget(otherPlayerTransform);
                homing.transform.position = player.Transform.position + fwd;
                homing.transform.forward = fwd;

                homing.gameObject.SetActive(true);

                homingMissileAlive = homing;
                activeMissiles[i] = homing;

                player.RumbleHeavy();
                generalSource.PlayOneShot(homingConfirmedClip);

                shot = true;

                break;
            }
        }

        if (!shot)
        {
            KillOldestMissile();
            ShootHomingMissile();
        }
    }

    private Vector3 GetShootDirection()
    {
        Vector3 frustrumEnd = transform.position + transform.forward * 400f; // optimal shooting distance

        Vector3 aimAt = frustrumEnd + (transform.right * AimingPosition.x + transform.up * AimingPosition.y) * aimingAmplitude;

        return (aimAt - transform.position).normalized;
    }

    void Shoot()
    {
        bool shot = false;

        for (int i = 0; i < activeMissiles.Length; i++)
        {
            if (activeMissiles[i] == null)
            {
                activeMissiles[i] = Pooler.DePool(this, missilePrefab, STRAY_MISSILE_POOL);
                activeMissiles[i].GetComponent<TrailRenderer>().Clear();

                activeMissiles[i].transform.position = player.Transform.position + player.Transform.forward;
                activeMissiles[i].SetOwner(player.Index);

                Vector3 shootDirection = (GetShootDirection() + Random.insideUnitSphere * currentImprecision).normalized;

                activeMissiles[i].transform.forward = shootDirection;

                shootLeft = !shootLeft;
                activeMissiles[i].transform.position += player.Transform.right * offCenterMultiplier * (shootLeft ? 1f : -1f);

                activeMissiles[i].gameObject.SetActive(true);

                player.RumbleLight();

                currentImprecision = Mathf.Clamp(currentImprecision + precisionDecreasePerShot, 0f, maxImprecisionAmount);

                shot = true;

                break;
            }
        }

        if (!shot)
        {
            KillOldestMissile();
            Shoot();
        }
    }

    void KillOldestMissile()
    {
        int? oldest = null;
        for (int i = 0; i < activeMissiles.Length; i++)
        {
            if (activeMissiles[i] && !(activeMissiles[i] is HomingMissile))
            {
                if (!oldest.HasValue)
                {
                    oldest = i;
                }
                else if (activeMissiles[i].LivedFor > activeMissiles[oldest.Value].LivedFor)
                {
                    oldest = i;
                }
            }
        }

        if (oldest.HasValue)
        {
            Pooler.Pool(this, activeMissiles[oldest.Value], STRAY_MISSILE_POOL);
            activeMissiles[oldest.Value] = null;
        }
    }

    void OnDestroy()
    {

        for (int i = 0; i < activeMissiles.Length; i++)
        {
            if (activeMissiles[i])
            {
                Destroy(activeMissiles[i].gameObject);
            }
        }
    }

    void UpdateMissiles()
    {
        for (int i = 0; i < activeMissiles.Length; i++)
        {
            if (activeMissiles[i])
            {
                activeMissiles[i].ManualUpdate();
                if (activeMissiles[i].Expired)
                {
                    activeMissiles[i].Expire();

                    Pooler.Pool(this, activeMissiles[i], activeMissiles[i] is HomingMissile ? HOMING_MISSILE_POOL : STRAY_MISSILE_POOL);

                    if (activeMissiles[i] is HomingMissile && activeMissiles[i] == homingMissileAlive)
                    {
                        homingMissileAlive = null;
                    }

                    activeMissiles[i] = null;
                }
            }
        }
    }
}
