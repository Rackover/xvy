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
    private string[] scenes;

    [SerializeField]
    private string[] mapNames;

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
    private float idleTimeBeforeMapSwitch = 60f;

    [SerializeField]
    private AudioClip wantsToPlayClip;

    [SerializeField]
    private RenderTexture[] texes;

    [Header("Debug switches")]
    [SerializeField]
    private bool alwaysReady = false;

    [SerializeField]
    private bool fillEmptySeats = false;

    [SerializeField]
    private bool showPerformanceInfo = false;

    [SerializeField]
    private bool alwaysHoming = false;

    [SerializeField]
    private bool allowMasterControl = true;

    [SerializeField]
    private bool forceFrog = false;

    public AudioSource GeneralAudioSource { get { return generalAudioSource; } }

    public float HorizontalSplitAmount { get { return splitRenders.HorizontalAmount; } }

    public float Flip01 { get { return splitRenders.Flip01; } }

    public IList<RenderTexture> Texes { get { return texes; } }

    public int ScoreToWin { get { return scoreToWin; } }

    public Level Level { get { return currentLevel; } }

    public string LevelName { get; private set; }

    public bool IsLoading{ get; private set; }

    public bool InGame { get { return wantsToPlay; } }

    public bool Playing { get { return currentLevel && InGame && Level.IsPlayerReady(0) && Level.IsPlayerReady(1); } }

    public bool AlwaysReady { get { return alwaysReady; } }

    public bool AlwaysHoming { get { return alwaysHoming; } }

    public bool ShowingCredits { get; private set; }

    public bool ShowPerformanceInfo { get { return showPerformanceInfo; } }

    public bool FrogForced { get { return forceFrog; } }

    public bool FillEmptySeats { get { return fillEmptySeats; } }

    private bool wantsToPlay = false;

    private Level currentLevel;

    private string currentScene;

    private int levelIndex = 0;

    private float animaticTarget = 0f;

    private float gameOverTimer = 0f;

    private float idleTimer = 0f;

    private float readyTimer = 0f;

    private bool wasPressingCreditsInput = false;

    void Awake()
    {
#if DEBUG


#else
        fillEmptySeats = false;
        forceFrog = false;
        alwaysReady = false;
        allowMasterControl = false;
        showPerformanceInfo = false;
#endif

#if UNITY_EDITOR


#else
        alwaysReady = false;
#endif

        i = this;

#if UNITY_WEBGL
        generalAudioSource.volume = 0.4f;
#endif

        // Shuffle level but first is first
        if (scenes.Length > 0)
        {
            Dictionary<string, string> sceneToName = new Dictionary<string, string>(scenes.Length);
            Dictionary<string, float> weights = new Dictionary<string, float>(scenes.Length);
            for (int index = 0; index < scenes.Length; index++)
            {
                sceneToName.Add(scenes[index], mapNames[index]);
                weights.Add(scenes[index], UnityEngine.Random.value);
            }

            List<string> levelsToShuffle = new List<string>(scenes);
            levelsToShuffle.RemoveAt(0);
            levelsToShuffle.Sort((a, b) => weights[a].CompareTo(weights[b]));

            levelsToShuffle.CopyTo(scenes, 1);

            for (int index = 0; index < scenes.Length; index++)
            {
                mapNames[index] = sceneToName[scenes[index]];
            }

        }

        if (allowMasterControl)
        {
            StartCoroutine(ListenToMaster());
        }

#if UNITY_5_4_1 && !UNITY_PS3
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
#else
        currentLevel = null;
        StartCoroutine(EnterLevelAsap());
#endif

        currentScene = scenes[levelIndex];
        this.LevelName = mapNames[levelIndex];

        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
        IsLoading = true;

        for (int playerIndex = 0; playerIndex < huds.Length; playerIndex++)
        {
            huds[playerIndex].SetID(playerIndex);
        }

        UnityEngine.Object.DontDestroyOnLoad(this);
    }

    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        if (arg0.name == currentScene)
        {
            StartCoroutine(EnterLevelAsap());
        }
    }

    IEnumerator WaitForXboxController()
    {
#if UNITY_EDITOR
        yield return new WaitForSeconds(1f);
#elif UNITY_XENON
        bool userLoggedIn = false;
        X360Core.BasicDelegate calledUserStateChanged = () =>
        {
            userLoggedIn = true;
            X360Core.OnControllerStateChange = null;
        };

        X360Core.BasicDelegateController calledControllerStateChanged = (uint index, bool connected) =>
        {
            if (connected == true)
            {
                userLoggedIn = true;
            }

            X360Core.OnControllerStateChange = null;
        };

        X360Core.OnUserStateChange = calledUserStateChanged;
        X360Core.OnControllerStateChange = calledControllerStateChanged;

        float t = Time.time;
        while (!userLoggedIn && !X360Core.GetUserLocalPlayerId(0).IsValid)
        {
            yield return null;
        
            if (Time.time - t > 1f)
            {
                break;
            }
        }
#endif

    }

    IEnumerator EnterLevelAsap()
    {
        while(currentLevel == null)
        {
            yield return null;
        }

        yield return WaitForXboxController();

        EnterLevel();
    }

    public void RegisterLevel(Level level)
    {
        Debug.Log("Registered level "+ level.name);
        currentLevel = level;
    }

    private IEnumerator ListenToMaster()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        bool needRelease = false;

        PlayerInput masterInput;

        masterInput = PlayerInput.MakeForPlatform();
        masterInput.SetPlayerIndex(0);

        while (true)
        {
            masterInput.Refresh();

            if (needRelease)
            {
                // Do nothing
                if (!masterInput.AnyKey())
                {
                    needRelease = false;
                }
            }
            else
            {
                if (masterInput.GetDPad().x > 0.8f)
                {
                    if (!Performance.i.IsPerformanceDisplayed)
                    {
                        needRelease = true;
                        NextLevel();
                    }
                }
                else if (masterInput.GetDPad().x < -0.8f)
                {
                    if (!Performance.i.IsPerformanceDisplayed)
                    {
                        needRelease = true;
                        Level.FillEmptySeats();
                    }
                }
                else if (masterInput.IsPressingRS())
                {
                    needRelease = true;
                    if (Performance.i)
                    {
                        Performance.i.Toggle();
                    }
                }
            }

            yield return wait;
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
        splitRenders.Unlock();
        wantsToPlay = false;

        IsLoading = false;

        if (OnLevelLoaded != null)
        {
            OnLevelLoaded.Invoke();
        }
    }

    [ContextMenu("Next level")]
    private void NextLevel()
    {
        ExitLevel();

        levelIndex = (levelIndex + 1) % scenes.Length;
        currentScene = scenes[levelIndex];
        this.LevelName = mapNames[levelIndex];

        this.IsLoading = true;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(currentScene);
    }

    void Update()
    {
        bool canSeeCredits = !Playing && !InGame;
        if (currentLevel.WantsCredits() && canSeeCredits)
        {
            if (wasPressingCreditsInput)
            {
                // Wait for release
            }
            else
            { 
                wasPressingCreditsInput = true;
                ShowingCredits = !ShowingCredits;
            }
        }
        else
        {
            ShowingCredits &= canSeeCredits;

            wasPressingCreditsInput = false;

            if (currentLevel.AnyKey())
            {
                if (!wantsToPlay && !ShowingCredits)
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
                if (Game.i.ShowingCredits)
                {
                    split = 0f;
                    huds[i].SetTitle(string.Empty);
                }
                else
                {
                    bothPlayersAlive = false;
                    animaticTarget += Time.deltaTime;
                    split = Mathf.Sin(animaticTarget * rotateIdleSpeed);

                    huds[i].SetTitle(i == 0 ? "X" : "Y");
                }
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

        if (idleTimer > idleTimeBeforeMapSwitch)
        {
            idleTimer = 0f;
            NextLevel();
        }
        else if (readyTimer > 15f)
        {
            readyTimer = 0f;
            wantsToPlay = false;
        }

        if (shouldLockHud && !bothPlayersAlive)
        {
            splitRenders.Lock(split);
        }
        else
        {
            splitRenders.Unlock();
        }
    }

}
