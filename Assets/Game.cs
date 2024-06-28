using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
    public static bool AlwaysReady = false;

    public static Game i;

    [SerializeField]
    private Level[] levels;

    [SerializeField]
    private SplitRenders splitRenders;

    [SerializeField]
    private PlayerHUD[] huds = new PlayerHUD[Level.PLAYERS];

    [SerializeField]
    private float rotateIdleSpeed = 0.5f;

    [SerializeField]
    private bool alwaysReady = false;

    public Level Level { get { return currentLevel; } }

    private bool wantsToPlay = false;

    private Level currentLevel;

    private float animaticTarget = 0f;

    void Awake()
    {
#if UNITY_EDITOR


#else
        alwaysReady = false;
#endif

        i = this;

        AlwaysReady = alwaysReady;

        currentLevel = levels[0];

        InitLevel();
    }

    private void InitLevel()
    {
        currentLevel.gameObject.SetActive(true);

        currentLevel.Setup();

        Transform[] targets = currentLevel.GetTrackingTargets();

        splitRenders.SetTrackingTargets(targets);
    }

    void Update()
    {
        for (int i = 0; i < Level.PLAYERS; i++)
        {
            if (currentLevel.AnyKey())
            {
                wantsToPlay = true;
            }
        }

        wantsToPlay |= alwaysReady;

        UpdateHud();
    }

    void UpdateHud()
    {
        bool shouldLockHud = true;
        float split = 0.5f;

        for (int i = 0; i < Level.PLAYERS; i++)
        {
            string txt = string.Empty;

            if (wantsToPlay)
            {
                animaticTarget = 0f;

                if (currentLevel.IsPlayerReady(i))
                {
                    if (currentLevel.HasPlayerSpawned(i))
                    {
                        shouldLockHud = false;
                    }
                    else
                    {
                        txt = "WAITING FOR OTHER PLAYERS";
                        split = Mathf.Max(split, 1f);
                    }
                }
                else
                {
                    if (currentLevel.IsPlayerConnected(i))
                    {
                        txt = "READY UP";
                        split = Mathf.Max(split, 0.75f);
                    }
                    else
                    {
                        txt = "PLUG IN";
                        split = Mathf.Max(split, 0.5f);
                    }
                }
            }
            else
            {
                animaticTarget += Time.deltaTime;
                split = Mathf.Sin(animaticTarget * rotateIdleSpeed);
            }

            huds[i].SetText(txt);
        }

        if (shouldLockHud)
        {
            splitRenders.Lock(split);
        }
        else
        {
            splitRenders.Unlock();
        }
    }

}
