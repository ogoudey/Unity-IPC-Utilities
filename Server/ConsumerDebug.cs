using UnityEngine;

public class ConsumerDebug : MessageConsumerBehavior
{

    protected override void ProcessMessage(string msg)
    // Called on Update
    {
        Debug.Log($"[DebugMessageConsumer] Received message: {msg}");
    }
}
