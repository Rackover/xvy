using UnityEngine;
using System.Collections;

public class CustomMeshBounds : MonoBehaviour {

    [SerializeField]
    private MeshRenderer meshRenderer;

    void Awake()
    {
        //meshRenderer.bounds = new Bounds(Vector3.zero, Vector3.one * 40000);
    }
}
