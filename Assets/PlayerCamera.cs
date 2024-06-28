using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PlayerCamera : MonoBehaviour
{

    [SerializeField] private float fov = 80f;
    [SerializeField] private float boostFov = 110f;

    [SerializeField] private float shakeForce = 0.1f;

    [SerializeField] private MotionBlur blur;

    [SerializeField] private ColorCorrectionCurves colorCorrection;

    [SerializeField]
    private SplitRenders split;

    [SerializeField]
    private float behindDistance = -7.25f;

    [SerializeField]
    private float behindDistanceHorizontalSplitBonus = -2f;

    [SerializeField]
    private float verticalDistance = 2.11f;

    [SerializeField]
    private float rotationLerpSpeed = 2f;

    [SerializeField]
    private Player player;

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private RenderTexture[] texes;

    public Camera Camera { get { return camera; } }

    // Start is called before the first frame update
    // Update is called once per frame
    void LateUpdate()
    {
        bool wasSpawned = camera.enabled;
        camera.enabled = player.IsSpawned;

        if (player.IsSpawned)
        {
            bool firstFrame = wasSpawned;
            camera.targetTexture = texes[player.Index];

            transform.position = playerMovement.transform.TransformPoint(new Vector3(0f, verticalDistance, behindDistance + split.HorizontalAmount * behindDistanceHorizontalSplitBonus));
            Quaternion targetRot = Quaternion.LookRotation(
                playerMovement.transform.position 
                + playerMovement.transform.up * verticalDistance
                - transform.position
            );

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, firstFrame ? 1f : rotationLerpSpeed * Time.deltaTime);

            // Shake
            if (playerMovement.SpeedAmount <= 1.0f && playerMovement.IsBoosting)
            {
                // don't
                //camera.transform.localPosition = new Vector3(Random.value, Random.value, Random.value) * Mathf.Sin(playerMovement.SpeedAmount * Mathf.PI) * Mathf.Sign(Random.value - 0.5f) * shakeForce;
            }
            else
            {
                camera.transform.localPosition = Vector3.zero;
            }

            var delta = playerMovement.SpeedAmount * playerMovement.SpeedAmount;

            camera.fieldOfView = Mathf.Lerp(fov, boostFov, delta);
            blur.blurAmount = Mathf.Lerp(0.2f, 1f, delta);

            colorCorrection.saturation = Mathf.Lerp(0.8f, 1f, delta);
        }
    }
}
