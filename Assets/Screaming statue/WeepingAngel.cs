using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WeepingAngelAI : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 8.0f;
    public float catchDistance = 1.2f;
    public Transform player;

    [Header("Jumpscare & UI")]
    public GameObject jumpscareFace;    // Het enge plaatje (Eerst)
    public GameObject deathScreenPanel; // Je bestaande Death Screen (Daarna)

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip screamSound;

    [Header("Vision")]
    public LayerMask obstacleLayer;

    private NavMeshAgent agent;
    private Camera cam;
    private Renderer enemyRenderer;
    private bool isAttacking = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponentInChildren<Renderer>();
        cam = Camera.main;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // Zorg dat bij start alles netjes uit staat
        if (jumpscareFace != null) jumpscareFace.SetActive(false);
        if (deathScreenPanel != null) deathScreenPanel.SetActive(false);

        agent.speed = 0;
        agent.enabled = false;
    }

    private void Update()
    {
        if (isAttacking) return;

        if (IsVisibleToPlayer())
        {
            StopMoving();
        }
        else
        {
            MoveToPlayer();
        }
    }

    bool IsVisibleToPlayer()
    {
        if (enemyRenderer == null) return false;

        Vector3 screenPoint = cam.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (!onScreen) return false;

        RaycastHit hit;
        Vector3 direction = transform.position - cam.transform.position;

        if (Physics.Raycast(cam.transform.position, direction, out hit, Mathf.Infinity, obstacleLayer))
        {
            if (hit.transform != transform && !hit.transform.IsChildOf(transform) && !hit.transform.CompareTag("Player"))
            {
                return false;
            }
        }
        return true;
    }

    void MoveToPlayer()
    {
        if (!agent.enabled) agent.enabled = true;
        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) < catchDistance)
        {
            StartCoroutine(TriggerJumpscare());
        }
    }

    void StopMoving()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.speed = 0;
            agent.velocity = Vector3.zero;
        }
    }

    IEnumerator TriggerJumpscare()
    {
        isAttacking = true;

        // 1. Stop de enemy en player controls
        StopMoving();
        agent.enabled = false;
        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in scripts) { script.enabled = false; }

        // 2. Toon Jumpscare (Enge gezicht)
        if (jumpscareFace != null) jumpscareFace.SetActive(true);

        // 3. Speel geluid
        if (audioSource != null && screamSound != null) audioSource.PlayOneShot(screamSound);

        // 4. Wacht 2 seconden (terwijl het spel nog loopt)
        yield return new WaitForSeconds(2.0f);

        // 5. Verberg jumpscare gezicht (optioneel, anders staat het door je tekst heen)
        if (jumpscareFace != null) jumpscareFace.SetActive(false);

        // 6. === TOON JE BESTAANDE DEATH SCREEN ===
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
        }

        // 7. Pauzeer de game (zodat enemy niet meer beweegt)
        Time.timeScale = 0f;

        // 8. Maak muis los
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}