using UnityEngine;
using System.Collections;

public class CameraObstructer : MonoBehaviour
{

    [SerializeField]
    private Transform obstruder;

    [SerializeField]
    private bool frustumMode = false;

    [SerializeField]
    private bool opposite = false;

    [SerializeField]
    private GlobalParameters parameters;

    [Range(-1f, 1f)]
    public float splitTest101 = 0f;

    public bool Opposite { set { opposite = value; } }

    private Camera targetCamera;

    void Awake()
    {
        // Disabled for now
        Destroy(obstruder.gameObject);
        Destroy(this);
        return;
        //

#pragma warning disable 0162
        targetCamera = GetComponent<Camera>();
#pragma warning restore 0162
    }


    [ExecuteInEditMode]
    private void Update()
    {
        float horizontalAmount101 = splitTest101;

        if (Game.i)
        {
            horizontalAmount101 =
                parameters.cameraObstructor.useSplitRenderHValue ?
                Game.i.HorizontalSplitAmount :
                (Mathf.Lerp(1f, Game.i.Flip01, parameters.cameraObstructor.followSquarifier01) * Game.i.Flip01) * 2f - 1;
            splitTest101 = horizontalAmount101;
        }

        float absHAmount = Mathf.Clamp01(Mathf.Abs(horizontalAmount101));

        if (frustumMode)
        {
        }
        else
        {
            if (obstruder && targetCamera)
            {
                obstruder.gameObject.SetActive(targetCamera.enabled);

                obstruder.transform.forward = targetCamera.transform.forward;
                obstruder.transform.localEulerAngles = new Vector3(0f, 0f, (opposite ? 0f : 180f) + horizontalAmount101 * 90f);
                obstruder.transform.position =
                    transform.position
                    + targetCamera.transform.forward * (targetCamera.nearClipPlane + parameters.cameraObstructor.distanceOffset)
                    + obstruder.transform.right * Mathf.Lerp(parameters.cameraObstructor.offset.x, parameters.cameraObstructor.offset.y, absHAmount);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        targetCamera = GetComponent<Camera>();
        Update();
    }
#endif
}
