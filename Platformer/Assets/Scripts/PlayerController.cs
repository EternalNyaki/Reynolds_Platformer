using System;
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

    public float apexHeight = 4f;
    public float apexTime = 20 / 60;
    public float terminalVelocity = 100f;

    public LayerMask groundMask;

    private float _acceleration;
    private Vector2 _playerInput;
    private FacingDirection _direction = FacingDirection.right;

    private float _gravity;
    private float _jumpVelocity;

    private Rigidbody2D _rb2d;

    // Start is called before the first frame update
    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();

        _acceleration = maxSpeed / accelerationTime;

        _gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        _jumpVelocity = 2 * apexHeight / apexTime;
    }

    // Update is called once per frame
    void Update()
    {
        _playerInput.x = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        MovementUpdate(_playerInput);

        GravityUpdate();
    }

    private void Jump()
    {
        _rb2d.velocity = new(_rb2d.velocity.x, _jumpVelocity);
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

    private void GravityUpdate()
    {
        _rb2d.velocity = new(_rb2d.velocity.x, Mathf.Clamp(_rb2d.velocity.y + _gravity * Time.deltaTime, -terminalVelocity, float.PositiveInfinity));
    }

    public bool IsWalking()
    {
        return Mathf.Abs(_rb2d.velocity.x) > 0f;
    }

    public bool IsGrounded()
    {
        if (!IsOnGround())
        {
            return false;
        }
        else if (_rb2d.velocity.y > 0.01f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool IsOnGround()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, 1f, groundMask);
    }

    public FacingDirection GetFacingDirection()
    {
        return _direction;
    }
}