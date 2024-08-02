// using UnityEngine;
// using UnityEngine.AI;

// public class WaypointNavigation : MonoBehaviour
// {
//     public GameObject waypointParent; // The empty GameObject with child waypoint GameObjects
//     public float desiredTimePerWaypoint = 2f; // Desired time the agent should take to reach each waypoint
//     private Transform[] waypoints; // Array of waypoint transforms
//     private int currentWaypoint = 0; // Index of the current waypoint
//     private NavMeshAgent agent;

//     void Start()
//     {
//         agent = GetComponent<NavMeshAgent>();
//         SetupWaypoints();
//         SetAgentSpeed();
//         agent.SetDestination(waypoints[currentWaypoint].position);
//         PrintWaypointPositions();
//     }

//     void Update()
//     {
//         // Check if the agent has reached the current waypoint
//         if (agent.remainingDistance <= agent.stoppingDistance)
//         {
//             // Move to the next waypoint
//             currentWaypoint++;

//             // If we've reached the end of the waypoint list, start over
//             if (currentWaypoint >= waypoints.Length)
//             {
//                 currentWaypoint = 0;
//             }

//             // Set the new destination and agent speed
//             agent.SetDestination(waypoints[currentWaypoint].position);
//             SetAgentSpeed();
//         }
//     }

//     private void SetupWaypoints()
//     {
//         // Get the child transforms of the waypoint parent GameObject
//         waypoints = waypointParent.GetComponentsInChildren<Transform>();

//         // Exclude the parent transform from the waypoints array
//         waypoints = System.Array.FindAll(waypoints, t => t != waypointParent.transform);
//     }

//     private void SetAgentSpeed()
//     {
//         // Calculate the distance to the next waypoint
//         float distanceToNextWaypoint = Vector3.Distance(agent.transform.position, waypoints[currentWaypoint].position);

//         // Set the agent's speed to reach the next waypoint in the desired time
//         agent.speed = distanceToNextWaypoint / desiredTimePerWaypoint;
//     }

//     private void PrintWaypointPositions()
//     {
//         Debug.Log("Waypoint Positions:");
//         for (int i = 0; i < waypoints.Length; i++)
//         {
//             Debug.Log($"Waypoint {i}: {waypoints[i].position}");
//         }
//     }
// }
using UnityEngine;

public class WaypointNavigation : MonoBehaviour
{
    public Transform point1;
    public Transform point2;
    public float transitionDuration = 1.0f;

    private float transitionTimer = 0.0f;

    void Update()
    {
        // Increment the timer
        transitionTimer += Time.deltaTime;

        // Calculate interpolation factor
        float t = Mathf.Clamp01(transitionTimer / transitionDuration);

        // Interpolate between the positions of point1 and point2
        transform.position = Vector3.Lerp(point1.position, point2.position, t);

        // Check if transition is complete
        if (t >= 1.0f)
        {
            // Reset the timer for next transition
            transitionTimer = 0.0f;
        }
    }
}
