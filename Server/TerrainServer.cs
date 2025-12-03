using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class HeightmapData
{
     public int width;
    public int height;
    public float[] data;

    

    public HeightmapData(float[,] arr)
    {
        int w = arr.GetLength(0);
        int h = arr.GetLength(1);

        float[] flat = new float[w * h];

        int index = 0;
        for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
                flat[index++] = arr[i, j];

        width = w;
        height = h;
        data = flat;
    }
}

[Serializable]
public class DestinationsData
{
    public DestinationInfo[] destinations;

    public DestinationsData(Transform[] transforms)
    {
        destinations = new DestinationInfo[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            destinations[i] = new DestinationInfo(transforms[i]);
        }
    }
}

[Serializable]
public class DestinationInfo
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;

    public DestinationInfo(Transform t)
    {
        name = t.name;
        position = t.position;
        rotation = t.rotation;
    }
}

public class TerrainServer : Server
{   
    private float[,] cachedHeightmap;
    private DestinationsData cachedDestinationsData;
    private DestinationInfo cachedBoatInfo;
    
    private TcpListener listener;
    private Thread serverThread;
    private bool running = true;
    
    [SerializeField] private Transform[] destinations;
    [SerializeField] private Transform boat;
    
    void Start()
    {
        cachedHeightmap = GetHeightMap();
        cachedBoatInfo = GetBoatInfo();
        cachedDestinationsData = new DestinationsData(GetDestinations());
        StartServer();
    }

    void Update()
    {
        cachedBoatInfo = GetBoatInfo();
    }
    
    protected override void HandleClient(TcpClient client)
    {
        Debug.Log("Request received");
        using (NetworkStream stream = client.GetStream())
        {
            Debug.Log("Request received");
            byte[] buffer = new byte[4096];
            int count = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, count).Trim();

            if (message == "getheightmap")
            {
                Debug.Log("Getting heightmap");
                HeightmapData data = new HeightmapData(cachedHeightmap);
                string json = JsonUtility.ToJson(data);
                Debug.Log(json);
                SendHeadedMessage(stream, json);
            }
            else
            {
            	if (message == "getdestinations")
            	{
            		Debug.Log("Getting destinations");
            		
            		string json = JsonUtility.ToJson(cachedDestinationsData);
            		Debug.Log(json);
            		SendHeadedMessage(stream, json);
            	}
            	else{
        	    if (message == "getboat")
		    	{
		    		Debug.Log("Getting boat coords");
		    		
		    		string json = JsonUtility.ToJson(cachedBoatInfo);
		    		SendHeadedMessage(stream, json);
		    	}
		    	else{
				Send(stream, "unknown command");
		    	}
            	}
                
            }
        }

        client.Close();
    }
    
    public DestinationInfo GetBoatInfo()
    {
    	return new DestinationInfo(boat);
    }
    
    public Transform[] GetDestinations()
    {
    	return destinations;
    }

    public float[,] GetHeightMap()
    {
        Terrain terrain = GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogError("Terrain reference is null.");
            return null;
        }

        TerrainData terrainData = terrain.terrainData;
        if (terrainData == null)
        {
            Debug.LogError("Terrain has no TerrainData.");
            return null;
        }

        int resolution = terrainData.heightmapResolution;

        // Extract the full height map
        float[,] heights = terrainData.GetHeights(
            0,                       // start x
            0,                       // start y
            resolution,              // width
            resolution               // height
        );

        Debug.Log($"Extracted height map: {resolution} × {resolution}");
        return heights;
    }
}
