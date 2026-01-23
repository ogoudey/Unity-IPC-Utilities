using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;
public class JsonTcpServer
{
    public event Action<InputMsg> OnMessageReceived;

    private TcpListener listener;
    private Thread listenerThread;
    private bool isRunning;
    private readonly int port;

    public ConcurrentQueue<OutputMsg> outboundMessageQueue;

    public JsonTcpServer(int port, ConcurrentQueue<OutputMsg> obmq)
    {
        this.port = port;
        outboundMessageQueue = obmq;
    }

    public void Start()
    {
        if (isRunning) return;

        isRunning = true;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        listenerThread = new Thread(ListenLoop);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    public void Update()
    {
        // Why not output the outboundMessageQueue here?
    }

    public void Stop()
    {
        isRunning = false;
        try
        {
            listener?.Stop();
        }
        catch { }
    }

    private void ListenLoop()
    {
        while (isRunning)
        {
            try
            {
                var client = listener.AcceptTcpClient();
                UnityEngine.Debug.Log("Accepted Client.");
                Thread t = new Thread(() => HandleClient(client));
                t.IsBackground = true;
                t.Start();
            }
            catch
            {
                if (!isRunning) return;
            }
        }
    }

    private void HandleClient(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            // Start reader and write threads
            Thread read_t = new Thread(() => Reader(client, reader));
            read_t.IsBackground = true;
            read_t.Start();
            Thread write_t = new Thread(() => Writer(client, writer));
            write_t.IsBackground = true;
            write_t.Start();
            // keep client, etc. open
            UnityEngine.Debug.Log($"Threads started.");
            while (isRunning && client.Connected) // Unity-side stop and Python-side stop, respectively
            {
                // wait? pass?
            }
            
        }
    }

    private void Writer(TcpClient client, StreamWriter writer)
    {
        while (isRunning && client.Connected)
        {
            while (outboundMessageQueue.TryDequeue(out var response))
            {
                string jsonResponse = JsonUtility.ToJson(response) + "\n";
                Debug.Log($"Sending: {jsonResponse}");
                writer.WriteLine(jsonResponse);
                Debug.Log($"Sent: {jsonResponse}");
            }
        }
        UnityEngine.Debug.Log($"Writer ended.");
    }

    private void Reader(TcpClient client, StreamReader reader)
    {
        while (isRunning && client.Connected)
        {
            string line;
            try
            {
                line = reader.ReadLine();
                if (line == null) continue;
                if (line == "") continue;
            }
            catch
            {
                continue;
            }
            UnityEngine.Debug.Log($"Got {line}");
            InputMsg msg;
            try
            {
                msg = UnityEngine.JsonUtility.FromJson<InputMsg>(line);
            }
            catch
            {
                continue;
            }

            UnityEngine.Debug.Log($"Reader sees {msg.method}({msg.arg}).");
            OnMessageReceived?.Invoke(msg);
        }
        UnityEngine.Debug.Log($"Reader ended.");
    }
}
