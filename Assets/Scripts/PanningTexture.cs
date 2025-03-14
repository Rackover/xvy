using UnityEngine;
using System.Collections;

public class PanningTexture : MonoBehaviour {

	[SerializeField]
	Vector2 scrollSpeed = Vector2.one;

    [SerializeField]
    Vector2 scale = Vector2.one;

    [SerializeField]
	private Renderer texRenderer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        float verticalOffset = Time.time * scrollSpeed.x;
        float horizontalOffset = Time.time * scrollSpeed.y;
        texRenderer.material.mainTextureOffset = new Vector2(horizontalOffset, verticalOffset);
		texRenderer.material.mainTextureScale = scale;
    }
}
