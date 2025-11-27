using UnityEngine;

public class BodyTilt : MonoBehaviour
{
    [Header("Instellingen")]
    public Transform spineBone; // Het bot dat moet buigen 
    public FPController playerController; // Je controller script

    [Header("Afstelling")]
    public float sensitivity = 1.0f; // Hoe ver buigt hij mee? (1 = precies gelijk)
    public float minAngle = -45f;    // Niet te ver naar beneden buigen
    public float maxAngle = 45f;     // Niet te ver naar boven buigen

    // Offset om te zorgen dat hij niet scheef start
    private Quaternion initialSpineRotation;

    void Start()
    {
        if (spineBone != null)
        {
            // Sla de beginstand van de rug op
            initialSpineRotation = spineBone.localRotation;
        }
    }

    // We gebruiken LateUpdate zodat we de Animator overschrijven
    void LateUpdate()
    {
        if (spineBone == null || playerController == null) return;

        float pitch = playerController.CurrentPitch;

     
        float bendAngle = pitch * sensitivity;

        // Beperk de hoek
        bendAngle = Mathf.Clamp(bendAngle, minAngle, maxAngle);

        Quaternion targetRotation = Quaternion.Euler(Vector3.right * bendAngle);

        spineBone.localRotation = initialSpineRotation * targetRotation;
    }
}