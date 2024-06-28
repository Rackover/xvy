using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    Player player;

    [SerializeField] private float minBoost = 9f;
    [SerializeField] private float maxBoost = 40f;
    [SerializeField] private float boostAcceleration = 5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float verticalStickMultiplier = -1f;

    [SerializeField]
    private float angleCap = 75f;

    [SerializeField]
    private float rollLerpSpeed = 7f;

    [SerializeField]
    private float pitchLerpSpeed = 4f;

    public bool IsBoosting { get { return gasPedal >= 0.5f; } }
    public float SpeedAmount { get { return speed / maxBoost; } }

    public Vector2 VirtualJoystick { get { return virtualStick; } }

    private float speed;
    private float gasPedal;
    private Vector2 virtualStick;
    private Vector2 realStick;

    void Awake()
    {
        player.OnSpawn += Player_OnSpawn;
    }

    private void Player_OnSpawn(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        transform.forward = direction;
    }

    void Update()
    {
        if (player.IsSpawned)
        {
            GrabInput();

            ApplyDirection();
            ApplyThrust();
        }
    }

    void GrabInput()
    {
        float gasAxis = player.IsBoosting ? 1f : (player.IsAiming ? 0f : 0.5f);
        //float gasAxis = (-Input.GetAxis("Gas")) * 0.5f + 0.5f;

        gasPedal = Mathf.Lerp(gasPedal, gasAxis, boostAcceleration * Time.deltaTime);

        realStick.x = player.StickDirection.x;
        realStick.y = verticalStickMultiplier * player.StickDirection.y;

        virtualStick.x = Mathf.Lerp(virtualStick.x, realStick.x, rollLerpSpeed * Time.deltaTime);
        virtualStick.y = Mathf.Lerp(virtualStick.y, realStick.y, pitchLerpSpeed * Time.deltaTime);

    }

    void ApplyThrust()
    {
        speed = minBoost + (maxBoost - minBoost) * gasPedal;
        transform.position += speed * Time.deltaTime * transform.forward;
    }

    void ApplyDirection()
    {
        if (virtualStick.sqrMagnitude > 0f)
        {
            float localRot = Mathf.Repeat(transform.eulerAngles.x + 180f, 360) - 180f;
            float yStick = virtualStick.y;
            if (localRot > angleCap)
            {
                // We're going down
                yStick = Mathf.Max(virtualStick.y, 0f);
            }
            else if (localRot < -angleCap)
            {
                yStick = Mathf.Min(virtualStick.y, 0f);
            }

            Vector2 fixedStick = new Vector2(
                virtualStick.x * (1f - Mathf.Abs(localRot) / 90f),
                yStick
            );

            transform.forward = Vector3.Lerp(transform.forward, transform.TransformDirection(fixedStick), rotationSpeed * Time.deltaTime);
        }

        //transform.Rotate(Vector3.forward, -virtualStick.x * rotationSpeed * Time.deltaTime);
        //transform.Rotate(Vector3.right, virtualStick.y * rotationSpeed * Time.deltaTime);
    }
}
