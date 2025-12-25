using UnityEngine;

public class KeypadDoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float openSpeed = 2f;

    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpening = false;
    private bool isOpen = false;

    private void Awake()
    {
        closedRotation = transform.rotation;
        targetRotation = closedRotation;
    }

    void Update()
    {
        if (!isOpening) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * openSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
        {
            transform.rotation = targetRotation;
            isOpening = false;
            isOpen = true;
        }
    }

    public void OpenDoor()
    {
        if (isOpen) return;
        targetRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        isOpening = true;
    }
}
