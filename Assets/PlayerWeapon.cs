using UnityEngine;
using System.Collections;
using LouveSystems;
using System.Collections.Generic;

public class PlayerWeapon : MonoBehaviour
{
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
    private float offCenterMultiplier = 0.5f;

    public float Imprecision01 { get { return currentImprecision / maxImprecisionAmount; } }

    public bool IsAiming { get { return player.IsAiming; } }

    public Vector2 AimingPosition { get { return player.StickDirection; } }

    private float lastFireTime = 0f;

    private bool shootLeft = false;

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
            if (player.IsShooting)
            {
                if (Time.time - lastFireTime > fireRate)
                {
                    Shoot();
                    lastFireTime = Time.time;
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
                activeMissiles[i].transform.position = player.Transform.position + player.Transform.forward;
                activeMissiles[i].transform.forward = Vector3.Lerp(player.Transform.forward, Random.insideUnitSphere, Imprecision01);

                shootLeft = !shootLeft;
                activeMissiles[i].transform.position += player.Transform.right * offCenterMultiplier * (shootLeft ? 1f : -1f);

                activeMissiles[i].gameObject.SetActive(true);

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
