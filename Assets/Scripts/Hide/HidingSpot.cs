using UnityEngine;
using UnityEngine.InputSystem; 

public class HidingSpot : MonoBehaviour
{
    [Header("Instellingen")]
    public Transform hidePosition; 
    public Transform exitPosition; 
    public GameObject doorModel;   
    public float hideSpeed = 5f; 

    [Header("Geluid")]
    public AudioSource soundSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    private bool isInside = false;
    private bool playerInRange = false;
    private FPController playerScript;
    private CharacterController playerController;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerScript = other.GetComponent<FPController>();
            playerController = other.GetComponent<CharacterController>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Update()
    {
       
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleHide();
        }
    }

    void ToggleHide()
    {
        isInside = !isInside;

        if (isInside)
        {
           
            playerScript.isHidden = true; 
            playerController.enabled = false; 


            playerScript.transform.position = hidePosition.position;
            playerScript.transform.rotation = hidePosition.rotation;

      
            if (doorModel != null) doorModel.transform.localRotation = Quaternion.Euler(0, 0, 0); 
            PlaySound(closeSound);
        }
        else
        {
    
            playerScript.transform.position = exitPosition.position;

            playerController.enabled = true; 
            playerScript.isHidden = false;

            if (doorModel != null) doorModel.transform.localRotation = Quaternion.Euler(0, 90, 0); 
            PlaySound(openSound);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (soundSource != null && clip != null)
        {
            soundSource.PlayOneShot(clip);
        }
    }
}