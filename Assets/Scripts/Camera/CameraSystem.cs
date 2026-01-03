using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSystem : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] GameObject cameraModel;
    [SerializeField] Animator characterAnimator;

    [Header("Settings")]
    [SerializeField] string layerName = "CameraLayer";

    public bool hasCameraItem = true;

    private bool isEquipped = false;
    private int layerIndex;

    void Start()
    {
        if (characterAnimator != null)
        {
            layerIndex = characterAnimator.GetLayerIndex(layerName);
        }

        isEquipped = false;
        UpdateCameraState();
    }

    void Update()
    {
        if (hasCameraItem && Keyboard.current.cKey.wasPressedThisFrame)
        {
            isEquipped = !isEquipped;
            UpdateCameraState();
        }
    }

    public void CollectCamera()
    {
        hasCameraItem = true; 
        isEquipped = true;        
        UpdateCameraState();
    }

    void UpdateCameraState()
    {
        // 1. Model aan/uit
        if (cameraModel != null)
        {
            cameraModel.SetActive(isEquipped);
        }

        // 3. Animatie Laag
        if (characterAnimator != null)
        {
            float targetWeight = isEquipped ? 1.0f : 0.0f;
            characterAnimator.SetLayerWeight(layerIndex, targetWeight);
        }
    }
}