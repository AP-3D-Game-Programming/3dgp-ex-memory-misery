using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance;

    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string text)
    {
        promptText.text = text;
        promptText.alpha = 1;
    }

    public void Hide()
    {
        promptText.alpha = 0;
    }
}
