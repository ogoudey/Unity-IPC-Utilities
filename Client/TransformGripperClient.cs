using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Diagnostics;

[Serializable]
public class TransformGripperData
{
    public float px, py, pz;
    public float rx, ry, rz, rw;
    public float sx, sy, sz;

    public float gripper;
    public TransformGripperData(Transform t, float squeeze)
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

        gripper = squeeze;
    }
}


public class TransformGripperClient : MessageClientBehavior
{      
    private int transformsSent = 1;

    
    protected override void OnConnected()
    {
        UnityEngine.Debug.Log("[DebugMessageSender] Connected to Python.");
        SendMessageString("{\"hello\": \"python\"}");
    }

    protected override void OnConnectionFailed(Exception e)
    {
        // Wasting the stream but it's whatevs
        UnityEngine.Debug.LogError($"[DebugMessageSender] Connection failed: {e.Message}");
    }

    void Update()
    {
        if (transformsSent % 1 == 0)
        {
            float squeezeRight = SteamVR_Actions.default_Squeeze.GetAxis(
                SteamVR_Input_Sources.RightHand
            );

            TransformGripperData data = new TransformGripperData(this.transform, squeezeRight);
            string json = JsonUtility.ToJson(data);
            //UnityEngine.Debug.Log(json);
            SendMessageString($"{json}");   
        }
        transformsSent ++;
    }
}
