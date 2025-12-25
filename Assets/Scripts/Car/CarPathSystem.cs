using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CarPathSystem : MonoBehaviour
{
    [Header("Overgang & Sfeer")]
    public Image whiteFadePanel;
    public float fadeDuration = 3f;
    public float dialogueDelay = 2f;

    [Header("Route & Sturen")]
    public Transform[] waypoints;
    public Transform steeringWheel;
    public float driveSpeed = 10f;
    public float turnSpeed = 2f;
    public float wheelSensitivity = 3f;

    [Header("Verhaal")]
    public AudioSource voiceOver;
    public string nextScene = "2_Intro_Forest";

    private int index = 0;
    private bool parked = false;
    private bool dialogueStarted = false; // Nieuwe variabele om dubbel afspelen te voorkomen

    void Start()
    {
        if (whiteFadePanel != null)
        {
            whiteFadePanel.color = Color.white;
            whiteFadePanel.canvasRenderer.SetAlpha(1.0f);
            whiteFadePanel.CrossFadeAlpha(0, fadeDuration, false);
        }

        if (voiceOver != null)
        {
            // Start de dialoog na een tijdje, maar via een naam zodat we hem kunnen annuleren
            Invoke("PlayDialogue", dialogueDelay);
        }
    }

    void PlayDialogue()
    {
        // Alleen afspelen als we nog niet geparkeerd zijn EN hij nog niet eerder gestart is
        if (!parked && !dialogueStarted)
        {
            voiceOver.Play();
            dialogueStarted = true;
        }
    }

    void Update()
    {
        if (parked) return;

        if (index < waypoints.Length)
        {
            DriveAndSteer();
        }
        else
        {
            StartCoroutine(ParkAndEndScene());
        }
    }

    void DriveAndSteer()
    {
        Transform target = waypoints[index];
        transform.position = Vector3.MoveTowards(transform.position, target.position, driveSpeed * Time.deltaTime);

        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.deltaTime);

            if (steeringWheel != null)
            {
                float targetSteerAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
                Quaternion wheelRot = Quaternion.Euler(0, 0, -targetSteerAngle * wheelSensitivity);
                steeringWheel.localRotation = Quaternion.Slerp(steeringWheel.localRotation, wheelRot, Time.deltaTime * 5f);
            }
        }

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            index++;
        }
    }

    IEnumerator ParkAndEndScene()
    {
        parked = true;

        // BELANGRIJK: Stop de timer uit Start() als die nog loopt!
        CancelInvoke("PlayDialogue");

        if (steeringWheel) steeringWheel.localRotation = Quaternion.identity;

        float waitTime = 1f;

        if (voiceOver != null)
        {
            if (voiceOver.isPlaying)
            {
                // Situatie 1: Hij is bezig -> wacht tot hij klaar is
                waitTime = voiceOver.clip.length - voiceOver.time;
            }
            else if (!dialogueStarted)
            {
                // Situatie 2: Hij heeft nog NOOIT gespeeld -> Speel hem nu af
                voiceOver.Play();
                dialogueStarted = true;
                waitTime = voiceOver.clip.length;
            }
            // Situatie 3: Hij is al klaar met spelen -> Doe niets, ga direct door naar fade out
        }

        // Wacht de berekende tijd
        yield return new WaitForSeconds(waitTime);

        // Fade Out
        if (whiteFadePanel != null)
        {
            whiteFadePanel.CrossFadeAlpha(1, 2.0f, false);
            yield return new WaitForSeconds(2.0f);
        }

        SceneManager.LoadScene(nextScene);
    }
}