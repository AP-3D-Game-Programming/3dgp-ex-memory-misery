using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorLockController : MonoBehaviour
{
    [Header("QTE")]
    [Tooltip("Reference to the KeyholeQTE component (assign in inspector).")]
    public KeyholeQTE qteHandler;

    [Header("Interaction")]
    [Tooltip("TextMeshPro UI text (optional). If null, InteractionPromptUI will be used).")]
    public TMPro.TMP_Text promptText;

    [Tooltip("Distance from player camera to start interaction")]
    public float interactDistance = 3f;

    [Header("Key / Inventory")]
    [Tooltip("Require a key (ItemData.isKeyItem == true) in inventory before starting QTE.")]
    public bool requireKey = true;
    [Tooltip("Consume one key on success")]
    public bool consumeKeyOnSuccess = true;

    [Header("Door Hinge Targets")]
    [Tooltip("If empty, DoorInteraction components in children will be auto-found.")]
    public List<DoorInteraction> hingeDoorInteractions = new List<DoorInteraction>();

    [Header("Behavior")]
    [Tooltip("If true, opening will be automatic (call ToggleDoor on each hinge) after unlock. If false, hinges are simply enabled and player must use them.")]
    public bool autoOpenOnSuccess = false;

    [Header("Audio")]
    public AudioSource audioSource;

    public AudioClip unlockClip;
    [Range(0f, 1f)]
    public float unlockVolume = 1f;

    public AudioClip noKeyClip;
    [Range(0f, 1f)]
    public float noKeyVolume = 1f;

    private Camera playerCamera;
    private bool playerInRange = false;
    private bool unlocked = false;

    // New: track if a QTE is currently active so we can block input/avoid races
    private bool qteActive = false;

    void Awake()
    {
        // Ensure camera is available as early as possible
        playerCamera = Camera.main;

        // auto-find DoorInteraction components if none assigned
        if (hingeDoorInteractions == null || hingeDoorInteractions.Count == 0)
        {
            hingeDoorInteractions = GetComponentsInChildren<DoorInteraction>(true).ToList();
        }

        // disable hinge scripts so door cannot be opened before unlock
        foreach (var hi in hingeDoorInteractions)
        {
            if (hi != null)
            {
                hi.enabled = false;
                Debug.Log($"DoorLockController: Disabled hinge {hi.gameObject.name} at Awake()");
            }
        }

        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    void Start()
    {
        // nothing critical here - Awake handled early disabling
    }

    void Update()
    {
        // If already unlocked, no further input for unlocking
        if (unlocked) return;

        if (playerCamera == null) return;

        float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
        playerInRange = dist <= interactDistance;

        if (playerInRange)
        {
            if (HasKey() || !requireKey)
            {
                // If a QTE is already active, show QTE prompt and ignore new E presses
                if (qteActive)
                {
                    ShowPrompt("QTE in progress...");
                    return;
                }

                ShowPrompt("Press [E] to attempt unlock");
                if (Input.GetKeyDown(KeyCode.E) && (qteHandler != null) && !qteHandler.IsRunning)
                {
                    // ensure hinges are disabled immediately to prevent any race
                    DisableHingesImmediate();

                    // mark QTE active and start it
                    qteActive = true;
                    Debug.Log("DoorLockController: Starting QTE");
                    qteHandler.StartQTE(OnQTESuccess, OnQTEFail);
                    ShowPrompt("QTE started: Press [Space] when icon is in the keyhole");
                }
            }
            else
            {
                ShowPrompt("You need a key!");

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

    private void DisableHingesImmediate()
    {
        foreach (var hi in hingeDoorInteractions)
        {
            if (hi != null && hi.enabled)
            {
                hi.enabled = false;
                Debug.Log($"DoorLockController: Force-disabled hinge {hi.gameObject.name} before QTE");
            }
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
        if (entry.itemData != null)
            InventoryManager.Instance.RemoveItem(entry.itemData, 1);
    }

    private void PlayUnlockSound()
    {
        if (unlockClip == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(unlockClip, unlockVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(unlockClip, transform.position, unlockVolume);
        }

    }

    private void PlayNoKeySound()
    {
        if (noKeyClip == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(noKeyClip, noKeyVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(noKeyClip, transform.position, noKeyVolume);
        }
    }

    private void OnQTESuccess()
    {
        qteActive = false;
        unlocked = true;
        ConsumeKeyIfConfigured();

        PlayUnlockSound();

        // enable hinge interactions
        foreach (var hi in hingeDoorInteractions)
        {
            if (hi != null)
            {
                hi.enabled = true;
                Debug.Log($"DoorLockController: Enabled hinge {hi.gameObject.name} after unlock");
            }
        }

        if (autoOpenOnSuccess)
        {
            // attempt to call ToggleDoor on each hinge via reflection (works with existing private method)
            foreach (var hi in hingeDoorInteractions)
            {
                if (hi == null) continue;
                var mi = hi.GetType().GetMethod("ToggleDoor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (mi != null) mi.Invoke(hi, null);
            }
        }

        ShowPrompt("Unlocked!");
        // hide prompt after a moment
        Invoke(nameof(HidePrompt), 2f);
    }

    private void OnQTEFail()
    {
        qteActive = false;
        ShowPrompt("Failed to unlock!");
        Invoke(nameof(HidePrompt), 1.5f);
        Debug.Log("DoorLockController: QTE failed - hinges remain disabled");
        // optional: you can add enemy alert or penalty here
    }

    private void ShowPrompt(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
            promptText.gameObject.SetActive(true);
        }
        else if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.Show(text);
        }
    }

    private void HidePrompt()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
        else if (InteractionPromptUI.Instance != null) InteractionPromptUI.Instance.Hide();
    }
}