using UnityEngine;
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

    private AudioClip[] allClips;

    private Coroutine syncTask;

    private Level levelRef;

    private void ResynchronizeIfNecessary()
    {
        if (allClips == null)
        {
            return;
        }

        AudioSource referenceSource = sourceForClip[lightDrum];
        float refTime = referenceSource.time;
        bool needsSync = false;

        for (int i = 0; i < allClips.Length; i++)
        {
            AudioSource source;

            if (sourceForClip.TryGetValue(allClips[i], out source))
            {
                if (Mathf.Abs(source.time - refTime) > 0.05f)
                {
                    needsSync = true;
                    break;
                }
            }
        }

        if (needsSync)
        {
            Resynchronize();
        }

    }

    private void Resynchronize()
    {
        if (syncTask != null)
        {
            // already synchronizing
            return;
        }

        if (allClips == null)
        {
            return;
        }


        syncTask = StartCoroutine(SynchronizationTask());
    }

    private IEnumerator SynchronizationTask()
    {
        AudioSource referenceSource = sourceForClip[lightDrum];
        referenceSource.loop = false;
        while (referenceSource.isPlaying)
        {
            yield return null;
        }

        referenceSource.loop = true;
        double startTime = AudioSettings.dspTime + 0.1;

        for (int i = 0; i < allClips.Length; i++)
        {
            AudioSource source;

            if (sourceForClip.TryGetValue(allClips[i], out source))
            {
                source.Stop();
                source.PlayScheduled(startTime);
            }
        }


        syncTask = null;
    }

    void Awake()
    {

#if UNITY_WEBGL
        menuVolume /= (float)Ambience.Count;
        ingameVolume /= (float)Ambience.Count;
#endif

        exampleSource.gameObject.SetActive(false);

        allClips = new AudioClip[]
        {
            bassLine, fightingDistance, pressure, heavyDrum, lightDrum, title, titleStep2
        };


        for (int i = 0; i < allClips.Length; i++)
        {
            AudioSource source = Pooler.DePool(this, exampleSource);
            source.playOnAwake = false;
            source.clip = allClips[i];
            source.volume = 0.0f;
            source.gameObject.SetActive(true);

            sourceForClip[allClips[i]] = source;
        }

        Game.i.OnLevelLoaded += I_OnLevelLoaded;

        if (Game.i.Level != null)
        {
            I_OnLevelLoaded();
        }
    }

    private void OnDestroy()
    {
        if (Game.i)
        {
            Game.i.OnLevelLoaded -= I_OnLevelLoaded;
        }
    }

    private void I_OnLevelLoaded()
    {
        if (levelRef)
        {
            levelRef.OnScoreChanged -= Level_OnScoreChanged;
        }

        levelRef = Game.i.Level;

        levelRef.OnScoreChanged += Level_OnScoreChanged;
        Resynchronize();
    }

    private void Level_OnScoreChanged()
    {
        ResynchronizeIfNecessary();
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

        foreach (var k in sourceForClip)
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
