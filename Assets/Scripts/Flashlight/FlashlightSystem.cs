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

    public bool hasFlashlightItem = false;

    private bool isEquipped = false;
    private int layerIndex;

    void Start()
    {
        if (characterAnimator != null)
        {
            layerIndex = characterAnimator.GetLayerIndex(layerName);
        }

        isEquipped = false;
        UpdateFlashlightState();
    }

    void Update()
    {
        if (hasFlashlightItem && Keyboard.current.fKey.wasPressedThisFrame)
        {
            isEquipped = !isEquipped;
            UpdateFlashlightState();
        }
    }

    public void CollectFlashlight()
    {
        hasFlashlightItem = true; 
        isEquipped = true;        
        UpdateFlashlightState();
    }

    void UpdateFlashlightState()
    {
        // 1. Model aan/uit
        if (flashlightModel != null)
        {
            flashlightModel.SetActive(isEquipped);
        }

        // 2. Lichtbron aan/uit 
        if (flashlightSource != null)
        {
            flashlightSource.enabled = isEquipped;
        }

        // 3. Animatie Laag
        if (characterAnimator != null)
        {
            float targetWeight = isEquipped ? 1.0f : 0.0f;
            characterAnimator.SetLayerWeight(layerIndex, targetWeight);
        }
    }
}