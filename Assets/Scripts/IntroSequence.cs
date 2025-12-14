using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.AI;
using System.Collections;
// Voeg Cinemachine toe als dat nodig is, maar we gebruiken hier vooral Transform
using Unity.Cinemachine;

public class IntroSequence : MonoBehaviour
{
    [Header("Speler Scripts (Sleep je Player hierin)")]
    public GameObject playerObject;      // Het hoofdobject (Player 3)
    public FPPlayer inputScript;         // Je Input script
    public FPController movementScript;  // Je Controller script
    public CharacterController physics;  // De CharacterController

    [Header("Camera & Targets")]
    public Transform playerCamera;       // SLEEP HIER JE 'CinemachineCamera' object in
    public Transform fallTarget;         // Het punt waar je hoofd moet eindigen

    [Header("Locaties")]
    public Transform wakeUpPoint;        // Cel positie

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
        // Probeer componenten automatisch te vinden als ze leeg zijn
        if (playerObject != null)
        {
            if (inputScript == null) inputScript = playerObject.GetComponent<FPPlayer>();
            if (movementScript == null) movementScript = playerObject.GetComponent<FPController>();
            if (physics == null) physics = playerObject.GetComponent<CharacterController>();
        }

        // Setup UI en Enemy
        if (enemyObject != null) enemyObject.SetActive(false);
        if (hudCanvas != null) hudCanvas.SetActive(false);

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.color = new Color(0, 0, 0, 0);
        }
        if (globalVolume.profile.TryGet(out dof)) dof.active = false;

        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        // 1. ZET ALLE CONTROLLERS UIT (Heel belangrijk!)
        if (inputScript != null) inputScript.enabled = false;
        if (movementScript != null) movementScript.enabled = false;
        if (physics != null) physics.enabled = false;

        // --- DEEL 1: BINNENKOMST ---
        float walkTime = 3.0f;
        float timer = 0;
        if (audioSource) audioSource.PlayOneShot(footstepsSound);

        while (timer < walkTime)
        {
            // We bewegen de speler handmatig omdat de controller uit staat
            playerObject.transform.Translate(Vector3.forward * 2f * Time.deltaTime);
            // Camera wiebelen
            if (playerCamera != null)
                playerCamera.localRotation = Quaternion.Euler(Mathf.Sin(timer * 2) * 2, Mathf.Sin(timer) * 5, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        // --- DEEL 2: DE VAL (Naar de Target) ---
        if (audioSource) audioSource.PlayOneShot(hitSound);

        float fallTime = 1.0f;
        timer = 0;

        Vector3 startBodyPos = playerObject.transform.position;
        Quaternion startCamRot = playerCamera.localRotation;

        // We vallen naar de positie en rotatie van jouw 'FallTarget'
        while (timer < fallTime)
        {
            float progress = timer / fallTime;
            // Versnelling curve (harde klap)
            float curve = Mathf.Sin(progress * Mathf.PI * 0.5f);

            if (fallTarget != null)
            {
                // A. Verplaats het lichaam naar de target positie (zakt door de grond als target laag is)
                playerObject.transform.position = Vector3.Lerp(startBodyPos, fallTarget.position, curve);

                // B. Draai de CAMERA zodat het lijkt alsof je hoofd op de grond ligt
                // We negeren de body rotatie even en focussen op de camera rotatie
                playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, fallTarget.rotation, curve);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (audioSource) audioSource.PlayOneShot(bodyFallSound);

        // --- DEEL 3: ENEMY LOOPT VOORBIJ ---
        if (enemyObject != null && enemyStartPoint != null && enemyEndPoint != null)
        {
            enemyObject.SetActive(true);
            enemyObject.transform.position = enemyStartPoint.position;
            enemyObject.transform.LookAt(enemyEndPoint);

            // Zet AI uit, we doen het handmatig
            if (enemyObject.GetComponent<EnemyAI>()) enemyObject.GetComponent<EnemyAI>().enabled = false;
            if (enemyObject.GetComponent<NavMeshAgent>()) enemyObject.GetComponent<NavMeshAgent>().enabled = false;

            float enemyPassTime = 4.0f;
            timer = 0;
            if (enemyAnimator != null) enemyAnimator.SetFloat("Speed", 2f);

            while (timer < enemyPassTime)
            {
                // Enemy loopt
                enemyObject.transform.position = Vector3.MoveTowards(enemyObject.transform.position, enemyEndPoint.position, 1.2f * Time.deltaTime);

                // Scherm wordt zwart (fade out)
                if (blackScreen != null && timer > 1.0f)
                    blackScreen.color = new Color(0, 0, 0, (timer - 1.0f) / 2.5f);

                timer += Time.deltaTime;
                yield return null;
            }
            enemyObject.SetActive(false);
        }
        else
        {
            // Fallback fade
            timer = 0; while (timer < 1f) { blackScreen.color = new Color(0, 0, 0, timer); timer += Time.deltaTime; yield return null; }
        }

        if (blackScreen != null) blackScreen.color = Color.black;
        yield return new WaitForSeconds(2.0f);

        // --- DEEL 4: WAKKER WORDEN ---
        // Verplaats speler
        playerObject.transform.position = wakeUpPoint.position;
        playerObject.transform.rotation = wakeUpPoint.rotation;

        // Reset camera (Kijk omhoog)
        playerCamera.localRotation = Quaternion.Euler(-60, 0, 0);

        // Reset Enemy (Zet AI weer aan!)
        if (enemyObject != null)
        {
            enemyObject.SetActive(true);
            if (enemyObject.GetComponent<EnemyAI>()) enemyObject.GetComponent<EnemyAI>().enabled = true;
            if (enemyObject.GetComponent<NavMeshAgent>()) enemyObject.GetComponent<NavMeshAgent>().enabled = true;
        }

        if (dof != null) { dof.active = true; dof.gaussianEnd.value = 5f; }
        if (audioSource) audioSource.PlayOneShot(wakeUpGasp);
        
        if (!wakeUpPlayed && audioSource != null && wakeUpText != null)
        {
            audioSource.PlayOneShot(wakeUpText);
            wakeUpPlayed = true;
        }

        timer = 0;
        while (timer < 10.0f)
        {
            float progress = timer / 3.0f;
            if (blackScreen != null) blackScreen.color = new Color(0, 0, 0, 1 - progress);
            if (dof != null) dof.gaussianEnd.value = Mathf.Lerp(5f, 30f, progress);

            // Hoofd komt omhoog
            playerCamera.localRotation = Quaternion.Slerp(Quaternion.Euler(-60, 0, 0), Quaternion.identity, progress);

            timer += Time.deltaTime;
            yield return null;
        }

        // --- HERSTEL ALLES ---
        if (dof != null) dof.active = false;
        if (blackScreen != null) blackScreen.color = new Color(0, 0, 0, 0);
        if (hudCanvas != null) hudCanvas.SetActive(true);

        // Zet scripts weer AAN
        if (physics != null) physics.enabled = true;
        if (movementScript != null) movementScript.enabled = true;
        if (inputScript != null) inputScript.enabled = true;

        Destroy(gameObject);
    }
}