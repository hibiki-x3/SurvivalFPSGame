using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Click2Move : MonoBehaviour
{
    private NavMeshAgent navAgent;

    [SerializeField] private LayerMask groundLayer; 

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update(){
        if (Input.GetMouseButtonDown(0)){
            if (Camera.main == null) return;
            // Create a ray from cam to mouse
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            // Check if ray hit ground
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer)){
                navAgent.SetDestination(hit.point); // Move to click point
            }
        }
    }
}
