using UnityEngine;
using UnityEngine.AI;

public class AxeZomb : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private int health = 100;
    private Animator animator;
    private NavMeshAgent navAgent;
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = speed;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            animator.SetTrigger("Die");
        }
        else
        {
            animator.SetTrigger("Hit");
        }
        GetComponent<CapsuleCollider>().enabled = false;
    }
}
