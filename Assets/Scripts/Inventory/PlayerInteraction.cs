using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance; // Singleton zodat de deur erbij kan

    [Header("Settings")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    [Header("UI")]
    public TMP_Text interactText;

    private Camera cam;
    private ItemPickup currentItem;
    private DoorController currentDoor; // Nieuw: Onthoud de deur

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
        if (interactText != null) interactText.gameObject.SetActive(false);
    }

    private void Update()
    {
        CheckInteraction();
    }

    private void CheckInteraction()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        // Resetten
        if (currentItem != null)
        {
            currentItem.Highlight(false);
            currentItem = null;
        }
        currentDoor = null; // Reset deur
        bool showDefaultText = false;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            // 1. IS HET EEN ITEM?
            ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                currentItem = pickup;
                currentItem.Highlight(true);

                if (interactText != null)
                {
                    interactText.text = "Press E to pick up " + pickup.item.itemName;
                    interactText.gameObject.SetActive(true);
                    showDefaultText = true;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    currentItem.PickUp();
                    interactText.gameObject.SetActive(false);
                }
                return; // Stop hier, item heeft voorrang
            }

            // 2. IS HET EEN DEUR?
            DoorController door = hit.collider.GetComponent<DoorController>();
            if (door != null)
            {
                currentDoor = door;

                if (interactText != null)
                {
                    // Toon andere tekst afhankelijk van slot
                    string action = door.isLocked ? "Unlock" : (door.isLocked ? "Open" : "Open/Close");
                    interactText.text = "Press E to " + action;
                    interactText.gameObject.SetActive(true);
                    showDefaultText = true;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    door.Interact();
                }
            }
        }

        // Als we niks raken, zet tekst uit (maar alleen als er geen tijdelijke melding is)
        if (!showDefaultText && !isShowingMessage)
        {
            if (interactText != null) interactText.gameObject.SetActive(false);
        }
    }

    // Functie voor tijdelijke meldingen (zoals "Locked!")
    private bool isShowingMessage = false;
    public void ShowTemporaryMessage(string message)
    {
        StartCoroutine(DisplayMessageRoutine(message));
    }

    IEnumerator DisplayMessageRoutine(string message)
    {
        isShowingMessage = true;
        if (interactText != null)
        {
            interactText.text = message;
            interactText.color = Color.red; // Maak het rood voor de duidelijkheid
            interactText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (interactText != null) interactText.color = Color.white; // Reset kleur
        isShowingMessage = false;
    }
}