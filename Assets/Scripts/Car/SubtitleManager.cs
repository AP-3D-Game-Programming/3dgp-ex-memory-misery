using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance; // Zorgt dat iedereen dit script kan vinden

    [Header("UI Koppeling")]
    public GameObject subtitlePanel;
    public TMP_Text subtitleText;

    private Coroutine currentRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Zeker weten dat hij uit staat bij start
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
    }

    // Deze functie roep je aan vanuit andere scripts
    public void ShowSubtitle(string text, float duration)
    {
        // Als er al een tekst staat, stop die eerst (zodat ze niet door elkaar lopen)
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(DisplayRoutine(text, duration));
    }

    IEnumerator DisplayRoutine(string text, float duration)
    {
        subtitlePanel.SetActive(true);
        subtitleText.text = text;

        yield return new WaitForSeconds(duration);

        subtitlePanel.SetActive(false);
        subtitleText.text = "";
    }
}