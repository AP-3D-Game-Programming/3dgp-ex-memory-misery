using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText; // Sleep hier je tekst in
    public Image iconImage;          // Sleep hier je icoon image in

    public void Setup(ItemData item)
    {
        // Vul de tekst en het plaatje in
        nameText.text = "+ " + item.itemName;

        if (item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        // Vernietig dit berichtje automatisch na 3 seconden
        Destroy(gameObject, 3f);
    }
}