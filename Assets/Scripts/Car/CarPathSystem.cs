using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
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
            Invoke("PlayDialogue", dialogueDelay);
        }
    }

    void PlayDialogue()
    {
        if (!parked) 
        {
            voiceOver.Play();
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
            ParkCar();
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

    void ParkCar()
    {
        parked = true;
        if (steeringWheel) steeringWheel.localRotation = Quaternion.identity;

       
        float waitTime = 2f; 

        if (voiceOver != null && voiceOver.isPlaying)
        {
            float remainingTime = voiceOver.clip.length - voiceOver.time;
            waitTime = remainingTime + 1f;
        }
        else if (voiceOver != null && !voiceOver.isPlaying && voiceOver.time == 0)
        {
            voiceOver.Play();
            waitTime = voiceOver.clip.length + 1f;
        }

        Invoke("LoadLevel", waitTime);
    }

    void LoadLevel()
    {
        SceneManager.LoadScene(nextScene);
    }
}