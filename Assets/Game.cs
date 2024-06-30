using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
    public static Game i;

    [SerializeField]
    private Level[] levels;

    [SerializeField]
    private SplitRenders splitRenders;

    [SerializeField]
    private PlayerHUD[] huds = new PlayerHUD[Level.PLAYERS];

    [SerializeField]
    private float rotateIdleSpeed = 0.5f;

    [Header("Debug switches")]
    [SerializeField]
    private bool alwaysReady = false;

    [SerializeField]
    private bool emulateP2 = false;

    [SerializeField]
    private bool showPerformanceInfo = false;

    public Level Level { get { return currentLevel; } }

    public bool InGame { get { return wantsToPlay; } }

    public bool AlwaysReady { get { return alwaysReady; } }

    public bool ShowPerformanceInfo { get { return showPerformanceInfo; } }

    public bool EmulateP2 { get { return emulateP2; } }

    private bool wantsToPlay = false;

    private Level currentLevel;

    private float animaticTarget = 0f;

    void Awake()
    {
#if UNITY_EDITOR


#else
        alwaysReady = false;
        emulateP2 = false;
#endif

        i = this;

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
            if (wantsToPlay)
            {
                string txt = string.Empty;

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

                huds[i].SetText(txt);
            }
            else
            {
                animaticTarget += Time.deltaTime;
                split = Mathf.Sin(animaticTarget * rotateIdleSpeed);

                huds[i].SetTitle(i == 0 ? "X" : "Y");
            }
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
