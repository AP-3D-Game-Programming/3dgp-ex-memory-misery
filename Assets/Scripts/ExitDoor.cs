using UnityEngine;
using TMPro;

public class ExitDoor : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text exitText; // Drag in your UI text

    private bool escaped = false;

    private void Start()
    {
        if (exitText != null)
            exitText.gameObject.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if inventory contains the key item
            bool hasKey = InventoryManager.Instance.GetInventory().Exists(i => i.itemData.isKeyItem);

            if (!escaped)
            {

                if (hasKey)
                {
                    if (exitText != null)
                    {
                        if (!escaped)
                        {
                            exitText.text = "Press E to exit";
                            exitText.gameObject.SetActive(true);
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        escaped = true;
                        Debug.Log("You escaped! Game Over.");
                        exitText.gameObject.SetActive(true);

                        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
                        enemy.SetActive(false);
                    }
                }
                else
                {
                    if (exitText != null)
                    {
                        exitText.text = "You need a key!";
                        exitText.gameObject.SetActive(true);
                    }
                }

            } else
            {
                exitText.text = "You escaped the asylum!";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && exitText != null)
            exitText.gameObject.SetActive(false);
    }
}
