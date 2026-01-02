using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Settings")]
    public GameObject notificationPrefab; // Sleep je prefab hierin
    public Transform notificationArea;    // Het gebied in je Canvas (Vertical Layout Group)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowNotification(ItemData item)
    {
        // Maak een nieuw berichtje aan
        GameObject newNotification = Instantiate(notificationPrefab, notificationArea);

        // Vul de data in
        NotificationUI uiScript = newNotification.GetComponent<NotificationUI>();
        if (uiScript != null)
        {
            uiScript.Setup(item);
        }
    }
}