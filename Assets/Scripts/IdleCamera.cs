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
#if UNITY_PS3
            animator.SetTrigger("Skip");
            animator.SetTime(0.5f);
#else
			animator.Update(1.5f / speed);
#endif
        }

#if !UNITY_PS3
		animator.Play("Entry");
#endif

        if (Game.i == null && initialSkip)
        {
            var cam = GetComponent<Camera>();
            if (cam)
            {
                cam.targetTexture = null;
                cam.targetDisplay = 0;

                cam.gameObject.AddComponent<AudioListener>();
            }
        }
	}

	void OnDisable()
    {
        animator.ResetTrigger("Skip");
    }
}
