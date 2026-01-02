using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class DoorLockController : MonoBehaviour
{
    [Header("QTE Settings")]
    public KeyholeQTE qteHandler;

    [Header("Interaction UI")]
    public TMP_Text promptText;
    public float interactDistance = 3f;

    [Header("Key / Inventory")]
    public bool requireKey = true;
    public ItemData requiredKey;
    public bool consumeKeyOnSuccess = true;

    [Header("Deur Settings")]
    public List<DoorInteraction> hingeDoorInteractions = new List<DoorInteraction>();
    public bool autoOpenOnSuccess = true; // Standaard AAN gezet voor jou

    private Camera playerCamera;
    private bool playerInRange = false;
    public bool unlocked = false;
    private bool qteActive = false;

    void Awake()
    {
        playerCamera = Camera.main;

        // Auto-find deuren als lijst leeg is
        if (hingeDoorInteractions == null || hingeDoorInteractions.Count == 0)
        {
            hingeDoorInteractions = GetComponentsInChildren<DoorInteraction>(true).ToList();
        }

        Debug.Log("Slot: Gevonden deuren aantal: " + hingeDoorInteractions.Count); // <--- DEBUG A

        DisableHingesImmediate();
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (unlocked) { HidePrompt(); return; }
        if (playerCamera == null) return;

        float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
        playerInRange = dist <= interactDistance;

        if (playerInRange)
        {
            if (HasCorrectKey() || !requireKey)
            {
                if (qteActive) { ShowPrompt("Unlocking..."); return; }

                ShowPrompt("Press [E] to attempt unlock");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (qteHandler != null && !qteHandler.IsRunning)
                    {
                        qteActive = true;
                        qteHandler.StartQTE(OnQTESuccess, OnQTEFail);
                        ShowPrompt("Click [Left Mouse] when inside the zone!");
                    }
                    else
                    {
                        Debug.LogError("Slot: QTE Handler is NIET gekoppeld in Inspector!"); // <--- FOUTMELDING
                    }
                }
            }
            else
            {
                string keyName = requiredKey != null ? requiredKey.itemName : "Key";
                ShowPrompt("Locked! Need: " + keyName);
            }
        }
        else
        {
            HidePrompt();
        }
    }

    private void OnQTESuccess()
    {
        Debug.Log("Slot: QTE Geslaagd! Deur gaat nu open."); // <--- DEBUG B

        qteActive = false;
        unlocked = true;
        ConsumeKeyIfConfigured();

        // 1. Scripts aanzetten
        foreach (var hi in hingeDoorInteractions)
        {
            if (hi != null) hi.enabled = true;
        }

        // 2. Deuren openen (Directe aanroep, geen reflection meer)
        if (autoOpenOnSuccess)
        {
            if (hingeDoorInteractions.Count == 0)
            {
                Debug.LogError("Slot: ERROR! Geen deuren in de lijst 'Hinge Door Interactions'!"); // <--- FOUTMELDING
            }

            foreach (var hi in hingeDoorInteractions)
            {
                if (hi != null)
                {
                    Debug.Log("Slot: Stuurt signaal naar deur: " + hi.name); // <--- DEBUG C
                    hi.ToggleDoor();
                }
            }
        }

        ShowPrompt("Unlocked!");
        Invoke(nameof(HidePrompt), 1.5f);
        this.enabled = false;
    }

    // --- (Rest van de functies: Fail, KeyChecks, etc. blijven hetzelfde) ---
    private void OnQTEFail() { qteActive = false; ShowPrompt("Failed!"); Invoke(nameof(HidePrompt), 1.5f); }
    private void DisableHingesImmediate() { foreach (var hi in hingeDoorInteractions) { if (hi != null) hi.enabled = false; } }
    private bool HasCorrectKey()
    {
        if (!requireKey) return true;
        if (InventoryManager.Instance == null) return false;
        if (requiredKey != null) return InventoryManager.Instance.HasItem(requiredKey);
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