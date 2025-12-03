using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class PathFollower : Server
{   
    [SerializeField] public float speed = 1f;
    
    private float[,] cachedHeightmap;
    private TcpListener listener;
    private Thread serverThread;
    private bool running = true;
    private float px, pz;
    private float px_i, pz_i;
    public float v_i;
    private float theta;
	
	
    
    
    
    public bool follow = false;
    void Start()
    {
        px = this.transform.position.x;
        pz = this.transform.position.z;
        StartServer();
    }

    protected override void HandleClient(TcpClient client)
    {

        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[4096];
            int count = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, count).Trim();
            Debug.Log($"Request received: {message}");
            if (message == "STOP"){
            	follow = false;
            	Send(stream, $"{px} {pz}\n");
            	return;
            }
            string[] coords = message.Split(" ");
            foreach (var c in coords) Debug.Log($"coord = '{c}'");
            px_i = float.Parse(coords[0]);
            pz_i = float.Parse(coords[1]);
            
            //SendHeadedMessage(stream, $"{px} {pz}\n");
            Send(stream, $"{px} {pz}\n");
            follow = true;
        }

        client.Close();
    }

    void Update()
    {
        if (v_i == null)
        {
            return;
        }
        if (follow)
        {
            px = this.transform.position.x;
            pz = this.transform.position.z;
            v_i = (float) Math.Sqrt(Math.Pow(px_i - px, 2) + Math.Pow(pz_i - pz, 2));
            theta = Mathf.Atan2(px_i - px, pz_i - pz) * Mathf.Rad2Deg;
            //theta = (float) Math.Atan2(px_i - px, pz_i - pz) * (float) (180.0 / Math.PI);
            //Debug.Log($"arctan({px_i} - {px}/{pz_i} - {pz}) = {Mathf.Atan2(px_i - px, pz_i - pz)} => {theta}");
            this.transform.rotation = Quaternion.Euler(0f, theta, 0f);
            this.transform.position += transform.forward * speed * Time.deltaTime;
        }
        
    }


}
