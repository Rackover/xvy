using UnityEngine;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{

    [SerializeField]
    private UnityEngine.UI.Text textMesh;

    [SerializeField]
    private UnityEngine.UI.Text titleMesh;

    [SerializeField]
    private UnityEngine.UI.Text trackerText;

    [SerializeField]
    private UnityEngine.UI.Image trackerImg;

    [SerializeField]
    private RectTransform splitRenderSize;

    [SerializeField]
    private RectTransform aimRect;

    [SerializeField]
    private Vector2 minAimSize;

    [SerializeField]
    private Vector2 maxAimSize;

    [SerializeField]
    private CanvasGroup aimGroup;

    [SerializeField]
    private int playerIndex;

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
                if (hasSelfSpawned)
                {
                    UpdateWeaponReticle(boosting, weapon);
                    UpdateTargetTracking((playerIndex + 1) % 2, weapon, Game.i.Level.GetPlayerCamera(playerIndex));
                    return;
                }
            }
        }

        trackerText.text = string.Empty;
        trackerImg.enabled = false;

        aimRect.sizeDelta = minAimSize;

        aimGroup.alpha = 0f;
    }

    private void UpdateTargetTracking(int otherIndex, PlayerWeapon weapon, Camera myCamera)
    {

        Vector3 otherPosition = Game.i.Level.GetPlayerPosition(otherIndex);
        Vector3 myPosition = Game.i.Level.GetPlayerPosition(playerIndex);

        if (Vector3.Dot(myCamera.transform.forward, (otherPosition - myPosition).normalized) < 0f)
        {
            trackerImg.enabled = false;
            trackerText.enabled = false;
        }
        else
        {
            trackerImg.enabled = true;
            trackerText.enabled = true;

            float distance = Vector3.Distance(otherPosition, myPosition);

            string distStr = distance.ToString("n0") + "m";
            string name = "TARGET";

            if (distance < weapon.AcquisitionDistanceMeters)
            {
                name = "ACQUIRING";
                trackerText.color = (Time.time * 5f) % 1f > 0.5f ? Color.white : new Color();
            }
            else
            {
                trackerText.color = Color.white;
            }

            string txt = name + "\n" + distStr;
            trackerText.text = txt;

            Vector3 viewPortPosition = myCamera.WorldToViewportPoint(otherPosition);
            trackerImg.rectTransform.anchoredPosition = Vector3.Scale(splitRenderSize.sizeDelta, viewPortPosition);
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
        aimRect.anchoredPosition = Vector2.Lerp(aimRect.anchoredPosition, 150 * weapon.AimingPosition, Time.deltaTime * 4f);

    }
}
