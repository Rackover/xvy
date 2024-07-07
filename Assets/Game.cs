using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
    public static Game i;

    public event Action OnLevelLoaded;

    [SerializeField]
    private Localization localization;

    [SerializeField]
    private Fade fade;

    [SerializeField]
    private AudioSource generalAudioSource;

    [SerializeField]
    private Level[] levels;

    [SerializeField]
    private SplitRenders splitRenders;

    [SerializeField]
    private PlayerHUD[] huds = new PlayerHUD[Level.PLAYERS];

    [SerializeField]
    private ScoreAnimation scoreAnim;

    [SerializeField]
    private int scoreToWin = 5;

    [SerializeField]
    private float rotateIdleSpeed = 0.5f;

    [SerializeField]
    private float gameOverDuration = 8f;

    [SerializeField]
    private AudioClip wantsToPlayClip;

    [SerializeField]
    private RenderTexture[] texes;

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
    private bool allowMasterControl = true;

    [SerializeField]
    private bool forceFrog = false;

    public AudioSource GeneralAudioSource { get { return generalAudioSource; } }

    public IList<RenderTexture> Texes { get { return texes; } }

    public int ScoreToWin { get { return scoreToWin; } }

    public Level Level { get { return currentLevel; } }

    public bool InGame { get { return wantsToPlay; } }

    public bool Playing { get { return currentLevel && InGame && Level.IsPlayerReady(0) && Level.IsPlayerReady(1); } }

    public bool AlwaysReady { get { return alwaysReady; } }

    public bool AlwaysHoming { get { return alwaysHoming; } }

    public bool ShowPerformanceInfo { get { return showPerformanceInfo; } }

    public bool FrogForced { get { return forceFrog; } }

    public bool EmulateP2 { get { return emulateP2; } }

    private bool wantsToPlay = false;

    private Level currentLevel;

    private int levelIndex = 0;

    private float animaticTarget = 0f;

    private float gameOverTimer = 0f;

    private float idleTimer = 0f;

    private float readyTimer = 0f;

    private PlayerInput masterInput;

    void Awake()
    {
#if UNITY_EDITOR


#else
        alwaysReady = false;
#endif

        i = this;

        if (allowMasterControl)
        {
            StartCoroutine(ListenToMaster());
        }

        currentLevel = levels[levelIndex];

        for (int playerIndex = 0; playerIndex < huds.Length; playerIndex++)
        {
            huds[playerIndex].SetID(playerIndex);
        }

        EnterLevel();
    }

    private IEnumerator ListenToMaster()
    {
        masterInput = PlayerInput.MakeForPlatform();
        masterInput.SetPlayerIndex(0);

        while (true)
        {
            masterInput.Refresh();
            if (masterInput.GetDPad().x > 0.8f)
            {
                NextLevel();
            }

            yield return new WaitForSeconds(0.6f);
        }
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

    private void ExitLevel()
    {
        idleTimer = 0f;

        currentLevel.Exit();
        currentLevel.gameObject.SetActive(false);
        wantsToPlay = false;
    }

    private void EnterLevel()
    {
        currentLevel.gameObject.SetActive(true);

        currentLevel.Enter();

        gameOverTimer = 0f;

        Transform[] targets = currentLevel.GetTrackingTargets();

        splitRenders.SetTrackingTargets(targets);

        if (OnLevelLoaded != null)
        {
            OnLevelLoaded.Invoke();
        }
    }

    [ContextMenu("Next level")]
    private void NextLevel()
    {
        ExitLevel();

        levelIndex = (levelIndex + 1)% levels.Length;
        currentLevel = levels[levelIndex];

        EnterLevel();
    }

    void Update()
    {
        for (int i = 0; i < Level.PLAYERS; i++)
        {
            if (currentLevel.AnyKey())
            {
                if (!wantsToPlay)
                {
                    wantsToPlay = true;
                    GeneralAudioSource.PlayOneShot(wantsToPlayClip);
                }

                idleTimer = 0f;
            }
        }

        wantsToPlay |= alwaysReady;

        UpdateHud();

        if (fade.IsAnimating)
        {
            return;
        }

        if (currentLevel.GameOver)
        {
            gameOverTimer += Time.deltaTime;

            if (gameOverTimer > gameOverDuration)
            {
                fade.FadeTransition(NextLevel);
            }
        }
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
                        if (currentLevel.GameOver)
                        {
                            if (currentLevel.Winner == i)
                            {
                                shouldLockHud = true;
                                bothPlayersAlive = false;
                                split = i;

                                if (!scoreAnim.IsAnimating)
                                {
                                    int winnerScore = currentLevel.Scores[currentLevel.Winner];
                                    int loserScore = currentLevel.Scores[1 - currentLevel.Winner];

                                    if (winnerScore - loserScore <= 1)
                                    {
                                        txt += localization.Lang.CloseOne + "\n\n";
                                    }
                                    else
                                    {
                                        txt += localization.Lang.WellPlayed + "\n\n";
                                    }

                                    txt += localization.Lang.GoodGame;
                                }
                            }
                        }
                        else if (currentLevel.IsPlayerAlive(i))
                        {

                        }
                        else
                        {
                            bothPlayersAlive = false;
                            shouldLockHud = true;
                            split = 0.5f;

                            if (huds[i].IsReadyForRebirth && !scoreAnim.IsAnimating && !currentLevel.GameOver)
                            {
                                Game.i.Level.RebirthPlayers();
                            }
                        }
                    }
                    else
                    {
                        bothPlayersAlive = false;
                        txt = currentLevel.IsPlayerReady(1 - i) ? string.Empty : localization.Lang.WaitingForOtherPlayers;
                        split = Mathf.Max(split, 1f);
                    }
                }
                else
                {
                    bothPlayersAlive = false;
                    if (currentLevel.IsPlayerConnected(i))
                    {
                        txt = localization.Lang.ReadyUp;
                        split = Mathf.Max(split, 0.75f);
                    }
                    else
                    {
                        txt = Mathf.Sin(Time.time * 10f) > 0f ? string.Empty : localization.Lang.PlugIn;
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

        if (wantsToPlay)
        {
            idleTimer = 0f;

            if (Playing)
            {
                readyTimer = 0f;
            }
            else
            {
                readyTimer += Time.deltaTime;
            }
        }
        else
        {
            idleTimer += Time.deltaTime;
        }

        if (idleTimer > 60f)
        {
            idleTimer = 0f;
            NextLevel();
        }
        else if (readyTimer > 15f)
        {
            readyTimer = 0f;
            wantsToPlay = false;
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
