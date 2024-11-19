using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }

    public float maxSpeed = 5f;
    public float accelerationTime = 0.2f;

    private float _acceleration;
    private Vector2 _playerInput;
    private FacingDirection _direction = FacingDirection.right;

    private Rigidbody2D _rb2d;

    // Start is called before the first frame update
    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();

        _acceleration = maxSpeed / accelerationTime;
    }

    // Update is called once per frame
    void Update()
    {
        _playerInput.x = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        MovementUpdate(_playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x == 0)
        {
            float tolerance = _acceleration * Time.deltaTime * 2;
            if (_rb2d.velocity.x < tolerance && _rb2d.velocity.x > -tolerance)
            {
                //Do nothing
                _rb2d.velocity = new(0, _rb2d.velocity.y);
            }
            else
            {
                //Decelerate
                _rb2d.velocity = new(Mathf.Clamp(-Mathf.Sign(_rb2d.velocity.x) * _acceleration * Time.deltaTime, -maxSpeed, maxSpeed), _rb2d.velocity.y);
            }
        }
        else
        {
            //Accelerate
            _rb2d.velocity = new(Mathf.Clamp(_rb2d.velocity.x + playerInput.x * _acceleration * Time.deltaTime, -maxSpeed, maxSpeed), _rb2d.velocity.y);

            //Change facing direction
            if (playerInput.x > 0)
            {
                _direction = FacingDirection.right;
            }
            else
            {
                _direction = FacingDirection.left;
            }
        }
    }

    public bool IsWalking()
    {
        return Mathf.Abs(_rb2d.velocity.x) > 0f;
    }

    public bool IsGrounded()
    {
        return true;
    }

    public FacingDirection GetFacingDirection()
    {
        return _direction;
    }
}