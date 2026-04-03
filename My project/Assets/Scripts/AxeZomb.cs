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
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        // Get the Animator, NavMeshAgent, and CapsuleCollider components
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        zombieCollider = GetComponent<CapsuleCollider>();
        navAgent.speed = speed;
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

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        if (player == null || playerHealth == null || playerHealth.IsDead)
        {
            return;
        }

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance > attackRange)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        playerHealth.TakeDamage(attackDamage);
    }
}
