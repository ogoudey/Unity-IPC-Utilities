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
    public ConcurrentQueue<string> frameQueue = new ConcurrentQueue<string>();
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
                //using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    ReadLoop(stream);
                    //ReadLoop(reader);
                    //ReadlineLoop(reader);
                }
            }
            
            
        }
        catch (Exception ex)
        {
            Debug.LogError($"Server error: {ex}");
        }
    }

    private void ReadLoop(NetworkStream stream)
    {
        byte[] lenBytes = new byte[4];
        while (running)
        {
            // Read 4-byte length header
            int readLen = 0;
            while (readLen < 4)
                readLen += stream.Read(lenBytes, readLen, 4 - readLen);
            Array.Reverse(lenBytes);  // reverses array in place
            int frameLength = BitConverter.ToInt32(lenBytes, 0);
            // Read the exact frame data
            byte[] frameBytes = new byte[frameLength];
            int offset = 0;
            while (offset < frameLength)
                offset += stream.Read(frameBytes, offset, frameLength - offset);

            string frame = Encoding.UTF8.GetString(frameBytes);

            frameCounter++;
            int hash = frame.GetHashCode();
            //Debug.Log($"Received frame {frameCounter}, len={frame.Length}, hash={hash}");

            lock (sharedLockObj)
            {
                frameQueue.Enqueue(frame);  // latest-frame logic can discard old ones later
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
