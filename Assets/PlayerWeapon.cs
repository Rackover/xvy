using UnityEngine;
using System.Collections;
using LouveSystems;
using System.Collections.Generic;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField]
    private float acquisitionDistance = 800;

    [SerializeField]
    private Player player;

    [SerializeField]
    private Projectile missilePrefab;

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

    public float AcquisitionDistanceMeters { get { return acquisitionDistance; } }

    public float Imprecision01 { get { return currentImprecision / maxImprecisionAmount; } }

    public Vector2 AimingPosition { get { return Vector2.Scale(player.StickDirection, new Vector2(1f, -1f)); } }

    private float reloadTimeRemaining = 0f;

    private bool shootLeft = false;

    private int burstIndex = 0;

    private readonly Projectile[] activeMissiles = new Projectile[100];



#if UNITY_EDITOR
    [SerializeField]
#endif

    private float currentImprecision = 0f;
    void Awake()
    {
        missilePrefab.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player.IsSpawned)
        {
            if (player.IsShooting || burstIndex != 0)
            {
                if (reloadTimeRemaining <= 0f)
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
                        reloadTimeRemaining = fireRate; ;
                    }
                }

                currentImprecision = Mathf.Clamp(currentImprecision + Time.deltaTime * precisionDecreaseOverTime, 0f, maxImprecisionAmount);
            }
            else
            {
                currentImprecision = Mathf.Clamp01(currentImprecision - Time.deltaTime * precisionIncreaseOverTime);
            }
        }
        else
        {
            currentImprecision = 0f;
        }

        reloadTimeRemaining -= Time.deltaTime;
        reloadTimeRemaining = Mathf.Max(0f, reloadTimeRemaining);

        UpdateMissiles();
    }

    void Shoot()
    {
        bool shot = false;

        for (int i = 0; i < activeMissiles.Length; i++)
        {
            if (activeMissiles[i] == null)
            {
                activeMissiles[i] = Pooler.DePool(this, missilePrefab);
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
            if (activeMissiles[i])
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
            Pooler.Pool(this, activeMissiles[oldest.Value]);
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
                    Pooler.Pool(this, activeMissiles[i]);
                    activeMissiles[i] = null;
                }
            }
        }
    }
}
