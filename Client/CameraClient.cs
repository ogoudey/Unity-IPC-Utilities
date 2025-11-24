using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TransformData
{
    public float px, py, pz;
    public float rx, ry, rz, rw;
    public float sx, sy, sz;

    public TransformData(Transform t)
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


public class CameraClient : MessageClientBehavior
{      
    private Camera cam;
    private TcpClient client;
    private NetworkStream stream;
    private Texture2D texture;
    private RenderTexture rt;
    void Start()
    {
        cam = GetComponent<Camera>();

        rt = new RenderTexture(640, 480, 24);
        texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);

        client = new TcpClient();
        client.Connect(IPAddress.Parse(ip), port);
        stream = client.GetStream();
    }
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

    override void OnDestroy()
    {
        stream?.Close();
        client?.Close();
        if (rt != null) rt.Release();
    }
    void LateUpdate()
    {
        // Render camera → Read into Texture2D
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();
        cam.targetTexture = null;

        // Encode frame
        byte[] pngBytes = texture.EncodeToPNG();

        SendMessagePNG(pngBytes);
    }
}
