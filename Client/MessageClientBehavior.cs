using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

public abstract class MessageClientBehavior : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] protected string host = "127.0.0.1";
    [SerializeField] protected int port = 5000;

    protected TcpClient client;
    protected NetworkStream stream;
    protected bool connected = false;

    protected virtual void Start()
    {
        TryConnect();
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
        }
        catch (Exception e)
        {
            Debug.Log($"[MessageSender] Send failed: {e}");
            connected = false;
            Close();
            TryConnect();
        }
    }

    public void SendMessagePNG(byte[] pngBytes)
    {
        if (!connected){
            TryConnect();
            return;
        }

        byte[] lengthBytes = System.BitConverter.GetBytes(pngBytes.Length);

        try
        {
            stream.Write(lengthBytes, 0, bytes.Length);
            stream.Write(pngBytes, 0, bytes.Length);
            stream.Flush();
        }
        catch (Exception e)
        {
            Debug.Log($"[MessageSender] Send failed: {e}");
            connected = false;
            Close();
            TryConnect();
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
        Debug.Log("Connection closed");
        connected = false;
    }
}

