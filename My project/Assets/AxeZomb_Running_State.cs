using UnityEngine;
using UnityEngine.AI;

public class AxeZomb_Running_State : StateMachineBehaviour
{
    UnityEngine.AI.NavMeshAgent agent;
    Transform player;

    public float runningSpeed = 6f;
    public float stopRunningRange = 21;
    public float attackRange = 2.5f;

    private bool CanNavigate()
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<UnityEngine.AI.NavMeshAgent>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;

        if (agent != null)
        {
            agent.speed = runningSpeed;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null || !CanNavigate())
        {
            return;
        }

        agent.SetDestination(player.position);

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer > stopRunningRange)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", true);
        }
        else if (distanceFromPlayer <= attackRange)
        {
            animator.SetBool("isAttacking", true);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
         if (CanNavigate())
         {
                agent.SetDestination(agent.transform.position);
         }
    }

    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
