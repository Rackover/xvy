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

    private float livedFor = 0f;

    void OnEnable()
    {
        livedFor = 0f;
    }

    public void ManualUpdate()
    {
        transform.position += transform.forward * velocity * Time.deltaTime;
        livedFor += Time.deltaTime;
    }

}
