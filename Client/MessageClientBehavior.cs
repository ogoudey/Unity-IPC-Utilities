using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

public abstract class MessageClientBehavior : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] protected string host = "192.168.0.211";
    [SerializeField] protected int port = 5002;

    protected TcpClient client;
    protected NetworkStream stream;
    protected bool connected = false;

    private int sentStringMessages = 0;
    private int failedStringMessages = 0;


    private VR_UI ui;
    protected virtual void Start()
    {
        TryConnect();
    }

    void Awake()
    {
        ui = FindObjectOfType<VR_UI>();
        UnityEngine.Debug.Log(ui);
        ui.SetClientConnected(connected);
    }

    protected virtual void OnDestroy()
    {
        Close();
    }

    // ----------------------
    // Abstract: subclasses implement
    // ----------------------
    protected abstract void OnConnected();
    protected abstract void OnConnectionFailed(Exception e);

    // ----------------------
    // Public send functions
    // ----------------------
    public void SendMessageString(string msg)
    {
        if (!connected){
            TryConnect();
            return;
        }

        byte[] bytes = Encoding.UTF8.GetBytes(msg + "\n");

        try
        {
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();

            sentStringMessages++;
            
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log($"[MessageSender] Send failed: {e}");
            //connected = false;
            //Close();
            //TryConnect();
            failedStringMessages++;
        }
        //UnityEngine.Debug.Log($"Sent messages: {sentStringMessages}, failedStringMessages: {failedStringMessages}");
    }

    public void SendMessagePNG(byte[] pngBytes)
    {
        if (!connected){
            UnityEngine.Debug.Log("Reconnecting!");
            TryConnect();
            return;
        }

        byte[] lengthBytes = System.BitConverter.GetBytes(pngBytes.Length);

        try
        {
            stream.Write(lengthBytes, 0, lengthBytes.Length);
            stream.Write(pngBytes, 0, pngBytes.Length);
            stream.Flush();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log($"[MessageSender] Send failed: {e}");
            //connected = false;
            //Close();
            //TryConnect();
        }
    }

    // ----------------------
    // Connection handling
    // ----------------------
    private void TryConnect()
    {
        try
        {
            client = new TcpClient();
            client.Connect(host, port);
            stream = client.GetStream();
            connected = true;
            ui.SetClientConnected(connected);
            OnConnected();
        }
        catch (Exception e)
        {
            connected = false;
            OnConnectionFailed(e);
        }
    }

    private void Close()
    {
        try
        {
            stream?.Close();
            client?.Close();
        }
        catch { }
        UnityEngine.Debug.Log("Connection closed");
        connected = false;
    }
}

