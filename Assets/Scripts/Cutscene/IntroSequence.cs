using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.AI;
using System.Collections;
using Unity.Cinemachine;

public class IntroSequence : MonoBehaviour
{
    // === DE OPLOSSING VOOR HET LOOP PROBLEEM ===
    // Static zorgt dat dit onthouden wordt zolang het spel aanstaat
    public static bool hasPlayed = false;

    [Header("Speler Scripts")]
    public GameObject playerObject;
    public FPPlayer inputScript;
    public FPController movementScript;
    public CharacterController physics;

    [Header("Camera & Targets")]
    public Transform playerCamera;
    public Transform fallTarget;

    [Header("Locaties")]
    public Transform wakeUpPoint;

    [Header("Enemy Scene")]
    public GameObject enemyObject;
    public Transform enemyStartPoint;
    public Transform enemyEndPoint;
    public Animator enemyAnimator;

    [Header("UI & Effecten")]
    public GameObject hudCanvas;
    public Image blackScreen;
    public Volume globalVolume;
    private DepthOfField dof;

    [Header("Geluiden")]
    public AudioSource audioSource;
    public AudioClip footstepsSound;
    public AudioClip hitSound;
    public AudioClip bodyFallSound;
    public AudioClip wakeUpGasp;
    public AudioClip wakeUpText;

    private bool wakeUpPlayed = false;

    void Start()
    {
        // 1. Componenten zoeken
        if (playerObject != null)
        {
            if (inputScript == null) inputScript = playerObject.GetComponent<FPPlayer>();
            if (movementScript == null) movementScript = playerObject.GetComponent<FPController>();
            if (physics == null) physics = playerObject.GetComponent<CharacterController>();
        }

        if (globalVolume.profile.TryGet(out dof)) dof.active = false;

        // 2. CHECK: HEBBEN WE DIT AL GEZIEN?
        if (!hasPlayed)
        {
            // --- EERSTE KEER: START CUTSCENE ---
            hasPlayed = true; // Markeer direct als gespeeld!

            // Setup (alles verbergen)
            if (enemyObject != null) enemyObject.SetActive(false);
            if (hudCanvas != null) hudCanvas.SetActive(false);
            if (blackScreen != null)
            {
                blackScreen.gameObject.SetActive(true);
                blackScreen.color = new Color(0, 0, 0, 0);
            }

            StartCoroutine(PlayCutscene());
        }
        else
        {
            // --- TWEEDE KEER (TRY AGAIN): SLA OVER ---
            SkipCutscene();
        }
    }

