using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour {

    [SerializeField]
    private UnityEngine.UI.Text titleMesh;


    void Update()
    {
        if (Game.i.InGame)
        {
            titleMesh.enabled = false;
        }
        else
        {
            titleMesh.enabled = true;
        }
    }
}
