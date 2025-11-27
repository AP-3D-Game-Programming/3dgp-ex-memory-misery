using UnityEngine;

public class FlashlightSway : MonoBehaviour
{
    [Header("Instellingen")]
    public float speed = 5f;          
    public float dragAmount = 3f;     
    public float maxSwayAmount = 10f; 

    [Header("Koppeling")]
    // Sleep hier je FPController script in, zodat we de muis-input kunnen lezen
    public FPController playerController;

    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        if (playerController == null) return;

        // 1. Haal de muisbeweging op uit je bestaande controller
        float mouseX = playerController.LookInput.x;
        float mouseY = playerController.LookInput.y;

        // 2. Bereken de nieuwe rotatie
        Quaternion xAdj = Quaternion.AngleAxis(-mouseX * dragAmount, Vector3.up);
        Quaternion yAdj = Quaternion.AngleAxis(mouseY * dragAmount, Vector3.right);

        Quaternion targetRotation = initialRotation * xAdj * yAdj;

        // 3. Smooth (Lerp) naar de nieuwe positie
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * speed);
    }
}