using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TransformGripperData
{
    public float px, py, pz;
    public float rx, ry, rz, rw;
    public float sx, sy, sz;

    public TransformGripperData(Transform t)
    {
        px = t.position.x;
        py = t.position.y;
        pz = t.position.z;

        rx = t.rotation.x;
        ry = t.rotation.y;
        rz = t.rotation.z;
        rw = t.rotation.w;

        sx = t.localScale.x;
        sy = t.localScale.y;
        sz = t.localScale.z;
    }
}


public class TransformGripperClient : MessageClientBehavior
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
        TransformGripperData data = new TransformGripperData(this.transform);
        string json = JsonUtility.ToJson(data);
        Debug.Log(json);
        SendMessageString($"{json}");
    }
}
