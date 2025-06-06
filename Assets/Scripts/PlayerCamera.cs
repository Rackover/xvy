﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking.Types;
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
    private CameraObstructer obstructor;

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

    [SerializeField]
    private AudioSource generalSource;

    [SerializeField]
    private AudioSource burnerSource;

    [SerializeField]
    private AudioSource windSource;

    [SerializeField]
    private AudioSource lockOnSource;

    private AudioSource[] localSources;

    public Camera Camera { get { return camera; } }

    private Vector3 lastPosition;
    private Vector3 lastVelocity;

    void Awake()
    {
        localSources = new AudioSource[]
        {
            generalSource,
            windSource,
            burnerSource,
            lockOnSource,
        };

        for (int i = 0; i < localSources.Length; i++)
        {
            if (localSources[i] != generalSource)
            {
                localSources[i].volume = 0f;
            }
        }

        lockOnSource.Stop();

#if UNITY_WEBGL
        lockOnSource.volume = 0.025f;
#else
        lockOnSource.volume = 1f;
#endif
    }

    void Start()
    {
        if (player)
        {
            if (camera)
            {
                var maskToRemove = LayerMask.GetMask(player.Index == 0 ? "SplitB" : "SplitA");
                var maskToAdd = LayerMask.GetMask(player.Index == 1 ? "SplitB" : "SplitA");

                camera.cullingMask |= maskToAdd;
                camera.cullingMask &= ~maskToRemove;
            }

            if (obstructor)
            {
                obstructor.Opposite = player.Index == 0;
            }
        }
    }

    void Update()
    {
        if (player && generalSource && Game.i)
        {
            generalSource.panStereo = Mathf.Lerp(0.5f, player.Index == 0 ? -1f : 1f, Game.i.HorizontalSplitAmount);
            for (int i = 0; i < localSources.Length; i++)
            {
                if (localSources[i] != generalSource)
                {
                    localSources[i].panStereo = generalSource.panStereo;
                }
            }
        }
    }

    // Start is called before the first frame update
    // Update is called once per frame
    void LateUpdate()
    {
        bool wasSpawned = camera.enabled;
        camera.enabled = player.IsSpawned;
        speedLines.gameObject.SetActive(player.IsSpawned && player.IsAlive);

        float burnerSourceVolume = 0f;
        float windSourceVolume = 0f;
        bool playLockOn = false;

        if (player.IsSpawned)
        {
            bool firstFrame = wasSpawned;

            if (Performance.i && Performance.i.HasDisabledRenderTarget)
            {
                // Do not set target texture
            }
            else
            {
                camera.targetTexture = Game.i.Texes[player.Index];
            }

            if (player.IsAlive)
            {
                transform.position = playerMovement.transform.TransformPoint(new Vector3(0f, verticalDistance, behindDistance + Game.i.HorizontalSplitAmount * behindDistanceHorizontalSplitBonus));

                lastVelocity = transform.position - lastPosition;
                lastPosition = transform.position;

                burnerSourceVolume = player.IsBoosting ? 0.5f : 0.2f;
                windSourceVolume = player.IsBoosting ? 0.2f : 0.5f;
                playLockOn = player.Weapon.TargetAcquired && !player.Weapon.HomingMissileAlive;
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


                {
                    float lifeRemaining01 = 0f;
                    if (Game.i.Level.IsOOB(player.Index, out lifeRemaining01))
                    {
                        colorCorrection.saturation *= lifeRemaining01;
                    }
                }
            }
            else
            {
                blur.blurAmount = 0f;
                colorCorrection.saturation = Mathf.Clamp01(colorCorrection.saturation - Time.deltaTime);
            }
        }


#if UNITY_WEBGL
        burnerSourceVolume *= 0.12f;
        windSourceVolume *= 0.1f;
#endif

        burnerSource.volume = burnerSourceVolume;
        windSource.volume = windSourceVolume;

        if (playLockOn && !lockOnSource.isPlaying)
        {
            lockOnSource.Play();
        }
        else if (!playLockOn && lockOnSource.isPlaying)
        {
            lockOnSource.Stop();
        }
    }
}
