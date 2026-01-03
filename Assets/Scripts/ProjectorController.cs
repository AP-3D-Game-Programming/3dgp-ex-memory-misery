using UnityEngine;
using System.Collections;

public class ProjectorController : MonoBehaviour
{
    [Header("Projector Onderdelen")]
    public Light projectorLight;       // De SpotLight die licht geeft
    public Renderer screenRenderer;    // Het witte scherm aan de muur
    public Texture[] slides;           // De enge plaatjes die hij moet tonen
    public float slideSpeed = 2f;      // Hoe snel wisselen de plaatjes

    [Header("Geluid")]
    public AudioSource audioSource;
    public AudioClip projectorStartSound;
    public AudioClip projectorLoopSound;

    private bool isOn = false;

    void Start()
    {
        // Zorg dat alles uit staat bij begin
        if (projectorLight != null) projectorLight.enabled = false;
        if (screenRenderer != null) screenRenderer.material.mainTexture = null; // Zwart scherm
    }

    // Deze functie roepen we aan als je de camera pakt
    public void TurnOnProjector()
    {
        if (isOn) return;
        isOn = true;

        StartCoroutine(PlaySlides());
    }

    IEnumerator PlaySlides()
    {
        // 1. Geluid aan
        if (audioSource != null)
        {
            audioSource.PlayOneShot(projectorStartSound);
            audioSource.clip = projectorLoopSound;
            audioSource.loop = true;
            audioSource.PlayDelayed(0.5f);
        }

        // 2. Licht aan
        if (projectorLight != null) projectorLight.enabled = true;

        // 3. Dia's afspelen (Loop)
        int index = 0;
        while (true)
        {
            if (slides.Length > 0 && screenRenderer != null)
            {
                screenRenderer.material.mainTexture = slides[index];

                // Ga naar volgende dia
                index++;
                if (index >= slides.Length) index = 0;
            }
            yield return new WaitForSeconds(slideSpeed);
        }
    }
}