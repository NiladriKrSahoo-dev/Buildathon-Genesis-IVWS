using UnityEngine;
using System.Collections;
using TMPro;

public class Door : Interactable
{
    [Header("Manager Reference")]
    public RoomStateManager roomManager;

    [Header("On-Screen Warning UI")]
    [Tooltip("Text element for popup warning. Auto-found if empty.")]
    public TextMeshProUGUI onScreenWarningUI;
    [TextArea(2, 4)]
    public string lockedWarningText = "The exit door is locked! I need to find the key.";

    [Header("Door Open Animation")]
    public float openAngle = -90f;
    public float openDuration = 1.2f;

    [Header("Audio")]
    public AudioSource doorAudio;
    public AudioClip doorOpenSound;
    public AudioClip doorLockedSound;

    private bool isOpening = false;
    private bool hasOpened = false;
    private Coroutine warningCoroutine;

    void Start()
    {
        if (roomManager == null)
        {
            roomManager = GameObject.FindObjectOfType<RoomStateManager>();
        }

        if (doorAudio == null)
        {
            doorAudio = GetComponent<AudioSource>();
        }

        EnsureWarningUI();
    }

    private void EnsureWarningUI()
    {
        if (onScreenWarningUI == null)
        {
            // Auto-find subtitle or caption UI
            TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var t in allTexts)
            {
                string n = t.gameObject.name.ToLower();
                if (n.Contains("caption") || n.Contains("subtitle") || n.Contains("warning"))
                {
                    onScreenWarningUI = t;
                    break;
                }
            }

            if (onScreenWarningUI == null)
            {
                TapePlayer tp = FindObjectOfType<TapePlayer>();
                if (tp != null && tp.captionUI != null) onScreenWarningUI = tp.captionUI;
            }
        }
    }

    public override void Interact()
    {
        base.Interact();
        if (hasOpened || isOpening) return;

        // Step 5: The Final Escape (Validate key)
        if (PlayerController.hasKey)
        {
            hasOpened = true;
            Debug.Log("Door: Key validated! Opening door and completing escape loop.");
            StartCoroutine(OpenDoorAndEscape());
        }
        else
        {
            Debug.Log("Door: Locked! Player needs key to escape.");
            if (doorAudio != null && doorLockedSound != null)
            {
                doorAudio.PlayOneShot(doorLockedSound);
            }

            // Show on-screen popup warning
            EnsureWarningUI();
            if (onScreenWarningUI != null)
            {
                if (warningCoroutine != null) StopCoroutine(warningCoroutine);
                warningCoroutine = StartCoroutine(ShowWarningRoutine(lockedWarningText));
            }

            // Update objective
            if (GameManager.instance != null)
            {
                GameManager.instance.SetObjective("The exit door is locked! Find the key to escape.");
            }
        }
    }

    private IEnumerator ShowWarningRoutine(string message)
    {
        Canvas parentCanvas = onScreenWarningUI.GetComponentInParent<Canvas>(true);
        if (parentCanvas != null && !parentCanvas.gameObject.activeSelf) parentCanvas.gameObject.SetActive(true);

        onScreenWarningUI.gameObject.SetActive(true);
        onScreenWarningUI.text = message;
        yield return new WaitForSeconds(3.0f);
        onScreenWarningUI.text = "";
        onScreenWarningUI.gameObject.SetActive(false);
    }

    private IEnumerator OpenDoorAndEscape()
    {
        isOpening = true;

        if (doorAudio != null && doorOpenSound != null)
        {
            doorAudio.PlayOneShot(doorOpenSound);
        }

        // Smoothly swing the door open
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, openAngle, 0f);
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / openDuration);
            yield return null;
        }

        transform.localRotation = targetRotation;

        // Trigger Step 5: Final Escape sequence in RoomStateManager
        if (roomManager != null)
        {
            roomManager.TriggerFinalEscape();
        }
        else
        {
            RoomStateManager rm = GameObject.FindObjectOfType<RoomStateManager>();
            if (rm != null) rm.TriggerFinalEscape();
        }

        isOpening = false;
    }
}