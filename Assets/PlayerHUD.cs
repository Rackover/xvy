using UnityEngine;
using System.Collections;

public class PlayerHUD : MonoBehaviour {

	[SerializeField]
	private UnityEngine.UI.Text textMesh;

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

	public void SetText(string text)
	{
		textMesh.text = text;
	}

	private void Update()
	{
		if (Game.i)
		{
			if (Game.i.Level)
			{
				PlayerWeapon weapon = Game.i.Level.GetPlayerWeapon(playerIndex);

				if (weapon && Game.i.Level.HasPlayerSpawned(playerIndex))
				{
					if (weapon.IsAiming)
					{
						aimGroup.alpha += Time.deltaTime;
                    }
					else
					{
                        aimGroup.alpha -= Time.deltaTime;
                    }

					aimRect.sizeDelta = Vector2.Lerp(minAimSize, maxAimSize, weapon.Imprecision01);
					aimRect.anchoredPosition = Vector2.Lerp(aimRect.anchoredPosition, 150 * weapon.AimingPosition, Time.deltaTime * 4f);

                    return;
				}
			}
		}

		aimRect.sizeDelta = minAimSize;

        aimGroup.alpha = 0f;
	}
}
