using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    [SerializeField]
    private float lifespan = 2f;

    [SerializeField]
    private float detonationDistance = 4f;

    [SerializeField]
    protected float velocity = 50f;

    protected virtual float Velocity { get { return velocity; } }

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

    void OnCollisionEnter(Collision other)
    {
        Detonate(null);
    }

    public virtual void SetOwner(int id)
    {
        this.owner = id;
    }

    public virtual void ManualUpdate()
    {
        MoveForward();
        Age();

        DetectCollision();
    }

    private void DetectCollision()
    {
        Vector3 otherPosition = GetOtherPlayerPosition();
        float mag = Vector3.SqrMagnitude(otherPosition - GetMyPosition());

        if (mag < detonationDistanceSqrd)
        {
            KillOtherPlayer();
            Player.SetDeathString(1 - owner, "Detonated "+GetType()+" from " + owner + " because distance magnitude (" + mag.ToString("n0") + " meters) between "+otherPosition+" and "+GetMyPosition()+" was closest than the DD2 (" + detonationDistanceSqrd.ToString("n0") + ")");
        }
    }

    private void Age()
    {
        livedFor += Time.deltaTime;
    }

    private void MoveForward()
    {
        transform.position += GetDirection() * Velocity * Time.deltaTime;
    }
   
    private void KillOtherPlayer()
    {
        int otherId = 1 - owner;
        Detonate(otherId);
    }

    private Vector3 GetMyPosition()
    {
        return transform.position;
    }

    private Vector3 GetOtherPlayerPosition()
    {
        int otherId = 1 - owner;
        Vector3 otherPosition = Game.i.Level.GetPlayerPosition(otherId);

        return otherPosition;
    }

    private bool IsOtherPlayerAlive()
    {
        int otherId = 1 - owner;

        if (Game.i.Level.IsPlayerAlive(otherId))
        {
            return true;
        }

        return false;
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

        if (target.HasValue && Game.i.Level.IsPlayerAlive(target.Value))
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
