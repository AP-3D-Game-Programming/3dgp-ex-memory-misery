using UnityEngine;

public class HandheldCamera : MonoBehaviour
{
    [Header("De Camera in je hand")]
    public GameObject handheldCameraModel; // Het 3D model onder je Main Camera
    public Camera nightVisionCamera;       // De NV_Camera die filmt

    void Start()
    {
        // Alles begint uit
        if (handheldCameraModel != null) handheldCameraModel.SetActive(false);
        if (nightVisionCamera != null) nightVisionCamera.gameObject.SetActive(false);
    }

    // Deze functie roep je aan vanuit de ItemPickup
    public void EquipCamera()
    {
        if (handheldCameraModel != null) handheldCameraModel.SetActive(true);
        if (nightVisionCamera != null) nightVisionCamera.gameObject.SetActive(true);
    }
}