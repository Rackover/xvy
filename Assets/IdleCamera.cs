using UnityEngine;
using System.Collections;

public class IdleCamera : MonoBehaviour {

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private bool initialSkip;

	[SerializeField]
	private float speed = 0.1f;

	void OnEnable()
	{
		animator.speed = speed;


        if (initialSkip)
		{
			//animator.SetTrigger("Skip");
			//animator.SetTime(0.5f);
			animator.Update(1.5f / speed);

        }

		animator.Play("Entry");
	}

	void OnDisable()
    {
        animator.ResetTrigger("Skip");
    }
}
