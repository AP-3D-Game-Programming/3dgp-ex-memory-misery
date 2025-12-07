using UnityEngine;
using TMPro;

public class PlayerInteractUI : MonoBehaviour
{
    [Header("Instellingen")]
    public float rayDistance = 3f;
    public LayerMask interactLayer;
    public TextMeshProUGUI uiText;

    public FPController playerScript;

    void Update()
    {
        if (uiText == null || playerScript == null) return;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, interactLayer))
        {
            uiText.gameObject.SetActive(true);

            if (playerScript.isHidden)
            {
                uiText.text = "Press [E] to get out";
            }
            else
            {
                uiText.text = "Press [E] to Hide";
            }
        }
        else
        {
            // We kijken nergens naar
            uiText.gameObject.SetActive(false);
        }
    }
}