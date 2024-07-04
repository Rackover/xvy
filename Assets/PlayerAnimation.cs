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
    private TrailRenderer[] trails;

    bool wasAlive = false;

    Material dynaMat;

    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;

        dynaMat = Instantiate(trails[0].sharedMaterial);

        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].sharedMaterial = dynaMat;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            Destroy(this.gameObject);
            return;
        }

        if (player.IsAlive)
        {
            bool firstFrame = !wasAlive;

            var localPos = playerMovement.transform.InverseTransformPoint(transform.position);
            localPos.z = 0f;
            transform.position = playerMovement.transform.TransformPoint(localPos);

            CatchUpPositionAndForward(firstFrame ? 1f : lerpSpeed * Time.deltaTime, firstFrame ? 1f : playerMovement.SpeedAmount);
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
    }

    void CatchUpPositionAndForward(float letpPosition, float lerpForward)
    {
        transform.position = Vector3.Lerp(transform.position, playerMovement.transform.position, letpPosition);

        transform.forward =
           Vector3.Lerp(
               transform.forward,
                playerMovement.transform.TransformDirection(new Vector3(playerMovement.VirtualJoystick.x, playerMovement.VirtualJoystick.y, 1f).normalized),
                lerpForward
            );

    }

    void OnEnable()
    {
        CatchUpPositionAndForward(1f, 1f);
    }

    void OnDestroy()
    {
        if (dynaMat)
        {
            Destroy(dynaMat);
        }
    }
}
