using UnityEngine;
using System.Collections;


public class Level : MonoBehaviour {

	public const byte PLAYERS = 2;

	[SerializeField]
	private Material skybox;

	[SerializeField]
	private Player playerPrefab;

	[SerializeField]
	private Transform[] spawns = new Transform[PLAYERS];

	[SerializeField]
	private IdleCamera[] idleCameras = new IdleCamera[PLAYERS];

	private readonly Player[] players = new Player[PLAYERS];

	private readonly Transform[] trackingTargets = new Transform[PLAYERS];

	public Transform[] GetTrackingTargets()
	{
		return trackingTargets;
	}

	public void Setup()
	{
		playerPrefab.gameObject.SetActive(false);

		RenderSettings.skybox = skybox;

        for (int i = 0; i < PLAYERS; i++)
		{
			if (players[i])
			{
				Destroy(players[i].gameObject);
			}

			players[i] = Instantiate(playerPrefab);
			players[i].gameObject.SetActive(true);
			players[i].Initialize(i);
        }

		UpdateTrackingTargets();

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

	public PlayerWeapon GetPlayerWeapon(int index)
	{
		return players[index].Weapon;
	}

	public bool IsPlayerBoosting(int index)
    {
		return players[index] != null && players[index].IsBoosting;
    }

    public Vector3 GetPlayerForward(int index)
    {
        return players[index].Transform.forward;
    }

    public Vector3 GetPlayerPosition(int index)
	{
		return players[index].Transform.position;
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
					players[i].Spawn(spawns[i]);
				}
			}
		}

		UpdateTrackingTargets();
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
}
