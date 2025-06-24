using System.Collections;
using UnityEngine;

public class Musuh : MonoBehaviour
{
    public Transform spriteTransform;
    public float speed = 2f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;  
    protected float lastAttackTime = -Mathf.Infinity;

    public Transform pointA;
    public Transform pointB;
    private Vector3 nextPoint;

    public float chaseRadius = 5f;
    protected bool isChasing = false;

    public int damage = 20;
    public float knockbackForce = 5f;
    public int maxHealth = 100;
    private int currentHealth;

    public bool isBoss = false; // Tambahan penting

    protected Rigidbody2D rb;
    protected Transform player;
    protected bool isKnockedBack = false;
    protected Animator animator;

    protected virtual void Start()
    {
        if (spriteTransform != null)
            animator = spriteTransform.GetComponent<Animator>();
        else
            Debug.LogError("spriteTransform belum diset di Inspector");

        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        nextPoint = pointB.position;
    }

    private void Update()
    {
        if (player == null || isKnockedBack) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isChasing = distanceToPlayer <= chaseRadius;

        if (isChasing)
            ChasePlayer();
        else
            Patrol();
    }

    private void Patrol()
    {
        animator.SetBool("isMoving", true);

        Vector2 direction = (nextPoint - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

        if (Vector2.Distance(transform.position, nextPoint) < 0.2f)
        {
            nextPoint = Vector2.Distance(nextPoint, pointA.position) < 0.1f ? pointB.position : pointA.position;
        }

        FlipSprite(direction.x);
    }

    private void ChasePlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
            animator.SetBool("isMoving", true);
            FlipSprite(direction.x);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetBool("isMoving", false);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                animator.SetTrigger("Attack");
                lastAttackTime = Time.time;
            }
        }
    }

    public void DealDamage()
    {
        if (player == null) return;

        float xDistance = Mathf.Abs(transform.position.x - player.position.x);
        float yDistance = Mathf.Abs(transform.position.y - player.position.y);

        if (xDistance <= attackRange && yDistance <= 1f)
        {
            PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
            if (playerScript != null)
            {
                Vector2 knockbackDir = (player.position - transform.position).normalized;
                playerScript.TakeDamage(damage, knockbackDir);
            }
        }
    }

    protected void FlipSprite(float xDir)
    {
        float originalX = Mathf.Abs(spriteTransform.localScale.x);

        spriteTransform.localScale = new Vector3(xDir > 0 ? originalX : -originalX, 1f, 1f);
    }

    public virtual void TakeDamage(int damage, Vector2 direction, bool fromPlayer)
    {
        if (isKnockedBack) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die(); // Gunakan method yang bisa dioverride
        }
        else
        {
            if (fromPlayer)
                StartCoroutine(HandleKnockback(direction.normalized));
        }
    }

    private IEnumerator HandleKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;

        Vector2 force = direction * knockbackForce * rb.mass;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.2f);
        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isChasing = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isChasing = false;
            rb.velocity = Vector2.zero;
        }
    }

    protected virtual void Die()
    {
        animator.SetTrigger("Die");
        rb.velocity = Vector2.zero;
        this.enabled = false;

        if (isBoss && BossManager.Instance != null)
        {
            BossManager.Instance.HandleBossDefeated();
        }

        StartCoroutine(WaitAndDestroy(1.5f));
    }

    private IEnumerator WaitAndDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
