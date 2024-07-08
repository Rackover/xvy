using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField]
    private Localization localization;

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
    private UnityEngine.UI.Image targetAcquisitionFiller;

    [SerializeField]
    private MaskableGraphic[] targetColorables;

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

    [SerializeField]
    private float minAcquisitionScreenDistance = 20;

    [SerializeField]
    private float maxAcquisitionScreenDistance = 300;

    [SerializeField]
    private Image aimTowardsArrow;

    [SerializeField]
    private Image missileDirectionImage;

    [SerializeField]
    private float missileDirectionDistance = 100f;

    public bool IsReadyForRebirth { get; private set; }

    private float deadAnimationTimer = 0f;

    private bool wasAlive = false;

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
                        if (!wasAlive)
                        {
                            whiteFade.color = Color.white;
                            whiteFade.enabled = true;
                        }

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
                        aimTargetingChild.enabled = false;
                        missileDirectionImage.enabled = false;

                        for (int i = 0; i < targetColorables.Length; i++)
                        {
                            targetColorables[i].enabled = false;
                        }
                    }

                    UpdateDeathAnimation(isAlive);
                    wasAlive = isAlive;

                    return;
                }
            }
        }

        wasAlive = false;
        warningText.enabled = false;
        missileDirectionImage.enabled = false;

        whiteFade.color = Color.white;
        whiteFade.enabled = false;

        trackerText.text = string.Empty;
        trackerImg.enabled = false;
        aimTargetingChild.enabled = false;

        aimRect.sizeDelta = minAimSize;
        rawImageRect.localScale = Vector3.one;

        aimGroup.alpha = 0f;

        for (int i = 0; i < targetColorables.Length; i++)
        {
            targetColorables[i].enabled = false;
        }

        IsReadyForRebirth = false;
    }

    private void UpdateWarnings()
    {
        if (Game.i.Level.IsPlayerBeingHomedTo(playerIndex))
        {
            warningText.enabled = true;
            warningText.color = Color.Lerp(Color.red, Color.white, Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed * Mathf.PI)));

            warningText.text = localization.Lang.MissileWarning + @"



