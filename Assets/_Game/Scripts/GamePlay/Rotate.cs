using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float angularSpeed = 120f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void FixedUpdate()
    {
        rb.MoveRotation(rb.rotation + angularSpeed * Time.fixedDeltaTime);
    }

    public void Reverse() => angularSpeed = -angularSpeed;
}
