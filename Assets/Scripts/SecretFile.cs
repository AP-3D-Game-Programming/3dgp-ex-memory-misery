using UnityEngine;

public class SecretFile : MonoBehaviour
{
    [Header("Instellingen")]
    public Color glowColor = Color.yellow; // De kleur van de glow
    public float glowIntensity = 1.5f;     // Hoe fel?

    [Header("Melding & Geluid")]
    public GameObject messageUI;
    public AudioSource pickupSound;

    private Renderer myRenderer;
    private Material myMaterial;
    private Color originalEmissionColor;

    void Start()
    {
        // Pak het materiaal om de kleur aan te passen
        myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            myMaterial = myRenderer.material; // Maak een kopie zodat niet alle mapjes oplichten
            myMaterial.EnableKeyword("_EMISSION"); // Zorg dat emission aan staat
            originalEmissionColor = myMaterial.GetColor("_EmissionColor");
        }
    }

    // Deze functie wordt aangeroepen door de Camera
    public void ToggleGlow(bool state)
    {
        if (myMaterial == null) return;

        if (state)
        {
            // AAN: Zet de kleur + intensiteit
            myMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
        else
        {
            // UIT: Zet terug naar zwart (of origineel)
            myMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    public void Collect()
    {
        // 1. Update GameStats
        GameStats.hasCollectedFiles = true;

        // 2. Geluid
        if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound.clip, transform.position);

        // 3. Melding
        if (messageUI != null)
        {
            messageUI.SetActive(true);
            // Omdat we het object vernietigen, moeten we de UI op een slimme manier uitzetten
            // (Bijv. via een coroutine op een ander object, of simpelweg:)
            Destroy(messageUI, 3f);
        }

        // 4. Weg ermee!
        Destroy(gameObject);
    }
}