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

    [SerializeField]
    private AnimationCurve axelCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    protected override float Velocity { get { return Mathf.Lerp(initialVelocity, velocity, acceleration); } }

    private Transform homingTarget;

    private float acceleration = 0f;

    private float initialVelocity = 0f;

    private float noTrackDistanceSqrd;

    public override void SetOwner(int id)
    {
        base.SetOwner(id);

        initialVelocity = Game.i.Level.GetPlayerSpeed(id);
    }

    public void SetHomingTarget(Transform target)
    {
        this.homingTarget = target;
        this.noTrackDistanceSqrd = (stopTrackingAtDistanceBonus + DetonationDistance) * (stopTrackingAtDistanceBonus + DetonationDistance);

        acceleration = 0f;

        if (Random.value < 0.3f)
        {
             // Perfect missiles once in a while
            this.noTrackDistanceSqrd = 0f;
        }
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

        acceleration = axelCurve.Evaluate(LivedFor);

        animTransform.Rotate(0f, 0F, Time.deltaTime * tailRotateSpeed, Space.Self);
    }

    protected override Vector3 GetDirection()
    {
        if (homingTarget)
        {
            Vector3 distVector = homingTarget.position - transform.position;
            Vector3 direction = distVector.normalized;

            if (LivedFor < trackAfterSeconds)
            {
                direction = Vector3.Lerp(direction, transform.forward, Mathf.Clamp01(trackAfterSeconds - LivedFor));
            }

            if (direction.sqrMagnitude < noTrackDistanceSqrd)
            {
                float trackAmount = Mathf.Clamp01(distVector.sqrMagnitude / noTrackDistanceSqrd);
                direction = Vector3.Lerp(transform.forward, direction, trackAmount);
            }

            return Vector3.Lerp(transform.forward, direction, Mathf.Clamp01(maxTurnRate * Time.deltaTime));
        }

        return base.GetDirection();
    }
}
