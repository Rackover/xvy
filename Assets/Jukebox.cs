﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LouveSystems;

public class Jukebox : MonoBehaviour
{
    public enum Ambience
    {
        Base,
        Additional,
        Drums,
        Count
    }

    [Header("Clips")]

    [SerializeField]
    private AudioClip bassLine;

    [SerializeField]
    private AudioClip fightingDistance;

    [SerializeField]
    private AudioClip pressure;

    [SerializeField]
    private AudioClip heavyDrum;

    [SerializeField]
    private AudioClip lightDrum;

    [SerializeField]
    private AudioClip title;

    [SerializeField]
    private AudioClip titleStep2;

    [SerializeField]
    private AudioSource exampleSource;

    [SerializeField]
    private float fadeSpeed = 2f;

    [SerializeField]
    private float menuVolume = 1f;

    [SerializeField]
    private float ingameVolume = 0.6f;

    private float time;

    private AudioClip[] selectedClips = new AudioClip[(int)Ambience.Count];

    private readonly Dictionary<AudioClip, AudioSource> sourceForClip = new Dictionary<AudioClip, AudioSource>();

    void Awake()
    {
        exampleSource.gameObject.SetActive(false);

        AudioClip[] clips = new AudioClip[]
        {
            bassLine, fightingDistance, pressure, heavyDrum, lightDrum, title, titleStep2
        };

        double startTime = AudioSettings.dspTime + 1;
        for (int i = 0; i < clips.Length; i++)
        {
            AudioSource source  = Pooler.DePool(this, exampleSource);
            source.clip = clips[i];
            source.volume = 0.0f;
            source.gameObject.SetActive(true);

            source.PlayScheduled(startTime);

            sourceForClip[clips[i]] = source;

        }
    }

    void Update()
    {
        if (Game.i.Playing)
        {
            SetClip(Ambience.Base, bassLine);

            if (Game.i.Level.IsPlayerAlive(0) && Game.i.Level.IsPlayerAlive(1))
            {
                SetClip(Ambience.Drums, heavyDrum);
            }
            else
            {
                SetClip(Ambience.Drums, lightDrum);
            }

            AudioClip additionalClip = null;
            for (int i = 0; i < Level.PLAYERS; i++)
            {
                if (Game.i.Level.IsPlayerBeingHomedTo(i))
                {
                    additionalClip = pressure;
                }
            }

            if (additionalClip != null)
            {
                SetClip(Ambience.Additional, additionalClip);
            }
            else if (Game.i.Level.IsPlayerAlive(0) && Game.i.Level.IsPlayerAlive(1) && 
                (Game.i.Level.GetPlayerWeapon(0).IsAcquiring || Game.i.Level.GetPlayerWeapon(1).IsAcquiring))
            {
                SetClip(Ambience.Additional, fightingDistance);
            }
            else
            {
                SetClip(Ambience.Additional, null);
            }
        }
        else
        {
            SetClip(Ambience.Drums, lightDrum);
            SetClip(Ambience.Base, title);

            if (Game.i.InGame)
            {
                SetClip(Ambience.Additional, titleStep2);
            }
            else
            {
                SetClip(Ambience.Additional, null);
            }
        }

        foreach(var k in sourceForClip)
        {
            AudioClip clip = k.Key;
            bool selected = false;

            for (int i = 0; i < selectedClips.Length; i++)
            {
                if (selectedClips[i] == clip)
                {
                    selected = true;
                    break;
                }
            }

            k.Value.volume = Mathf.Clamp01(k.Value.volume + Time.deltaTime * fadeSpeed * (selected ? 1 : -1));

            k.Value.volume = Mathf.Min(k.Value.volume, Game.i.Playing ? ingameVolume : menuVolume);
        }
    }

    void SetClip(Ambience ambience, AudioClip clip)
    {
        int index = (int)ambience;

        selectedClips[index] = clip;
    }
}
