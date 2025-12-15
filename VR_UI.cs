using UnityEngine;
using UnityEngine.UI;
public class VR_UI : MonoBehaviour

{
    [SerializeField] private Transform uiRoot; // assign UI

    private Text clientConnectedText;
    private Text serverHasClientText;

    public static VR_UI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Transform connected = uiRoot.Find("Connected?");
        Transform server = uiRoot.Find("Server");
        clientConnectedText = connected.GetComponent<Text>();
        serverHasClientText = server.GetComponent<Text>();
        
    }

    public void SetClientConnected(bool connected)
    {
        clientConnectedText.text = connected ? "Connected" : "Disconnected";
        clientConnectedText.color = connected ? Color.green : Color.black;
    }

    public void ServerHasClient(string clientIP, int clientPort)
    {
        serverHasClientText.text = "\n" + clientIP + ":" + clientPort.ToString();
        serverHasClientText.color =  Color.black;
    }

    public void ServerHasClient()
    {
        serverHasClientText.text = "\nNo client connected.";
        serverHasClientText.color =  Color.red;
    }

}