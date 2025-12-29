using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // Nodig voor herladen van scenes
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    // Statussen: 
    public enum State { Patrolling, Chasing, Attacking }
    public State currentState;

    [Header("Onderdelen")]
    public NavMeshAgent agent;
    public Animator enemyAnimator;
    public FPController playerScript;
    public Transform playerTarget;
    public Transform enemyEyes;
    public LayerMask obstacleMask;

    [Header("Death Screen UI")]
    public GameObject deathScreenUI; 
    public GameObject inGameUI;     

    [Header("Geluid: Achtervolging")]
    public AudioSource chaseMusicSource;
    public float musicFadeSpeed = 2f;

    [Header("Geluid: Voetstappen")]
    public AudioSource stepSource;
    public AudioClip[] steps;
    public float stepRateWalk = 0.6f;
    public float stepRateRun = 0.3f;
    private float stepTimer;

    [Header("Gedrag Instellingen")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 5f;
    public float patrolRadius = 20f;

    [Header("Zicht Instellingen")]
    public float viewDistance = 15f;
    [Range(0, 360)] public float viewAngle = 110f;

    [SerializeField] private float currentNoiseRadius;

    [Header("Gehoor")]
    public float hearRangeIdle = 3f;
    public float hearRangeWalk = 10f;
    public float hearRangeSprint = 25f;

    [Header("Zoek Timer")]
    public float searchDuration = 5f;
    private float searchTimer;

    [Header("Aanval & Jumpscare")]
    public float attackDistance = 1.2f;
    public GameObject jumpscareCam;      // De camera vlak voor zijn gezicht
    public GameObject mainPlayerCam;     // Je eigen camera
    public AudioSource attackScream;

    private Vector3 randomDestination;
    private bool isJumpscaring = false;

    void Start()
    {
        currentState = State.Patrolling;
        FindNewPatrolPoint();

        if (playerScript == null && playerTarget != null)
            playerScript = playerTarget.GetComponent<FPController>();

        if (chaseMusicSource != null)
        {
            chaseMusicSource.volume = 0;
            chaseMusicSource.Play();
        }

        if (deathScreenUI != null) deathScreenUI.SetActive(false);
    }

    void Update()
    {
        if (isJumpscaring) return; // Doe niks meer als we de speler vermoorden

        // 1. Check of speler verstopt is
        if (playerScript.isHidden)
        {
            currentState = State.Patrolling;
            // Als we bijna bij het punt zijn, zoek een nieuw punt
            if (agent.remainingDistance < 1f && !agent.pathPending)
            {
                FindNewPatrolPoint();
            }
        }

        // 2. Animatie snelheid
        if (enemyAnimator != null)
        {
            enemyAnimator.SetFloat("Speed", agent.velocity.magnitude);
        }

        HandleFootsteps();
        HandleMusic();

        // 3. Zintuigen checken
        bool seesPlayer = CanSeePlayer();
        bool hearsPlayer = CanHearPlayer();

        if (seesPlayer || hearsPlayer)
        {
            currentState = State.Chasing;
            searchTimer = searchDuration;
        }
        else if (currentState == State.Chasing)
        {
            // Speler kwijtgeraakt? Zoek nog even door
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                currentState = State.Patrolling;
                FindNewPatrolPoint();
            }
            else
            {
                // Ga naar de laatste bekende positie
                agent.SetDestination(playerTarget.position);
            }
        }

        // 4. Gedrag uitvoeren
        switch (currentState)
        {
            case State.Patrolling:
                PatrolBehavior();
                break;
            case State.Chasing:
                ChaseBehavior();
                break;
        }
    }

    void PatrolBehavior()
    {
        agent.speed = patrolSpeed;
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            FindNewPatrolPoint();
        }
    }

    void ChaseBehavior()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(playerTarget.position);

        // Check of we dichtbij genoeg zijn voor de kill
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (distanceToPlayer <= attackDistance)
        {
            StartJumpscare();
        }
    }

    void StartJumpscare()
    {
        if (isJumpscaring) return;
        isJumpscaring = true;

        // Stop de enemy
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Zet speler uit
        if (playerScript != null) playerScript.enabled = false;

        StartCoroutine(JumpscareSequence());
    }

    IEnumerator JumpscareSequence()
    {
        // 1. Camera wissel
        if (jumpscareCam != null)
        {
            jumpscareCam.SetActive(true);
            if (mainPlayerCam != null) mainPlayerCam.SetActive(false);
        }

        // 2. Geluid
        if (attackScream != null) attackScream.Play();

        // 3. Animatie triggeren 
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack");

        // 4. Wacht even (de schrik)
        yield return new WaitForSeconds(1.5f);

        // 5. Toon Death Screen
        ShowDeathScreen();
    }

    void ShowDeathScreen()
    {
        // Verberg monster cam weer 
        if (jumpscareCam != null) jumpscareCam.SetActive(false);

        // Zet HUD uit
        if (inGameUI != null) inGameUI.SetActive(false);

        // Zet Death Screen aan
        if (deathScreenUI != null) deathScreenUI.SetActive(true);

        // Muis zichtbaar maken
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Tijd stilzetten
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 0.05f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 0.05f;SceneManager.LoadScene("StartScreen");
    }

    void HandleMusic()
    {
        if (chaseMusicSource == null) return;
        float targetVolume = (currentState == State.Chasing) ? 1f : 0f;
        chaseMusicSource.volume = Mathf.Lerp(chaseMusicSource.volume, targetVolume, Time.deltaTime * musicFadeSpeed);
    }

    void HandleFootsteps()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            float interval = (currentState == State.Chasing) ? stepRateRun : stepRateWalk;
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0)
            {
                if (stepSource != null && steps.Length > 0)
                {
                    stepSource.pitch = Random.Range(0.8f, 1.1f);
                    stepSource.PlayOneShot(steps[Random.Range(0, steps.Length)]);
                }
                stepTimer = interval;
            }
        }
    }

    void FindNewPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, 1))
        {
            randomDestination = hit.position;
            agent.SetDestination(randomDestination);
        }
    }

    bool CanHearPlayer()
    {
        if (playerTarget == null || playerScript == null) return false;
        if (playerScript.isHidden) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        currentNoiseRadius = hearRangeIdle;

        if (playerScript.CurrentSpeed > 0.1f)
        {
            currentNoiseRadius = playerScript.Sprinting ? hearRangeSprint : hearRangeWalk;
        }

        return distanceToPlayer < currentNoiseRadius;
    }

    bool CanSeePlayer()
    {
        if (playerTarget == null || playerScript.isHidden) return false;

        Vector3 dirToPlayer = (playerTarget.position - enemyEyes.position).normalized;
        float distanceToPlayer = Vector3.Distance(enemyEyes.position, playerTarget.position);

        if (distanceToPlayer < viewDistance)
        {
            if (Vector3.Angle(enemyEyes.forward, dirToPlayer) < viewAngle / 2)
            {
                if (!Physics.Raycast(enemyEyes.position, dirToPlayer, distanceToPlayer, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }
}