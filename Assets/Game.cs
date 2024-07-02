using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
    public static Game i;

    public event Action OnLevelLoaded;

    [SerializeField]
    private Level[] levels;

    [SerializeField]
    private SplitRenders splitRenders;

    [SerializeField]
    private PlayerHUD[] huds = new PlayerHUD[Level.PLAYERS];

    [SerializeField]
    private ScoreAnimation scoreAnim;

    [SerializeField]
    private float rotateIdleSpeed = 0.5f;

    [Header("Debug switches")]
    [SerializeField]
    private bool alwaysReady = false;

    [SerializeField]
    private bool emulateP2 = false;

    [SerializeField]
    private bool showPerformanceInfo = false;

    [SerializeField]
    private bool alwaysHoming = false;

    [SerializeField]
    private RenderTexture[] texes;

    public IList<RenderTexture> Texes { get { return texes; } }

    public Level Level { get { return currentLevel; } }

    public bool InGame { get { return wantsToPlay; } }

    public bool AlwaysReady { get { return alwaysReady; } }

    public bool AlwaysHoming { get { return alwaysHoming; } }

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

        for (int playerIndex = 0; playerIndex < huds.Length; playerIndex++)
        {
            huds[playerIndex].SetID(playerIndex);
        }

        InitLevel();
    }

    public void ReplaceTexture(RenderTexture rt, RenderTexture newRt)
    {
        for (int i = 0; i < texes.Length; i++)
        {
            if (texes[i] == rt)
            {
                texes[i] = newRt;
                break;
            }
        }
    }

    private void InitLevel()
    {
        currentLevel.gameObject.SetActive(true);

        currentLevel.Setup();

        Transform[] targets = currentLevel.GetTrackingTargets();

        splitRenders.SetTrackingTargets(targets);

        if (OnLevelLoaded != null)
        {
            OnLevelLoaded.Invoke();
        }
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
        bool bothPlayersAlive = true;
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
                        if (currentLevel.IsPlayerAlive(i))
                        {

                        }
                        else
                        {
                            bothPlayersAlive = false;
                            shouldLockHud = true;
                            split = 0.5f;

                            if (huds[i].IsReadyForRebirth && !scoreAnim.IsAnimating)
                            {
                                Game.i.Level.RebirthPlayers();
                            }
                        }
                    }
                    else
                    {
                        bothPlayersAlive = false;
                        txt = "WAITING FOR OTHER PLAYERS";
                        split = Mathf.Max(split, 1f);
                    }
                }
                else
                {
                    bothPlayersAlive = false;
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
                bothPlayersAlive = false;
                animaticTarget += Time.deltaTime;
                split = Mathf.Sin(animaticTarget * rotateIdleSpeed);

                huds[i].SetTitle(i == 0 ? "X" : "Y");
            }
        }

        if (shouldLockHud  && !bothPlayersAlive)
        {
            splitRenders.Lock(split);
        }
        else
        {
            splitRenders.Unlock();
        }
    }

}
