using UnityEngine;
using UnityEngine.SceneManagement;

public class CarPathSystem : MonoBehaviour
{
    [Header("Route & Sturen")]
    public Transform[] waypoints;       
    public Transform steeringWheel;    
    public float driveSpeed = 10f;
    public float turnSpeed = 2f;       
    public float wheelSensitivity = 3f; 

    [Header("Verhaal")]
    public AudioSource voiceOver;       
    public string nextScene = "";

    private int index = 0;
    private bool parked = false;

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
                // Bereken hoever we moeten draaien
                float targetSteerAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

                
                // Probeer hieronder 'Vector3.back' of 'Vector3.up' of 'Vector3.forward' als het raar draait.
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

        if (voiceOver != null)
        {
            voiceOver.Play();
            Invoke("LoadLevel", voiceOver.clip.length + 2f);
        }
        else
        {
            Invoke("LoadLevel", 3f);
        }
    }

    void LoadLevel()
    {
        SceneManager.LoadScene(nextScene);
    }
}