using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Describes the direction the player is facing
    /// </summary>
    public enum FacingDirection
    {
        left = -1, right = 1
    }

    /// <summary>
    /// Describes the animation state of the player-character
    /// </summary>
    public enum CharacterState
    {
        idle, walking, jumping, death, dashing, wallCling
    }
    //Current animation state
    public CharacterState currentState = CharacterState.idle;
    //Previous animation state
    public CharacterState previousState = CharacterState.idle;

    //Player health, only used to trigger death animation
    //HACK: Unused functionality to demonstrate death animation
    public int health = 10;

    //Maximum walking speed (in units per second)
    public float maxSpeed = 7f;
    //Time to reach maximum speed (in seconds)
    public float accelerationTime = 0.1f;

    //Maximum apex jump height (in units)
    public float apexHeight = 2.5f;
    //Time to reach maximum apex jump height (in seconds)
    public float apexTime = 0.33f;
    //Terminal speed when falling (in units/s)
    public float terminalVelocity = 20f;

    //The duration of time during which the player can still jump after leaving the ground (in seconds)
    public float coyoteTime = 0.13f;

    //Speed of the player's dash (in units/s)
    public float dashSpeed = 15f;
    //Length of the player's dash (in units)
    public float dashDistance = 3f;

    //Size and offset of BoxCast to check for terrain below the player
    public Vector2 groundCheckSize, groundCheckOffset;

    //Size and offset of BoxCast to check for terrain on either side of the player
    public Vector2 wallCheckSize, wallCheckOffset;

    //Layermask for terrain the player can walk on and wall jump off of
    public LayerMask groundMask;

    //Vector for storing directional input
    private Vector2 _playerInput;
    //The direction the player is facing
    private FacingDirection _direction = FacingDirection.right;

    //The player's horizontal acceleration (in units/s^2)
    //Derived from the player's maximum walking speed and time to reach maximum walking speed
    private float _acceleration;
    //The minimum amount of movement for the player to be considered moving
    //Used to stop the player from vibrating endlessly instead of stopping
    private float _minMovementTolerance;

    //The player's vertical acceleration due to gravity (in units/s^2)
    //Derived from the player's maximum apex height and time to reach maximum apex height
    private float _gravity;
    //The initial vertical speed of the player's jump (in units/s)
    //Derived from the player's maximum apex height and time to reach maximum apex height
    private float _jumpVelocity;

    //The duration of the player's dash (in seconds)
    //Derived from the player's dash speed and dash distance
    private float _dashTime;

    //Booleans from storing player input between Update() and FixedUpdate()
    private bool _jumpTrigger, _jumpReleaseTrigger;

    //Amount of time since the player was last grounded
    //Used for coyote time
    private float _timeSinceLastGrounded = 0;

    //Whether the player can dash (limits the player's dashes after leaving the ground)
    private bool _canDash = true;

    //The player's previous falling speed
    //Used to calculate camera shake intensity when landing
    private float _prevFallingSpeed = 0f;

    //Reference to the player's Rigidbody
    private Rigidbody2D _rb2d;

    // Start is called before the first frame update
    void Start()
    {
        //Get component references
        _rb2d = GetComponent<Rigidbody2D>();

        //Calculate movement values
        _acceleration = maxSpeed / accelerationTime;
        _gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        _jumpVelocity = 2 * apexHeight / apexTime;
        _dashTime = dashDistance / dashSpeed;
        _minMovementTolerance = _acceleration * Time.deltaTime * 2;
    }

    // Update is called once per frame
    void Update()
    {
        previousState = currentState;

        //Update animation state
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

        //Get directional input
        _playerInput.x = Input.GetAxisRaw("Horizontal");
        _playerInput.y = Input.GetAxisRaw("Vertical");
        _playerInput.Normalize();

        //Get action input (jump/dash)
        if (currentState != CharacterState.dashing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _jumpTrigger = true;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                _jumpReleaseTrigger = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash)
            {
                StartCoroutine(Dash(_playerInput));
            }
        }

        //Set previous falling speed
        if (_rb2d.velocity.y < 0)
        {
            _prevFallingSpeed = _rb2d.velocity.y;
        }

        //Increment coyote time timer
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
        //Don't calculate movement when dashing (dash coroutine handles its own movement)
        if (currentState != CharacterState.dashing)
        {
            MovementUpdate(_playerInput);
        }

        //Reset input triggers
        _jumpTrigger = false;
        _jumpReleaseTrigger = false;
    }

    //Update movement and apply physics (called in FixedUpdate())
    private void MovementUpdate(Vector2 playerInput)
    {
        Vector2 velocity = _rb2d.velocity;

        //Calculate movement
        HorizontalMovement(playerInput.x, ref velocity.x);
        VerticalMovement(playerInput.y, ref velocity.y);
        if (_jumpTrigger && currentState == CharacterState.wallCling)
        {
            WallJump(ref velocity); //Wall jumping effects both horizontal and vertical movement
        }

        //Apply movement
        _rb2d.velocity = velocity;
    }

    //Calculate horizontal movement (horizontal input)
    private void HorizontalMovement(float horizontalInput, ref float xVelocity)
    {
        if (horizontalInput == 0)
        {
            if (xVelocity < _minMovementTolerance && xVelocity > -_minMovementTolerance)
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
            else if (horizontalInput < 0)
            {
                _direction = FacingDirection.left;
            }
        }
    }

    //Calculate vertical movement (gravity and jumping)
    private void VerticalMovement(float verticalInput, ref float yVelocity)
    {
        if (!(IsGrounded() || currentState == CharacterState.wallCling))
        {
            //Calculate gravity
            yVelocity = Mathf.Clamp(yVelocity + _gravity * Time.deltaTime, -terminalVelocity, float.PositiveInfinity);
        }

        if (_jumpTrigger && (IsGrounded() || _timeSinceLastGrounded < coyoteTime))
        {
            //Jump
            Jump(ref yVelocity);
        }
        if (_jumpReleaseTrigger && yVelocity > 0f)
        {
            //Shorten jump (for dynamic jump height)
            yVelocity /= 2;
        }
    }

    private void Jump(ref float yVelocity)
    {
        yVelocity = _jumpVelocity;
    }

    private void WallJump(ref Vector2 velocity)
    {
        //Horizontal velocity is added in the opposite of the direction the player is facing,
        //to get the effect of the player jumping off of the wall
        velocity = new(_jumpVelocity * -(int)_direction, _jumpVelocity);
    }

    //Coroutine for dashing
    //Handles its own movement, so all other movement calculation should be
    //disabled while this coroutine is running
    private IEnumerator Dash(Vector2 dashDirection)
    {
        //Set animation state
        currentState = CharacterState.dashing;
        _canDash = false;

        //If there is no input, dash in the direction the player is facing
        if (dashDirection.magnitude == 0f)
        {
            dashDirection = new((float)_direction, 0f);
        }

        float t = 0f;
        while (t < _dashTime)
        {
            //While dashing, the player will move at a constant speed
            _rb2d.velocity = dashDirection * dashSpeed;

            t += Time.deltaTime;
            yield return null;
        }
        currentState = CharacterState.jumping;
    }

    public void Die()
    {
        gameObject.SetActive(false);
    }

    //Returns true if the player is moving horizontally
    public bool IsWalking()
    {
        return Mathf.Abs(_rb2d.velocity.x) > 0f;
    }

    //Returns true if the player is standing on ground
    public bool IsGrounded()
    {
        if (!IsOnGround())
        {
            return false;
        }
        else if (_rb2d.velocity.y > 0.01f) //This case catches the scenario where the player is jumping through a one-way platform
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //Returns true if the player is slightly above a piece of terrain
    //Different from IsGrounded() as it doesn't check the player's velocity,
    //meaning it can return a false positive when jumping through one-way platforms
    private bool IsOnGround()
    {
        return Physics2D.OverlapBox((Vector2)transform.position + groundCheckOffset, groundCheckSize, 0f, groundMask);
    }

    //Returns true if the player is touching a wall in the given direction
    public bool IsTouchingWall(FacingDirection direction)
    {
        Vector2 offset = new(wallCheckOffset.x * (int)direction, wallCheckOffset.y);
        return Physics2D.OverlapBox((Vector2)transform.position + offset, wallCheckSize, 0f, groundMask);
    }

    //Returns true if the player's health is 0
    public bool IsDead()
    {
        return health <= 0;
    }

    //Returns the direction the player is facing
    public FacingDirection GetFacingDirection()
    {
        return _direction;
    }

    //Returns the player's last negative vertical velocity
    public float GetGroundImpact()
    {
        return _prevFallingSpeed;
    }
}