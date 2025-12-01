using UnityEngine;
using System.Collections;

public class SimpleJumpscare : MonoBehaviour
{
    [Header("Wat moet er gebeuren?")]
    public GameObject scareImage; // image
    public AudioSource scareSound; //geluid

    [Header("Instellingen")]
    public float imageDuration = 0.5f; // Hoe lang blijft het plaatje staan?

    //start als in trigger 
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(DoScare());
        }
    }

    IEnumerator DoScare()
    {
        // Zet image aan 
        if (scareImage != null) scareImage.SetActive(true);
        if (scareSound != null) scareSound.Play();

        yield return new WaitForSeconds(imageDuration);

        // Zet image uit
        if (scareImage != null) scareImage.SetActive(false);

        // verwijder trigger 
        GetComponent<BoxCollider>().enabled = false;
    }
}