using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections.Concurrent;

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


    // Communication
    public int port = 5000;
    private JsonTcpServer server;
    private ConcurrentQueue<InputMsg> inboundMessageQueue = new();
    private ConcurrentQueue<OutputMsg> outboundMessageQueue = new();
    // end Communication

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
            destinations.Add(obj.transform); // should be obj.name
        }

        ForceRepath();
        // Communication
        server = new JsonTcpServer(port, outboundMessageQueue);
        server.OnMessageReceived += msg =>
        {
            // background thread → queue
            inboundMessageQueue.Enqueue(msg);
        };
        server.Start();
    }

    string getDestinations()
    {
        return destinations.ToString();
    }

    void Update()
    {
        // Communications
        while (inboundMessageQueue.TryDequeue(out var msg))
        {
            Debug.Log($"Received func={msg.func}, y={msg.arg}");
            if (msg.x == "updateGoal")
            {
                currentGoal = msg.y;
                outboundMessageQueue.enqueue(new OutputMsg { type = "status", content = "Goal updated" });
            }
            if (msg.x == "getDestinations")
            {
                outboundMessageQueue.enqueue(new OutputMsg { type = "destinations", content = getDestinations() });
            }
        }


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
