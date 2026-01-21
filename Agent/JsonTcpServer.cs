using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class JsonTcpServer
{
    public event Action<InputMsg> OnMessageReceived;

    private TcpListener listener;
    private Thread listenerThread;
    private bool isRunning;
    private readonly int port;

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
        while (outboundMessageQueue.TryDequeue(out var msg))
        {
            msg
        }
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
            Thread reader = new Thread(() => Reader(client, reader));
            reader.IsBackground = true;
            reader.Start();
            Thread reader = new Thread(() => Writer(client, writer));
            reader.IsBackground = true;
            reader.Start();
            // keep client, etc. open
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
            while (inboundMessageQueue.TryDequeue(out var response))
            {
                string jsonResponse = JsonUtility.ToJson(response) + "\n";
                Debug.Log($"Sending: {jsonResponse}");
                writer.WriteLine(jsonResponse);
            }
        }
    }

    private void Reader(TcpClient client, StreamReader reader)
    {
        while (isRunning && client.Connected)
        {
            string line;
            try
            {
                line = reader.ReadLine();
                if (line == null) break;
            }
            catch
            {
                break;
            }

            InputMsg msg;
            try
            {
                msg = UnityEngine.JsonUtility.FromJson<InputMsg>(line);
            }
            catch
            {
                continue;
            }

            // Notify Unity side (optional?)
            OnMessageReceived?.Invoke(msg);
        }
    }
}
