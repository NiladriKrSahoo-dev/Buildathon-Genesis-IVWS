using UnityEngine;
using TMPro;
using System.Collections;

public class TapePlayer : Interactable
{
    [Header("UI Setup")]
    public TextMeshProUGUI captionUI;
    public AudioSource audioSource;

    [Header("TV Glow Effect")]
    [Tooltip("Drag the tv_desk_open GameObject here (or leave empty to auto-find)")]
    public GameObject tvGlowObject;
    [Tooltip("Drag your Mat_TV_On material here to apply it to the TV screen")]
    public Material tvGlowMaterial;
    [Tooltip("Drag the TV screen MeshRenderer here (the renderer that should glow)")]
    public MeshRenderer tvScreenRenderer;

    [Header("Warnings")]
    [TextArea(2, 5)] 
    public string warningCaption = "YOU CAN'T DO A SINGLE THING PROPERLY AND YOU'RE SAYING I CAN HANDLE EVERYTHING?!";

    [Header("Tape 1")]
    public AudioClip tape1Audio;
    [TextArea(3, 10)] public string tape1Script = "Playing Tape 1...";
    public float tape1Time = 3.5f;

    [Header("Tape 2")]
    public AudioClip tape2Audio;
    [TextArea(3, 10)] public string tape2Script = "Playing Tape 2...";
    public float tape2Time = 2f;
    
    [Header("Ending Sequence Setup")]
    public ClockController clockController;
    public RoomStateManager roomStateManager;

    [Header("State")]
    public static TapePlayer instance;
    [System.NonSerialized] public int currentTape = 1;
    private bool isPlaying = false;
    private Coroutine warningCoroutine;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentTape = 1; // Always start on Tape 1
        EnsureAudioSource();
        EnsureUI();

        if (clockController == null)
        {
            clockController = GameObject.FindAnyObjectByType<ClockController>();
        }

        if (roomStateManager == null)
        {
            roomStateManager = GameObject.FindAnyObjectByType<RoomStateManager>();
        }

