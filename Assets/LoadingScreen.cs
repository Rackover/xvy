using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour {

	[SerializeField]
	private Canvas canvas;
	
	[SerializeField]
	private Text textMesh;

	void Update()
	{
		canvas.enabled = Game.i.IsLoading;
		if (canvas.enabled )
		{
			string composed = "entering\n"+Game.i.LevelName.ToLower();
			char[] chars = new char[composed.Length * 2];
			for (int i = 0; i < composed.Length; i++)
			{
				chars[i*2] = composed[i];
				chars[i*2+1] = ' ';
			}

			textMesh.text = new string(chars);
		}
	}
}
