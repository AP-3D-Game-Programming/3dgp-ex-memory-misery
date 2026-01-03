using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class DoorLockController : MonoBehaviour
{
    [Header("Status")]
    [Tooltip("Vink AAN als de deur op slot zit. Vink UIT voor een gewone deur.")]
    public bool isLocked = true;

    [Header("QTE Settings (Alleen als Locked)")]
    public KeyholeQTE qteHandler;

    [Header("Interaction UI")]
    public TMP_Text promptText;
    public float interactDistance = 3f;

    [Header("Key / Inventory (Alleen als Locked)")]
    public bool requireKey = true;
    public ItemData requiredKey;
    public bool consumeKeyOnSuccess = true;

    [Header("Deur Koppeling")]
    public List<DoorInteraction> hingeDoorInteractions = new List<DoorInteraction>();

    private Camera playerCamera;
    private bool playerInRange = false;
    private bool qteActive = false;

    void Awake()
    {
        playerCamera = Camera.main;

        // Auto-find deuren als lijst leeg is
        if (hingeDoorInteractions == null || hingeDoorInteractions.Count == 0)
        {
            hingeDoorInteractions = GetComponentsInChildren<DoorInteraction>(true).ToList();
        }

        // Zorg dat deuren die op slot zitten niet per ongeluk bewegen
        if (isLocked) DisableHingesImmediate();

        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Check afstand
        float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
        playerInRange = dist <= interactDistance;

        if (playerInRange)
        {
            if (isLocked)
            {
                // === SCENARIO A: DEUR ZIT OP SLOT ===
                HandleLockedDoor();
            }
            else
            {
                // === SCENARIO B: DEUR IS OPEN / NORMAAL ===
                HandleNormalDoor();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    // --- FUNCTIE VOOR GEWONE DEUREN ---
    void HandleNormalDoor()
    {
        ShowPrompt("Press [E] to Open/Close");

        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoors();
        }
    }

    // --- FUNCTIE VOOR DEUREN OP SLOT ---
    void HandleLockedDoor()
    {
        // 1. Check of we de sleutel hebben
        if (HasCorrectKey() || !requireKey)
        {
            // We hebben de sleutel (of hebben er geen nodig)
            if (qteActive)
            {
                ShowPrompt("Press [E] when inside zone!"); // AANGEPASTE TEKST
                return;
            }

            ShowPrompt("Press [E] to Unlock");

            if (Input.GetKeyDown(KeyCode.E))
            {
                // Start de minigame
                if (qteHandler != null && !qteHandler.IsRunning)
                {
                    if (requiredKey != null)
                    {
                        qteHandler.SetKeyImage(requiredKey.icon);
                    }
                    // ==============================================

                    qteActive = true;
                    qteHandler.StartQTE(OnQTESuccess, OnQTEFail);
                }
                else if (qteHandler == null)
                {
                    Debug.LogError("FOUT: Je bent de QTE Handler vergeten te koppelen!");
                }
            }
        }
        else
        {
            // Geen sleutel
            string keyName = requiredKey != null ? requiredKey.itemName : "Key";
            ShowPrompt("Locked! Need: " + keyName);
        }
    }

    private void OnQTESuccess()
    {
        qteActive = false;

        // DEUR IS NU VAN HET SLOT AF!
        isLocked = false;

        ConsumeKeyIfConfigured();

        // Activeer de deuren weer en doe ze open
        foreach (var hi in hingeDoorInteractions) { if (hi != null) hi.enabled = true; }
        ToggleDoors();

        ShowPrompt("Unlocked!");
        Invoke(nameof(HidePrompt), 1.5f);
    }

    private void OnQTEFail()
    {
        qteActive = false;
        ShowPrompt("Failed! Try again.");
        Invoke(nameof(HidePrompt), 1.5f);
    }

    private void ToggleDoors()
    {
        foreach (var hi in hingeDoorInteractions)
        {
            if (hi != null) hi.ToggleDoor();
        }
    }

    // --- HULP FUNCTIES ---
    private void DisableHingesImmediate() { foreach (var hi in hingeDoorInteractions) { if (hi != null) hi.enabled = false; } }

    private bool HasCorrectKey()
    {
        if (!requireKey) return true;
        if (InventoryManager.Instance == null) return false;
        if (requiredKey != null) return InventoryManager.Instance.HasItem(requiredKey);
        // Fallback: kijk of we 'een' key hebben
        return InventoryManager.Instance.GetInventory().Exists(i => i.itemData != null && i.itemData.isKeyItem);
    }

    private void ConsumeKeyIfConfigured()
    {
        if (!requireKey || !consumeKeyOnSuccess) return;
        if (requiredKey != null) InventoryManager.Instance.RemoveItem(requiredKey, 1);
    }

    private void ShowPrompt(string text) { if (promptText != null) { promptText.text = text; promptText.gameObject.SetActive(true); } }
    private void HidePrompt() { if (promptText != null) promptText.gameObject.SetActive(false); }
}