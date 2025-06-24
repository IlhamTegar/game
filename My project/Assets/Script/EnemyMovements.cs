using System.Collections;
using UnityEngine;

public class EnemyMovements : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float moveSpeed;
    public int patrolDestination;
    public float idleDuration;
    private bool isWaiting;

    [Header("Player Settings")]
    public GameObject player;
    public float chaseRadius = 5f;
    private bool hasDetectedPlayer = false;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    private float lastAttackTime = -Mathf.Infinity;

    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    private bool isKnockedBack = false;

    private Animator anim;
    private Rigidbody2D rb;

    private enum MovementsState
    {
        idle,
        run,
        attack
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        // Auto cari player hanya jika belum diset manual
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                player = found;
        }
    }

    void Update()
    {
        if (isKnockedBack || isWaiting) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        MoveAndAnimate(distanceToPlayer);
    }

    private void MoveAndAnimate(float distanceToPlayer)
    {
        MovementsState state = MovementsState.idle;
        Transform targetPoint = patrolPoints[patrolDestination];

        // === 1. Cek apakah mulai mengejar ===
        if (!hasDetectedPlayer && distanceToPlayer <= chaseRadius)
        {
            hasDetectedPlayer = true;
            Debug.Log("Player masuk zona kejar.");
        }

        // === 2. Jika belum deteksi player, tetap patroli ===
        if (!hasDetectedPlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
            {
                patrolDestination = (patrolDestination + 1) % patrolPoints.Length;
            }

            FlipSpriteTo(targetPoint.position);
            state = MovementsState.run;

            Debug.Log("Patroli Aktif. Posisi ke Player: " + distanceToPlayer);
        }
        else
        {
            // === 3. Setelah deteksi player, kejar / serang ===
            if (distanceToPlayer < attackRange)
            {
                rb.velocity = Vector2.zero;

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    anim.SetTrigger("Attack");
                    Attack();
                    lastAttackTime = Time.time;
                }

                state = MovementsState.attack;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
                state = MovementsState.run;
            }

            FlipSpriteTo(player.transform.position);
        }

        anim.SetInteger("state", (int)state);
    }

    private void FlipSpriteTo(Vector3 target)
    {
        if (target.x > transform.position.x)
            transform.localScale = new Vector2(-1, 1);
        else
            transform.localScale = new Vector2(1, 1);
    }

    private void Attack()
    {
        if (player == null) return;

        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
        if (playerScript != null)
        {
            Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
            playerScript.TakeDamage(20, knockbackDir);
        }
    }

    public void TakeDamage(int damage, Vector2 direction, bool fromPlayer)
    {
        if (isKnockedBack) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            anim.SetTrigger("Die");
            rb.velocity = Vector2.zero;
            this.enabled = false;
            StartCoroutine(WaitAndDestroy(1.5f));
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

    private IEnumerator WaitAndDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}


// using System.Collections;
// using UnityEngine;

// public class Musuh : MonoBehaviour
// {
//     public Transform spriteTransform;
//     public float speed = 2f;
//     public float attackRange = 1f;
//     public float attackCooldown = 1f;
//     private float lastAttackTime = -Mathf.Infinity;

//     public Transform pointA;
//     public Transform pointB;
//     private Vector3 nextPoint;

//     public float chaseRadius = 5f;
//     private bool isChasing = false;
//     private bool hasEnteredZone = false;

//     public int damage = 20;
//     public float knockbackForce = 5f;
//     public int maxHealth = 100;
//     private int currentHealth;

//     private Rigidbody2D rb;
//     protected Transform player;
//     private bool isKnockedBack = false;
//     private Animator animator;

//     protected virtual void Start()
//     {
//         if (spriteTransform != null)
//             animator = spriteTransform.GetComponent<Animator>();
//         else
//             Debug.LogError("spriteTransform belum diset di Inspector");

//         rb = GetComponent<Rigidbody2D>();
//         currentHealth = maxHealth;

//         GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//         if (playerObj != null)
//             player = playerObj.transform;

//         nextPoint = pointB.position;
//     }

//     private void Update()
//     {
//         if (player == null || isKnockedBack) return;

//         float distanceToPlayer = Vector2.Distance(transform.position, player.position);
//         float distanceToZone = Mathf.Min(Vector2.Distance(player.position, pointA.position), Vector2.Distance(player.position, pointB.position));

//         if (!hasEnteredZone && distanceToZone <= chaseRadius)
//         {
//             hasEnteredZone = true;
//             isChasing = true;
//         }
//         else if (hasEnteredZone && distanceToZone > chaseRadius * 1.2f)
//         {
//             isChasing = false;
//         }

//         if (isChasing)
//             ChasePlayer();
//         else
//             Patrol();
//     }

//     private void Patrol()
//     {
//         animator.SetBool("isMoving", true);
//         animator.SetBool("isAttacking", false);

//         Vector2 direction = (nextPoint - transform.position).normalized;
//         rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

//         if (Vector2.Distance(transform.position, nextPoint) < 0.2f)
//         {
//             nextPoint = (nextPoint == pointA.position) ? pointB.position : pointA.position;
//         }

//         FlipSprite(direction.x);
//     }

//     private void ChasePlayer()
//     {
//         float distance = Vector2.Distance(transform.position, player.position);

//         if (distance > attackRange)
//         {
//             Vector2 direction = (player.position - transform.position).normalized;
//             rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

//             animator.SetBool("isMoving", true);
//             animator.SetBool("isAttacking", false);
//             FlipSprite(direction.x);
//         }
//         else
//         {
//             rb.velocity = new Vector2(0, rb.velocity.y);
//             animator.SetBool("isMoving", false);

//             if (Time.time >= lastAttackTime + attackCooldown)
//             {
//                 animator.SetTrigger("Attack");
//                 lastAttackTime = Time.time;
//             }
//         }
//     }

//     // Dipanggil dari event di animasi
//     public void DealDamageToPlayer()
//     {
//         if (player == null) return;

//         PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
//         if (playerScript != null)
//         {
//             Vector2 knockbackDir = (player.position - transform.position).normalized;
//             playerScript.TakeDamage(damage, knockbackDir);
//         }
//     }

//     private void FlipSprite(float xDir)
//     {
//         float originalX = Mathf.Abs(spriteTransform.localScale.x);
//         spriteTransform.localScale = new Vector3(xDir > 0 ? originalX : -originalX, 1f, 1f);
//     }

//     public virtual void TakeDamage(int damage, Vector2 direction, bool fromPlayer)
//     {
//         if (isKnockedBack) return;

//         currentHealth -= damage;

//         if (currentHealth <= 0)
//         {
//             animator.SetTrigger("Die");
//             rb.velocity = Vector2.zero;
//             this.enabled = false;
//             StartCoroutine(WaitAndDestroy(1.5f));
//         }
//         else
//         {
//             if (fromPlayer)
//                 StartCoroutine(HandleKnockback(direction.normalized));
//         }
//     }

//     private IEnumerator HandleKnockback(Vector2 direction)
//     {
//         isKnockedBack = true;
//         rb.velocity = Vector2.zero;
//         Vector2 force = direction * knockbackForce * rb.mass;
//         rb.AddForce(force, ForceMode2D.Impulse);
//         yield return new WaitForSeconds(0.2f);
//         rb.velocity = Vector2.zero;
//         isKnockedBack = false;
//     }

//     private IEnumerator WaitAndDestroy(float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         Destroy(gameObject);
//     }
// }
