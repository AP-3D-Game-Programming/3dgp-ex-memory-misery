using UnityEngine;

public class GameStats : MonoBehaviour
{
    // Dit is een 'static' variabele. Dat betekent dat hij voor de hele game geldt.
    public static bool hasCollectedFiles = false;

    // Zorg dat we bij het begin van de game altijd op 'false' beginnen
    void Start()
    {
        hasCollectedFiles = false;
    }
}