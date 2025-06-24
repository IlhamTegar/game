using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

    private float mobileInputX = 0f;
    private Vector2 moveInput;
    private bool isJumping = false;

    private enum MovementState { idle, jump, fall, walk, run, attack }

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround;
    private BoxCollider2D coll;

    [Header("Health System")]
    public int maxHealth = 100;
    public int currentHealth;
    public Image healthBarImage;

    [Header("Knockback Settings")]
    [SerializeField] private float knockBackThrust = 20f;
    [SerializeField] private float knockBackTime = 0.5f;
    private bool isKnockedBack = false;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

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
        moveInput = Application.isMobilePlatform ? new Vector2(mobileInputX, 0f) : playerController.Movement.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (isKnockedBack)
        {
            UpdateAnimation();
            return;
        }

        if (isAttacking && currentState == MovementState.attack)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else
        {
            Vector2 targetVelocity = new Vector2((moveInput.x + mobileInputX) * moveSpeed, rb.velocity.y);
            rb.velocity = targetVelocity;
        }

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
            anim.SetInteger("state", (int)MovementState.attack);
            return;
        }

        MovementState state;
        float horizontal = moveInput.x != 0 ? moveInput.x : mobileInputX;

        if (horizontal > 0f)
        {
            state = MovementState.walk;
            sprite.flipX = false;
            UpdateAttackPointDirection(1f);
        }
        else if (horizontal < 0f)
        {
            state = MovementState.walk;
            sprite.flipX = true;
            UpdateAttackPointDirection(-1f);
        }
        else
        {
            state = MovementState.idle;
        }

        if (!isGrounded())
        {
            if (rb.velocity.y > 0.1f) state = MovementState.jump;
            else if (rb.velocity.y < -0.1f) state = MovementState.fall;
        }

        currentState = state;
        anim.SetInteger("state", (int)state);
    }

    private void UpdateAttackPointDirection(float xDir)
    {
        if (attackPoint == null) return;

        Vector3 localPos = attackPoint.localPosition;
        localPos.x = Mathf.Abs(localPos.x) * Mathf.Sign(xDir);
        attackPoint.localPosition = localPos;
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
        if (isAttacking || !isGrounded()) return; // Tidak bisa menyerang di udara

        isAttacking = true;
        currentState = MovementState.attack;
        anim.SetInteger("state", (int)currentState);

        DealDamageToEnemy();
        StartCoroutine(ResetAttack());
    }

    public void MobileAttack() => PerformAttack();

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(1.05f);
        isAttacking = false;
    }

    private void DealDamageToEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            Musuh musuhScript = enemy.GetComponent<Musuh>();
            if (musuhScript != null)
            {
                int randomDamage = Random.Range(25, 40);
                Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
                musuhScript.TakeDamage(randomDamage, knockDir, true);
            }
        }
    }

    public void MoveRight(bool isPressed) => mobileInputX = isPressed ? 1f : 0f;
    public void MoveLeft(bool isPressed) => mobileInputX = isPressed ? -1f : 0f;
    public void MobileJump() { if (isGrounded()) Jump(); }

    public void TakeDamage(int damage, Vector2 direction)
    {
        if (isKnockedBack) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player Mati");
            FindObjectOfType<LoadScene>().ShowGameOverScreen();
        }

        isKnockedBack = true;
        StartCoroutine(HandleKnockback(direction.normalized));
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBarImage != null)
        {
            float fill = Mathf.Clamp01((float)currentHealth / maxHealth);
            healthBarImage.fillAmount = fill;
            Debug.Log("Bar HP di-update: " + fill + ", Ref: " + healthBarImage.name);
        }
        else
        {
            Debug.LogWarning("healthBarImage belum di-assign!");
        }
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

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateHealthUI(); // ⬅ Penting agar bar terupdate
        Debug.Log("Healed: " + amount + ", Current Health: " + currentHealth);
    }
}
