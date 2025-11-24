using System.Collections.Generic;
using UnityEngine;


public abstract class MessageConsumerBehavior : MonoBehaviour
{
    [SerializeField] protected MessageProducerBehavior messageProducerBehavior;
    
    protected readonly Queue<string> messageQueue;
    
    void Start()
    {
        if (messageProducerBehavior == null)
        {
            Debug.LogError("MessageEnqueuerBehavior not assigned!");
        }
    }
    
    protected virtual void Update()
    {
        if (messageProducerBehavior == null) return;
        // Process messages on Unity's main thread
        while (messageProducerBehavior.messageQueue.Count > 0)
        {
            string msg = messageProducerBehavior.messageQueue.Dequeue();
            ProcessMessage($"Dequeueing... {msg}");
        }
        
        
    }
    
    protected abstract void ProcessMessage(string msg);
}
