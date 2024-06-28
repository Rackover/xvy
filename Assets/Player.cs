using UnityEngine;
using System.Collections;
using System;

public class Player : MonoBehaviour {

	public event Action<Vector3, Vector3> OnSpawn;

	[SerializeField]
	private Transform playerMovement;

	[SerializeField]
	private PlayerWeapon weapon;

	[SerializeField]
	private PlayerCamera playerCamera;

	public int Index { get { return index; } }

	public bool IsReady { get; private set; }

	public bool IsSpawned { get; private set; }

	public bool IsAiming { get; private set; }
	public bool IsBoosting { get; private set; }
	public bool IsShooting {  get; private set; }

	public Vector2 StickDirection { get; private set; }

	public Transform Transform { get { return playerMovement; } }

	public PlayerWeapon Weapon { get { return weapon; } }

	public Camera Camera { get { return playerCamera.Camera; } }

	private IPlayerInput input;

	private int index;

	public void Initialize(int index)
    {
		if (input != null)
		{
			input.Dispose();
        }

//        input = new XInputDLLPlayerInput();
        input = new MockInput();
        
        input.SetPlayerIndex(index);

		this.index = index;
		name = "PLAYER #" + index;

		IsReady = Game.AlwaysReady;
		IsSpawned = false;
		IsAiming = false;
		IsBoosting = false;
		IsShooting = false;
		StickDirection = new Vector2();
    }

	public bool GamepadConnected() { return input.GamepadPresent() || Game.AlwaysReady; }

	public bool AnyKey() { return input.AnyKey(); }

	public void Spawn(Transform spawner)
	{
		Debug.Log("Spawning "+name+" on "+spawner);

		if (OnSpawn != null)
		{
			OnSpawn.Invoke(spawner.position, spawner.forward);
		}

		IsSpawned = true;
    }

	private void OnDestroy()
	{
		if (input != null)
		{
			input.Dispose();
		}
	}

	private void Update()
	{
		if (input == null)
		{
			return;
		}

		input.Refresh();

		if (IsSpawned)
		{

			float aimAxis = input.LeftTrigger();
			float shootAxis = input.RightTrigger();
			StickDirection = input.GetDirection();


			if (aimAxis > 0f)
			{
				IsAiming = true;
			}
			else
			{
				IsAiming = false;
			}

			if (shootAxis > 0f)
			{
				if (IsAiming)
				{
					if (IsBoosting)
					{
						// Do nothing
					}
					else
					{
						IsShooting = true;
					}
				}
				else
				{
					IsBoosting = true;
				}
			}
			else
			{
				IsShooting = false;
				IsBoosting = false;
			}
		}
		else
		{
			if (!IsReady)
			{
				IsReady = input.IsPressingStart();
            }
		}
    }
}
