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
using System.Collections.Concurrent;
public class MessageListenerServer
{
    private readonly int port;
    private TcpListener listener;
    private Thread listenerThread;
    public readonly object sharedLockObj = new object();
    public string latestMessage = null;
    private bool running = false;
    int frameCounter = 0;
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
        listener = new TcpListener(IPAddress.Any, port);
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
                    ReadLoop(reader);
                    //ReadlineLoop(reader);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Server error: {ex}");
        }
    }

    private void ReadLoop(StreamReader reader)
    {
        StringBuilder buffer = new StringBuilder();
        char[] chunk = new char[4096];

        while (running)
        {
            int read = reader.Read(chunk, 0, chunk.Length);
            if (read <= 0)
                break;

            buffer.Append(chunk, 0, read);

            int newlineIndex;
            while ((newlineIndex = IndexOfNewline(buffer)) >= 0)
            {
                // Extract one frame
                string frame = buffer.ToString(0, newlineIndex);
                buffer.Remove(0, newlineIndex + 1);

                frameCounter++;
                if (frameCounter % 1 == 0)
                    Debug.Log($"Received frame {frameCounter}, len={frame.Length}");

                lock (sharedLockObj)
                {
                    latestMessage = frame;  // overwrite only, no queue
                    Debug.Log($"Changing to (hash) {latestMessage.GetHashCode()}");
                }
            }
        }
    }

    private int IndexOfNewline(StringBuilder sb)
    {
        for (int i = 0; i < sb.Length; i++)
            if (sb[i] == '\n')
                return i;
        return -1;
    }



    private void ReadlineLoop(StreamReader reader)
    {
        string line;
        while (running && (line = reader.ReadLine()) != null)
        {
            try
            {
                frameCounter++;
                if (frameCounter % 30 == 0)
                    Debug.Log($"Received frame {frameCounter}");
                Debug.Log($"ReadLine() returned {line.Length} chars");
                OnMessageReceived?.Invoke(line);
            }
            catch (Exception ex)
            {
                Debug.Log("Handling error");
            }

        }
    }

    
}
