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
    public float flashFadeInDuration = 1.2f;
    public float flashHoldDuration = 0.8f;
    public float flashFadeOutDuration = 2.5f;

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
    [Tooltip("Leave FALSE so your manual knife placement on the wife model is preserved!")]
    public bool autoAttachKnife = false;
    public Vector3 knifeScale = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Thanos Snap Dissolve Effect (Paced for presentation)")]
    [Tooltip("How long each piece of furniture takes to dissolve.")]
    public float furnitureDissolveDuration = 2.0f;
    [Tooltip("How long each small prop/book takes to dissolve.")]
    public float propDissolveDuration = 1.4f;
    [Tooltip("How long wall/architecture pieces take to dissolve.")]
    public float architectureDissolveDuration = 2.0f;
    [Tooltip("How long the final spinning clock takes to dissolve.")]
    public float clockDissolveDuration = 3.2f;
    [Tooltip("Pause between waves in seconds.")]
    public float waveDelay = 0.8f;

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
        // Clean up any old blinder cube if one was created
        GameObject oldBlinder = GameObject.Find("DoorwayWhiteBlinder");
        if (oldBlinder != null) Destroy(oldBlinder);

        AutoFindRoomReferences();
        if (GetComponent<CeilingLampManager>() == null)
        {
            gameObject.AddComponent<CeilingLampManager>();
        }
        EnsureAudioSource();
        EnsureFlashCanvas();
        EnsureEscapeCanvas();

        // Game starts with Normal Room active and Ending Sequence hidden
        EnsureWifeAndKnife();
        if (normalRoom != null) normalRoom.SetActive(true);
        if (endingSequence != null) endingSequence.SetActive(false);

        // Hide keys located inside Normal_Room so the player cannot pick them up prematurely
        KeyPickup[] existingKeys = Resources.FindObjectsOfTypeAll<KeyPickup>();
        foreach (var k in existingKeys)
        {
            if (k.gameObject.scene.isLoaded && normalRoom != null && k.transform.IsChildOf(normalRoom.transform))
            {
                k.gameObject.SetActive(false);
            }
        }

        SetupDeskInvestigation();
    }

    private void SetupDeskInvestigation()
    {
        GameObject[] rootObjects = gameObject.scene.GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
            {
                string n = t.name.ToLower();
                if ((n.Contains("photo_frame_desk") || n == "desk") && !n.Contains("drawer"))
                {
                    if (t.GetComponent<Collider>() == null)
                    {
                        t.gameObject.AddComponent<BoxCollider>();
                    }
                    if (t.GetComponent<DeskInvestigation>() == null && t.GetComponent<Drawer>() == null)
                    {
                        t.gameObject.AddComponent<DeskInvestigation>();
                    }
                }
            }
        }
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
        audioSource.spatialBlend = 0f; // 2D sound for cinematic impact
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
        bgImg.color = new Color(0.01f, 0.01f, 0.02f, 1f);
        RectTransform bgRt = bgImg.rectTransform;
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Escape Text
        GameObject textObj = new GameObject("EscapeText");
        textObj.transform.SetParent(escObj.transform, false);
        escapeTextUI = textObj.AddComponent<TextMeshProUGUI>();
        escapeTextUI.text = "<size=125%><b>You remember now.</b></size>\n\n<size=65%>You were never trapped here by someone else.\n\nYou locked the door from the inside.</size>";
        escapeTextUI.fontSize = 46;
        escapeTextUI.alignment = TextAlignmentOptions.Center;
        escapeTextUI.color = Color.white;
        RectTransform textRt = escapeTextUI.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(40f, 40f);
        textRt.offsetMax = new Vector2(-40f, -40f);
    }

    // Trigger mid-game atmosphere after Tape 1
    public void TriggerMidGameAtmosphere()
    {
        StartCoroutine(FlickerRoomLights(3, 0.08f));
    }

    // Trigger climax strobing right before Thanos dissolve
    public void TriggerClimaxFlicker()
    {
        StartCoroutine(FlickerRoomLights(10, 0.04f));
    }

    private IEnumerator FlickerRoomLights(int count, float speed)
    {
        Light[] allLights = FindObjectsByType<Light>();
        if (allLights == null || allLights.Length == 0) yield break;

        float[] originalIntensities = new float[allLights.Length];
        for (int j = 0; j < allLights.Length; j++)
        {
            originalIntensities[j] = allLights[j] != null ? allLights[j].intensity : 1f;
        }

        for (int i = 0; i < count; i++)
        {
            float mult = Random.Range(0.02f, 0.25f);
            for (int j = 0; j < allLights.Length; j++)
            {
                if (allLights[j] != null) allLights[j].intensity = originalIntensities[j] * mult;
            }
            yield return new WaitForSeconds(speed);

            for (int j = 0; j < allLights.Length; j++)
            {
                if (allLights[j] != null) allLights[j].intensity = originalIntensities[j];
            }
            yield return new WaitForSeconds(speed * Random.Range(1f, 2f));
        }

        for (int j = 0; j < allLights.Length; j++)
        {
            if (allLights[j] != null) allLights[j].intensity = originalIntensities[j];
        }
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

        if (GramophoneController.instance != null)
        {
            GramophoneController.instance.StopGramophone();
        }

        // Play harsh audio distortion
        PlayDistortionAudio();

        // Step 2a: THANOS SNAP — slower, paced wave dissolve
        Debug.Log("RoomStateManager: Step 2a — Paced Thanos Snap dissolve!");
        if (normalRoom != null)
        {
            yield return StartCoroutine(DissolveRoomFurniture());
        }

        // Step 2b: White flash smoothly fills screen AFTER all furniture is gone
        float elapsed = 0f;
        float postDissolveFlashIn = 1.3f;
        if (whiteFlashCanvasGroup != null)
        {
            whiteFlashCanvasGroup.blocksRaycasts = true;
            while (elapsed < postDissolveFlashIn)
            {
                elapsed += Time.deltaTime;
                whiteFlashCanvasGroup.alpha = Mathf.Clamp01(elapsed / postDissolveFlashIn);
                yield return null;
            }
            whiteFlashCanvasGroup.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(postDissolveFlashIn);
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

    // ==================== PACED THANOS SNAP DISSOLVE (~7 Seconds Total) ====================

    private IEnumerator DissolveRoomFurniture()
    {
        if (normalRoom == null) yield break;

        // Group objects into tiers so surface items vanish BEFORE the surfaces supporting them!
        System.Collections.Generic.List<Transform> propsTier = new System.Collections.Generic.List<Transform>();
        System.Collections.Generic.List<Transform> furnitureTier = new System.Collections.Generic.List<Transform>();
        System.Collections.Generic.List<Transform> architectureTier = new System.Collections.Generic.List<Transform>();
        Transform clockObj = null;

        // Also check if cassette player exists outside normalRoom
        TapePlayer tp = FindAnyObjectByType<TapePlayer>();
        if (tp != null && (normalRoom == null || !tp.transform.IsChildOf(normalRoom.transform)))
        {
            propsTier.Add(tp.transform);
        }

        foreach (Transform child in normalRoom.transform)
        {
            if (!child.gameObject.activeSelf) continue;

            string n = child.name.ToLower();

            // Skip player, camera, floor (player shouldn't fall), lights, managers
            if (n.Contains("eventsystem") || n.Contains("manager") ||
                n.Contains("directional") || n.Contains("player") ||
                n.Contains("camera") || n.Contains("floor") ||
                n.Contains("light"))
                continue;

            // Clock is the absolute finale
            if (n.Contains("clock"))
            {
                clockObj = child;
                continue;
            }

            // TIER 1: Props & Clutter (Books, cassette player, tapes, dishes, lamps, papers, etc.)
            // MUST dissolve first so they never hover when shelves/tables vanish
            if (n.Contains("book") || n.Contains("paper") || n.Contains("notebook") ||
                n.Contains("cassete") || n.Contains("player") || n.Contains("tape") ||
                n.Contains("cup") || n.Contains("mug") || n.Contains("bottle") ||
                n.Contains("dish") || n.Contains("plate") || n.Contains("lamp") ||
                n.Contains("knife") || n.Contains("candle") || n.Contains("pen") ||
                n.Contains("decor") || n.Contains("flower") || n.Contains("vase") ||
                n.Contains("item") || n.Contains("glass") || n.Contains("photo"))
            {
                propsTier.Add(child);
            }
            // TIER 2: Furniture supporting props (Shelves, Desks, Tables, Chairs, Bed, Cabinets, Drawers)
            else if (n.Contains("shelf") || n.Contains("bookcase") || n.Contains("desk") ||
                     n.Contains("table") || n.Contains("chair") || n.Contains("bed") ||
                     n.Contains("drawer") || n.Contains("cabinet") || n.Contains("dresser") ||
                     n.Contains("stand") || n.Contains("nightstand") || n.Contains("couch") ||
                     n.Contains("sofa") || n.Contains("wardrobe"))
            {
                furnitureTier.Add(child);
            }
            // TIER 3: Room Structure (Walls, doors, frames, pillars, ceilings)
            else
            {
                architectureTier.Add(child);
            }
        }

        Debug.Log($"RoomStateManager: Paced Thanos Dissolve — Props: {propsTier.Count}, Furniture: {furnitureTier.Count}, Architecture: {architectureTier.Count}");

        // WAVE 1: Disintegrate all surface props (Books & Cassette Player disappear FIRST)
        float propAnimDuration = propDissolveDuration;
        int propBatch = Mathf.Max(1, propsTier.Count / 22);
        for (int i = 0; i < propsTier.Count; i++)
        {
            StartCoroutine(DissolveObject(propsTier[i], propAnimDuration));
            if ((i + 1) % propBatch == 0) yield return new WaitForSeconds(0.06f);
        }

        // Delay so props are clearly disintegrating before furniture starts moving
        yield return new WaitForSeconds(waveDelay);

        // WAVE 2: Disintegrate Furniture (Shelves, Desks, Tables, Chairs) — Slower, cinematic dissolve
        float furnAnimDuration = furnitureDissolveDuration;
        int furnBatch = Mathf.Max(1, furnitureTier.Count / 16);
        for (int i = 0; i < furnitureTier.Count; i++)
        {
            StartCoroutine(DissolveObject(furnitureTier[i], furnAnimDuration));
            if ((i + 1) % furnBatch == 0) yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(waveDelay);

        // WAVE 3: Disintegrate Architecture (Walls, frames, paintings)
        float archAnimDuration = architectureDissolveDuration;
        int archBatch = Mathf.Max(1, architectureTier.Count / 14);
        for (int i = 0; i < architectureTier.Count; i++)
        {
            StartCoroutine(DissolveObject(architectureTier[i], archAnimDuration));
            if ((i + 1) % archBatch == 0) yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(0.6f);

        // WAVE 4: Dramatic Clock Finale (Clock is spinning wildly, then dissolves into dust)
        if (clockObj != null)
        {
            Debug.Log("RoomStateManager: Centerpiece Clock dissolving LAST...");
            yield return StartCoroutine(DissolveObject(clockObj, clockDissolveDuration));
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator DissolveObject(Transform obj, float duration)
    {
        Vector3 startScale = obj.localScale;
        Vector3 startPos = obj.localPosition;
        Quaternion startRot = obj.localRotation;
        float randomTilt = Random.Range(-15f, 15f);
        float floatHeight = Random.Range(1.0f, 2.2f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t * (3f - 2f * t);

            obj.localScale = Vector3.Lerp(startScale, Vector3.zero, eased);
            obj.localPosition = startPos + Vector3.up * (eased * floatHeight);
            obj.localRotation = startRot * Quaternion.Euler(0f, eased * randomTilt * 3f, eased * randomTilt);

            yield return null;
        }

        obj.gameObject.SetActive(false);
        obj.localScale = startScale;
        obj.localPosition = startPos;
        obj.localRotation = startRot;
    }

    // ==================== ROOM SWAP ====================

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

        // Hide any world tapes so they don't float in Ending Sequence
        TapePickup[] tapes = Resources.FindObjectsOfTypeAll<TapePickup>();
        foreach (var t in tapes)
        {
            if (t.gameObject.scene.isLoaded) t.gameObject.SetActive(false);
        }

        // Hide Cassette Player if it was outside normalRoom
        TapePlayer tp = FindAnyObjectByType<TapePlayer>();
        if (tp != null && (normalRoom == null || !tp.transform.IsChildOf(normalRoom.transform)))
        {
            tp.gameObject.SetActive(false);
        }

        // Setup the Key in Ending Sequence:
        // Position the key clearly visible in the Ending Room with a warm golden glint!
        SetupEndingKey();

        isMemoryShifted = true;
        StartHeartbeat();
    }

    private void SetupEndingKey()
    {
        if (PlayerController.hasKey) return;

        KeyPickup[] keys = Resources.FindObjectsOfTypeAll<KeyPickup>();
        foreach (var k in keys)
        {
            if (k.gameObject.scene.isLoaded)
            {
                // Unparent from normalRoom so it remains active
                if (endingSequence != null)
                {
                    k.transform.SetParent(endingSequence.transform, true);
                }
                k.gameObject.SetActive(true);

                // Position the key near the wife's body on the floor with safe elevation so it never clips!
                if (wifeModel != null)
                {
                    Vector3 wifePos = wifeModel.transform.position;
                    // Slightly offset from wife, raised above floor
                    k.transform.position = wifePos + new Vector3(0.8f, 0.25f, 0.35f);
                    k.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    k.transform.localScale = Vector3.one * 8f; // Ensure standard visible scale
                }

                // Add a warm glowing light to make the key stand out and guide the player
                Light keyLight = k.GetComponent<Light>();
                if (keyLight == null) keyLight = k.gameObject.AddComponent<Light>();
                keyLight.type = LightType.Point;
                keyLight.color = new Color(1f, 0.88f, 0.45f);
                keyLight.intensity = 3.5f;
                keyLight.range = 3.0f;

                Debug.Log($"RoomStateManager: Key spawned in Ending Room at {k.transform.position} with golden glint!");
                break;
            }
        }
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

        if (wifeModel == null)
        {
            GameObject girlObj = GameObject.Find("civilian_girl");
            if (girlObj != null) wifeModel = girlObj;
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

        // NOTE: Knife transform is NOT modified at runtime.
        // Your manual placement of the knife on the wife model in the editor is 100% preserved!
    }

    // ==================== PROCEDURAL TENSION HEARTBEAT ====================
    private Coroutine heartbeatCoroutine;
    private AudioSource heartbeatSource;

    public void StartHeartbeat()
    {
        if (heartbeatCoroutine != null) StopCoroutine(heartbeatCoroutine);
        heartbeatCoroutine = StartCoroutine(HeartbeatRoutine());
    }

    public void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
        if (heartbeatSource != null)
        {
            heartbeatSource.Stop();
        }
    }

    private IEnumerator HeartbeatRoutine()
    {
        if (heartbeatSource == null)
        {
            heartbeatSource = gameObject.AddComponent<AudioSource>();
            heartbeatSource.playOnAwake = false;
            heartbeatSource.spatialBlend = 0f;
            heartbeatSource.volume = 0.65f;
        }

        AudioClip heartbeatClip = CreateProceduralHeartbeatClip();

        // Pulsing heartbeat sound loop with increasing psychological tension
        while (!isEscaped && !PlayerController.hasKey)
        {
            heartbeatSource.PlayOneShot(heartbeatClip, 0.7f);
            yield return new WaitForSeconds(0.2f);
            heartbeatSource.PlayOneShot(heartbeatClip, 0.45f); // Second lub-dub beat

            // Distance-based tension: If player is close to wife, heart beats faster!
            float distToWife = 6f;
            if (wifeModel != null && Camera.main != null)
            {
                distToWife = Vector3.Distance(Camera.main.transform.position, wifeModel.transform.position);
            }
            float interval = Mathf.Lerp(0.5f, 1.25f, Mathf.Clamp01(distToWife / 6f));

            yield return new WaitForSeconds(interval);
        }
    }

    private AudioClip CreateProceduralHeartbeatClip()
    {
        int sampleRate = 44100;
        float duration = 0.18f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            // Deep 55Hz bass thump with exponential decay envelope
            float freq = 55f - (t * 20f);
            float envelope = Mathf.Exp(-t * 22f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.85f;
        }

        AudioClip clip = AudioClip.Create("ProceduralHeartbeat", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Step 5: The Final Escape - Doorway White Light crescendo into Black revelation screen
    public void TriggerDoorwayEscape(float openDuration, Transform doorTransform)
    {
        if (isEscaped) return;
        isEscaped = true;
        StartCoroutine(DoorwayEscapeSequence(openDuration, doorTransform));
    }

    public void TriggerFinalEscape()
    {
        TriggerDoorwayEscape(1.8f, null);
    }

    private IEnumerator DoorwayEscapeSequence(float openDuration, Transform doorTransform)
    {
        Debug.Log("RoomStateManager: Step 5 — Doorway White Light Escape Sequence!");
        StopHeartbeat();
        EnsureFlashCanvas();
        EnsureEscapeCanvas();

        if (GameManager.instance != null)
        {
            GameManager.instance.CompleteObjective();
            GameManager.instance.SetObjective("ESCAPE SUCCESSFUL");
        }

        // Immediately set the camera's clearFlags to SolidColor with pure white
        // so that NO empty skybox/world can ever be visible through the door!
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = Color.white;
        }

        // Spawn brilliant white light inside the open doorway
        if (doorTransform != null)
        {
            GameObject doorwayLightObj = new GameObject("DoorwayEscapeLight");
            doorwayLightObj.transform.position = doorTransform.position + doorTransform.forward * 0.4f + Vector3.up * 1.2f;
            Light escapeLight = doorwayLightObj.AddComponent<Light>();
            escapeLight.type = LightType.Point;
            escapeLight.color = Color.white;
            escapeLight.intensity = 15f;
            escapeLight.range = 8f;
        }

        // Fade in pure white light canvas as the door swings open
        float elapsed = 0f;
        float whiteFadeDuration = Mathf.Max(1.8f, openDuration);
        if (whiteFlashCanvasGroup != null)
        {
            whiteFlashCanvasGroup.blocksRaycasts = true;
            while (elapsed < whiteFadeDuration)
            {
                elapsed += Time.deltaTime;
                whiteFlashCanvasGroup.alpha = Mathf.Clamp01(elapsed / whiteFadeDuration);
                yield return null;
            }
            whiteFlashCanvasGroup.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(whiteFadeDuration);
        }

        // Hold in pure blinding white crescendo
        yield return new WaitForSeconds(0.8f);

        // Smoothly fade from pure white to cinematic pitch black with revelation text over 2.0s
        elapsed = 0f;
        float blackFadeDuration = 2.0f;
        if (escapeTextUI != null)
        {
            escapeTextUI.text = EvidenceManager.GetEndingText();
        }
        if (escapeCanvasGroup != null)
        {
            escapeCanvasGroup.blocksRaycasts = true;
            while (elapsed < blackFadeDuration)
            {
                elapsed += Time.deltaTime;
                escapeCanvasGroup.alpha = Mathf.Clamp01(elapsed / blackFadeDuration);
                yield return null;
            }
            escapeCanvasGroup.alpha = 1f;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void TriggerEndingSequence()
    {
        TriggerMemoryFlash();
    }

    void Update()
    {
#if UNITY_EDITOR || DEBUG
        // PRESENTATION BYPASSES
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("DEMO BYPASS: Force switching room layer!");
            SwitchLayer();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("DEMO BYPASS: Force triggering Clock Spin & Memory Flash!");
            ClockController clock = GameObject.FindAnyObjectByType<ClockController>();
            if (clock != null) clock.TriggerTimeShift();
            TriggerMemoryFlash();
        }
#endif
    }
}