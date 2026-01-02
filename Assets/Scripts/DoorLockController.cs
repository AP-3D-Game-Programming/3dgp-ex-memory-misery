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
    public bool consumeKeyOnSuccess = true;

    [Header("Deur Settings")]
    public List<DoorInteraction> hingeDoorInteractions = new List<DoorInteraction>();
    public bool autoOpenOnSuccess = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip unlockClip;
    [Range(0f, 1f)] public float unlockVolume = 1f;

    public AudioClip noKeyClip;
    [Range(0f, 1f)] public float noKeyVolume = 1f;

    private Camera playerCamera;
    private bool playerInRange = false;

    // Public gemaakt zodat je in de inspector kan zien of hij unlocked is
    public bool unlocked = false;
    private bool qteActive = false;

    void Awake()
    {
        playerCamera = Camera.main;

        if (hingeDoorInteractions == null || hingeDoorInteractions.Count == 0)
        {
            hingeDoorInteractions = GetComponentsInChildren<DoorInteraction>(true).ToList();
        }

        DisableHingesImmediate();
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 1. VEILIGHEIDSCHECK: Als de deur al open is, stop dit script direct.
        if (unlocked)
        {
            HidePrompt();
            return;
        }

        if (playerCamera == null) return;

        float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
        playerInRange = dist <= interactDistance;

        if (playerInRange)
        {
            if (HasKey() || !requireKey)
            {
                if (qteActive)
                {
                    ShowPrompt("Unlocking...");
                    return;
                }

                ShowPrompt("Press [E] to attempt unlock");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (qteHandler != null && !qteHandler.IsRunning)
                    {
                        qteActive = true;
                        DisableHingesImmediate();

                        // Start QTE: Muisklik wordt in het andere script geregeld
                        qteHandler.StartQTE(OnQTESuccess, OnQTEFail);
                        ShowPrompt("Click [Left Mouse] when inside the zone!");
                    }
                }
            }
            else
            {
                ShowPrompt("Locked! You need a key.");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PlayNoKeySound();
                }
            }
        }
        else
        {
            HidePrompt();
        }
    }

    private void OnQTESuccess()
    {
        qteActive = false;
        unlocked = true; // Markeer als open

        ConsumeKeyIfConfigured();
        PlayUnlockSound();

        // Zet de deur scripts weer aan
        foreach (var hi in hingeDoorInteractions)
        {
            if (hi != null) hi.enabled = true;
        }

        if (autoOpenOnSuccess)
        {
            foreach (var hi in hingeDoorInteractions)
            {
                if (hi == null) continue;
                var mi = hi.GetType().GetMethod("ToggleDoor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (mi != null) mi.Invoke(hi, null);
            }
        }

        ShowPrompt("Unlocked!");

        // Verberg prompt na 1.5 seconde
        Invoke(nameof(HidePrompt), 1.5f);

        // === BUGFIX: Schakel dit script uit zodat hij NOOIT meer kan runnen ===
        // We gebruiken een kleine vertraging zodat de "Unlocked!" tekst nog net te zien is
        Invoke(nameof(DisableScriptPermanently), 1.6f);
    }

    private void DisableScriptPermanently()
    {
        // Dit zorgt ervoor dat dit script stopt met werken. 
        // De deur is nu open/van het slot, dus we hebben dit script niet meer nodig.
        this.enabled = false;
    }

    private void OnQTEFail()
    {
        qteActive = false;
        ShowPrompt("Failed! Try again.");
        Invoke(nameof(HidePrompt), 1.5f);
    }

    // --- (Overige functies zijn ongewijzigd) ---
    private void DisableHingesImmediate()
    {
        foreach (var hi in hingeDoorInteractions)
        {
            if (hi != null) hi.enabled = false;
        }
    }

    private bool HasKey()
    {
        if (!requireKey) return true;
        if (InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.GetInventory().Exists(i => i.itemData != null && i.itemData.isKeyItem);
    }

    private void ConsumeKeyIfConfigured()
    {
        if (!requireKey || !consumeKeyOnSuccess) return;
        if (InventoryManager.Instance == null) return;
        var inv = InventoryManager.Instance.GetInventory();
        var entry = inv.Find(i => i.itemData != null && i.itemData.isKeyItem);
        if (entry.itemData != null) InventoryManager.Instance.RemoveItem(entry.itemData, 1);
    }

    private void PlayUnlockSound()
    {
        if (unlockClip != null)
        {
            if (audioSource != null) audioSource.PlayOneShot(unlockClip, unlockVolume);
            else AudioSource.PlayClipAtPoint(unlockClip, transform.position, unlockVolume);
        }
    }

    private void PlayNoKeySound()
    {
        if (noKeyClip != null)
        {
            if (audioSource != null) audioSource.PlayOneShot(noKeyClip, noKeyVolume);
            else AudioSource.PlayClipAtPoint(noKeyClip, transform.position, noKeyVolume);
        }
    }

    private void ShowPrompt(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
            promptText.gameObject.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}