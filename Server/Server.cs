using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public abstract class Server : MonoBehaviour
{   
    [SerializeField] private int port = 5000;

    private TcpListener listener;
    private Thread serverThread;
    private bool running = true;
    void Start()
    {
        StartServer();
    }

    protected void StartServer()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        serverThread = new Thread(ServerLoop);
        serverThread.Start();
    }

    void OnApplicationQuit()
    {
        running = false;
        listener?.Stop();
        serverThread?.Abort();
    }
    protected void ServerLoop()
    {
        while (running)
        {

        TcpClient client = listener.AcceptTcpClient();
        HandleClient(client);


        }
    }

    protected abstract void HandleClient(TcpClient client);
 
    public void Send(NetworkStream stream, string msg)
    {
        if (!stream.CanWrite)
        {
            Debug.LogError("Cannot write to stream: it might be closed.");
            return;
        }

        byte[] response = Encoding.UTF8.GetBytes($"{msg}");
        stream.Write(response, 0, response.Length);
        stream.Flush();
    }

    public void SendHeadedMessage(NetworkStream stream, string json)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        int length = bytes.Length;

        // 4-byte little-endian length header
        byte[] header = BitConverter.GetBytes(length);

        try
        {
            // Send header
            stream.Write(header, 0, header.Length);
            // Send payload
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }
        catch (Exception e)
        {
            Debug.LogError("SendMessage failed: " + e);
        }
    }
}
