using UnityEngine;
using System.Collections;

public class TurbineBlade : MonoBehaviour {

	[SerializeField]
	private float rotateSpeed = 180f;

	void Awake()
	{
		transform.Rotate(Vector3.right * 180f * Random.value);
	}

	void Update()
	{
        transform.Rotate(Vector3.right * rotateSpeed * Time.deltaTime);
	}
}
