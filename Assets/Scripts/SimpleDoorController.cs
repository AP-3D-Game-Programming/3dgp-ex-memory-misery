using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SimpleDoorController : MonoBehaviour
{
    [Header("Instellingen")]
    [Tooltip("Sleep hier de deuren (scharnieren) in die moeten draaien.")]
    public List<DoorInteraction> doors = new List<DoorInteraction>();

    private void Awake()
    {
        // Als je vergeten bent de deuren erin te slepen, zoekt hij ze zelf op dit object
        if (doors.Count == 0)
        {
            doors = GetComponentsInChildren<DoorInteraction>(true).ToList();
        }
    }

    // Deze functie wordt aangeroepen door PlayerInteraction
    public void Interact()
    {
        foreach (var door in doors)
        {
            if (door != null)
            {
                door.ToggleDoor();
            }
        }
    }
}