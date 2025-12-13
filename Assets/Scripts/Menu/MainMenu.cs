using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Instellingen")]
    public string startSceneName = "1_Intro_Car"; 
    public AudioSource clickSound; 

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        if (clickSound) clickSound.Play();
        SceneManager.LoadScene(startSceneName);
    }

    public void QuitGame()
    {
        if (clickSound) clickSound.Play();
        Debug.Log("Quit Game!"); 
        Application.Quit();
    }
}