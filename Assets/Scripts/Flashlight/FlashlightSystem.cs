using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightSystem : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] GameObject flashlightModel; 
    [SerializeField] Light flashlightSource;    
    [SerializeField] Animator characterAnimator; 

    [Header("Settings")]
    [SerializeField] string layerName = "FlashlightLayer";

    private bool isEquipped = false;
    private int layerIndex;

    void Start()
    {
        if (characterAnimator != null)
        {
            layerIndex = characterAnimator.GetLayerIndex(layerName);
        }
        UpdateFlashlightState();
    }

    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            isEquipped = !isEquipped;
            UpdateFlashlightState();
        }
    }

    void UpdateFlashlightState()
    {
        // 1. Model aan/uit (Het ding in je hand)
        if (flashlightModel != null)
        {
            flashlightModel.SetActive(isEquipped);
        }

        // 2. Lichtbron aan/uit (De straal) 
        if (flashlightSource != null)
        {
            flashlightSource.enabled = isEquipped;
        }

        // 3. Animatie Laag (Arm omhoog/omlaag)
        if (characterAnimator != null)
        {
            float targetWeight = isEquipped ? 1.0f : 0.0f;

            characterAnimator.SetLayerWeight(layerIndex, targetWeight);
        }
    }
}