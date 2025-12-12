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
        if (updates % 3 != 0){
            
            Debug.Log($"Update # {updates}");
            return;
        }
        
        string frameToApply = null;

        //Debug.Log($"[Dequeue] Applying frame, queue size before: {server.frameQueue.Count}");
        // Discard all but the newest frame
        lock (server.sharedLockObj)
        {
            while (server.frameQueue.Count > 1)
                server.frameQueue.TryDequeue(out _);

            if (server.frameQueue.TryDequeue(out var frame))
                frameToApply = frame;
        }

        if (frameToApply != null)
            //Debug.Log($"[Dequeue] Applying frame, queue size after: {server.frameQueue.Count}");
            ProcessMessage(frameToApply);
        
        
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
