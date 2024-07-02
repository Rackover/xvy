using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PlayerCamera : MonoBehaviour
{

    [SerializeField] private float fov = 80f;
    [SerializeField] private float boostFov = 110f;
    [SerializeField] private float kickFov = 140f;

    [SerializeField] private float shakeForce = 0.1f;

    [SerializeField]
    private float minBlur = 0f;

    [SerializeField]
    private float maxBlur = 0.3f;

    [SerializeField]
    private float kickBlur = 1f;

    [SerializeField]
    private float maxSpeedLinesOpacity = 0.3f;

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
    private float fovLerpSpeed = 4f;

    [SerializeField]
    private Player player;

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private ParticleSystem speedLines;

    public Camera Camera { get { return camera; } }

    private Vector3 lastPosition;
    private Vector3 lastVelocity;


    // Start is called before the first frame update
    // Update is called once per frame
    void LateUpdate()
    {
        bool wasSpawned = camera.enabled;
        camera.enabled = player.IsSpawned;
        speedLines.gameObject.SetActive(player.IsSpawned);

        if (player.IsSpawned)
        {
            bool firstFrame = wasSpawned;
            camera.targetTexture = Game.i.Texes[player.Index];

            if (player.IsAlive)
            {
                transform.position = playerMovement.transform.TransformPoint(new Vector3(0f, verticalDistance, behindDistance + split.HorizontalAmount * behindDistanceHorizontalSplitBonus));
              
                lastVelocity = transform.position - lastPosition;
                lastPosition = transform.position;
            }
            else
            {
                transform.position += lastVelocity;
                lastVelocity = Vector3.Lerp(lastVelocity, Vector3.zero, Time.deltaTime * 8);
            }

            Quaternion targetRot = Quaternion.LookRotation(
                  playerMovement.transform.position
                  + playerMovement.transform.up * verticalDistance
                  - transform.position
              );

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, firstFrame ? 1f : rotationLerpSpeed * Time.deltaTime);


            // Shake
            if (!player.IsAlive || (playerMovement.KickAmount > 0f && player.IsAlive))
            {
                camera.transform.localPosition =
                    UnityEngine.Random.insideUnitSphere *
                    Mathf.Sin(playerMovement.KickAmount * Mathf.PI) * Mathf.Sign(UnityEngine.Random.value - 0.5f) * shakeForce;
            }
            else
            {
                camera.transform.localPosition = Vector3.zero;
            }

            var delta = playerMovement.SpeedAmount * playerMovement.SpeedAmount;

            var fovTarget = Mathf.Lerp(fov, boostFov, delta);

            speedLines.startColor = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, maxSpeedLinesOpacity), playerMovement.BoostAmount * playerMovement.BoostAmount);

            if (player.IsAlive)
            {
                blur.blurAmount = Mathf.Lerp(minBlur, maxBlur, delta);

                if (playerMovement.KickAmount > 0F)
                {
                    blur.blurAmount = Mathf.Lerp(blur.blurAmount, kickBlur, playerMovement.KickAmount);
                    fovTarget = Mathf.Lerp(fovTarget, kickFov, playerMovement.KickAmount);
                }

                colorCorrection.saturation = Mathf.Lerp(1f, 1.2f, delta);
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fovTarget, fovLerpSpeed * Time.deltaTime);

                if (player.IsBeingHomedTo)
                {
                    // Feedback anyone??
                }
            }
            else
            {
                blur.blurAmount = 0f;
                colorCorrection.saturation = Mathf.Clamp01(colorCorrection.saturation - Time.deltaTime);
            }
        }
    }
}
