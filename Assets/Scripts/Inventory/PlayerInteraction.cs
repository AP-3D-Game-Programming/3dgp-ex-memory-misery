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
    [Header("Note System")]
    public GameObject noteUI;       // Sleep hier je 'NotePanel' in
    public TMP_Text noteTextArea;   // Sleep hier het tekstvak van je panel in
    private bool isReading = false;

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
        // 1. ALS WE AAN HET LEZEN ZIJN
        if (isReading)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
            {
                // Sluit de notitie
                noteUI.SetActive(false);
                isReading = false;
                Time.timeScale = 1f; // Tijd weer aan
            }
            return; // Stop hier, zodat we niet per ongeluk iets anders aanklikken
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        // Resetten
        if (currentItem != null) { currentItem.Highlight(false); currentItem = null; }
        bool showDefaultText = false;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            // --- A. IS HET EEN NOTITIE? ---
            NoteItem note = hit.collider.GetComponent<NoteItem>();
            if (note != null)
            {
                if (interactText != null) { interactText.text = "Press E to Read"; interactText.gameObject.SetActive(true); showDefaultText = true; }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    noteUI.SetActive(true);
                    noteTextArea.text = note.noteText;
                    isReading = true;
                    Time.timeScale = 0f; // Pauzeer het spel (veilig lezen)
                    interactText.gameObject.SetActive(false);
                }
                return;
            }

            // --- B. IS HET EEN ITEM? ---
            ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                currentItem = pickup;
                currentItem.Highlight(true);
                if (interactText != null) { interactText.text = "Press E to pick up " + pickup.item.itemName; interactText.gameObject.SetActive(true); showDefaultText = true; }
                if (Input.GetKeyDown(KeyCode.E)) { currentItem.PickUp(); interactText.gameObject.SetActive(false); }
                return;
            }

            // --- C. IS HET EEN LADE? ---
            DrawerController drawer = hit.collider.GetComponent<DrawerController>();
            if (drawer != null)
            {
                if (interactText != null) { interactText.text = "Press E to Open/Close"; interactText.gameObject.SetActive(true); showDefaultText = true; }
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)) { drawer.Interact(); }
                return;
            }

            // --- D. IS HET EEN GEWONE DEUR? (Scharnier) ---
            SimpleDoorController simpleDoor = hit.collider.GetComponent<SimpleDoorController>();
            if (simpleDoor != null)
            {
                if (interactText != null) { interactText.text = "Press E to Open/Close"; interactText.gameObject.SetActive(true); showDefaultText = true; }
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)) { simpleDoor.Interact(); }
                return;
            }
        }

        // Tekst uitzetten
        if (!showDefaultText && !isShowingMessage && interactText != null) interactText.gameObject.SetActive(false);
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