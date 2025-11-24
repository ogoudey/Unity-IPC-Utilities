using UnityEngine;

public class ConsumerDebug : MessageConsumerBehavior
{
    protected override void ProcessMessage(string msg)
    {
        Debug.Log($"[DebugMessageConsumer] Received message: {msg}");
    }
}
