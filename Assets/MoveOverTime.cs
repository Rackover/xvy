using UnityEngine;
using System.Collections;

public class MoveOverTime : MonoBehaviour {

	[SerializeField]
	private Vector3 direction;

	[SerializeField]
	private float speed;

	// Update is called once per frame
	void Update () {
		transform.position += direction * Time.deltaTime * speed;
	}
}
