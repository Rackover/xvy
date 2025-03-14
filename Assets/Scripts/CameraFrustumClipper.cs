using UnityEngine;
using System.Collections;

public class CameraFrustumClipper : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Camera targetCamera = GetComponent<Camera>();
        Update();

        Vector3[] nearCorners = new Vector3[4];
        Vector3[] farCorners = new Vector3[4];

        Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);

        Plane temp = camPlanes[1];
        camPlanes[1] = camPlanes[2];
        camPlanes[2] = temp;

        for (int i = 0; i < 4; i++)
        {
            nearCorners[i] = Plane3Intersect(camPlanes[4], camPlanes[i], camPlanes[(i + 1) % 4]); //near corners on the created projection matrix
            farCorners[i] = Plane3Intersect(camPlanes[5], camPlanes[i], camPlanes[(i + 1) % 4]); //far corners on the created projection matrix
        }

        {
            string str = string.Empty;
            for (int i = 0; i < farCorners.Length; i++)
            {
                str += "   " + farCorners[i];
            }

            Debug.Log(str);
        }

        Color[] colors = new Color[]{
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow
        };

        for (int i = 0; i < 4; i++)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(nearCorners[i], nearCorners[(i + 1) % 4]);

            Gizmos.color = Color.Lerp(colors[i], Color.white, 0.5f);
            Gizmos.DrawWireCube(nearCorners[i], Vector3.one);


            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(farCorners[i], farCorners[(i + 1) % 4]);

            Gizmos.color = Color.Lerp(colors[i], Color.black, 0.5f);
            Gizmos.DrawWireCube(farCorners[i], Vector3.one);


            Gizmos.color = Color.gray;
            Gizmos.DrawLine(nearCorners[i], farCorners[i]);
        }
    }

    Vector3 Plane3Intersect(Plane p1, Plane p2, Plane p3)
    { //get the intersection point of 3 planes
        return ((-p1.distance * Vector3.Cross(p2.normal, p3.normal)) +
                (-p2.distance * Vector3.Cross(p3.normal, p1.normal)) +
                (-p3.distance * Vector3.Cross(p1.normal, p2.normal))) /
            (Vector3.Dot(p1.normal, Vector3.Cross(p2.normal, p3.normal)));
    }
#endif
}
