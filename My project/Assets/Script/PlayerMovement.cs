using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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

    // Untuk input dari button UI
    private float mobileInputX = 0f;

    private Vector2 moveInput;
    private bool isJumping = false;

    private enum MovementState { idle, jump, fall, walk, run, attack, airAttack }

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround;
    private BoxCollider2D coll;

    [Header("Health System")]
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("knockback Settings")]
    [SerializeField] private float knockBackTime = 0.2f;
    [SerializeField] private float knockBackThrust = 10f;
    private bool isKnockedBack = false;

    [Header("Attack Settings")]
    public GameObject slashEffect;
    public GameObject airSlashEffect;

    private bool isAttacking = false;
    private MovementState currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();

        playerController = new PlayerController();

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void OnEnable()
    {
        playerController.Enable();

        playerController.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerController.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        playerController.Movement.Jump.performed += ctx => Jump();
        playerController.Movement.Attack.performed += ctx => PerformAttack();
    }

    private void OnDisable()
    {
        playerController.Disable();
    }

    private void Update()
    {
        if (Application.isMobilePlatform)
        {
            moveInput = new Vector2(mobileInputX, 0f);
        }
        else
        {
            moveInput = playerController.Movement.Move.ReadValue<Vector2>();
        }
    }

    private void FixedUpdate()
    {
        if (isAttacking && currentState == MovementState.attack)
        {
            // Diam di tempat hanya saat attack biasa (grounded attack)
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else
        {
            Vector2 targetVelocity = new Vector2((moveInput.x + mobileInputX) * moveSpeed, rb.velocity.y);
            rb.velocity = targetVelocity;
        }

        if (isKnockedBack) return;

        UpdateAnimation();

        if (isGrounded() && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            isJumping = false;
        }
    }


    private void UpdateAnimation()
    {
        if (isAttacking)
        {
            // Prioritaskan animasi attack
            anim.SetInteger("state", (int)currentState);
            return;
        }

        MovementState state;

        float horizontal = moveInput.x != 0 ? moveInput.x : mobileInputX;

        if (horizontal > 0f)
        {
            state = MovementState.walk;
            sprite.flipX = false;
        }
        else if (horizontal < 0f)
        {
            state = MovementState.walk;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jump;
        }
        else if (rb.velocity.y < -0.1f)
        {
            state = MovementState.fall;
        }

        currentState = state;
        anim.SetInteger("state", (int)state);
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private void Jump()
    {
        if (isGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
        }
    }

    private void PerformAttack()
    {
        if (isAttacking) return;

        isAttacking = true;

        if (isGrounded())
        {
            currentState = MovementState.attack;
            anim.SetInteger("state", (int)currentState);
            if (slashEffect != null)
                Instantiate(slashEffect, transform.position, Quaternion.identity);
        }
        else
        {
            currentState = MovementState.airAttack;
            anim.SetInteger("state", (int)currentState);
            if (airSlashEffect != null)
                Instantiate(airSlashEffect, transform.position, Quaternion.identity);
        }

        StartCoroutine(ResetAttack());
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(1.05f); // Sesuaikan durasi animasi attack
        isAttacking = false;
    }

    public void MoveRight(bool isPressed)
    {
        if (isPressed)
            mobileInputX = 1f;
        else if (mobileInputX == 1f)
            mobileInputX = 0f;
    }

    public void MoveLeft(bool isPressed)
    {
        if (isPressed)
            mobileInputX = -1f;
        else if (mobileInputX == -1f)
            mobileInputX = 0f;
    }

    public void MobileJump()
    {
        if (isGrounded())
        {
            Jump();
        }
    }

    public void TakeDamage(int damage, Vector2 direction)
    {
        if (isKnockedBack) return; // Jangan stack knockback

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player Mati");
        }

        StartCoroutine(HandleKnockback(direction.normalized));
        UpdateHealthUI();
    }


    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "Health: " + currentHealth;
    }

    private IEnumerator HandleKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;

        Vector2 force = direction * knockBackThrust * rb.mass;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockBackTime);
        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }
}
