using UnityEngine;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField]
    private RectTransform rawImageRect;

    [SerializeField]
    private UnityEngine.UI.Text textMesh;

    [SerializeField]
    private UnityEngine.UI.Text titleMesh;

    [SerializeField]
    private UnityEngine.UI.Text trackerText;

    [SerializeField]
    private UnityEngine.UI.Text warningText;

    [SerializeField]
    private UnityEngine.UI.Image trackerImg;

    [SerializeField]
    private UnityEngine.UI.Image whiteFade;

    [SerializeField]
    private RectTransform splitRenderSize;

    [SerializeField]
    private RectTransform aimRect;

    [SerializeField]
    private UnityEngine.UI.Image aimTargetingChild;

    [SerializeField]
    private Vector2 minAimSize;

    [SerializeField]
    private Vector2 maxAimSize;

    [SerializeField]
    private CanvasGroup aimGroup;

    private int playerIndex;

    [SerializeField]
    private float rotateAimTarget = 360f;

    [SerializeField]
    private int sizeMultiplier = 10;

    [SerializeField]
    private int blinkSpeed = 10;

    public bool IsReadyForRebirth { get; private set; }

    private float deadAnimationTimer = 0f;

    public void SetID(int index)
    {
        this.playerIndex = index;
    }

    public void SetTitle(string text)
    {
        textMesh.text = string.Empty;
        titleMesh.text = text;

    }

    public void SetText(string text)
    {
        textMesh.text = text;
        titleMesh.text = string.Empty;

    }

    private void Update()
    {
        if (Game.i)
        {
            if (Game.i.Level)
            {
                bool boosting = Game.i.Level.IsPlayerBoosting(playerIndex);
                PlayerWeapon weapon = Game.i.Level.GetPlayerWeapon(playerIndex);

                bool hasSelfSpawned = Game.i.Level.HasPlayerSpawned(playerIndex);
                bool isAlive = Game.i.Level.IsPlayerAlive(playerIndex);
                if (hasSelfSpawned)
                {
                    if (isAlive)
                    {
                        UpdateWeaponReticle(boosting, weapon);
                        UpdateTargetTracking((playerIndex + 1) % 2, weapon, Game.i.Level.GetPlayerCamera(playerIndex));
                        UpdateWarnings();
                        IsReadyForRebirth = false;
                    }
                    else
                    {
                        aimGroup.alpha = 0f;
                        trackerImg.enabled = false;
                        trackerText.enabled = false;
                        warningText.enabled = false;
                    }

                    UpdateDeathAnimation(isAlive);

                    return;
                }
            }
        }

        warningText.enabled = false;

        whiteFade.color = Color.white;
        whiteFade.enabled = false;

        trackerText.text = string.Empty;
        trackerImg.enabled = false;
        aimTargetingChild.enabled = false;

        aimRect.sizeDelta = minAimSize;
        rawImageRect.localScale = Vector3.one;

        aimGroup.alpha = 0f;

        IsReadyForRebirth = false;
    }

    static readonly Color transparentWhite = new Color(1f, 1f, 1f, 0f);

    private void UpdateWarnings()
    {
        if (Game.i.Level.IsPlayerBeingHomedTo(playerIndex))
        {
            warningText.enabled = true;
            warningText.color = Color.Lerp(Color.red, transparentWhite, Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));

            warningText.text = @"/!\ MISSILE WARNING /!\



PLEASE PERFORM EVASION MANEUVERS";
        }
        else
        {
            warningText.enabled = false;
        }
    }

    private void UpdateDeathAnimation(bool isAlive)
    {
        if (isAlive)
        {
            rawImageRect.localScale = Vector3.one;

            if (whiteFade.color.a > 0f)
            {
                whiteFade.color = new Color(whiteFade.color.r, whiteFade.color.g, whiteFade.color.b, whiteFade.color.a - Time.deltaTime);
            }

            deadAnimationTimer = 0f;
        }
        else
        {
            if (deadAnimationTimer < 1f)
            {
                deadAnimationTimer += Time.deltaTime;
            }
            else if (rawImageRect.localScale.y > 0.01f)
            {
                rawImageRect.localScale = new Vector3(rawImageRect.localScale.x , rawImageRect.localScale.y - Time.deltaTime * 3f, rawImageRect.localScale.z);
            }
            else if (rawImageRect.localScale.x > 0.01f)
            {
                rawImageRect.localScale = new Vector3(rawImageRect.localScale.x - Time.deltaTime * 3f, 0f, rawImageRect.localScale.z);
            }
            else
            {
                rawImageRect.localScale = new Vector3(0f, 0f, rawImageRect.localScale.z);

                whiteFade.color = Color.white;
                IsReadyForRebirth = true;
            }
        }
    }

    private static Color lightGreen = Color.Lerp(Color.green, Color.white, 0.5f);
    private void UpdateTargetTracking(int otherIndex, PlayerWeapon weapon, Camera myCamera)
    {
        Vector3 otherPosition = Game.i.Level.GetPlayerPosition(otherIndex);
        Vector3 myPosition = Game.i.Level.GetPlayerPosition(playerIndex);

        weapon.HasEnemyInAcquisitionSights = false;

        if (!Game.i.Level.IsPlayerAlive(otherIndex) || Vector3.Dot(myCamera.transform.forward, (otherPosition - myPosition).normalized) < 0f)
        {
            trackerImg.enabled = false;
            trackerText.enabled = false;
            aimTargetingChild.enabled = false;
        }
        else
        {
            trackerImg.enabled = true;
            trackerText.enabled = true;

            weapon.HasEnemyInAcquisitionSights = true;

            float distance = Vector3.Distance(otherPosition, myPosition);

            string distStr = distance.ToString("n0") + "m";
            string name = "TARGET";

            if (weapon.HomingMissileAlive)
            {
                name = "BYE-BYE";
                aimTargetingChild.enabled = true;
                aimTargetingChild.rectTransform.sizeDelta = new Vector2(32f, 32f) * Mathf.Lerp(1f, 1.2f, Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));


                var aimTrackerColor = Color.Lerp(lightGreen, Color.white, Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));

                aimTargetingChild.color = aimTrackerColor;
                aimTargetingChild.rectTransform.localEulerAngles = Vector3.zero;
                trackerText.color = aimTargetingChild.color;

                distStr = ":)";
            }
            else if (weapon.TargetAcquired)
            {
                name = "TARGET";
                distStr = "LOCKED";

                var aimTrackerColor = Color.Lerp(lightGreen, Color.white, Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));

                aimTargetingChild.enabled = true;
                aimTargetingChild.rectTransform.sizeDelta = new Vector2(32f, 32f);
                aimTargetingChild.rectTransform.localEulerAngles += new Vector3(0f, 0f, rotateAimTarget * Time.deltaTime);
                aimTargetingChild.color = aimTrackerColor;
                trackerText.color = aimTargetingChild.color;
            }
            else if (weapon.IsAcquiring)
            {
                name = "ACQUIRING";
                Color orange = Color.Lerp(Color.red, Color.yellow, 0.5f);
                trackerText.color = (Time.time * blinkSpeed) % 1f > 0.5f ? Color.white : orange;

                var aimTrackerColor = Color.Lerp(Color.red, Color.yellow, Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));

                aimTrackerColor.a = Mathf.Sqrt(weapon.Acquisition01);

                aimTargetingChild.enabled = true;
                aimTargetingChild.rectTransform.sizeDelta = new Vector2(32f, 32f) * Mathf.Lerp(sizeMultiplier, 1f, weapon.Acquisition01);
                aimTargetingChild.rectTransform.localEulerAngles += new Vector3(0f, 0f, rotateAimTarget * Time.deltaTime);
                aimTargetingChild.color = aimTrackerColor;

                distStr = (weapon.Acquisition01 * 100).ToString("n0") + "%";
            }
            else
            {
                aimTargetingChild.enabled = false;
                trackerText.color = Color.white;
                aimTargetingChild.rectTransform.localEulerAngles = Vector3.zero;
            }

            string txt = name + "\n" + distStr;
            trackerText.text = txt;

            Vector3 viewPortPosition = myCamera.WorldToViewportPoint(otherPosition);
            trackerImg.rectTransform.anchoredPosition = Vector3.Scale(rawImageRect.sizeDelta, viewPortPosition);

            bool enemyAbove = otherPosition.y > myPosition.y;
            if (enemyAbove)
            {
                trackerText.alignment = TextAnchor.LowerRight;
            }
            else
            {
                trackerText.alignment = TextAnchor.UpperLeft;
            }

            trackerImg.rectTransform.localEulerAngles = Vector3.Lerp(trackerImg.rectTransform.localEulerAngles, new Vector3(0f, 0f, enemyAbove ? 180f : 0f), Time.deltaTime* 20f);
            trackerText.rectTransform.localEulerAngles = -trackerImg.rectTransform.localEulerAngles;
        }
    }

    private void UpdateWeaponReticle(bool boosting, PlayerWeapon weapon)
    {
        if (boosting)
        {
            aimGroup.alpha -= Time.deltaTime;
        }
        else
        {
            aimGroup.alpha += Time.deltaTime;
        }

        aimRect.sizeDelta = Vector2.Lerp(minAimSize, maxAimSize, weapon.Imprecision01);
        aimRect.anchoredPosition = Vector2.Lerp(aimRect.anchoredPosition, weapon.AimingAmplitude * weapon.AimingPosition, Time.deltaTime * 4f);

    }
}
