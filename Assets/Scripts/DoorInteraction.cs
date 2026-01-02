using UnityEngine;
using UnityEngine.AI;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float openSpeed = 2f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("NavMesh")]
    [SerializeField] private NavMeshObstacle navMeshObstacle;

    private Camera playerCamera;
    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpen = false;
    private bool isRotating = false;
    private bool playerInRange = false;

    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }

        closedRotation = transform.rotation;
        targetRotation = closedRotation;

        if (navMeshObstacle != null)
        {
            navMeshObstacle.carving = true;
            navMeshObstacle.enabled = true;

        }
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            return;

        }
        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        playerInRange = distance <= interactionDistance;

        if (playerInRange && Input.GetKeyDown(interactionKey) && !isRotating)
        {
            Debug.Log("E button pressed");

            ToggleDoor();
        }
        if (isRotating)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
            {
                transform.rotation = targetRotation;
                isRotating = false;

                UpdateNavMeshObstacle();

                Debug.Log("Door finished moving");
            }
        }
    }

    void ToggleDoor()
    {
        if (!isOpen)
        {
            Vector3 doorToPlayer = (playerCamera.transform.position - transform.position).normalized;
            Vector3 doorForward = transform.forward;

            float dot = Vector3.Dot(doorForward, doorToPlayer);

            float angle = dot > 0 ? openAngle : -openAngle;
            targetRotation = closedRotation * Quaternion.Euler(0, angle, 0);

            isOpen = true;
        }
        else
        {
            targetRotation = closedRotation;
            isOpen = false;
        }
        isRotating = true;
    }

    void UpdateNavMeshObstacle()
    {
        if (navMeshObstacle == null)
        {
            return;
        }
        if (isOpen)
        {
            navMeshObstacle.enabled = false;
        }
        else
        {
            navMeshObstacle.enabled = true;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}
