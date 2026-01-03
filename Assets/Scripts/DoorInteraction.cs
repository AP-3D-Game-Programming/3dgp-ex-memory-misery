using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Instellingen")]
    public bool reverseRotation = false;
    public float openAngle = 90f;
    public float speed = 2f;

    [Header("Geluid")]
    public AudioSource audioSource; // De speaker
    public AudioClip doorSound;     // Het krakende geluidje

    private Quaternion closedRotation;
    private bool isOpen = false;
    private bool isMoving = false;
    private bool initialized = false;

    void Awake()
    {
        closedRotation = transform.localRotation;
        initialized = true;

        // Als je vergeten bent de AudioSource te koppelen, zoekt hij hem zelf
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void ToggleDoor()
    {
        if (!initialized) closedRotation = transform.localRotation;
        if (isMoving) return;

        // Speel geluid af (als het er is)
        PlaySound();

        if (isOpen)
        {
            // Dicht draaien
            StartCoroutine(RotateDoor(closedRotation));
        }
        else
        {
            // Open draaien
            float angle = reverseRotation ? -openAngle : openAngle;
            Quaternion openRot = closedRotation * Quaternion.Euler(0, angle, 0);
            StartCoroutine(RotateDoor(openRot));
        }

        isOpen = !isOpen;
    }

    void PlaySound()
    {
        if (audioSource != null && doorSound != null)
        {
            // Verander de toonhoogte een klein beetje voor variatie (extra eng)
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.PlayOneShot(doorSound);
        }
    }

    IEnumerator RotateDoor(Quaternion target)
    {
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
    }
}