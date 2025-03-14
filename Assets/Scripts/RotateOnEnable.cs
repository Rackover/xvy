using UnityEngine;
using System.Collections;

public class RotateOnEnable : MonoBehaviour
{

    void OnEnable()
    {
        transform.Rotate(Vector3.up * 360f * Random.value);
    }
}
