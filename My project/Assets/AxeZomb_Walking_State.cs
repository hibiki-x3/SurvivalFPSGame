using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AxeZomb_Walking_State : StateMachineBehaviour
{
    float timer;
    public float walkTime = 0f;
    public float detectionRange = 24f;
    public float walkingSpeed = 2f;

    Transform player;
    NavMeshAgent agent;

    List<Transform> waypoints = new List<Transform>();

    private bool CanNavigate()
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }

    private bool TrySetRandomWaypoint()
    {
        // Avoid random index errors when no waypoints are configured.
        if (waypoints.Count == 0 || !CanNavigate())
        {
            return false;
        }

        Vector3 randomWaypoint = waypoints[Random.Range(0, waypoints.Count)].position;
        agent.SetDestination(randomWaypoint);
        return true;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cache references safely so missing scene tags do not throw at runtime.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
        agent = animator.GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            return;
        }

        agent.speed = walkingSpeed;
        timer = 0;

        // Get all waypoints from the scene and add them to the list
        waypoints.Clear();
        GameObject waypointsParent = GameObject.FindGameObjectWithTag("Waypoints");
        if (waypointsParent != null)
        {
            foreach (Transform waypoint in waypointsParent.transform)
            {
                waypoints.Add(waypoint);
            }
        }

        // Start patrol only if at least one waypoint exists.
        TrySetRandomWaypoint();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Skip update work when required references are missing.
        if (player == null || !CanNavigate())
        {
            return;
        }

        // Check if the agent has reached its destination and reset the destination to a new random waypoint
       if(agent.hasPath && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Pick a new patrol target when the current one is reached.
            TrySetRandomWaypoint();
        }

        // Check distance from player and transition to running state if within detection range like in idle state
        timer += Time.deltaTime;
        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer < detectionRange)
        {
            animator.SetBool("isRunning", true);
        }
        if (timer >= walkTime)
        {
            animator.SetBool("isIdle", true);
            animator.SetBool("isWalking", false);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
            // Guard against disabled/off-mesh agent (e.g. after death) to prevent SetDestination errors.
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
