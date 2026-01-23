using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Text;

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
    public int port = 5006;
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

    string[] GetDestinations()
    {
        string[] names = new string[destinations.Count];

        for (int i = 0; i < destinations.Count; i++)
        {
            names[i] = destinations[i].name;
        }

        return names;
    }
    string[] GetFunctions()
    {
        string[] functionNames = new string[] {"SetGoalTo", "GetDestinations"};

        

        return functionNames;
    }

    void Update()
    {
        // Communications
        while (inboundMessageQueue.TryDequeue(out var msg))
        {
            Debug.Log($"Received method={msg.method}, arg={msg.arg}");
            if (msg.method == "SetGoalTo")
            {
                currentGoal = currentGoal = destinations.Find(t => t.name == msg.arg);
                if (currentGoal == null)
                {
                    outboundMessageQueue.Enqueue(new OutputMsg { type = "status", content = new string[] { "could not find destination" } });
                }
                else
                {
                    outboundMessageQueue.Enqueue(new OutputMsg { type = "status", content = new string[] { "goal updated" } });
                }
                
            }
            if (msg.method == "GetDestinations")
            {
                UnityEngine.Debug.Log($"getting Destinations...");
                outboundMessageQueue.Enqueue(new OutputMsg { type = "destinations", content = GetDestinations() });
            }
            if (msg.method == "GetFunctions")
            {
                UnityEngine.Debug.Log($"getting Functions...");
                outboundMessageQueue.Enqueue(new OutputMsg { type = "functions", content = GetFunctions() });
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
