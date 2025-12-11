using UnityEngine;
using UnityEngine.UI;
using System; // This 'using' statement gives you access to System.Convert
using System.Threading.Tasks;
using System.Diagnostics;

public class TextureProjector : MessageLatestBehavior
{
    [SerializeField] private GameObject canvas;

    private RawImage rawImage;      // The component we draw to
    private Texture2D texture;      // Reused texture to avoid allocations

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
    }

    protected override void ProcessMessage(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return;

        // Remove trailing newline (if any)
        msg = msg.TrimEnd('\n', '\r');

        ApplyFrame(msg);
    }

    private void ApplyFrame(string msg)
    {
        if (string.IsNullOrEmpty(msg))
        return;
        UnityEngine.Debug.Log($"Applying frame length {msg.Length}");
        // Required: strip prefix if present
        int comma = msg.IndexOf(',');
        if (comma != -1)
            msg = msg.Substring(comma + 1);

        // Now msg starts with pure Base64 (must not include whitespace!)
        msg = msg.Trim();
        UnityEngine.Debug.Log($"Applying frame length {msg.Length}");
        byte[] bytes;
        try
        {
            bytes = System.Convert.FromBase64String(msg); // THIS will now work
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"Conversion failed.");
            return;
        }
        if (texture == null)
            texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        rawImage.texture = texture;
    }

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
            lock (lockObj)
            {
                texture = tex; // overwrite previous
            }
        });
    }

    private void OnDestroy()
    {
        if (texture != null)
            Destroy(texture);
    }
}