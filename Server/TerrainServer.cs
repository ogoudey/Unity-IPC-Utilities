using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class HeightmapGrid
{
    public int width;
    public int height;
    public float[] data;

    

    public HeightmapGrid(float[,] arr)
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

public static class MeshHeightmapJson
{
    // Converts a mesh to JSON like: { "1.500": { "5.200": 2.0, "5.100": 2.2 }, ... }
    public static string MeshToCompactJson(Mesh mesh, Transform meshTransform = null)
    {
        // Step 1: collect into dictionary
        var dict = new Dictionary<string, Dictionary<string, float>>();

        foreach (var v in mesh.vertices)
        {
            Vector3 vertex = v;
            if (meshTransform != null)
                vertex = meshTransform.localToWorldMatrix.MultiplyPoint3x4(v);

            string xKey = vertex.x.ToString("F3");
            string zKey = vertex.z.ToString("F3");

            if (!dict.ContainsKey(xKey))
                dict[xKey] = new Dictionary<string, float>();

            dict[xKey][zKey] = vertex.y;
        }

        // Step 2: manually serialize to JSON
        var sb = new StringBuilder();
        sb.Append("{");

        bool firstX = true;
        foreach (var xKvp in dict)
        {
            if (!firstX) sb.Append(",");
            firstX = false;

            sb.Append("\"").Append(xKvp.Key).Append("\":{");

            bool firstZ = true;
            foreach (var zKvp in xKvp.Value)
            {
                if (!firstZ) sb.Append(",");
                firstZ = false;

                sb.Append("\"").Append(zKvp.Key).Append("\":").Append(zKvp.Value.ToString("F3"));
            }

            sb.Append("}");
        }

        sb.Append("}");
        return sb.ToString();
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
    private string cachedHeightmap;
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
                
                Debug.Log(cachedHeightmap);
                SendHeadedMessage(stream, cachedHeightmap);
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

    public string GetHeightMap()
    {
        Terrain terrain = GetComponent<Terrain>();

        if (terrain == null)
        {
            MeshFilter mf = GetComponentInChildren<MeshFilter>();
            Mesh mesh = mf.mesh;

            return MeshHeightmapJson.MeshToCompactJson(mesh, mf.transform);
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
        HeightmapGrid data = new HeightmapGrid(heights);
        return JsonUtility.ToJson(data);
    }
}
