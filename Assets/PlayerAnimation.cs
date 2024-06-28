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

    bool wasSpawned = false;

    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
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

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 60f * (-playerMovement.VirtualJoystick.x));
        }

        wasSpawned = player.IsSpawned;
    }
}
