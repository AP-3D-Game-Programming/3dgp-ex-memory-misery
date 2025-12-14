using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class PoltergeistProp : MonoBehaviour
{
    [Header("Instellingen")]
    public float detectionRange = 5f; 
    public float throwForce = 15f;   
    public float upwardForce = 2f;    
    public float randomDelay = 0.5f;  

    [Header("Audio")]
    public AudioClip throwSound;      
    public AudioClip impactSound;     

    private Transform player;
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasThrown = false;
    private bool isWaiting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rb.isKinematic = true;
    }

    void Update()
    {
        if (hasThrown || isWaiting || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < detectionRange)
        {
            StartCoroutine(PrepareThrow());
        }
    }

    System.Collections.IEnumerator PrepareThrow()
    {
        isWaiting = true;

        yield return new WaitForSeconds(Random.Range(0f, randomDelay));

        ThrowAtPlayer();
    }

    void ThrowAtPlayer()
    {
        hasThrown = true;
        rb.isKinematic = false; 

        Vector3 direction = (player.position - transform.position).normalized;

        rb.AddForce(direction * throwForce + Vector3.up * upwardForce, ForceMode.Impulse);

        rb.AddTorque(Random.insideUnitSphere * throwForce, ForceMode.Impulse);

        if (throwSound && audioSource) audioSource.PlayOneShot(throwSound);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasThrown && collision.relativeVelocity.magnitude > 2f)
        {
            if (impactSound && audioSource)
            {
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(impactSound);
            }
            hasThrown = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}