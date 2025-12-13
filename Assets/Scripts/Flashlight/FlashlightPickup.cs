using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; 
public class FlashlightPickup : MonoBehaviour
{
    [Header("Sleep hier je Player in")]
    public FlashlightSystem playerScript; 

    [Header("Opties")]
    public TextMeshProUGUI tipText;
    public AudioSource pickupSound;

    private bool playerInRange = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Speler staat bij de zaklamp!"); 
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    void Update()
    {
        if(playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PakOp();
        }
    }

    void PakOp()
    {
        Debug.Log("E gedrukt - Oppakken gestart!"); // Debug check
        if (playerScript != null)
        {
            playerScript.CollectFlashlight();
        }

        if (pickupSound) pickupSound.Play();

        if (tipText != null)
        {
            tipText.text = "Press [F] to toggle Flashlight";
            tipText.gameObject.SetActive(true);
            Destroy(tipText.gameObject, 4f);
        }

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 1f);
    }
}