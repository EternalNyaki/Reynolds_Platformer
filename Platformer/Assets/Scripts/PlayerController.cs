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
    public float apexTime = 0.5f;
    public float terminalVelocity = 100f;

    public float coyoteTime = 0.2f;

    public LayerMask groundMask;

    private float _acceleration;
    private Vector2 _playerInput;
    private FacingDirection _direction = FacingDirection.right;

    private float _gravity;
    private float _jumpVelocity;

    private float _timeSinceLastGrounded = 0;

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

        float yVelocity = _rb2d.velocity.y;
        if (Input.GetKeyDown(KeyCode.Space) && (IsGrounded() || _timeSinceLastGrounded < coyoteTime))
        {
            Jump(ref yVelocity);
        }
        else if (Input.GetKeyUp(KeyCode.Space) && yVelocity > 0)
        {
            yVelocity /= 2;
        }
        _rb2d.velocity = new(_rb2d.velocity.x, yVelocity);

        if (!IsGrounded())
        {
            _timeSinceLastGrounded += Time.deltaTime;
        }
        else
        {
            _timeSinceLastGrounded = 0;
        }
    }

    void FixedUpdate()
    {
        MovementUpdate(_playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        Vector2 velocity = _rb2d.velocity;

        HorizontalMovement(playerInput.x, ref velocity.x);
        VerticalMovement(playerInput.y, ref velocity.y);

        _rb2d.velocity = velocity;
    }

    private void HorizontalMovement(float horizontalInput, ref float xVelocity)
    {
        if (horizontalInput == 0)
        {
            float tolerance = _acceleration * Time.deltaTime * 2;
            if (xVelocity < tolerance && xVelocity > -tolerance)
            {
                //Do nothing
                xVelocity = 0f;
            }
            else
            {
                //Decelerate
                xVelocity = Mathf.Clamp(-Mathf.Sign(xVelocity) * _acceleration * Time.deltaTime, -maxSpeed, maxSpeed);
            }
        }
        else
        {
            //Accelerate
            xVelocity = Mathf.Clamp(xVelocity + horizontalInput * _acceleration * Time.deltaTime, -maxSpeed, maxSpeed);

            //Change facing direction
            if (horizontalInput > 0)
            {
                _direction = FacingDirection.right;
            }
            else
            {
                _direction = FacingDirection.left;
            }
        }
    }

    private void VerticalMovement(float verticalInput, ref float yVelocity)
    {
        yVelocity = Mathf.Clamp(yVelocity + _gravity * Time.deltaTime, -terminalVelocity, float.PositiveInfinity);
    }

    private void Jump(ref float yVelocity)
    {
        yVelocity = _jumpVelocity;
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