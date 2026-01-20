using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveTo : MonoBehaviour
{
    [Header("Destinations (auto-loaded by tag)")]
    private string destinationTag = "Destination";
    public List<Transform> destinations = new List<Transform>();

    [Header("Current Goal")]
    public Transform currentGoal;

    [Header("Movement")]
    public float reachDistance = 1.0f;

    private NavMeshAgent agent;
    private Transform lastGoal;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = reachDistance;
    }

    void Start()
    {
        destinations.Clear();
        foreach (var obj in GameObject.FindGameObjectsWithTag(destinationTag))
        {
            destinations.Add(obj.transform);
        }

        ForceRepath();
    }

    void Update()
    {
        if (currentGoal == null)
            return;

        // 🔹 Detect goal change
        if (currentGoal != lastGoal)
        {
            ForceRepath();
            return;
        }

        if (agent.pathPending)
            return;

        // 🔹 Reached goal?
        if (agent.remainingDistance <= agent.stoppingDistance &&
            !agent.hasPath)
        {
            OnReachedGoal();
        }
    }

    public void SetDestination(Transform goal)
    {
        if (goal == null)
            return;

        currentGoal = goal;
        ForceRepath();
    }

    private void ForceRepath()
    {
        if (currentGoal == null)
            return;

        lastGoal = currentGoal;

        agent.ResetPath();                 // IMPORTANT
        agent.SetDestination(currentGoal.position);
    }

    private void OnReachedGoal()
    {
        // Hook for patrol / idle / next goal
    }
}
