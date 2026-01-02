using UnityEngine;
using UnityEngine.AI;

public class WeepingAngel : MonoBehaviour
{
    [Header("Instellingen")]
    public float moveSpeed = 10f;
    public float catchDistance = 1.5f;

    [Header("Referenties")]
    public Transform player;         // Sleep je Player hierin
    public Camera playerCamera;      // Sleep je Main Camera hierin
    public Renderer enemyRenderer;   // Het plaatje/model van de enemy

    [Header("Geluid")]
    public AudioSource moveSound;    // Geluid van schuivend steen (Looping)

    private NavMeshAgent agent;
    private bool isFrozen = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        // Als referenties leeg zijn, zoek ze zelf
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        if (playerCamera == null) playerCamera = Camera.main;
        if (enemyRenderer == null) enemyRenderer = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        if (player == null) return;

        // 1. Check of we gezien worden
        // Zoek alle lampen in de buurt (simpele versie)
        SpookyLight lamp = FindObjectOfType<SpookyLight>();

        // Als er een lamp is, EN hij staat uit (Blackout) -> Dan is de Angel NIET zichtbaar
        bool isDark = (lamp != null && !lamp.GetComponent<Light>().enabled);

        // De Angel is alleen gezien als de camera kijkt EN het licht aan is
        bool isSeen = CheckIfSeen() && !isDark;

        if (isSeen)
        {
            Freeze();
        }
        else
        {
            Move();
        }

        // 2. Check of we de speler hebben gevangen
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < catchDistance && !isSeen)
        {
            JumpscareKill();
        }
    }

    void Freeze()
    {
        if (isFrozen) return; // Doe niks als we al stilstaan

        isFrozen = true;
        agent.isStopped = true; // Stop beweging
        agent.velocity = Vector3.zero;

        // Stop geluid
        if (moveSound != null) moveSound.Stop();
    }

    void Move()
    {
        if (!isFrozen)
        {
            // Update bestemming continu (zodat hij niet naar je oude plek loopt)
            agent.SetDestination(player.position);
            return;
        }

        isFrozen = false;
        agent.isStopped = false;

        // Start geluid
        if (moveSound != null && !moveSound.isPlaying) moveSound.Play();
    }

    bool CheckIfSeen()
    {
        // A. Is het object überhaupt in beeld van de camera? (Frustum Culling)
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        if (!GeometryUtility.TestPlanesAABB(planes, enemyRenderer.bounds))
        {
            return false; // Niet op het scherm
        }

        // B. Is er een muur tussen ons? (Raycast)
        // We trekken een lijn van de camera naar het hoofd van de enemy
        RaycastHit hit;
        Vector3 direction = (enemyRenderer.bounds.center - playerCamera.transform.position).normalized;

        if (Physics.Raycast(playerCamera.transform.position, direction, out hit, 100f))
        {
            // Als we DEZE enemy raken, of een deel ervan, dan zien we hem echt
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return true;
            }
        }

        return false; // Wel op scherm, maar achter een muur
    }

    void JumpscareKill()
    {
        // Hier roep je jouw bestaande Death Screen logica aan!
        Debug.Log("JE BENT DOOD - ANGEL HEEFT JE!");

        // Voorbeeld: Hergebruik je LobbyTrap logica of laad de scene opnieuw
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Of activeer je DeathScreen UI script:
        // FindObjectOfType<LobbyTrap>().ShowDeathScreen(); 

        this.enabled = false; // Stop dit script
    }
}