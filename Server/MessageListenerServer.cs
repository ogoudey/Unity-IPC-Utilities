using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.IO;
using System.Collections;

public class MessageListenerServer
{
    private readonly int port;
    private TcpListener listener;
    private Thread listenerThread;
    
    private bool running = false;

    public Action<string> OnMessageReceived;
     
    public MessageListenerServer(int port)
    {
        this.port = port;
    } 
        
    public void Start()
    {
        StartServer();
    }
    
    public void Stop()
    {
        running = false;
        listener?.Stop();
        listenerThread?.Abort();
    }

    public void StartServer()
    {
        running = true;
        listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();

        listenerThread = new Thread(ListenLoop);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    private void ListenLoop()
    {
        try
        {
            while (running)
            {
                using (TcpClient client = listener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    while (running && (line = reader.ReadLine()) != null)
                    {

                        OnMessageReceived?.Invoke(line);

                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Server error: {ex}");
        }
    }

    
}
