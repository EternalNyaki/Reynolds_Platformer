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

    public enum CharacterState
    {
        idle, walking, jumping, death
    }
    public CharacterState currentState = CharacterState.idle;
    public CharacterState previousState = CharacterState.idle;

    public int health = 10;

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

    private bool _jumpTrigger;
    private bool _jumpReleaseTrigger;

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
        previousState = currentState;

        _playerInput.x = Input.GetAxisRaw("Horizontal");

        switch (currentState)
        {
            case CharacterState.idle:
                if (IsWalking())
                {
                    currentState = CharacterState.walking;
                }

                if (!IsGrounded())
                {
                    currentState = CharacterState.jumping;
                }
                break;

            case CharacterState.walking:
                if (!IsWalking())
                {
                    currentState = CharacterState.idle;
                }

                if (!IsGrounded())
                {
                    currentState = CharacterState.jumping;
                }
                break;

            case CharacterState.jumping:
                if (IsGrounded())
                {
                    if (IsWalking())
                    {
                        currentState = CharacterState.walking;
                    }
                    else
                    {
                        currentState = CharacterState.idle;
                    }
                }
                break;

            case CharacterState.death:

                break;
        }
        if (IsDead())
        {
            currentState = CharacterState.death;
        }

        float yVelocity = _rb2d.velocity.y;
        if (Input.GetKeyDown(KeyCode.Space) && (IsGrounded() || _timeSinceLastGrounded < coyoteTime))
        {
            _jumpTrigger = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space) && yVelocity > 0)
        {
            _jumpReleaseTrigger = true;
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

        if (_jumpTrigger)
        {
            Jump(ref yVelocity);

            _jumpTrigger = false;
        }
        if (_jumpReleaseTrigger)
        {
            yVelocity /= 2;

            _jumpReleaseTrigger = false;
        }
    }

    private void Jump(ref float yVelocity)
    {
        yVelocity = _jumpVelocity;
    }

    public void OnDie()
    {
        gameObject.SetActive(false);
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
        return Physics2D.Raycast(transform.position, Vector2.down, 0.2f, groundMask) ||
               Physics2D.Raycast(transform.position + new Vector3(-0.375f, 0f), Vector2.down, 0.2f, groundMask) ||
               Physics2D.Raycast(transform.position + new Vector3(0.375f, 0f), Vector2.down, 0.2f, groundMask);
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    public FacingDirection GetFacingDirection()
    {
        return _direction;
    }
}