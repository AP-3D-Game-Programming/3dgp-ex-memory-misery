using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleRestart : MonoBehaviour
{
    // --- NIEUW STUKJE ---
    void Update()
    {
        // Als je op R drukt, moet hij herladen
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Toets R ingedrukt: Herladen!");
            ReloadScene();
        }
    }
    // --------------------

    public void ReloadScene()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearInventory();
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScreen");
    }
}