" + localization.Lang.Evasion;


            missileDirectionImage.enabled = true;

            HomingMissile missile = Game.i.Level.GetPlayerWeapon(1 - playerIndex).HomingMissileAlive;

            if (missile)
            {
                // Should always be the case anyway
                Color color = Color.red;

                float ratio = 0f;
                float warningStartDistance = missileDirectionDistance;

                Transform myTransform = Game.i.Level.GetPlayerTransform(playerIndex);
                Vector3 relativeMissilePosition = myTransform.InverseTransformPoint(missile.transform.position);
                relativeMissilePosition.y = 0f;


                float dist = relativeMissilePosition.magnitude;
                relativeMissilePosition.Normalize();

                if (dist < warningStartDistance)
                {
                    ratio = 1f - dist / warningStartDistance;

                    Vector2 missileDirection = new Vector2(relativeMissilePosition.x, relativeMissilePosition.z);
                    float angle = Mathf.Atan2(missileDirection.x, -missileDirection.y);

                    missileDirectionImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle * Mathf.Rad2Deg);
                }

                color.a = ratio;

                missileDirectionImage.color = color;
            }
        }
        else
        {
            warningText.enabled = false;
            missileDirectionImage.enabled = false;
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
            if (!IsReadyForRebirth)
            {
                if (deadAnimationTimer < 1f)
                {
                    deadAnimationTimer += Time.deltaTime;
                }
                else if (rawImageRect.localScale.y > 0.01f)
                {
                    rawImageRect.localScale = new Vector3(rawImageRect.localScale.x, rawImageRect.localScale.y - Time.deltaTime * 3f, rawImageRect.localScale.z);
                }
                else if (rawImageRect.localScale.x > 0.01f)
                {
                    rawImageRect.localScale = new Vector3(rawImageRect.localScale.x - Time.deltaTime * 3f, 0f, rawImageRect.localScale.z);
                }
                else
                {
                    rawImageRect.localScale = new Vector3(1f, 1f, rawImageRect.localScale.z);

                    whiteFade.color = Color.white;
                    whiteFade.enabled = true;

                    IsReadyForRebirth = true;
                }
            }
        }
    }

    private static Color lightGreen = Color.Lerp(Color.green, Color.white, 0.5f);
    private void UpdateTargetTracking(int otherIndex, PlayerWeapon weapon, Camera myCamera)
    {
        Vector3 otherPosition = Game.i.Level.GetPlayerPosition(otherIndex);
        Vector3 myPosition = Game.i.Level.GetPlayerPosition(playerIndex);

        weapon.HasEnemyInAcquisitionSights = false;

        if (!Game.i.Level.IsPlayerAlive(otherIndex) ||
            Vector3.Dot(myCamera.transform.forward, (otherPosition - myPosition).normalized) < 0f ||
            !weapon.HasLineOfSightOnEnemy)
        {
            trackerImg.enabled = false;
            trackerText.enabled = false;
            aimTargetingChild.enabled = false;
            aimTowardsArrow.enabled = false;

            for (int i = 0; i < targetColorables.Length; i++)
            {
                targetColorables[i].enabled = false;
            }
        }
        else
        {
            trackerImg.enabled = true;
            trackerText.enabled = true;

            Vector2 acquisitionRectSize = new Vector2(32f, 32f) * Mathf.Lerp(sizeMultiplier, 1f, weapon.Acquisition01);

            weapon.HasEnemyInAcquisitionSights =
                !weapon.HomingMissileAlive &&
                (
                    weapon.TargetAcquired ||
                    Vector2.Distance(trackerImg.transform.position, aimRect.transform.position) < Mathf.Lerp(maxAcquisitionScreenDistance, minAcquisitionScreenDistance, weapon.Acquisition01)
                );

            float distance = Vector3.Distance(otherPosition, myPosition) / Game.i.Level.SpeedMultiplier;

            string distStr = distance.ToString("n0") + "m";
            string name = localization.Lang.Target;
            bool enableProgressBar = false;

            Color aimTrackerColor = Color.white;

            if (weapon.HomingMissileAlive)
            {
                name = localization.Lang.ByeBye;
                aimTargetingChild.enabled = true;
                aimTargetingChild.rectTransform.sizeDelta = new Vector2(32f, 32f) * Mathf.Lerp(1f, 1.2f, Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));


                aimTrackerColor = Color.Lerp(lightGreen, Color.white, Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));

                aimTargetingChild.color = aimTrackerColor;
                aimTargetingChild.rectTransform.localEulerAngles = Vector3.zero;
                trackerText.color = aimTargetingChild.color;

                distStr = ":)";
            }
            else if (weapon.TargetAcquired)
            {
                name = localization.Lang.Target;
                distStr = localization.Lang.Locked;

                aimTrackerColor = Color.Lerp(lightGreen, Color.white, Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));

                aimTargetingChild.enabled = true;
                aimTargetingChild.rectTransform.sizeDelta = new Vector2(32f, 32f);
                aimTargetingChild.rectTransform.localEulerAngles += new Vector3(0f, 0f, rotateAimTarget * Time.deltaTime);
                aimTargetingChild.color = aimTrackerColor;
                trackerText.color = aimTargetingChild.color;
            }
            else if (weapon.IsAcquiring)
            {
                name = localization.Lang.Acquiring;
                Color orange = Color.Lerp(Color.red, Color.yellow, 0.5f);
                trackerText.color = (Time.time * blinkSpeed) % 1f > 0.5f ? Color.white : orange;

                aimTrackerColor = Color.Lerp(Color.red, Color.yellow, Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));

                aimTrackerColor.a = Mathf.Sqrt(weapon.Acquisition01);

                aimTargetingChild.enabled = true;
                aimTargetingChild.rectTransform.sizeDelta = acquisitionRectSize;
                aimTargetingChild.rectTransform.localEulerAngles += new Vector3(0f, 0f, rotateAimTarget * Time.deltaTime);
                aimTargetingChild.color = aimTrackerColor;

                enableProgressBar = true;
                targetAcquisitionFiller.fillAmount = weapon.Acquisition01;

                distStr = string.Empty;
                //distStr = (weapon.Acquisition01 * 100).ToString("n0") + "%";
            }
            else
            {
                aimTargetingChild.enabled = false;
                trackerText.color = Color.white;
                aimTargetingChild.rectTransform.localEulerAngles = Vector3.zero;
            }

            string txt = name + (distStr == string.Empty ? distStr : "\n" + distStr);
            trackerText.text = txt;

            Vector3 viewPortPosition = myCamera.WorldToViewportPoint(otherPosition);
            trackerImg.rectTransform.anchoredPosition = Vector3.Scale(rawImageRect.sizeDelta, viewPortPosition);

            bool enemyAbove = otherPosition.y > myPosition.y;
            if (enemyAbove)
            {
                trackerText.alignment = TextAnchor.LowerRight;
                targetAcquisitionFiller.fillOrigin = 1;
            }
            else
            {
                trackerText.alignment = TextAnchor.UpperLeft;
                targetAcquisitionFiller.fillOrigin = 0;
            }

            for (int i = 0; i < targetColorables.Length; i++)
            {
                targetColorables[i].color = aimTrackerColor;
                targetColorables[i].enabled = enableProgressBar;
            }

            trackerImg.rectTransform.localEulerAngles = Vector3.Lerp(trackerImg.rectTransform.localEulerAngles, new Vector3(0f, 0f, enemyAbove ? 180f : 0f), Time.deltaTime * 20f);
            trackerText.rectTransform.localEulerAngles = -trackerImg.rectTransform.localEulerAngles;

            UpdateTrackerBar(enable: weapon.IsAcquiring && !weapon.HomingMissileAlive && !weapon.TargetAcquired);
        }
    }

    private void UpdateTrackerBar(bool enable)
    {
        RectTransform targetTracker = trackerImg.rectTransform;
        aimTowardsArrow.enabled = enable;

        if (aimTowardsArrow.isActiveAndEnabled)
        {
            Vector3 direction = (targetTracker.transform.position - aimTowardsArrow.transform.position);
            direction.z = 0f;
            Vector3 normalizedDirection = direction.normalized;

            float angle = Mathf.Atan2(-normalizedDirection.x, normalizedDirection.y) * Mathf.Rad2Deg;

            aimTowardsArrow.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
            aimTowardsArrow.rectTransform.sizeDelta = new Vector2(1f, direction.magnitude);
            aimTowardsArrow.color = aimTargetingChild.color;
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
