using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;

    [Header("Settings")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    [Header("UI")]
    public TMP_Text interactText;

    private Camera cam;
    private ItemPickup currentItem;

    // We hebben 'DoorController' weggehaald, want 'DoorLockController' regelt zichzelf nu!

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
        
        bool showDefaultText = false;

        // Schiet een straal
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            // --- 1. IS HET EEN ITEM? ---
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
                return; 
            }

            // --- 2. IS HET EEN LADE? ---
            DrawerController drawer = hit.collider.GetComponent<DrawerController>();
            if (drawer != null)
            {
                if (interactText != null)
                {
                    interactText.text = "Press E to Open/Close";
                    interactText.gameObject.SetActive(true);
                    showDefaultText = true;
                }
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
                {
                    drawer.Interact();
                }
                return; 
            }

            // --- 3. IS HET EEN SLOT (QTE DEUR)? ---
            // Dit script regelt zijn eigen input/tekst al, dus we doen hier niks.
            // (Het script op de deur zelf checkt distance en input).

            // --- 4. IS HET EEN SIMPELE DEUR? (NIEUW!) ---
            SimpleDoorController simpleDoor = hit.collider.GetComponent<SimpleDoorController>();
            if (simpleDoor != null)
            {
                if (interactText != null)
                {
                    interactText.text = "Press E to Open/Close";
                    interactText.gameObject.SetActive(true);
                    showDefaultText = true;
                }
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
                {
                    simpleDoor.Interact();
                }
                return;
            }
        }

        // Tekst uitzetten als we niks raken
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
            interactText.color = Color.red;
            interactText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (interactText != null) interactText.color = Color.white;
        isShowingMessage = false;
    }
}