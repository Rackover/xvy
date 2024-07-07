using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour {

    [SerializeField]
    private UnityEngine.UI.Text titleMesh;

    [SerializeField]
    private float maxLifespan = 0f;

    private float currentLife = 0f;

    void Awake()
    {
        currentLife = maxLifespan;
    }


    void Update()
    {
        if (Game.i.InGame)
        {
            if (titleMesh.enabled)
            {
                titleMesh.enabled = false;
            }

            currentLife = maxLifespan;
        }
        else
        {
            if ( titleMesh.enabled)
            {
                if (maxLifespan > 0f && currentLife > 0f)
                {
                    currentLife -= Time.deltaTime;
                }

                if (maxLifespan > 0f)
                {
                    titleMesh.color = new Color(1f, 1f, 1f, currentLife / maxLifespan);
                }
            }
            else
            {
                titleMesh.enabled = true;
            }
        }
    }
}
