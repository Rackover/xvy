using UnityEngine;
using System.Collections;

public class LoadingDots : MonoBehaviour {

	[SerializeField]
	private float speed = 5f;

	[SerializeField]
	private RectTransform rt;

	RectTransform[] children;

	void Start()
	{
		children = new RectTransform[rt.childCount];

		for (int i = 0; i < children.Length; i++)
		{
			children[i] = rt.GetChild(i) as RectTransform;
		}
	}

	// Update is called once per frame
	void Update () {
		rt.Rotate(Vector3.forward * speed * Time.unscaledDeltaTime);

		for (int i = 0; i < children.Length; i++)
		{
			children[i].eulerAngles = Vector3.zero;
		}
	}
}
