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

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = animator.GetComponent<NavMeshAgent>();

        agent.speed = walkingSpeed;
        timer = 0;

        // Get all waypoints from the scene and add them to the list
        waypoints.Clear();
        GameObject waypointsParent = GameObject.FindGameObjectWithTag("Waypoints");
        foreach (Transform waypoint in waypointsParent.transform)
        {
            waypoints.Add(waypoint);
        }
        Vector3 randomWaypoint = waypoints[Random.Range(0, waypoints.Count)].position;
        agent.SetDestination(randomWaypoint);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check if the agent has reached its destination and reset the destination to a new random waypoint
       if(agent.hasPath && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 randomWaypoint = waypoints[Random.Range(0, waypoints.Count)].position;
            agent.SetDestination(randomWaypoint);
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
       agent.SetDestination(agent.transform.position);
    }

    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
