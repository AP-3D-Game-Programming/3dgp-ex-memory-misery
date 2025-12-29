using UnityEngine;
using UnityEngine.UI; // Nodig voor de Crosshair

public class PlayerInteract : MonoBehaviour
{
    [Header("Instellingen")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    [Header("Crosshair UI")]
    public Image crosshairImage;      // Sleep hier je Crosshair Image in (uit je Canvas)
    public Sprite defaultIcon;        // Sleep hier je stipje in
    public Sprite handIcon;           // Sleep hier je handje in

    private SecretFile currentFile;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Schiet straal
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            // --- 1. CHECK VOOR CROSSHAIR WISSEL ---
            // Als het object de tag "Interactable" heeft OF een SecretFile script heeft
            if (hit.collider.CompareTag("Interactable") || hit.collider.GetComponent<SecretFile>() != null)
            {
                ChangeCrosshair(true); // Toon handje
            }
            else
            {
                ChangeCrosshair(false); // Toon stipje
            }

            // --- 2. LOGICA VOOR SECRET FILE ---
            SecretFile file = hit.collider.GetComponent<SecretFile>();
            if (file != null)
            {
                if (currentFile != file)
                {
                    if (currentFile != null) currentFile.ToggleGlow(false);
                    currentFile = file;
                    currentFile.ToggleGlow(true);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    file.Collect();
                }
            }
            else
            {
                ResetInteraction();
            }
        }
        else
        {
            // Als we niks raken: Reset alles
            ChangeCrosshair(false);
            ResetInteraction();
        }
    }

    void ResetInteraction()
    {
        if (currentFile != null)
        {
            currentFile.ToggleGlow(false);
            currentFile = null;
        }
    }

    void ChangeCrosshair(bool showHand)
    {
        if (crosshairImage != null)
        {
            if (showHand)
            {
                crosshairImage.sprite = handIcon;
                // Optioneel: Maak handje iets groter
                crosshairImage.rectTransform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                crosshairImage.sprite = defaultIcon;
                // Reset grootte
                crosshairImage.rectTransform.localScale = Vector3.one;
            }
        }
    }
}