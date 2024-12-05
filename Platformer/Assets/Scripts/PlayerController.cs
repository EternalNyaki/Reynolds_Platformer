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
        idle, walking, jumping, death, dashing, wallCling
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

    public float dashSpeed = 7f;
    public float dashDistance = 3f;

    public LayerMask groundMask;

    private float _acceleration;
    private Vector2 _playerInput;
    private FacingDirection _direction = FacingDirection.right;

    private float _gravity;
    private float _jumpVelocity;

    private float _dashTime;

    private bool _jumpTrigger;
    private bool _jumpReleaseTrigger;
    private bool _wallJumpTrigger;

    private float _timeSinceLastGrounded = 0;

    private bool _canDash = true;

    private float _prevFallingSpeed = 0f;

    private Rigidbody2D _rb2d;

    // Start is called before the first frame update
    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();

        _acceleration = maxSpeed / accelerationTime;

        _gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        _jumpVelocity = 2 * apexHeight / apexTime;

        _dashTime = dashDistance / dashSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        previousState = currentState;

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
                    _canDash = true;

                    if (IsWalking())
                    {
                        currentState = CharacterState.walking;
                    }
                    else
                    {
                        currentState = CharacterState.idle;
                    }
                }
                else
                {
                    if ((IsTouchingWall(FacingDirection.left) && _playerInput.x < 0) || (IsTouchingWall(FacingDirection.right) && _playerInput.x > 0))
                    {
                        currentState = CharacterState.wallCling;
                    }
                }
                break;

            case CharacterState.death:

                break;

            case CharacterState.dashing:

                break;

            case CharacterState.wallCling:
                if (!((IsTouchingWall(FacingDirection.left) && _playerInput.x < 0) || (IsTouchingWall(FacingDirection.right) && _playerInput.x > 0)))
                {
                    currentState = CharacterState.jumping;
                }
                break;
        }
        if (IsDead())
        {
            currentState = CharacterState.death;
        }

        _playerInput.x = Input.GetAxisRaw("Horizontal");
        _playerInput.y = Input.GetAxisRaw("Vertical");
        _playerInput.Normalize();

        if (currentState != CharacterState.dashing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (IsGrounded() || _timeSinceLastGrounded < coyoteTime)
                {
                    _jumpTrigger = true;
                }
                if (currentState == CharacterState.wallCling)
                {
                    _wallJumpTrigger = true;
                }
            }
            else if (Input.GetKeyUp(KeyCode.Space) && _rb2d.velocity.y > 0)
            {
                _jumpReleaseTrigger = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash)
            {
                StartCoroutine(Dash(_playerInput));
            }
        }

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

        if (_wallJumpTrigger)
        {
            if (_direction == FacingDirection.left)
            {
                velocity = new(_jumpVelocity, _jumpVelocity);
            }
            else
            {
                velocity = new(-_jumpVelocity, _jumpVelocity);
            }
            _wallJumpTrigger = false;
        }

        if (velocity.y < 0)
        {
            _prevFallingSpeed = velocity.y;
        }

        _rb2d.velocity = velocity;
    }

    private void HorizontalMovement(float horizontalInput, ref float xVelocity)
    {
        if (currentState != CharacterState.dashing)
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
    }

    private void VerticalMovement(float verticalInput, ref float yVelocity)
    {
        if (currentState != CharacterState.dashing)
        {
            if (currentState != CharacterState.wallCling)
            {
                yVelocity = Mathf.Clamp(yVelocity + _gravity * Time.deltaTime, -terminalVelocity, float.PositiveInfinity);
            }

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
    }

    private void Jump(ref float yVelocity)
    {
        yVelocity = _jumpVelocity;
    }

    private IEnumerator Dash(Vector2 dashDirection)
    {
        currentState = CharacterState.dashing;
        _canDash = false;

        if (dashDirection.magnitude == 0f)
        {
            switch (_direction)
            {
                case FacingDirection.left:
                    dashDirection = Vector2.left;
                    break;

                case FacingDirection.right:
                    dashDirection = Vector2.right;
                    break;
            }
        }

        float t = 0f;
        while (t < _dashTime)
        {
            t += Time.deltaTime;

            _rb2d.velocity = dashDirection * dashSpeed;

            yield return null;
        }
        currentState = CharacterState.jumping;
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

    public bool IsTouchingWall(FacingDirection direction)
    {
        int dir = direction == FacingDirection.left ? -1 : 1;
        return Physics2D.Raycast((Vector2)transform.position + new Vector2(0.35f * dir, 0.4f), Vector2.right * dir, 0.5f, groundMask);
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    public FacingDirection GetFacingDirection()
    {
        return _direction;
    }

    public float GetGroundImpact()
    {
        return _prevFallingSpeed;
    }
}