using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCrate : MonoBehaviour
{
    //Rigidbody component reference
    private Rigidbody2D _rb2d;

    private void Start()
    {
        //Set component reference
        _rb2d = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //Set x velocity to 0 when collision stops to avoid sliding
        _rb2d.velocity = new(0f, _rb2d.velocity.y);
    }
}
