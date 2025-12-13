using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Instellingen")]
    public string startSceneName = "Scene_1_Car";
    public AudioSource clickSound;

    [Header("Camera & Licht Effecten")]
    public Camera mainCamera;     
    public Light sceneLight;     
    public Image whiteFadePanel;  
    public MenuFlicker flickerScript;
    public float transitionDuration = 2.5f;

    // Interne startposities
    private Vector3 camStartPos;
    private Quaternion camStartRot;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;

        if (whiteFadePanel != null)
            whiteFadePanel.color = new Color(1, 1, 1, 0);

        // Onthoud waar de camera start
        if (mainCamera != null)
        {
            camStartPos = mainCamera.transform.position;
            camStartRot = mainCamera.transform.rotation;
        }
    }

    public void StartGame()
    {
        if (clickSound) clickSound.Play();
        StartCoroutine(PlayTransition());
    }

    public void QuitGame()
    {
        if (clickSound) clickSound.Play();
        Application.Quit();
    }

    IEnumerator PlayTransition()
    {
        if (mainCamera != null)
        {
            Animator camAnim = mainCamera.GetComponent<Animator>();
            if (camAnim != null) camAnim.enabled = false;
        }

        if (flickerScript != null) flickerScript.enabled = false;

        float timer = 0f;
        float startIntensity = sceneLight != null ? sceneLight.intensity : 1f;
        float targetIntensity = 8000f; 

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;

            float progress = timer / transitionDuration;
            float curvedProgress = progress * progress * progress;

            if (sceneLight != null)
                sceneLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, curvedProgress);

            if (whiteFadePanel != null)
                whiteFadePanel.color = new Color(1, 1, 1, progress);

            if (mainCamera != null && sceneLight != null)
            {
               
                Vector3 targetPos = sceneLight.transform.position - (sceneLight.transform.forward * 0.5f);

                mainCamera.transform.position = Vector3.Lerp(camStartPos, targetPos, curvedProgress);

                Quaternion targetRot = Quaternion.LookRotation(sceneLight.transform.position - mainCamera.transform.position);
                mainCamera.transform.rotation = Quaternion.Slerp(camStartRot, targetRot, curvedProgress);
            }

            yield return null;
        }

        SceneManager.LoadScene(startSceneName);
    }
}