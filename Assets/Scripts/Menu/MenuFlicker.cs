using UnityEngine;
using System.Collections;

public class MenuFlicker : MonoBehaviour
{
    [Header("Instellingen")]
    public Light targetLight;
    public float minIntensity = 0.0f; 
    public float maxIntensity = 5.0f; 

    [Header("Snelheid")]
    public float minWait = 0.05f;
    public float maxWait = 0.2f; 

    void Start()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();
        StartCoroutine(FlickerLoop());
    }

    IEnumerator FlickerLoop()
    {
        while (true)
        {
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);

            if (Random.Range(0, 5) == 0)
            {
                targetLight.intensity = 0;
            }


            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
        }
    }
}
