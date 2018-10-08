using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerPlatformerController : MonoBehaviour
{

    public float speed = 5f;
    public float climbSpeed = 2;
    public float jumpSpeed = 8.5f;
    public float jumpAbortSpeedReduction = 20f;
    public float gravity = 15f;

    //[Header("Audio")]
    //public RandomAudioPlayer footStepAudioPlayer;
    //public RandomAudioPlayer landAudioPlayer;


    private CharacterController2D m_CharacterController2D;
    private Collider2D m_Collider2D;
    private Animator m_Animator;
    private SpriteRenderer m_SpriteRenderer;
    private Vector2 m_MoveVector;
    private PlatformEffector2D m_PlatformEffector2D;

    private int m_HashGroundedPara = Animator.StringToHash("Grounded");
    private int m_HashVelocityXPara = Animator.StringToHash("VelocityX");
    private int m_HashVelocityYPara = Animator.StringToHash("VelocityY");
    private int m_HashHurtPara = Animator.StringToHash("Hurt");

    private bool m_IsOnLadder = false;
    private bool m_TriggerUse = false;
    private bool m_CanJump = true;

    private const float k_GroundedStickingVelocityMultiplier = 3f;    // This is to help the character stick to vertically moving platforms.

    private void Awake()
    {
        m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        m_Animator = GetComponent<Animator>();
        m_CharacterController2D = GetComponent<CharacterController2D>();
        m_Collider2D = GetComponent<Collider2D>();
        //m_PlatformEffector2D = FindObjectOfType<PlatformEffector2D>();
    }

    private void FixedUpdate()
    {

        TakeAction();
        Face();
        Animate();

    }


    private void TakeAction()
    {

        if (CheckForObstacle(0.5f, 0.2f))
        {
            if (m_CanJump)
            {
                Jump();
            }
        }

 
        //Reduce jump speed
        JumpAbortReduction();

        //Vertical movement
        if (!m_CharacterController2D.IsGrounded)
        {
            AirborneVerticalMovement();
        }
        else
        {
            GroundedVerticalMovement();
        }

        //Set horizontal movement
        SetHorizontalMovement(m_SpriteRenderer.flipX ? -speed : speed);



        //Move
        Move();

    }


    private void Face()
    {
        if (!m_SpriteRenderer.flipX && m_MoveVector.x < 0)
        {
            m_SpriteRenderer.flipX = true;

        }

        if (m_SpriteRenderer.flipX && m_MoveVector.x > 0)
        {
            m_SpriteRenderer.flipX = false;
        }

    }

    private void Animate()
    {
        m_Animator.SetBool(m_HashGroundedPara, m_CharacterController2D.IsGrounded);
        m_Animator.SetFloat(m_HashVelocityXPara, Mathf.Abs(m_MoveVector.x));
        m_Animator.SetFloat(m_HashVelocityYPara, m_MoveVector.y);
    }

    private void Move()
    {
        m_CharacterController2D.Move(m_MoveVector * Time.fixedDeltaTime);
    }

    public void Jump()
    {
        if (m_CharacterController2D.IsGrounded)
        {
            SetVerticalMovement(jumpSpeed);
        }
    }

    public void Jump(float jump)
    {

        SetVerticalMovement(jump);

    }

    public void SetMoveVector(Vector2 newMoveVector)
    {
        m_MoveVector = newMoveVector;
    }

    public void SetHorizontalMovement(float newHorizontalMovement)
    {
        m_MoveVector.x = newHorizontalMovement;
    }

    public void SetVerticalMovement(float newVerticalMovement)
    {
        m_MoveVector.y = newVerticalMovement;
    }

    public void IncrementMovement(Vector2 additionalMovement)
    {
        m_MoveVector += additionalMovement;
    }

    public void IncrementHorizontalMovement(float additionalHorizontalMovement)
    {
        m_MoveVector.x += additionalHorizontalMovement;
    }

    public void IncrementVerticalMovement(float additionalVerticalMovement)
    {
        m_MoveVector.y += additionalVerticalMovement;
    }

    public void JumpAbortReduction()
    {
        if (m_MoveVector.y > 0.0f)
        {
            m_MoveVector.y -= jumpAbortSpeedReduction * Time.deltaTime;
        }
    }

    public void GroundedVerticalMovement()
    {
        m_MoveVector.y -= gravity * Time.deltaTime;

        if (m_MoveVector.y < -gravity * Time.deltaTime * k_GroundedStickingVelocityMultiplier)
        {
            m_MoveVector.y = -gravity * Time.deltaTime * k_GroundedStickingVelocityMultiplier;
        }
    }

    public void AirborneVerticalMovement()
    {
        if (Mathf.Approximately(m_MoveVector.y, 0f) || m_CharacterController2D.IsCeilinged && m_MoveVector.y > 0f)
        {
            m_MoveVector.y = 0f;
        }
        m_MoveVector.y -= gravity * Time.deltaTime;
    }

    public bool CheckForObstacle(float obstacleForwardDistance, float gulfForwardDistance)
    {


        //we circle cast with a size sligly smaller than the collider height. That avoid to collide with very small bump on the ground
        if (Physics2D.CircleCast(m_Collider2D.bounds.center, m_Collider2D.bounds.extents.y - 0.2f,
                                                                    m_SpriteRenderer.flipX ? Vector2.left : Vector2.right,
                                                                    obstacleForwardDistance, m_CharacterController2D.groundedLayerMask.value))
        {
            return true;
        }

        Vector3 castingPosition = (Vector2)(m_Collider2D.bounds.center) +
                             (m_SpriteRenderer.flipX ? Vector2.left : Vector2.right) * (m_Collider2D.bounds.extents.x + gulfForwardDistance);


        if (!Physics2D.CircleCast(castingPosition, 0.1f, Vector2.down, m_Collider2D.bounds.extents.y + 0.2f, m_CharacterController2D.groundedLayerMask.value))
        {
            return true;
        }

        return false;
    }


    public void PlayFootStepAudioPlayer()
    {
        //if (footStepAudioPlayer != null)
        //    footStepAudioPlayer.PlayRandomSound();
    }

    public void PlayLandAudioPlayer()
    {
        //if (landAudioPlayer != null)
        //    landAudioPlayer.PlayRandomSound();
    }


    private void SetCanJump(bool canJump)
    {
        m_CanJump = canJump;
    }

}
