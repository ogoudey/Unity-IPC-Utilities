using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Gate to control")]
    public GameObject gate;

    [Header("Activation settings")]
    public float interactRadius = 2f;

    private bool isPulled = false;

    void Update()
    {
        // Check for player proximity
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius);
        bool playerNearby = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerNearby = true;
                break;
            }
        }

        // If player is nearby and presses "E", pull lever
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            isPulled = true;
            OpenGate();
            // Optional: animate lever
            Animator anim = GetComponent<Animator>();
            if (anim) anim.SetTrigger("Pull");
        }

        // Gate stays open if lever is pulled
        if (isPulled)
        {
            OpenGate();
        }
    }

    void OpenGate()
    {
        Animator anim = gate.GetComponent<Animator>();
        if (anim) anim.SetBool("Open", true);
        else gate.SetActive(false);
    }

    public void CloseGate()
    {
        if (!isPulled) // Only close if lever not pulled
        {
            Animator anim = gate.GetComponent<Animator>();
            if (anim) anim.SetBool("Open", false);
            else gate.SetActive(true);
        }
    }
}
