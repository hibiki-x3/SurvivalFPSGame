using UnityEngine;
using UnityEngine.AI;

public class AxeZomb_Running_State : StateMachineBehaviour
{
    UnityEngine.AI.NavMeshAgent agent;
    Transform player;

    public float runningSpeed = 6f;
    public float attackRange = 2.5f;

    private bool CanNavigate()
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }

    private bool TryCachePlayer()
    {
        if (player != null)
        {
            return true;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
        return player != null;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<UnityEngine.AI.NavMeshAgent>();
        TryCachePlayer();

        animator.SetBool("isRunning", true);

        if (agent != null)
        {
            agent.speed = runningSpeed;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!TryCachePlayer() || !CanNavigate())
        {
            return;
        }

        agent.SetDestination(player.position);

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer <= attackRange)
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
