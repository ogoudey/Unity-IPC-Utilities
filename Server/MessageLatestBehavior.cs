using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;

public abstract class MessageLatestBehavior : MonoBehaviour
{
    [SerializeField] private int port = 5000;

    private MessageListenerServer server;
    
    public string latestMessage = null;
    protected readonly object lockObj = new object();
    
    void Start()
    {
        server = new MessageListenerServer(port);
        server.OnMessageReceived += HandleMessage;
        server.Start();
        //Debug.Log($"Server started on port {port}");
    }
    
    public void HandleMessage(string line)
    {
        //Debug.Log("Attempting handle");
        lock (lockObj)
        {
            //Debug.Log($"Handling: {line}");
   
            latestMessage = line;  // overwrite old frames
        }
    }

    protected virtual void Update()
    {
        string frameToApply = null;

        lock (lockObj)
        {
            if (latestMessage != null)
            {
                //Debug.Log($"Setting frame to apply to: {latestMessage}");
                frameToApply = latestMessage;
                latestMessage = null;         // consume exactly one per frame
                ProcessMessage(frameToApply);
            }
            else
            {
                //Debug.Log("Latest message is null");
            }
        }
        
    }
    
    protected abstract void ProcessMessage(string msg);

    void OnDestroy()
    {
        if (server != null)
        {
            server.Stop();
        }
    }
}
