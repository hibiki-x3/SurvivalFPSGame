using UnityEngine;
using UnityEngine.AI;

public class AxeZomb_Attack_State : StateMachineBehaviour
{
    Transform player;
    NavMeshAgent agent;

    public float attackRange = 2.5f;

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

    private void LookAtPlayer(Animator animator)
    {
        Vector3 direction = (player.position - animator.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cache references. Player may not exist yet, so keep retrying in update.
        TryCachePlayer();
        agent = animator.GetComponent<NavMeshAgent>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!TryCachePlayer())
        {
            return;
        }

        AxeZomb zombie = animator.GetComponent<AxeZomb>();
        if (zombie != null)
        {
            zombie.TryAttackPlayer();
        }

        // Face the player using smooth manual rotation.
        LookAtPlayer(animator);

        // Check distance and exit attack if player moved beyond range.
        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer > attackRange)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isRunning", true);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }

    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

}
