using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;

public class MessageProducerBehavior : MonoBehaviour
{
    [SerializeField] private int port = 5000;

    private MessageListenerServer server;
    
    public Queue<string> messageQueue = new Queue<string>();
    
    void Start()
    {
        server = new MessageListenerServer(port);
        server.OnMessageReceived += HandleMessage;
        server.Start();
        Debug.Log($"Server started on port {port}");
    }
    
    public void HandleMessage(string line, TcpClient client)
    {
        Debug.Log($"Enqueueing line");
        messageQueue.Enqueue(line);
    }
    public void HandleMessage(string line)
    {
        Debug.Log($"Enqueueing line");
        messageQueue.Enqueue(line);
    }
    
    void OnDestroy()
    {
        if (server != null)
        {
            server.Stop();
        }
    }
}
