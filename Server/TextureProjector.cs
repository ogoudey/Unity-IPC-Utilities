using UnityEngine;
using UnityEngine.UI;
using System; // This 'using' statement gives you access to System.Convert
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Security;

public class TextureProjector : MessageLatestBehavior
{
    [SerializeField] private GameObject canvas;

            

    private RawImage rawImage;      // The component we draw to
    private Texture2D texture;      // Reused texture to avoid allocations

    private VR_UI ui;
    private int numFramesApplied = 1;
    private void Awake()
    {   

        if (canvas == null)
        {
            //UnityEngine.Debug.LogError("[TextureProjector] No canvas assigned.");
            return;
        }

        rawImage = canvas.GetComponent<RawImage>();
        if (rawImage == null)
            UnityEngine.Debug.LogError("[TextureProjector] Canvas object must have a RawImage component.");
    
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    

    protected override void ProcessMessage(string msg)
    {

        //if (string.IsNullOrWhiteSpace(msg)) return;
        //if (msg == null) return;
        // Remove trailing newline (if any)
        //msg = msg.TrimEnd('\n', '\r');

        ApplyFrame(msg);

    }

    private void ApplyFrame(string msg)
    {
        if (string.IsNullOrEmpty(msg))
        return;
        //UnityEngine.Debug.Log($"Applying frame {numFramesApplied} of hash {msg.GetHashCode()}");
        // Required: strip prefix if present
        int comma = msg.IndexOf(',');
        if (comma != -1)
            msg = msg.Substring(comma + 1);

        msg = msg.Trim();
        byte[] bytes = Convert.FromBase64String(msg);

        // ALWAYS create a new texture
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        tex.LoadImage(bytes);       // decode PNG
        rawImage.texture = tex;     // assign new texture

        // free old one
        if (texture != null)
            Destroy(texture);

        texture = tex;
        numFramesApplied++;

    }

    
    /*
    private void ApplyFrameAsync(string msg)
    {
        // Run decoding in a background thread
        Task.Run(() =>
        {
            int comma = msg.IndexOf(',');
            if (comma != -1) msg = msg.Substring(comma + 1);
            byte[] bytes = Convert.FromBase64String(msg.Trim());

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            // Push to main thread
            lock (server.sharedLockObj)
            {
                texture = tex; // overwrite previous
            }
        });
    }
    */

    private void OnDestroy()
    {
        if (texture != null)
            Destroy(texture);
    }
}
