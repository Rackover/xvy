using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class GlobalParameters : ScriptableObject {

    [System.Serializable]
    public class CameraObstructor
    {

        public Vector2 offset;

        public float distanceOffset = 0f;

        [Range(0f, 1f)]
        public float additiveDiagonalOffsetMultiplier = 0.5f;

        [Range(0f, 1f)]
        public float followSquarifier01 = 0f;

        public bool useSplitRenderHValue = false;
    }

    public CameraObstructor cameraObstructor = new CameraObstructor();

}
