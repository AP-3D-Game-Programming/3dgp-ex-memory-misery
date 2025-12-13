using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class EntranceDoor : MonoBehaviour
{
    [Header("Instellingen")]
    public string sceneToLoad = "Main2";
    public AudioSource doorSound; 

    private bool playerInRange = false;
    private bool isOpening = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    void Update()
    {
        if (playerInRange && !isOpening && Keyboard.current.eKey.wasPressedThisFrame)
        {
            EnterBuilding();
        }
    }

    void EnterBuilding()
    {
        isOpening = true;

        if (doorSound != null)
        {
            doorSound.Play();
            Invoke("LoadScene", 1f);
        }
        else
        {
            LoadScene();
        }
    }

    void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}