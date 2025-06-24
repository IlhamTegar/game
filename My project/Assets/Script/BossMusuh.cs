using UnityEngine;

public class BossMusuh : Musuh
{
    public float bossSpeed = 1f;
    public BossManager bossManager; // Tambahkan ini

    protected override void Start()
    {
        base.Start();
        pointA = null;
        pointB = null;

        if (bossManager == null)
        {
            bossManager = GetComponent<BossManager>(); // Auto assign jika belum dari Inspector
        }
    }

    private new void Update()
    {
        if (player == null || isKnockedBack) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        isChasing = distanceToPlayer <= chaseRadius;

        if (isChasing)
            ChasePlayerAsBoss();
        else
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isMoving", false);
        }
    }

    private void ChasePlayerAsBoss()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * bossSpeed, rb.velocity.y);
            animator.SetBool("isMoving", false); // animasi idle tetap dipakai
            FlipSprite(direction.x);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetBool("isMoving", false);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                int random = Random.Range(0, 2); // 0 atau 1
                if (random == 0)
                    animator.SetTrigger("Attack1");
                else
                    animator.SetTrigger("Attack2");

                lastAttackTime = Time.time;
            }
        }
    }

    protected override void Die()
    {
        base.Die();
        BossManager.Instance?.HandleBossDefeated();
    }

}
