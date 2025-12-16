using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections; 
public class EntranceDoor : MonoBehaviour
{
    [Header("Instellingen")]
    public string sceneToLoad = "Main2";
    public AudioSource doorSound;
    public CanvasGroup blackScreenCanvasGroup; 
    public float fadeDuration = 1.0f; 

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
            StartCoroutine(TransitionToNextScene());
        }
    }

    IEnumerator TransitionToNextScene()
    {
        isOpening = true;

        if (doorSound != null)
        {
            doorSound.Play();
        }

        if (blackScreenCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                blackScreenCanvasGroup.alpha = timer / fadeDuration;
                yield return null; 
            }
            blackScreenCanvasGroup.alpha = 1f; 
        }

       
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}