using UnityEngine;
using System.Collections;

public class FlyingJumpscare : MonoBehaviour
{
   
    public GameObject monsterObject;   
    public Transform playerTarget;     
    public AudioSource scareSound;    

    [Header("Instellingen")]
    public float flySpeed = 15f;       // Hoe snel
    public float stopDistance = 0.5f;  // Hoe dichtbij 

    private bool isFlying = false;

    void OnTriggerEnter(Collider other)
    {
        // Als de speler de doos inloopt EN we zijn nog niet aan het vliegen
        if (other.CompareTag("Player") && !isFlying)
        {
            StartCoroutine(StartTheScare());
        }
    }

    IEnumerator StartTheScare()
    {
        isFlying = true;

        // Speel geluid
        if (scareSound != null) scareSound.Play();

        // Maak het monster zichtbaar
        if (monsterObject != null)
        {
            monsterObject.SetActive(true);

            // Zorg dat hij meteen naar de speler kijkt bij het spawnen
            monsterObject.transform.LookAt(playerTarget);
        }

        // Wacht even zodat de speler het ziet (optioneel, mini seconde)
        yield return new WaitForEndOfFrame();

    }

    void Update()
    {
        // alleen als de scare actief is
        if (isFlying && monsterObject != null && playerTarget != null)
        {
            // Laat het monster naar de speler kijken
            monsterObject.transform.LookAt(playerTarget);

            // Beweeg het monster richting het doel
            // Vector3.MoveTowards berekent de nieuwe positie voor dit frame
            monsterObject.transform.position = Vector3.MoveTowards(
                monsterObject.transform.position, // Huidige positie
                playerTarget.position,            // Doel positie
                flySpeed * Time.deltaTime         // Snelheid
            );

            //  Check de afstand
            float distance = Vector3.Distance(monsterObject.transform.position, playerTarget.position);

            if (distance <= stopDistance)
            {
                EndScare();
            }
        }
    }

    void EndScare()
    {
        isFlying = false;

        // Monster weer onzichtbaa
        if (monsterObject != null) monsterObject.SetActive(false);

        // Trigger uitzetten 
        GetComponent<BoxCollider>().enabled = false;
    }
}