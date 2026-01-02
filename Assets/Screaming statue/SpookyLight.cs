using UnityEngine;

public class SpookyLight : MonoBehaviour
{
    [Header("Instellingen")]
    public float minFlickerSpeed = 0.05f; // Hoe snel flikkert hij?
    public float maxFlickerSpeed = 0.2f;

    [Header("Blackout (Duisternis)")]
    public bool useBlackouts = true;      // Mag het licht uitvallen?
    public float minLightOnTime = 2f;     // Hoe lang blijft hij minstens aan?
    public float maxLightOnTime = 10f;    // Hoe lang blijft hij maximaal aan?
    public float blackoutDuration = 3f;   // Hoe lang is het donker?

    [Header("Geluid")]
    public AudioSource humSound;          // Het zoemende geluid (Loop)
    public AudioClip sparkSound;          // Het "Kzzzt" geluid bij knipperen

    private Light myLight;
    private float timer;
    private bool isBlackout = false;

    void Start()
    {
        myLight = GetComponent<Light>();
        timer = Random.Range(minLightOnTime, maxLightOnTime);

        // Start met flikkeren
        StartCoroutine(FlickerRoutine());
    }

    void Update()
    {
        if (!useBlackouts) return;

        // Timer aftellen voor de volgende blackout
        timer -= Time.deltaTime;

        if (timer <= 0 && !isBlackout)
        {
            StartCoroutine(DoBlackout());
        }
    }

    System.Collections.IEnumerator DoBlackout()
    {
        isBlackout = true;
        myLight.enabled = false; // LICHT UIT
        if (humSound) humSound.Stop(); // Zoem geluid stopt (eng!)

        // Wacht in het donker... (Hier beweegt de Angel!)
        yield return new WaitForSeconds(blackoutDuration);

        // Licht weer AAN
        myLight.enabled = true;
        isBlackout = false;

        if (humSound) humSound.Play();
        if (sparkSound) AudioSource.PlayClipAtPoint(sparkSound, transform.position);
        // Reset timer voor de volgende keer
        timer = Random.Range(minLightOnTime, maxLightOnTime);

        // Herstart het geflikker
        StartCoroutine(FlickerRoutine());
    }

    System.Collections.IEnumerator FlickerRoutine()
    {
        while (!isBlackout)
        {
            // Even uit...
            myLight.enabled = !myLight.enabled;
            yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));

            // ...en weer aan (of andersom)
            myLight.enabled = !myLight.enabled;

            // Wacht even voordat we weer flikkeren (random pauze)
            yield return new WaitForSeconds(Random.Range(0.1f, 1.5f));
        }
    }
}