        if (tvGlowObject == null)
        {
            GameObject tvOpen = GameObject.Find("tv_desk_open");
            if (tvOpen != null) tvGlowObject = tvOpen;
        }
    }

    private void EnsureAudioSource()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f;
        audioSource.mute = false;
    }

    private void EnsureUI()
    {
        if (captionUI == null)
        {
            captionUI = GameObject.FindAnyObjectByType<TextMeshProUGUI>();
        }
    }

    private void ActivateTVGlow()
    {
        if (tvGlowObject != null) tvGlowObject.SetActive(true);

        if (tvGlowMaterial != null && tvScreenRenderer != null)
        {
            tvScreenRenderer.material = tvGlowMaterial;
        }
    }

    public override void Interact()
    {
        base.Interact();
        if (isPlaying) return;

        EnsureAudioSource();
        EnsureUI();

        if (!PlayerController.hasTape)
        {
            if (warningCoroutine != null) StopCoroutine(warningCoroutine);
            string warn = (currentTape == 1) ? "Find Tape 1 and insert it into the player." : "The deck is empty. Search the room for Tape 2.";
            warningCoroutine = StartCoroutine(ShowWarningRoutine(warn));
            return;
        }

        // Use currentTape directly — advances from 1 to 2 after Tape 1 finishes
        AudioClip clipToPlay = (currentTape == 1) ? tape1Audio : tape2Audio;
        string scriptToPlay = (currentTape == 1) ? tape1Script : tape2Script;
        float timeToPlay = (currentTape == 1) ? tape1Time : tape2Time;

        Debug.Log($"TapePlayer: Selecting Tape {currentTape} | Clip: {(clipToPlay != null ? clipToPlay.name : "MISSING")}");

        StartCoroutine(PlayTapeSequence(clipToPlay, scriptToPlay, timeToPlay, currentTape));
    }

    IEnumerator ShowWarningRoutine(string message)
    {
        if (captionUI != null)
        {
            Canvas parentCanvas = captionUI.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null && !parentCanvas.gameObject.activeSelf) parentCanvas.gameObject.SetActive(true);

            captionUI.fontSize = 19f;
            captionUI.fontStyle = FontStyles.Normal;
            captionUI.color = Color.white;
            captionUI.text = message;
            yield return new WaitForSeconds(3.5f);
            captionUI.text = "";
            captionUI.gameObject.SetActive(false);
        }
    }

    IEnumerator PlayTapeSequence(AudioClip clip, string scriptText, float displayTime, int tapeNumber)
    {
        isPlaying = true;
        Debug.Log($"TapePlayer: >>> PLAYING Tape {tapeNumber} (Clip: {(clip != null ? clip.name : "None")})");

        if (GameManager.instance != null)
        {
            GameManager.instance.HideTapeInInventory();
        }

        ActivateTVGlow();
        if (GramophoneController.instance != null) GramophoneController.instance.DuckVolume(true);
        if (EerieMusicManager.instance != null) EerieMusicManager.instance.DuckVolume(true);

        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            Debug.Log($"TapePlayer: Audio started — {clip.name} ({clip.length}s)");
        }
        else
        {
            Debug.LogWarning($"TapePlayer: NO AUDIO for Tape {tapeNumber}!");
        }

        bool clockTriggered = false;

        if (captionUI != null)
        {
            Canvas parentCanvas = captionUI.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null && !parentCanvas.gameObject.activeSelf) parentCanvas.gameObject.SetActive(true);

            captionUI.gameObject.SetActive(true);

            string textToDisplay = string.IsNullOrEmpty(scriptText) ? $"[Playing Tape {tapeNumber}]" : scriptText;
            string[] lines = textToDisplay.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                // Support per-line timing: "Dialogue text|3.5"
                string captionText = line;
                float lineTime = displayTime;

                int pipeIndex = line.LastIndexOf('|');
                if (pipeIndex >= 0 && pipeIndex < line.Length - 1)
                {
                    string timeStr = line.Substring(pipeIndex + 1).Trim();
                    if (float.TryParse(timeStr, System.Globalization.NumberStyles.Float, 
                        System.Globalization.CultureInfo.InvariantCulture, out float parsedTime))
                    {
                        captionText = line.Substring(0, pipeIndex).Trim();
                        lineTime = parsedTime;
                    }
                }

                captionUI.fontSize = 19f;
                captionUI.fontStyle = FontStyles.Normal;
                captionUI.color = Color.white;
                captionUI.text = captionText;

                // Step 1: The Catalyst - Check if line is knife climax or time is officially up
                bool isCatalystLine = (tapeNumber == 2 && 
                    (captionText.IndexOf("knife", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                     captionText.IndexOf("time is officially up", System.StringComparison.OrdinalIgnoreCase) >= 0));

                yield return new WaitForSeconds(lineTime);

                // As soon as this catalyst line finishes, spin the clock out of control!
                if (isCatalystLine && !clockTriggered)
                {
                    clockTriggered = true;
                    if (clockController != null) clockController.TriggerTimeShift();
                    if (roomStateManager != null) roomStateManager.TriggerClimaxFlicker();
                    if (EerieMusicManager.instance != null) EerieMusicManager.instance.TransitionToNightmare();
                }
            }

            captionUI.text = "";
            captionUI.gameObject.SetActive(false);
        }
        else if (clip != null)
        {
            yield return new WaitForSeconds(clip.length);
        }

        // TAPE COMPLETION & ENDING TRANSITION
        if (tapeNumber == 1)
        {
            currentTape = 2;
            PlayerController.hasTape1 = false;
            PlayerController.hasTapeStatic = false;
            Debug.Log("TapePlayer: Tape 1 done. Objective: Search the room for Tape 2.");

            if (GameManager.instance != null)
            {
                GameManager.instance.SetObjective("Search the room for Tape 2.");
            }

            if (roomStateManager != null)
            {
                roomStateManager.TriggerMidGameAtmosphere();
            }
            if (GramophoneController.instance != null)
            {
                GramophoneController.instance.DuckVolume(false);
            }
            if (EerieMusicManager.instance != null)
            {
                EerieMusicManager.instance.DuckVolume(false);
            }
        }
        else if (tapeNumber == 2)
        {
            Debug.Log("TapePlayer: Tape 2 finished! Triggering Ending Sequence.");

            // 1. The Catalyst (The Clock Spin)
            if (!clockTriggered && clockController != null)
            {
                clockController.TriggerTimeShift();
            }

            // Brief suspense beat (0.5s) while the clock hands spin violently
            yield return new WaitForSeconds(0.5f);

            // 2, 3, 4. The Memory Flash -> The Invisible Swap -> The Reveal
            if (roomStateManager == null)
            {
                roomStateManager = GameObject.FindAnyObjectByType<RoomStateManager>();
            }

            if (roomStateManager != null)
            {
                roomStateManager.TriggerMemoryFlash();
            }
            else
            {
                Debug.LogWarning("TapePlayer: RoomStateManager not found!");
            }

            if (GameManager.instance != null)
            {
                GameManager.instance.CompleteObjective();
            }
        }

        isPlaying = false;
    }
}