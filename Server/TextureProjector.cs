using UnityEngine;
using UnityEngine.UI;
public class TextureProjector : MessageConsumerBehavior
{
    [SerializeField] private GameObject canvas;

    private RawImage rawImage;      // The component we draw to
    private Texture2D texture;      // Reused texture to avoid allocations

    private void Awake()
    {
        if (canvas == null)
        {
            Debug.LogError("[TextureProjector] No canvas assigned.");
            return;
        }

        rawImage = canvas.GetComponent<RawImage>();
        if (rawImage == null)
            Debug.LogError("[TextureProjector] Canvas object must have a RawImage component.");
    }

    protected override void ProcessMessage(string msg)
    {
        if (rawImage == null || string.IsNullOrEmpty(msg))
            return;
        
        string[] parts = msg.Split('\n');
        foreach (var part in parts)
        {
            if (!string.IsNullOrWhileSpace(part))
            ApplyFrame(part);
        }
    }

    private void ApplyFrame(string part)
    {
        // Strip data URI prefix if present
        int commaIndex = part.IndexOf(',');
        if (commaIndex != -1 && part.StartsWith("data:"))
            part = part.Substring(commaIndex + 1);

        byte[] imageData = System.Convert.FromBase64String(part);

        if (texture == null)
            texture = new Texture2D(2, 2);

        bool loaded = texture.LoadImage(imageData);
        if (!loaded)
        {
            Debug.LogWarning("[TextureProjector] Failed to decode image.");
            return;
        }

        rawImage.texture = texture;
    }

    private void OnDestroy()
    {
        if (texture != null)
            Destroy(texture);
    }
}