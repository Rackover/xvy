using UnityEngine;
using System.Collections;
using LouveSystems;
using System.Collections.Generic;

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
    private float precisionDecreaseOverTime = 0.25f;

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
    private float aimAcquisitionMultiplier = 1f;

    public float AcquisitionDistanceMeters { get { return acquisitionDistance; } }

    public bool HasEnemyInAcquisitionSights { set; private get; }

    public bool IsAcquiring { get { return acquisitionAmount > 0f; } }

    public float Imprecision01 { get { return currentImprecision / maxImprecisionAmount; } }

    public Vector2 AimingPosition { get { return player.StickDirection; } }

    public float Acquisition01 { get { return acquisitionAmount; } }

    public bool TargetAcquired { get { return acquisitionAmount >= 1f || homingMissileAlive; } }

    public bool HomingMissileAlive { get { return homingMissileAlive; } }

    private float reloadTimeRemaining = 0f;

    private bool shootLeft = false;

    private int burstIndex = 0;

#if UNITY_EDITOR
    [SerializeField]
#endif
    private float acquisitionAmount = 0f;

    private readonly Projectile[] activeMissiles = new Projectile[100];

    private bool homingMissileAlive = false;

    private float acquisitionDistanceSquared;

#if UNITY_EDITOR
    [SerializeField]
#endif

    private float currentImprecision = 0f;

    void Awake()
    {
        missilePrefab.gameObject.SetActive(false);
        homingMissilePrefab.gameObject.SetActive(false);

        acquisitionDistanceSquared = acquisitionDistance * acquisitionDistance;
    }

    void Update()
    {
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

                currentImprecision = Mathf.Clamp(currentImprecision + Time.deltaTime * precisionDecreaseOverTime, 0f, maxImprecisionAmount);
            }
            else
            {
                int otherPlayer = 1 - player.Index;
                if (Game.i.Level.IsPlayerAlive(otherPlayer))
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
                Vector3 fwd = otherPlayerTransform ? (otherPlayerTransform.position - player.Transform.position).normalized : player.Transform.forward;

                homing.ClearTrails();
                homing.SetHomingTarget(otherPlayerTransform);
                homing.transform.position = player.Transform.position + fwd;
                homing.transform.forward = fwd;

                homing.gameObject.SetActive(true);

                homingMissileAlive = true;
                activeMissiles[i] = homing;

                player.RumbleHeavy();

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

                Vector3 shootDirection = (player.Transform.forward + (Random.insideUnitSphere * 2f - Vector3.one) * currentImprecision).normalized;

                activeMissiles[i].transform.forward = shootDirection;

                shootLeft = !shootLeft;
                activeMissiles[i].transform.position += player.Transform.right * offCenterMultiplier * (shootLeft ? 1f : -1f);

                activeMissiles[i].gameObject.SetActive(true);

                player.RumbleLight();

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

                    if (activeMissiles[i] is HomingMissile)
                    {
                        homingMissileAlive = false;
                    }

                    activeMissiles[i] = null;
                }
            }
        }
    }
}
