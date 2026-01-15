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
        px = t.localPosition.x;
        py = t.localPosition.y;
        pz = t.localPosition.z;

        rx = t.localEulerAngles.x;
        ry = t.localEulerAngles.y;
        rz = t.localEulerAngles.z;

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
            // Get other inputs to send {"message": "do thing"}

            if (SteamVR_Actions.default_MenuPress.GetStateDown(SteamVR_Input_Sources.RightHand)) { // Select - the three horizontal lines - is pressed
                string jsonGo = $"{{\"message\": \"go\"}}";
                UnityEngine.Debug.Log(jsonGo);
                SendMessageString($"{jsonGo}");
                return;
            }

            if (SteamVR_Actions.default_SystemPress.GetStateDown(SteamVR_Input_Sources.RightHand)) { // Exit -  the button below the touchpad - is pressed
                string jsonStop = $"{{\"message\": \"stop\"}}";
                UnityEngine.Debug.Log(jsonStop);
                SendMessageString($"{jsonStop}");
                return;
            }

            float squeezeRight = SteamVR_Actions.default_Squeeze.GetAxis(
                SteamVR_Input_Sources.RightHand
            );
            // Other: send the transform...

            



            TransformGripperData data = new TransformGripperData(this.transform, squeezeRight);
            string json = JsonUtility.ToJson(data);
            if (transformsSent % 10 == 0)
            {
                UnityEngine.Debug.Log(json);
            }
            SendMessageString($"{json}");   
        }
        transformsSent ++;
    }
}
