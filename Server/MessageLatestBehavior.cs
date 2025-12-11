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

    private int updates = 0;
    protected MessageListenerServer server;
        
    void Start()
    {
        server = new MessageListenerServer(port);
        //server.OnMessageReceived += HandleMessage;
        server.Start();
        //Debug.Log($"Server started on port {port}");
    }
    
    /*
    public void HandleMessage(string line)
    {
        //Debug.Log("Attempting handle");
        lock (server.sharedLockObj)
        {
            //Debug.Log($"Handling: {line}");
   
            latestMessage = line;  // overwrite old frames
        }
    }*/

    protected virtual void Update()
    {
        updates++;
        if (updates % 10 != 0){
            
            Debug.Log($"Update # {updates}");
            return;
        }
        
        string frameToApply = null;

        
        if (server.latestMessage != null)
        {
            //Debug.Log($"Setting frame to apply to: {latestMessage}");
            
            lock (server.sharedLockObj)
            {   
                frameToApply = server.latestMessage;
                Debug.Log($"Change to latestMessage {updates.ToString()}");
                server.latestMessage = null;        // consume exactly one per frame
                
            }
            Debug.Log($"We just nullified {server.latestMessage}");
            if (frameToApply != null)
                ProcessMessage(frameToApply);
        }
        else
        {
            //Debug.Log("Latest message is null");
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
