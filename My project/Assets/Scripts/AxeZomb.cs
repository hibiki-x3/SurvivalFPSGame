using UnityEngine;
using UnityEngine.AI;

public class AxeZomb : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int health = 100;
    [SerializeField] private float destroyAfterDeathDelay = 5f;
    private Animator animator;
    private NavMeshAgent navAgent;
    private GameObject player;
    private CapsuleCollider zombieCollider;
    private ScoreManager scoreManager;
    private bool isDead;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        // Get the Animator, NavMeshAgent, and CapsuleCollider components
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        zombieCollider = GetComponent<CapsuleCollider>();
        scoreManager = FindObjectOfType<ScoreManager>();
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

            scoreManager?.AddScore(10);

            // Destroy the zombie after a delay to allow the death animation to play
            Destroy(gameObject, destroyAfterDeathDelay);
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }
}
