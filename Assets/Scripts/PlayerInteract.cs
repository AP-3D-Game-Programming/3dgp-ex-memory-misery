using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer; // Zet dit op 'Default' of maak een aparte layer

    private SecretFile currentFile; // Houdt bij waar we naar kijken

    void Update()
    {
        // Schiet een straal vanuit het midden van de camera
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Raak ik iets?
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            // Check of het object het 'SecretFile' script heeft
            SecretFile file = hit.collider.GetComponent<SecretFile>();

            if (file != null)
            {
                // JA! We kijken naar een file
                if (currentFile != file)
                {
                    // Als we naar een NIEUWE file kijken, zet de oude uit (voor de zekerheid)
                    if (currentFile != null) currentFile.ToggleGlow(false);

                    currentFile = file;
                    currentFile.ToggleGlow(true); // Zet glow AAN
                }

                // Check voor E toets
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
            ResetInteraction();
        }
    }

    void ResetInteraction()
    {
        // Als we nergens meer naar kijken, zet de glow uit
        if (currentFile != null)
        {
            currentFile.ToggleGlow(false);
            currentFile = null;
        }
    }
}