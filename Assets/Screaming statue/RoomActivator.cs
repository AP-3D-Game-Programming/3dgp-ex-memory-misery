using UnityEngine;

public class RoomActivator : MonoBehaviour
{
    public WeepingAngelAI angelScript; // Sleep hier je Enemy in

    private void Start()
    {
        // Zorg dat de enemy in het begin 100% uit staat
        if (angelScript != null)
        {
            angelScript.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (angelScript != null)
            {
                angelScript.enabled = true; // Nu wordt Update() aangeroepen en begint hij
                Debug.Log("Enemy geactiveerd!");

                // Optioneel: Destroy deze trigger zodat hij niet reset
                Destroy(gameObject);
            }
        }
    }
}