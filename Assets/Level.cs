using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Level : MonoBehaviour
{

    public const byte PLAYERS = 2;

    public event Action OnScoreChanged;

    [SerializeField]
    private Material skybox;

    [SerializeField]
    private bool fogEnabled = false;

    [SerializeField]
    private Color fogColor;

    [SerializeField]
    private float fogStart;

    [SerializeField]
    private float fogEnd;

    [SerializeField]
    private Player playerPrefab;

    [SerializeField]
    private Transform[] spawns = new Transform[PLAYERS];

    [SerializeField]
    private IdleCamera[] idleCameras = new IdleCamera[PLAYERS];

    [SerializeField]
    private Bounds boundaries;

    [SerializeField]
    private float maxOobTime = 3f;

    [SerializeField]
    private float speedMultiplier = 1f;

    public int Winner { get { return scores[0] > scores[1] ? 0 : 1; } }

    public float SpeedMultiplier { get { return speedMultiplier; } }

    public IList<int> Scores { get { return scores; } }
    public bool GameOver { get { return gameOver; } }

    private bool gameOver = false;

    private readonly Player[] players = new Player[PLAYERS];

    private readonly Transform[] trackingTargets = new Transform[PLAYERS];

    private readonly int[] scores = new int[PLAYERS];

    private readonly float[] oobTime = new float[PLAYERS];

    private void Awake()
    {
        Game.i.RegisterLevel(this);
    }

    public Transform[] GetTrackingTargets()
    {
        return trackingTargets;
    }

    public void Exit()
    {
        for (int i = 0; i < PLAYERS; i++)
        {
            if (players[i])
            {
                Destroy(players[i].gameObject);
            }
        }
    }
    
    public void Enter()
    {

        RenderSettings.fog = fogEnabled;
        RenderSettings.fogStartDistance = fogStart;
        RenderSettings.fogEndDistance = fogEnd;
        RenderSettings.fogDensity = fogColor.a;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.Linear;

        playerPrefab.gameObject.SetActive(false);
        gameOver = false;

        RenderSettings.skybox = skybox;

        for (int i = 0; i < PLAYERS; i++)
        {
            players[i] = Instantiate(playerPrefab);
            players[i].gameObject.SetActive(true);
            players[i].Initialize(i);

            scores[i] = 0;
            oobTime[i] = 0f;

            int index = i;
            players[i].OnKilled += () => OnPlayerKilled(index);

            Debug.Log("created player " + i);
        }

        UpdateTrackingTargets();

    }

    public void FillEmptySeats()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].GamepadConnected())
            {
                players[i].MakeMock();
            }
        }
    }

    private void OnPlayerKilled(int player)
    {
        if (GameOver)
        {
            players[player].Birth(spawns[player]);
        }
        else
        {
            scores[1 - player]++;


            if (OnScoreChanged != null)
            {
                OnScoreChanged.Invoke();
            }

            for (int i = 0; i < Scores.Count; i++)
            {
                if (Scores[i] >= Game.i.ScoreToWin)
                {
                    gameOver = true;
                    break;
                }
            }
        }
    }

    public bool IsOOB(int player, out float lifeRemaining01)
    {
        if (players[player])
        {
            lifeRemaining01 = 1f - oobTime[player] / maxOobTime;
            if (boundaries.Contains(players[player].Transform.position))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        lifeRemaining01 = 0f;
        return false;

    }

    public bool HasAlmostWon(int? player = null)
    {
        if (player.HasValue)
        {
            return scores[player.Value] == Game.i.ScoreToWin - 1;
        }

        for (int i = 0; i < PLAYERS; i++)
        {
            if (scores[i] == Game.i.ScoreToWin - 1)
            {
                return true;
            }
        }

        return false;
    }
    
    public bool WantsCredits()
    {
        for (int i = 0; i < PLAYERS; i++)
        {
            if (players[i] != null && players[i].WantsCredits())
            {
                return true;
            }
        }

        return false;
    }

    public bool AnyKey()
    {
        for (int i = 0; i < PLAYERS; i++)
        {
            if (players[i] != null && players[i].AnyKey())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPlayerConnected(int index)
    {
        return players[index] != null && players[index].GamepadConnected();
    }

    public bool IsPlayerBeingHomedTo(int index)
    {
        return players[index] != null && players[index].IsBeingHomedTo;
    }

    public PlayerWeapon GetPlayerWeapon(int index)
    {
        return players[index].Weapon;
    }

    public bool IsPlayerBoosting(int index)
    {
        return players[index] != null && players[index].IsBoosting;
    }

    public Transform GetPlayerTransform(int index)
    {
        return players[index].Transform;
    }

    public void PlaySoundOnPlayer(int index, AudioClip clip)
    {
        if (players[index] != null)
        {
            players[index].LocalSource.PlayOneShot(clip);
        }
    }

    public float GetPlayerGees(int index)
    {
        return players[index].Gees01;
    }

    public float GetPlayerSpeed(int index)
    {
        return players[index].Speed;
    }

    public Vector3 GetPlayerPosition(int index)
    {
        return players[index].Transform.position;
    }

    public void KillPlayerFromMissile(int index)
    {
        if (players[index])
        {
            players[index].NotifyKilled();
        }
    }

    public string GetPlayerDebugStateDump(int index)
    {
        return players[index].DebugDump;
    }

    public Camera GetPlayerCamera(int index)
    {
        return players[index].Camera;
    }

    public bool IsPlayerReady(int index)
    {
        return players[index] != null && players[index].IsReady;
    }

    public bool HasPlayerSpawned(int index)
    {
        return players[index] != null && players[index].IsSpawned;
    }

    public bool IsPlayerAlive(int index)
    {
        return players[index] != null && players[index].IsAlive;
    }

    public void RebirthPlayers()
    {
        for (int i = 0; i < PLAYERS; i++)
        {
            if (!players[i].IsSpawned || !players[i].IsAlive)
            {
                players[i].Birth(PickBestSpawn(i));
            }
        }
    }

    private Transform PickBestSpawn(int player)
    {
        int otherPlayer = 1 - player;

        if (!players[otherPlayer].IsAlive)
        {
            return spawns[player];
        }

        float bestSpawnDistance = float.NegativeInfinity;
        Transform bestSpawn = spawns[0];
        for (int i = 0; i < spawns.Length; i++)
        {
            float dist = Vector3.SqrMagnitude(spawns[i].position - players[otherPlayer].Transform.position);
            if (dist > bestSpawnDistance)
            {
                bestSpawn = spawns[i];
                bestSpawnDistance = dist;
            }
        }

        return bestSpawn;
    }

    void Update()
    {
        bool needsSpawn = true;
        for (int i = 0; i < PLAYERS; i++)
        {
            idleCameras[i].gameObject.SetActive(!players[i].IsSpawned);

            if (!IsPlayerReady(i))
            {
                needsSpawn = false;
            }
        }

        if (needsSpawn)
        {
            for (int i = 0; i < PLAYERS; i++)
            {
                if (!players[i].IsSpawned)
                {
                    players[i].Birth(spawns[i]);
                }
            }
        }

        UpdateOOB();
        UpdateTrackingTargets();
    }

    void UpdateOOB()
    {
        for (int i = 0; i < PLAYERS; i++)
        {
            if (players[i].IsAlive && players[i].IsReady)
            {
                bool oob = !boundaries.Contains(players[i].Transform.position);
                if (oob)
                {
                    oobTime[i] += Time.deltaTime;

                    if (oobTime[i] > maxOobTime)
                    {
                        players[i].NotifyOobDeath();
                        oobTime[i] = 0f;
                    }
                }
                else if (oobTime[i] > 0f)
                {
                    oobTime[i] -= Time.deltaTime;
                }
                else
                {
                    oobTime[i] = 0f;
                }
            }
            else
            {
                oobTime[i] = 0f;
            }
        }
    }

    private void UpdateTrackingTargets()
    {
        // Update tracking targets
        for (int i = 0; i < PLAYERS; i++)
        {
            if (players[i].IsSpawned)
            {
                trackingTargets[i] = players[i].Transform;
            }
            else
            {
                trackingTargets[i] = spawns[i];
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(boundaries.center, boundaries.size);
    }

#endif
}