    // Deze functie slaat alles over en zet de speler klaar om te spelen
    void SkipCutscene()
    {
        Debug.Log("Intro overgeslagen (al gezien).");

        // 1. Zet speler direct op de wakeUpPoint
        if (playerObject != null && wakeUpPoint != null)
        {
            // Physics even uit om teleportatie glitches te voorkomen
            if (physics != null) physics.enabled = false;

            playerObject.transform.position = wakeUpPoint.position;
            playerObject.transform.rotation = wakeUpPoint.rotation;

            // Camera rechtzetten
            if (playerCamera != null) playerCamera.localRotation = Quaternion.identity;

            if (physics != null) physics.enabled = true;
        }

        // 2. Zet controls AAN
        if (inputScript != null) inputScript.enabled = true;
        if (movementScript != null) movementScript.enabled = true;

        // 3. Zet UI AAN
        if (hudCanvas != null) hudCanvas.SetActive(true);

        // 4. Zorg dat scherm helder is
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 0);
            blackScreen.gameObject.SetActive(false);
        }

        // 5. Zorg dat de enemy er is (zoals aan het eind van de cutscene)
        if (enemyObject != null)
        {
            enemyObject.SetActive(true);
            if (enemyObject.GetComponent<EnemyAI>()) enemyObject.GetComponent<EnemyAI>().enabled = true;
            if (enemyObject.GetComponent<NavMeshAgent>()) enemyObject.GetComponent<NavMeshAgent>().enabled = true;
        }

        // 6. Weg met dit script object
        Destroy(gameObject);
    }

    IEnumerator PlayCutscene()
    {
        // 1. ZET ALLE CONTROLLERS UIT
        if (inputScript != null) inputScript.enabled = false;
        if (movementScript != null) movementScript.enabled = false;
        if (physics != null) physics.enabled = false;

        // --- DEEL 1: BINNENKOMST (3 sec) ---
        float walkTime = 3.0f;
        float timer = 0;
        if (audioSource) audioSource.PlayOneShot(footstepsSound);

        while (timer < walkTime)
        {
            playerObject.transform.Translate(Vector3.forward * 2f * Time.deltaTime);
            if (playerCamera != null)
                playerCamera.localRotation = Quaternion.Euler(Mathf.Sin(timer * 2) * 2, Mathf.Sin(timer) * 5, 0);
            timer += Time.deltaTime;
            yield return null;
        }

        // --- DEEL 2: DE VAL (1 sec) ---
        if (audioSource) audioSource.PlayOneShot(hitSound);
        float fallTime = 1.0f;
        timer = 0;
        Vector3 startBodyPos = playerObject.transform.position;

        while (timer < fallTime)
        {
            float progress = timer / fallTime;
            float curve = Mathf.Sin(progress * Mathf.PI * 0.5f);
            if (fallTarget != null)
            {
                playerObject.transform.position = Vector3.Lerp(startBodyPos, fallTarget.position, curve);
                playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, fallTarget.rotation, curve);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (audioSource) audioSource.PlayOneShot(bodyFallSound);

        // --- DEEL 3: ENEMY LOOPT VOORBIJ (4 sec) ---
        if (enemyObject != null)
        {
            enemyObject.SetActive(true);
            enemyObject.transform.position = enemyStartPoint.position;
            enemyObject.transform.LookAt(enemyEndPoint);
            // AI UIT
            if (enemyObject.GetComponent<EnemyAI>()) enemyObject.GetComponent<EnemyAI>().enabled = false;
            if (enemyObject.GetComponent<NavMeshAgent>()) enemyObject.GetComponent<NavMeshAgent>().enabled = false;

            float enemyPassTime = 4.0f;
            timer = 0;
            if (enemyAnimator != null) enemyAnimator.SetFloat("Speed", 2f);

            while (timer < enemyPassTime)
            {
                enemyObject.transform.position = Vector3.MoveTowards(enemyObject.transform.position, enemyEndPoint.position, 1.2f * Time.deltaTime);
                // Fade out
                if (blackScreen != null && timer > 1.0f)
                    blackScreen.color = new Color(0, 0, 0, (timer - 1.0f) / 2.5f);
                timer += Time.deltaTime;
                yield return null;
            }
            enemyObject.SetActive(false);
        }
        else
        {
            timer = 0; while (timer < 1f) { blackScreen.color = new Color(0, 0, 0, timer); timer += Time.deltaTime; yield return null; }
        }

        if (blackScreen != null) blackScreen.color = Color.black;
        yield return new WaitForSeconds(2.0f); // Stilte in het donker

        // --- DEEL 4: WAKKER WORDEN ---

        // Reset Posities
        playerObject.transform.position = wakeUpPoint.position;
        playerObject.transform.rotation = wakeUpPoint.rotation;

        // Reset Enemy AI
        if (enemyObject != null)
        {
            enemyObject.SetActive(true);
            if (enemyObject.GetComponent<EnemyAI>()) enemyObject.GetComponent<EnemyAI>().enabled = true;
            if (enemyObject.GetComponent<NavMeshAgent>()) enemyObject.GetComponent<NavMeshAgent>().enabled = true;
        }

        if (dof != null) { dof.active = true; dof.gaussianEnd.value = 1f; }
        if (audioSource) audioSource.PlayOneShot(wakeUpGasp);

        if (!wakeUpPlayed && audioSource != null && wakeUpText != null)
        {
            StartCoroutine(PlaySoundDelayed(wakeUpText, 1.5f));
            wakeUpPlayed = true;
        }

        // Hoofdpijn animatie
        float wakeUpDuration = 12.0f;
        timer = 0;
        Quaternion startRot = Quaternion.Euler(-60, 0, 0);

        while (timer < wakeUpDuration)
        {
            float progress = timer / wakeUpDuration;

            if (blackScreen != null)
            {
                float fadeAlpha = Mathf.Clamp01(1 - (timer / 4.0f));
                blackScreen.color = new Color(0, 0, 0, fadeAlpha);
            }

            if (dof != null)
            {
                float pulse = Mathf.Sin(timer * 3f);
                float baseClarity = Mathf.Lerp(5f, 35f, progress);
                float painIntensity = Mathf.Lerp(10f, 0f, progress);
                dof.gaussianEnd.value = baseClarity + (pulse * painIntensity);
            }

            float liftWeight = Mathf.SmoothStep(0f, 1f, progress);
            Quaternion baseLook = Quaternion.Slerp(startRot, Quaternion.identity, liftWeight);

            float swayIntensity = Mathf.Lerp(10f, 0f, progress * 1.2f);
            float noiseX = (Mathf.PerlinNoise(timer * 0.5f, 0) - 0.5f) * swayIntensity;
            float noiseY = (Mathf.PerlinNoise(0, timer * 0.5f) - 0.5f) * swayIntensity * 2;

            playerCamera.localRotation = baseLook * Quaternion.Euler(noiseX, noiseY, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        // Camera definitief rechtzetten
        float fixTimer = 0;
        Quaternion finalRot = playerCamera.localRotation;
        while (fixTimer < 1.0f)
        {
            playerCamera.localRotation = Quaternion.Slerp(finalRot, Quaternion.identity, fixTimer);
            fixTimer += Time.deltaTime;
            yield return null;
        }

        // --- HERSTEL ALLES ---
        if (dof != null) dof.active = false;
        if (blackScreen != null) blackScreen.color = new Color(0, 0, 0, 0);
        if (hudCanvas != null) hudCanvas.SetActive(true);

        if (physics != null) physics.enabled = true;
        if (movementScript != null) movementScript.enabled = true;
        if (inputScript != null) inputScript.enabled = true;

        Destroy(gameObject);
    }

    IEnumerator PlaySoundDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip);
    }
}