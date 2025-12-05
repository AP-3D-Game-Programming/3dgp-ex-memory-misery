using UnityEngine;
using UnityEngine.AI;
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
    public LayerMask obstacleMask; // Wat blokkeert 

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

    [Header("Zicht Instellingen (Field of View)")]
    public float viewDistance = 15f; 
    [Range(0, 360)] public float viewAngle = 110f;

    [SerializeField] private float currentNoiseRadius;

    [Header("Gehoor ")] 
    public float hearRangeIdle = 3f;   
    public float hearRangeWalk = 10f;  
    public float hearRangeSprint = 25f;

    [Header("Zoek Timer")] 
    public float searchDuration = 5f; 
    private float searchTimer;        

    [Header("Aanval Instellingen")]
    public float attackDistance = 1.2f; 
    public GameObject jumpscareCam;     // De camera voor zijn gezicht
    public GameObject mainPlayerCam;    // Je eigen camera
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
    }

    void Update()
    {
        if (isJumpscaring) return;
        if (playerScript.isHidden)
        {
            currentState = State.Patrolling;
            if (agent.remainingDistance < 1f)
            {
                FindNewPatrolPoint();
            }
            return; 
        }
        if (enemyAnimator != null)
        {
            enemyAnimator.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (enemyAnimator != null) enemyAnimator.SetFloat("Speed", agent.velocity.magnitude);
        HandleFootsteps();
        HandleMusic();

        bool seesPlayer = CanSeePlayer();
        bool hearsPlayer = CanHearPlayer();

        if (seesPlayer || hearsPlayer)
        {
            if (hearsPlayer && !seesPlayer)
            {
                Debug.Log("Ik hoor je!");
            }

            currentState = State.Chasing;
            searchTimer = searchDuration;
        }
        else if (currentState == State.Chasing)
        {  
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                currentState = State.Patrolling;
                FindNewPatrolPoint();
            }
            else
            {
                ChaseBehavior();
            }
        }

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
    void HandleMusic()
    {
        if (chaseMusicSource == null) return;

        float targetVolume = 0f;

        if (currentState == State.Chasing)
        {
            targetVolume = 1f;
        }

        chaseMusicSource.volume = Mathf.Lerp(chaseMusicSource.volume, targetVolume, Time.deltaTime * musicFadeSpeed);
    }

    void HandleFootsteps()
    {
        // Speel alleen geluid als hij beweegt
        if (agent.velocity.magnitude > 0.1f)
        {
            // Bepaal snelheid van stappen
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
    void PatrolBehavior()
    {
        agent.speed = patrolSpeed;
        agent.SetDestination(playerTarget.position);
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            FindNewPatrolPoint();
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
            if (playerScript.Sprinting)
            {
                currentNoiseRadius = hearRangeSprint; 
            }
            else
            {
                currentNoiseRadius = hearRangeWalk; 
            }
        }
        if (distanceToPlayer < currentNoiseRadius)
        {
            return true;
        }

        return false;
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
    // Kiest een willekeurig punt op de NavMesh om heen te lopen
    void FindNewPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += transform.position;
        NavMeshHit hit;
        // Check of het punt op de loopbare vloer ligt
        if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, 1))
        {
            randomDestination = hit.position;
            agent.SetDestination(randomDestination);
        }
    }
    bool CanSeePlayer()
    {
        if (playerTarget == null) return false;
        if (playerScript.isHidden) return false;
        Vector3 dirToPlayer = (playerTarget.position - enemyEyes.position).normalized;
        float distanceToPlayer = Vector3.Distance(enemyEyes.position, playerTarget.position);

        // Afstand Check:
        if (distanceToPlayer < viewDistance)
        {
            // Hoek Check
            if (Vector3.Angle(enemyEyes.forward, dirToPlayer) < viewAngle / 2)
            {
                // Muren Check (Raycast)
                // We schieten een straal van zijn oog naar het midden van de speler
                if (!Physics.Linecast(enemyEyes.position, playerTarget.position, obstacleMask))
                {
                    return true; // IK ZIE JE!
                }
            }
        }
        return false;
    }
    void StartJumpscare()
    {
        isJumpscaring = true;
        currentState = State.Attacking;
        agent.isStopped = true; // Stop met rennen

        if (enemyAnimator != null)
        {
            enemyAnimator.SetFloat("Speed", 0); 
            enemyAnimator.SetTrigger("Attack"); 
        }
        playerScript.enabled = false;
        // Zet de besturing uit
        
        // Wissel van camera
        if (mainPlayerCam != null) mainPlayerCam.SetActive(false);
        if (jumpscareCam != null) jumpscareCam.SetActive(true);
        //Speel geluid
        if (attackScream != null) attackScream.Play();

        // Zorg dat de enemy je recht aankijkt voor de death cam
        // verplaatsen recht voor de originele speler positie
        Vector3 killPos = playerTarget.position + (playerTarget.forward * 0.8f);
        killPos.y = transform.position.y; // Hou dezelfde hoogte
        transform.position = killPos;
        transform.LookAt(playerTarget);

        Debug.Log("GAME OVER");
    }

    //hulp viir visueel 
    void OnDrawGizmos()
    {
        // Teken Zicht
        if (enemyEyes != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(enemyEyes.position, viewDistance);
        }

        // Teken Gehoor (Let op: dit tekenen we rondom de SPELER, niet de enemy)
        if (playerTarget != null && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTarget.position, currentNoiseRadius);
        }
    }
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += enemyEyes.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
