using UnityEngine;
using UnityEngine.Playables; // Nodig als je Unity Timeline gebruikt

public class CutsceneManager : MonoBehaviour
{
    // Door 'static' toe te voegen, blijft deze variabele bewaard 
    // zolang het spel draait, zelfs na een Scene Reload!
    public static bool hasPlayed = false;

    [Header("Referenties")]
    public GameObject cutsceneObject; // Het object met je Timeline/Animatie
    public GameObject playerUI;       // Je inventory/health bar (optioneel)

    void Start()
    {
        if (!hasPlayed)
        {
            // --- SITUATIE 1: De allereerste keer ---
            Debug.Log("Eerste keer: Start Cutscene");

            // Zet cutscene aan
            if (cutsceneObject != null) cutsceneObject.SetActive(true);

            // Markeer dat we hem gezien hebben
            hasPlayed = true;
        }
        else
        {
            // --- SITUATIE 2: Na 'Try Again' ---
            Debug.Log("Al gezien: Sla Cutscene over");

            // Zet de cutscene direct UIT
            if (cutsceneObject != null)
            {
                cutsceneObject.SetActive(false);
            }

            // Zorg dat de speler/UI direct actief is (indien nodig)
            if (playerUI != null) playerUI.SetActive(true);
        }
    }
}