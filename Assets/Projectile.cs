using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    [SerializeField]
    private float lifespan = 2f;

    [SerializeField]
    private float detonationDistance = 4f;

    [SerializeField]
    private float velocity = 50f;

    public bool Expired { get { return livedFor > lifespan; } }

    public float LivedFor { get { return livedFor; } }

    protected float DetonationDistance { get { return detonationDistance; } }

    private float livedFor = 0f;

    private int owner;

    private bool detonated = false;

    private float detonationDistanceSqrd;


    void OnEnable()
    {
        livedFor = 0f;
        detonated = false;
        detonationDistanceSqrd = detonationDistance * detonationDistance;
    }

    public void SetOwner(int id)
    {
        this.owner = id;
    }

    public virtual void ManualUpdate()
    {
        transform.position += GetDirection() * velocity * Time.deltaTime;
        livedFor += Time.deltaTime;

        int otherId = 1 - owner;

        if (Game.i.Level.IsPlayerAlive(otherId))
        {
            Vector3 otherPosition = Game.i.Level.GetPlayerPosition(otherId);

            if (Vector3.SqrMagnitude(otherPosition - transform.position) < detonationDistanceSqrd)
            {
                Detonate(otherId);
            }
        }
    }

    public void Expire()
    {
        Detonate(null);
    }

    protected virtual void Detonate(int? target)
    {
        if (detonated)
        {
            return;
        }

        if (target.HasValue)
        {
            Game.i.Level.KillPlayerFromMissile(target.Value);
        }

        livedFor = float.MaxValue;

        detonated = true;
    }

    protected virtual Vector3 GetDirection()
    {
        return transform.forward;
    }

}
