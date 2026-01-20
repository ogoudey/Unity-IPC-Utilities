using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Gate to control")]
    public GameObject gate;

    [Header("Activation settings")]
    public float triggerRadius = 1f; // X/Y distance
    public bool isActive = false;

    private void Update()
    {
        // Check if player is within range
        Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius);
        bool playerOnPlate = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerOnPlate = true;
                break;
            }
        }

        if (playerOnPlate && !isActive)
        {
            isActive = true;
            OpenGate();
        }
        else if (!playerOnPlate && isActive)
        {
            isActive = false;
            CloseGate();
        }
    }

    void OpenGate()
    {
        // Example: use Animator trigger if gate has Animator
        Animator anim = gate.GetComponent<Animator>();
        if (anim) anim.SetBool("Open", true);
        else gate.SetActive(false); // fallback: hide
    }

    void CloseGate()
    {
        Animator anim = gate.GetComponent<Animator>();
        if (anim) anim.SetBool("Open", false);
        else gate.SetActive(true); // fallback: show
    }
}
