using UnityEngine;
using System.Collections;

public class MenuFlicker : MonoBehaviour
{
    [Header("Instellingen")]
    public Light targetLight;
    public float minIntensity = 0.0f; // Helemaal uit
    public float maxIntensity = 5.0f; // Lekker fel

    [Header("Snelheid")]
    public float minWait = 0.05f; // Heel snel
    public float maxWait = 0.2f;  // Iets langzamer

    void Start()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();
        // Start de oneindige loop
        StartCoroutine(FlickerLoop());
    }

    IEnumerator FlickerLoop()
    {
        while (true)
        {
            // 1. Kies een willekeurige lichtsterkte
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);

            // 2. HORROR TRUC: Soms (1 op 5 keer) valt hij echt even uit
            if (Random.Range(0, 5) == 0)
            {
                targetLight.intensity = 0;
            }

            // 3. Wacht een willekeurige tijd voordat we weer veranderen
            // Dit zorgt voor het onregelmatige 'kapotte' effect
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
        }
    }
}
