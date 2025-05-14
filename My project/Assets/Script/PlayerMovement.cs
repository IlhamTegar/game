using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private PlayerController playerController;

    private Vector2 moveInput;
    private bool isJumping = false;
    private bool isRunning = false;
    private bool isCrouching = false;
    private bool isAttacking = false;

    private enum MovementState { Idle, Jump, Fall, Run, Crouch, JumpAttack, upLead }

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround;
    private BoxCollider2D coll;

    private bool IsBlockedForward()
    {
        Vector2 direction = sprite.flipX ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.5f, jumpableGround);
        return hit.collider != null;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();

        playerController = new PlayerController();
    }

    private void OnEnable()
    {
        playerController.Enable();

        playerController.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerController.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        playerController.Movement.Jump.performed += ctx => Jump();
        playerController.Movement.Crouch.performed += ctx => isCrouching = true;
        playerController.Movement.Crouch.canceled += ctx => isCrouching = false;

        playerController.Movement.Run.performed += ctx => isRunning = true;
        playerController.Movement.Run.canceled += ctx => isRunning = false;

        playerController.Movement.Attack.performed += ctx => isAttacking = true;
    }

    private void OnDisable()
    {
        playerController.Disable();
    }

    private void Update()
    {
        moveInput = playerController.Movement.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        float speed = isRunning ? runSpeed : moveSpeed;
        Vector2 targetVelocity = new Vector2(moveInput.x * speed, rb.velocity.y);
        rb.velocity = targetVelocity;

        UpdateAnimation();

        // Reset one-time actions
        isAttacking = false;
    }

    private void UpdateAnimation()
    {
        MovementState state = MovementState.Idle;

        if (isGrounded())
        {
            if (isCrouching)
            {
                state = MovementState.Crouch;
            }
            else if (moveInput.x != 0)
            {
                state = MovementState.Run;
            }
        }

        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.Jump;

            if (isAttacking)
            {
                state = MovementState.JumpAttack;
            }
        }
        else if (rb.velocity.y < -0.1f)
        {
            state = MovementState.Fall;
        }

        if (isRunning && isGrounded() && IsBlockedForward())
        {
            state = MovementState.upLead;
        }

        if (moveInput.x != 0)
            sprite.flipX = moveInput.x < 0;

        anim.SetInteger("state", (int)state);
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private void Jump()
    {
        if (isGrounded() && !isCrouching)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
}
