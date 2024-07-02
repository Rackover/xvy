using UnityEngine;
using System.Collections;
using System;

public class PlayerCollisions : MonoBehaviour
{

    public event Action<Collision> OnCollide;

    [SerializeField]
    private Rigidbody rb;

    private void OnCollisionEnter(Collision collision)
    {
        if (OnCollide != null)
        {
            OnCollide.Invoke(collision);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(transform.position);
    }
}
