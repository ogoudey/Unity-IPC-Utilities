using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


public class ClientDebug : MessageClientBehavior
{
    protected override void OnConnected()
    {
        Debug.Log("[DebugMessageSender] Connected to Python.");
        SendMessageString("{\"hello\": \"python\"}");
    }

    protected override void OnConnectionFailed(Exception e)
    {
        // Wasting the stream but it's whatevs
        //Debug.LogError($"[DebugMessageSender] Connection failed: {e.Message}");
    }

    void Update()
    {
        SendMessageString("{\"event\": \"space_pressed\"}");
    }
}
