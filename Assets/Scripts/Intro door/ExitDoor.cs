using UnityEngine;
using TMPro;

public class ExitDoor : MonoBehaviour
{
    [Header("UI Koppelingen")]
    public GameObject winScreenUI;      // Het Win Canvas
    public TextMeshProUGUI warningText; // Tekst in je HUD: "I need the files first!"

    [Header("Speler Controle")]
    public GameObject player;           // Sleep je Player hierin (om hem te stoppen bij winst)
    public GameObject inGameHUD;        // Je crosshair etc.

    private bool isPlayerRange;

    void Start()
    {
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Als speler bij de deur staat en op E drukt
        if (isPlayerRange && Input.GetKeyDown(KeyCode.E))
        {
            CheckExitCondition();
        }
    }

    void CheckExitCondition()
    {
        if (GameStats.hasCollectedFiles == true)
        {
            // === GEWONNEN! ===
            WinGame();
        }
        else
        {
            // === NIET GEWONNEN ===
            ShowWarning();
        }
    }

    void WinGame()
    {
        // 1. UI Wisselen
        if (inGameHUD != null) inGameHUD.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(true);

        // 2. Muis losmaken
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Tijd stilzetten
        Time.timeScale = 0f;

        // 4. (Optioneel) Speler input uitzetten
        // if (player.GetComponent<FPPlayer>()) player.GetComponent<FPPlayer>().enabled = false;
    }

    void ShowWarning()
    {
        if (warningText != null)
        {
            warningText.text = "I can't leave yet... I need the evidence!";
            warningText.gameObject.SetActive(true);

            // Verberg de tekst na 3 seconden
            CancelInvoke("HideWarning"); // Reset timer als je vaak klikt
            Invoke("HideWarning", 3f);
        }
    }

    void HideWarning()
    {
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    // Trigger detectie
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerRange = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerRange = false;
    }
}