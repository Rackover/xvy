using UnityEngine;
using System.Collections;

public class HomingMissile : Projectile
{
    [SerializeField]
    [Range(0f, 100)]
    private float maxTurnRate =10f;

    [SerializeField]
    private float tailRotateSpeed = 90f;

    [SerializeField]
    private TrailRenderer[] trails;

    [SerializeField]
    private float trackAfterSeconds = 0.5f;

    [SerializeField]
    private float stopTrackingAtDistanceBonus = 5f;


    [SerializeField]
    private Transform animTransform;

    private Transform homingTarget;

    private float noTrackDistanceSqrd;

    public void SetHomingTarget(Transform target)
    {
        this.homingTarget = target;
        this.noTrackDistanceSqrd = (stopTrackingAtDistanceBonus + DetonationDistance) * (stopTrackingAtDistanceBonus + DetonationDistance);
    }

    public void ClearTrails()
    {
        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].Clear();
        }
    }

    public override void ManualUpdate()
    {
        base.ManualUpdate();

        animTransform.Rotate(0f, 0F, Time.deltaTime * tailRotateSpeed, Space.Self);
    }

    protected override Vector3 GetDirection()
    {
        if (homingTarget)
        {
            Vector3 direction = homingTarget.position - transform.position;

            if (LivedFor < trackAfterSeconds)
            {
                direction = Vector3.Lerp(direction, transform.forward, Mathf.Clamp01(trackAfterSeconds - LivedFor)).normalized;
            }

            if (direction.sqrMagnitude < noTrackDistanceSqrd)
            {
                direction = Vector3.Lerp(transform.forward, direction, Mathf.Clamp01(noTrackDistanceSqrd/ direction.sqrMagnitude)).normalized;
            }

            return Vector3.Lerp(transform.forward, direction, Mathf.Clamp01(maxTurnRate * Time.deltaTime));
        }

        return base.GetDirection();
    }
}
