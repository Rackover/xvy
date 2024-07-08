using UnityEngine;
using System.Collections;

public class Fake3DSound : MonoBehaviour
{

    [SerializeField]
    private AudioSource source;

    [SerializeField]
    private bool linear = true;

    private float maxVolume = 0f;

    void Awake()
    {
        maxVolume = source.volume;
        source.spatialBlend = 0.0f;
    }

    void Update()
    {
        if (Game.i.Level)
        {
            if (Game.i.Playing)
            {
                float closestDistanceSquared = float.PositiveInfinity;

                for (int i = 0; i < Level.PLAYERS; i++)
                {
                    float distSquared = Vector3.SqrMagnitude(Game.i.Level.GetPlayerPosition(i) - transform.position);

                    if (closestDistanceSquared > distSquared)
                    {
                        closestDistanceSquared = distSquared;
                    }
                }

                if (linear)
                {
                    float closestDistance = Mathf.Sqrt(closestDistanceSquared);
                    source.volume = (1f - closestDistance / source.maxDistance) * maxVolume;
                }
                else
                {
                    source.volume = (1f - closestDistanceSquared / (source.maxDistance * source.maxDistance)) * maxVolume;
                }
            }
            else
            {
                source.volume = 0f;
            }
        }
    }
}
