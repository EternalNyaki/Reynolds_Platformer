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
    public CameraController cameraController;
    public float cameraShakeIntensityMult;

    private int idleHash, walkingHash, jumpingHash, deathHash, dashingHash, wallClingHash;

    // Start is called before the first frame update
    void Start()
    {
        idleHash = Animator.StringToHash("Idle");
        walkingHash = Animator.StringToHash("Walking");
        jumpingHash = Animator.StringToHash("Jumping");
        deathHash = Animator.StringToHash("Death");
        dashingHash = Animator.StringToHash("Dashing");
        wallClingHash = Animator.StringToHash("Wall Cling");
    }

    // Update is called once per frame
    void Update()
    {
        VisualsUpdate();
    }

    private void VisualsUpdate()
    {
        if (playerController.previousState != playerController.currentState)
        {
            switch (playerController.currentState)
            {
                case PlayerController.CharacterState.idle:
                    if (playerController.previousState == PlayerController.CharacterState.jumping ||
                        playerController.previousState == PlayerController.CharacterState.dashing)
                    {
                        cameraController.Shake(playerController.GetGroundImpact() * cameraShakeIntensityMult, 0.35f);
                    }
                    animator.CrossFade(idleHash, 0f);
                    break;

                case PlayerController.CharacterState.walking:
                    if (playerController.previousState == PlayerController.CharacterState.jumping ||
                        playerController.previousState == PlayerController.CharacterState.dashing)
                    {
                        cameraController.Shake(playerController.GetGroundImpact() * cameraShakeIntensityMult, 0.35f);
                    }
                    animator.CrossFade(walkingHash, 0f);
                    break;

                case PlayerController.CharacterState.jumping:
                    animator.CrossFade(jumpingHash, 0f);
                    break;

                case PlayerController.CharacterState.death:
                    animator.CrossFade(deathHash, 0f);
                    break;

                case PlayerController.CharacterState.dashing:
                    animator.CrossFade(dashingHash, 0f);
                    break;

                case PlayerController.CharacterState.wallCling:
                    animator.CrossFade(wallClingHash, 0f);
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
