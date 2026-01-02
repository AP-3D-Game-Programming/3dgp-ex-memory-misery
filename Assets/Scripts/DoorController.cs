using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Settings")]
    public bool isLocked = true;
    public float openAngle = 90f;     // Hoe ver gaat hij open?
    public float openSpeed = 2f;      // Hoe snel?

    [Header("Key Settings")]
    public ItemData requiredKey;      // Sleep hier je 'Key' ItemData in

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isMoving = false; // Om te voorkomen dat je spamt

    private void Start()
    {
        // Onthoud hoe de deur nu staat (dicht)
        closedRotation = transform.rotation;
        // Bereken hoe de deur staat als hij open is (huidige rotatie + 90 graden om Y-as)
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    // Deze functie wordt aangeroepen door PlayerInteraction
    public void Interact()
    {
        if (isMoving) return; // Wacht tot hij klaar is met bewegen

        if (isLocked)
        {
            // 1. Check of we de sleutel hebben
            if (requiredKey != null && InventoryManager.Instance.HasItem(requiredKey))
            {
                // JA: We hebben de sleutel!

                // Verwijder sleutel uit inventory
                InventoryManager.Instance.RemoveItem(requiredKey, 1);

                // Deur is nu voor altijd open
                isLocked = false;

                // Melding geven (Optioneel, als je NotificationManager hebt)
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowNotification(requiredKey); // Of een speciaal tekstje

                Debug.Log("Deur ontgrendeld!");
                StartCoroutine(MoveDoor(openRotation));
                isOpen = true;
            }
            else
            {
                // NEE: Geen sleutel
                Debug.Log("Deur is op slot. Je hebt de sleutel nodig.");

                // Laat melding zien op scherm
                if (PlayerInteraction.Instance != null && PlayerInteraction.Instance.interactText != null)
                {
                    // Hacky manier om even snel een melding te tonen via je bestaande UI
                    PlayerInteraction.Instance.ShowTemporaryMessage("Locked! Need: " + (requiredKey ? requiredKey.itemName : "Key"));
                }
            }
        }
        else
        {
            // Deur is niet op slot, doe gewoon open/dicht
            if (isOpen)
            {
                StartCoroutine(MoveDoor(closedRotation));
                isOpen = false;
            }
            else
            {
                StartCoroutine(MoveDoor(openRotation));
                isOpen = true;
            }
        }
    }

    IEnumerator MoveDoor(Quaternion targetRot)
    {
        isMoving = true;
        float timer = 0;
        Quaternion startRot = transform.rotation;

        while (timer < 1f)
        {
            timer += Time.deltaTime * openSpeed;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, timer);
            yield return null;
        }

        isMoving = false;
    }
}