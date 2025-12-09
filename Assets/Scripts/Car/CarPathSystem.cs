using UnityEngine;
using UnityEngine.SceneManagement;

public class CarPathSystem : MonoBehaviour
{
    [Header("De Route")]
    public Transform[] waypoints; 
    public float driveSpeed = 8f;
    public float turnSpeed = 2f;  
    public float stopDistance = 0.5f; 

    [Header("Verhaal")]
    public AudioSource voiceOver; 
    public AudioSource carEngineSound; 
    public string nextSceneName = "2_intro_Exterior";

    private int currentPointIndex = 0;
    private bool isParked = false;

    void Start()
    {
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            transform.LookAt(waypoints[1].position); 
        }
    }

    void Update()
    {
        if (isParked) return;

        if (currentPointIndex < waypoints.Length)
        {
            DriveToPoint();
        }
        else
        {
            ParkTheCar();
        }
    }

    void DriveToPoint()
    {
        Transform targetPoint = waypoints[currentPointIndex];

        Vector3 direction = targetPoint.position - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        }

        transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < stopDistance)
        {
            currentPointIndex++; 
        }
    }

    void ParkTheCar()
    {
        if (isParked) return; 
        isParked = true;

        Debug.Log("Geparkeerd!");

        if (carEngineSound != null) carEngineSound.Stop();

        if (voiceOver != null)
        {
            voiceOver.Play();
            Invoke("LoadNextLevel", voiceOver.clip.length + 2f);
        }
        else
        {
            Invoke("LoadNextLevel", 3f);
        }
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}