using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField]
    Player player;

    [SerializeField]
    PlayerMovement playerMovement;

    [SerializeField]
    float lerpSpeed = 5f;

    [SerializeField]
    private float trailAlphaMin = 0.5f;

    [SerializeField]
    private float trailAlphaMax = 1f;

    [SerializeField]
    private TrailRenderer trail;

    bool wasSpawned = false;

    Material dynaMat;

    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;

        dynaMat = Instantiate(trail.sharedMaterial);
        trail.sharedMaterial = dynaMat;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.IsSpawned)
        {
            bool firstFrame = !wasSpawned;

            var localPos = playerMovement.transform.InverseTransformPoint(transform.position);
            localPos.z = 0f;
            transform.position = playerMovement.transform.TransformPoint(localPos);

            transform.position = Vector3.Lerp(transform.position, playerMovement.transform.position, firstFrame ? 1f : lerpSpeed * Time.deltaTime);

            transform.forward =
               Vector3.Lerp(
                   transform.forward,
                    playerMovement.transform.TransformDirection(new Vector3(playerMovement.VirtualJoystick.x, playerMovement.VirtualJoystick.y, 1f).normalized),
                    firstFrame ? 1f : playerMovement.SpeedAmount
                );

            var joyVal = 40f * -playerMovement.VirtualJoystick.x;

            transform.eulerAngles = new Vector3(
                //Mathf.Clamp(transform.eulerAngles.x, -playerMovement.MaxDownwardsAngle, playerMovement.MaxDownwardsAngle), 
                transform.eulerAngles.x,
                transform.eulerAngles.y, 
                joyVal
            );

            Color trailColor = Color.Lerp(Color.white, Color.red, playerMovement.KickAmount);
            trailColor.a = Mathf.Lerp(trailAlphaMin, trailAlphaMax, playerMovement.SpeedAmount);
            dynaMat.SetColor("_TintColor", trailColor);
        }

        wasSpawned = player.IsSpawned;
    }

    void OnDestroy()
    {
        if (dynaMat)
        {
            Destroy(dynaMat);
        }
    }
}
