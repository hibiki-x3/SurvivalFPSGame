using UnityEngine;
using UnityEngine.AI;

public class AxeZomb : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int health = 100;
    [SerializeField] private float destroyAfterDeathDelay = 5f;
    [Header("Melee Attack")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 2.4f;
    private Animator animator;
    private NavMeshAgent navAgent;
    private GameObject player;
    private PlayerHealth playerHealth;
    private CapsuleCollider zombieCollider;
    private bool isDead;
    private float nextAttackTime;

    private void Start()
    {
        CachePlayerReferences();

        // Get the Animator, NavMeshAgent, and CapsuleCollider components
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        zombieCollider = GetComponent<CapsuleCollider>();
        navAgent.speed = speed;
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        // Fallback attack check so damage still applies if animator state callbacks are skipped.
        TryAttackPlayer();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        health -= damage;
        if (health <= 0)
        {
            isDead = true;

            int randomDeath = Random.Range(0, 2);
            if (randomDeath == 0)
            {
                animator.SetTrigger("Die1");
            }
            else
            {
                animator.SetTrigger("Die2");
            }

            // Stop the zombie from moving and disable its collider
            navAgent.isStopped = true;
            navAgent.enabled = false;
            zombieCollider.enabled = false;

            HUDManager.Instance?.AddScore(10);

            // Destroy the zombie after a delay to allow the death animation to play
            Destroy(gameObject, destroyAfterDeathDelay);
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }

    public void TryAttackPlayer()
    {
        if (isDead || Time.time < nextAttackTime)
        {
            return;
        }

        CachePlayerReferences();

        if (player == null || playerHealth == null || playerHealth.IsDead)
        {
            return;
        }

        Vector3 zombiePos = transform.position;
        Vector3 playerPos = player.transform.position;
        zombiePos.y = 0f;
        playerPos.y = 0f;

        float distance = Vector3.Distance(playerPos, zombiePos);
        if (distance > attackRange)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        playerHealth.TakeDamage(attackDamage);
    }

    private void CachePlayerReferences()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerHealth == null)
        {
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();

                if (playerHealth == null)
                {
                    playerHealth = player.AddComponent<PlayerHealth>();
                }
            }

            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
            }
        }
    }
}
