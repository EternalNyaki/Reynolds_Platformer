using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCrate : MonoBehaviour
{
    private Rigidbody2D _rb2d;

    private void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _rb2d.velocity = new(0f, _rb2d.velocity.y);
    }
}
