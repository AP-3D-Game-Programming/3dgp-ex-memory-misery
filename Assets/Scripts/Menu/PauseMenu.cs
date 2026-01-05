using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    [Header("Scenes")]
    public string mainMenuSceneName = "StartScreen";

    [Header("AudioSources")]
    public AudioSource[] gameAudioSources;

    [Header("Animators")]
    public Animator[] animators;

    public FPController playerController;

    private bool isPaused = false;
    void Start()
    {

        if (pausePanel!= null)
        {
            pausePanel.SetActive(false);
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        Time.timeScale = 0f;

        foreach (AudioSource src in gameAudioSources)
        {
            if (src.isPlaying)
            {
                src.Pause();
            }
        }

        foreach (Animator anim in animators)
        {
            anim.speed = 0f;
        }

        if (playerController != null)
        {
            playerController.PauseController();
        }

        isPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void Resume()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        foreach (AudioSource src in gameAudioSources)
        {
            if (src != null)
            {
                src.UnPause();
            }
        }

        foreach (Animator anim in animators)
        {
            anim.speed = 1f;
        }

        if (playerController != null)
        {
            playerController.ResumeController();
        }

        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
