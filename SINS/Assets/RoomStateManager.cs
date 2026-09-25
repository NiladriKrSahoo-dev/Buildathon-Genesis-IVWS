using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class RoomStateManager : MonoBehaviour
{
    public static RoomStateManager instance;

    [Header("Room Environments")]
    [Tooltip("Drag Normal_Room here (auto-found if empty)")]
    public GameObject normalRoom;
    [Tooltip("Drag Ending_Sequence here (auto-found if empty)")]
    public GameObject endingSequence;

    [Header("Cinematic Memory Flash (Step 2, 3, 4)")]
    [Tooltip("CanvasGroup with a pure white image. If left empty, will be auto-created at runtime.")]
    public CanvasGroup whiteFlashCanvasGroup;
    public float flashFadeInDuration = 5.0f;
    public float flashHoldDuration = 0.8f;
    public float flashFadeOutDuration = 3.5f;

    [Header("Cinematic Audio")]
    public AudioSource audioSource;
    [Tooltip("Harsh distortion sound played during the memory flash.")]
    public AudioClip distortionSound;

    [Header("Escape Sequence UI (Step 5)")]
    [Tooltip("UI screen shown when escaping through the door. Auto-created if empty.")]
    public CanvasGroup escapeCanvasGroup;
    public TextMeshProUGUI escapeTextUI;

    [Header("Ending 4: The Twisted Memory (Wife & Knife Reveal)")]
    [Tooltip("Drag the civilian_girl GameObject or leave empty to auto-find under Ending_Sequence.")]
    public GameObject wifeModel;
    [Tooltip("Drag the knife GameObject or leave empty to auto-find under Ending_Sequence.")]
    public GameObject knifeModel;
    [Tooltip("Automatically snaps knife to wife's hand (hand_right) if not already attached.")]
    public bool autoAttachKnifeToHand = true;

    [Header("State")]
    public bool isMemoryShifted = false;
    public bool isEscaped = false;
    private bool isFlashing = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        AutoFindRoomReferences();
        EnsureAudioSource();
        EnsureFlashCanvas();
        EnsureEscapeCanvas();

        // Game always starts with Normal Room active and Ending Sequence hidden
        EnsureWifeAndKnife();
        if (normalRoom != null) normalRoom.SetActive(true);
        if (endingSequence != null) endingSequence.SetActive(false);
    }

    private void AutoFindRoomReferences()
    {
        if (normalRoom == null || endingSequence == null)
        {
            GameObject[] rootObjects = gameObject.scene.GetRootGameObjects();
            foreach (GameObject go in rootObjects)
            {
                if (normalRoom == null && go.name == "Normal_Room") normalRoom = go;
                if (endingSequence == null && go.name == "Ending_Sequence") endingSequence = go;
            }
        }
    }

    private void EnsureAudioSource()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for cinematic blinder impact
    }

    private void EnsureFlashCanvas()
    {
        if (whiteFlashCanvasGroup != null) return;

        GameObject existing = GameObject.Find("MemoryFlashCanvas");
        if (existing != null)
        {
            whiteFlashCanvasGroup = existing.GetComponent<CanvasGroup>();
            if (whiteFlashCanvasGroup != null) return;
        }

        // Dynamically create full-screen pure white canvas
        GameObject flashObj = new GameObject("MemoryFlashCanvas");
        Canvas canvas = flashObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 998;
        flashObj.AddComponent<CanvasScaler>();

        whiteFlashCanvasGroup = flashObj.AddComponent<CanvasGroup>();
        whiteFlashCanvasGroup.alpha = 0f;
        whiteFlashCanvasGroup.interactable = false;
        whiteFlashCanvasGroup.blocksRaycasts = false;

        GameObject imgObj = new GameObject("WhiteImage");
        imgObj.transform.SetParent(flashObj.transform, false);
        Image img = imgObj.AddComponent<Image>();
        img.color = Color.white;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void EnsureEscapeCanvas()
    {
        if (escapeCanvasGroup != null) return;

        GameObject existing = GameObject.Find("EscapeCanvas");
        if (existing != null)
        {
            escapeCanvasGroup = existing.GetComponent<CanvasGroup>();
            if (escapeCanvasGroup != null) return;
        }

        // Dynamically create escape victory screen
        GameObject escObj = new GameObject("EscapeCanvas");
        Canvas canvas = escObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        escObj.AddComponent<CanvasScaler>();

        escapeCanvasGroup = escObj.AddComponent<CanvasGroup>();
        escapeCanvasGroup.alpha = 0f;
        escapeCanvasGroup.interactable = false;
        escapeCanvasGroup.blocksRaycasts = false;

        // Dark background
        GameObject bgObj = new GameObject("BlackBG");
        bgObj.transform.SetParent(escObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.02f, 0.02f, 0.05f, 0.95f);
        RectTransform bgRt = bgImg.rectTransform;
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Escape Victory Text
        GameObject textObj = new GameObject("EscapeText");
        textObj.transform.SetParent(escObj.transform, false);
        escapeTextUI = textObj.AddComponent<TextMeshProUGUI>();
        escapeTextUI.text = "<size=130%>YOU ESCAPED</size>\n\n<size=65%>The nightmare has ended.</size>";
        escapeTextUI.fontSize = 54;
        escapeTextUI.alignment = TextAlignmentOptions.Center;
        escapeTextUI.color = Color.white;
        RectTransform textRt = escapeTextUI.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
    }

    // Step 2, 3, 4: Triggered by TapePlayer when Tape 2 climax is reached
    public void TriggerMemoryFlash()
    {
        if (!isFlashing)
        {
            StartCoroutine(CinematicMemoryFlash());
        }
    }

    public IEnumerator CinematicMemoryFlash()
    {
        isFlashing = true;
        EnsureFlashCanvas();
        EnsureAudioSource();

        Debug.Log("RoomStateManager: Step 2 — Memory Flash initiated!");

        // Play harsh audio distortion
        PlayDistortionAudio();

        // Step 2: Pure white UI canvas gradually overtakes screen over 2.5 seconds
        float elapsed = 0f;
        if (whiteFlashCanvasGroup != null)
        {
            whiteFlashCanvasGroup.blocksRaycasts = true;
            while (elapsed < flashFadeInDuration)
            {
                elapsed += Time.deltaTime;
                whiteFlashCanvasGroup.alpha = Mathf.Clamp01(elapsed / flashFadeInDuration);
                yield return null;
            }
            whiteFlashCanvasGroup.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(flashFadeInDuration);
        }

        // Step 3: The Invisible Swap (technical trick while completely blinded)
        Debug.Log("RoomStateManager: Step 3 — Executing Invisible Swap!");
        SwitchLayer();
        isMemoryShifted = true;

        yield return new WaitForSeconds(flashHoldDuration);

        // Step 4: The Reveal (blinding white light slowly fades back to normal over 2.5s)
        Debug.Log("RoomStateManager: Step 4 — Revealing altered erased reality!");
        elapsed = 0f;
        if (whiteFlashCanvasGroup != null)
        {
            while (elapsed < flashFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                whiteFlashCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / flashFadeOutDuration));
                yield return null;
            }
            whiteFlashCanvasGroup.alpha = 0f;
            whiteFlashCanvasGroup.blocksRaycasts = false;
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.SetObjective("The room has changed... Find the key and unlock the exit door.");
        }

        isFlashing = false;
    }

    private void PlayDistortionAudio()
    {
        if (distortionSound != null)
        {
            audioSource.PlayOneShot(distortionSound);
        }
        else
        {
            // Procedural harsh glitch/distortion burst if no audio file is assigned in Inspector
            AudioClip procDistortion = CreateProceduralDistortionClip();
            if (procDistortion != null)
            {
                audioSource.PlayOneShot(procDistortion);
            }
        }
    }

    private AudioClip CreateProceduralDistortionClip()
    {
        int sampleRate = 44100;
        int sampleCount = sampleRate * 3; // 3 seconds
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float f = 60f + (t * 120f);
            float sine = Mathf.Sin(2f * Mathf.PI * f * t);
            float square = Mathf.Sign(sine) * 0.25f;
            float noise = (Random.value * 2f - 1f) * 0.2f;
            float envelope = (t < 0.25f) ? (t / 0.25f) : Mathf.Clamp01((3f - t) / 2.75f);
            samples[i] = (square + noise) * envelope * 0.35f;
        }
        AudioClip clip = AudioClip.Create("ProceduralDistortion", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void SwitchLayer()
    {
        AutoFindRoomReferences();
        if (normalRoom != null) normalRoom.SetActive(false);
        if (endingSequence != null)
        {
            endingSequence.SetActive(true);
            EnsureWifeAndKnife();
            if (wifeModel != null) wifeModel.SetActive(true);
            if (knifeModel != null) knifeModel.SetActive(true);
        }
        isMemoryShifted = true;
    }

    public void EnsureWifeAndKnife()
    {
        if (endingSequence == null) AutoFindRoomReferences();

        // 1. Auto-find Wife if not assigned
        if (wifeModel == null && endingSequence != null)
        {
            Transform[] allChildren = endingSequence.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                string n = t.name.ToLower();
                if (n.Contains("civilian") || n.Contains("wife") || n.Contains("girl"))
                {
                    wifeModel = t.gameObject;
                    break;
                }
            }
        }

        // 2. Auto-find Knife if not assigned
        if (knifeModel == null && endingSequence != null)
        {
            Transform[] allChildren = endingSequence.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                string n = t.name.ToLower();
                if (n.Contains("knife"))
                {
                    knifeModel = t.gameObject;
                    break;
                }
            }
        }

        // 3. Auto-attach knife to right hand
        if (autoAttachKnifeToHand && wifeModel != null && knifeModel != null)
        {
            Transform hand = FindRightHandBone(wifeModel.transform);
            if (hand != null && knifeModel.transform.parent != hand)
            {
                knifeModel.transform.SetParent(hand, false);
                knifeModel.transform.localPosition = new Vector3(0.04f, 0.02f, 0.06f);
                knifeModel.transform.localRotation = Quaternion.Euler(0f, 90f, -40f);
                knifeModel.transform.localScale = Vector3.one * 0.8f;
                Debug.Log($"RoomStateManager: Snapped knife into wife's hand '{hand.name}'!");
            }
        }
    }

    private Transform FindRightHandBone(Transform root)
    {
        Transform[] bones = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform b in bones)
        {
            string bn = b.name.ToLower();
            if (bn == "hand_right" || bn == "hand_r" || bn == "righthand" || bn.Contains("hand_r"))
            {
                return b;
            }
        }
        return null;
    }

    // Step 5: The Final Escape - Called by Door.cs when interacted with key
    public void TriggerFinalEscape()
    {
        if (isEscaped) return;
        isEscaped = true;
        StartCoroutine(EscapeSequence());
    }

    private IEnumerator EscapeSequence()
    {
        Debug.Log("RoomStateManager: Step 5 — Final Escape Sequence!");
        EnsureEscapeCanvas();

        if (GameManager.instance != null)
        {
            GameManager.instance.CompleteObjective();
            GameManager.instance.SetObjective("ESCAPE SUCCESSFUL");
        }

        // Fade in escape screen over 1.5 seconds
        float elapsed = 0f;
        float duration = 1.5f;
        if (escapeCanvasGroup != null)
        {
            escapeCanvasGroup.blocksRaycasts = true;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                escapeCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            escapeCanvasGroup.alpha = 1f;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Backwards compatibility with EndingTrigger.cs
    public void TriggerEndingSequence()
    {
        TriggerMemoryFlash();
    }

    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // PRESENTATION BYPASSES
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("DEMO BYPASS: Force switching room layer!");
            SwitchLayer();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("DEMO BYPASS: Force triggering Clock Spin & Memory Flash!");
            ClockController clock = GameObject.FindObjectOfType<ClockController>();
            if (clock != null) clock.TriggerTimeShift();
            TriggerMemoryFlash();
        }
#endif
    }
}