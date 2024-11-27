using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script manages updating the visuals of the character based on the values that are passed to it from the PlayerController.
/// NOTE: You shouldn't make changes to this script when attempting to implement the functionality for the W10 journal.
/// </summary>
public class PlayerVisuals : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer bodyRenderer;
    public PlayerController playerController;

    private int idleHash, walkingHash, jumpingHash, deathHash;

    // Start is called before the first frame update
    void Start()
    {
        idleHash = Animator.StringToHash("Idle");
        walkingHash = Animator.StringToHash("Walking");
        jumpingHash = Animator.StringToHash("Jumping");
        deathHash = Animator.StringToHash("Death");

    }

    // Update is called once per frame
    void Update()
    {
        VisualsUpdate();
    }

    //It is not recommended to make changes to the functionality of this code for the W10 journal.
    private void VisualsUpdate()
    {
        if (playerController.previousState != playerController.currentState)
        {
            switch (playerController.currentState)
            {
                case PlayerController.CharacterState.idle:
                    animator.CrossFade("Idle", 0);
                    break;

                case PlayerController.CharacterState.walking:
                    animator.CrossFade("Walking", 0);
                    break;

                case PlayerController.CharacterState.jumping:
                    animator.CrossFade("Jumping", 0);
                    break;

                case PlayerController.CharacterState.death:
                    animator.CrossFade("Death", 0);
                    break;
            }
        }

        switch (playerController.GetFacingDirection())
        {
            case PlayerController.FacingDirection.left:
                bodyRenderer.flipX = true;
                break;
            case PlayerController.FacingDirection.right:
            default:
                bodyRenderer.flipX = false;
                break;
        }
    }
}
