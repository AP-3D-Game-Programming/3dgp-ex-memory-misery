using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance;

    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        Instance = this;

        if (promptText == null)
        {
            Debug.LogWarning("InteractionPromptUI: promptText is not assigned in the inspector. Calls to Show/Hide will be ignored.");
            return;
        }

        Hide();
    }

    public void Show(string text)
    {
        if (promptText == null) return;
        promptText.text = text;
        promptText.alpha = 1;
    }

    public void Hide()
    {
        if (promptText == null) return;
        promptText.alpha = 0;
    }
}
