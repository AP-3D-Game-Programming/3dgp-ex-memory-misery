using UnityEngine;

public class DrawerController : MonoBehaviour
{
    [Header("Settings")]
    public Animator animator;
    public string openParameter = "isOpen"; // De naam van je Bool in de Animator

    [Header("Geluid (Optioneel)")]
    public AudioSource audioSource;
    public AudioClip drawerSound;

    private bool isOpen = false;

    public void Interact()
    {
        // Wissel tussen open en dicht
        isOpen = !isOpen;

        // Stuur dit naar de animator
        if (animator != null)
        {
            animator.SetBool(openParameter, isOpen);
        }

        // Speel geluid
        if (audioSource != null && drawerSound != null)
        {
            audioSource.PlayOneShot(drawerSound);
        }
    }
}