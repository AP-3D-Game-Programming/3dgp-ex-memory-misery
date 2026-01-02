using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Instellingen")]
    public bool reverseRotation = false;
    public float openAngle = 90f;
    public float speed = 2f;

    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpen = false;
    private bool isMoving = false;
    private bool initialized = false; // Check of we al gestart zijn

    void Awake()
    {
        // We gebruiken Awake i.p.v. Start, zodat dit altijd werkt
        // ook als het script eerst uit staat.
        closedRotation = transform.localRotation;
        initialized = true;
    }

    public void ToggleDoor()
    {
        Debug.Log("Deur: ToggleDoor is aangeroepen op " + gameObject.name); // <--- DEBUG 1

        if (!initialized) closedRotation = transform.localRotation;
        if (isMoving) return;

        if (isOpen)
        {
            StartCoroutine(RotateDoor(closedRotation));
        }
        else
        {
            float angle = reverseRotation ? -openAngle : openAngle;
            Quaternion openRot = closedRotation * Quaternion.Euler(0, angle, 0);
            StartCoroutine(RotateDoor(openRot));
        }

        isOpen = !isOpen;
    }

    IEnumerator RotateDoor(Quaternion target)
    {
        Debug.Log("Deur: Start met draaien..."); // <--- DEBUG 2
        isMoving = true;
        float time = 0;
        Quaternion startRot = transform.localRotation;

        while (time < 1)
        {
            time += Time.deltaTime * speed;
            transform.localRotation = Quaternion.Slerp(startRot, target, time);
            yield return null;
        }

        isMoving = false;
        Debug.Log("Deur: Klaar met draaien."); // <--- DEBUG 3
    }
}