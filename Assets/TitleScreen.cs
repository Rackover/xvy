using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour {

    [SerializeField]
    private UnityEngine.UI.Text titleMesh;


    void Update()
    {
        if (Game.i.InGame)
        {
            if (titleMesh.enabled)
            {
                titleMesh.enabled = false;
            }
        }
        else
        {
            if (!titleMesh.enabled)
            {
                titleMesh.enabled = true;
            }
        }
    }
}
