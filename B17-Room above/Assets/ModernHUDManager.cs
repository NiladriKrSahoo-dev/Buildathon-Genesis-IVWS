using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ModernHUDManager : MonoBehaviour
{
    public static ModernHUDManager instance;

    [Header("Inventory Colors & Styling")]
    public Color slotBgColor = new Color(0.08f, 0.08f, 0.10f, 0.85f);
    public Color slotBorderColor = new Color(0.30f, 0.30f, 0.34f, 0.45f);
    public Color slotActiveBorderColor = new Color(0.88f, 0.88f, 0.92f, 0.85f); // Clean muted silver-white

    [Header("Proximity Prompt Styling")]
    public Color promptBgColor = new Color(0.07f, 0.07f, 0.09f, 0.92f);
    public Color promptBorderColor = new Color(0.30f, 0.30f, 0.34f, 0.55f);
    public float promptActionFontSize = 14f;
    public float promptKeyFontSize = 18f;

    [Header("Panel Layout Offsets")]
    public float inventoryBottomOffset = 6f; // Lowered right near the bottom edge

    [Header("Raycast Settings")]
    public float interactDistance = 5.5f;

    // References
    private Canvas mainCanvas;
    private RectTransform inventoryPanel;

    // Interaction Prompt UI
    private GameObject promptRoot;
    private CanvasGroup promptCanvasGroup;
    private RectTransform promptRect;
    private TextMeshProUGUI promptKeyText;
    private TextMeshProUGUI promptActionText;
    private Image promptKeyBg;
    private Image promptContainerBg;

    // Crosshair
    private GameObject crosshairRoot;
    private Image crosshairDot;

    // Cached slot images
    private Image[] slotImages = new Image[4];
    private Image[] slotActiveGlows = new Image[4];

    private Camera playerCam;
    private float currentPromptAlpha = 0f;
    private float targetPromptAlpha = 0f;
    private float currentPromptScale = 0.85f;
    private float targetPromptScale = 0.85f;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        FindCanvasAndComponents();
        StyleObjectiveHUD();
        StyleInventoryPanel();
        CreateCrosshair();
        CreateProximityPrompt();
    }

    private void FindCanvasAndComponents()
    {
        if (mainCanvas == null)
        {
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            foreach (var c in allCanvases)
            {
                if (c.gameObject.name == "Canvas")
                {
                    mainCanvas = c;
                    break;
                }
            }
            if (mainCanvas == null && allCanvases.Length > 0) mainCanvas = allCanvases[0];
        }

        if (mainCanvas != null)
        {
            Transform inv = mainCanvas.transform.Find("InventoryPanel");
            if (inv != null) inventoryPanel = inv.GetComponent<RectTransform>();

            // Clean up any old AccentBar on ObjectivePanel
            Transform accentBar = mainCanvas.transform.Find("ObjectivePanel/AccentBar");
            if (accentBar != null) Destroy(accentBar.gameObject);
        }

        playerCam = Camera.main;
    }

    // ==================== OBJECTIVE PANEL STYLING & ALIGNMENT ====================

    private void StyleObjectiveHUD()
    {
        if (mainCanvas == null) return;

        Transform objPanelTr = mainCanvas.transform.Find("ObjectivePanel");
        if (objPanelTr == null)
        {
            var allRTs = mainCanvas.GetComponentsInChildren<RectTransform>(true);
            foreach (var r in allRTs)
            {
                if (r.name == "ObjectivePanel") { objPanelTr = r; break; }
            }
        }

        Transform objTextTr = mainCanvas.transform.Find("ObjectiveText");
        if (objTextTr == null && objPanelTr != null)
        {
            objTextTr = objPanelTr.Find("ObjectiveText");
        }
        if (objTextTr == null)
        {
            var allTMPs = mainCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTMPs)
            {
                if (t.name.ToLower().Contains("objective")) { objTextTr = t.transform; break; }
            }
        }

        if (objPanelTr != null)
        {
            RectTransform prt = objPanelTr.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(1f, 1f);
            prt.anchorMax = new Vector2(1f, 1f);
            prt.pivot = new Vector2(1f, 1f);
            prt.anchoredPosition = new Vector2(-24f, -24f);
            prt.sizeDelta = new Vector2(310f, 56f);

            // Clean up any old accent bar if still present
            Transform oldAccent = objPanelTr.Find("AccentBar");
            if (oldAccent != null) Destroy(oldAccent.gameObject);

            Image panelImg = objPanelTr.GetComponent<Image>();
            if (panelImg != null)
            {
                // Clean subtle translucent dark card - NO NEON, quiet horror aesthetic
                panelImg.sprite = CreateRoundedRectSprite(160, 60, 10, new Color(0.07f, 0.08f, 0.10f, 0.82f), new Color(0.28f, 0.30f, 0.35f, 0.40f), 1);
                panelImg.color = Color.white;
                panelImg.type = Image.Type.Sliced;
            }

            if (objTextTr != null)
            {
                objTextTr.SetParent(objPanelTr, false);
                objTextTr.localScale = Vector3.one;

                RectTransform trt = objTextTr.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.offsetMin = new Vector2(18f, 6f);
                trt.offsetMax = new Vector2(-18f, -6f);

                TextMeshProUGUI tmp = objTextTr.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize = 15f;
                    tmp.fontStyle = FontStyles.Normal;
                    tmp.color = new Color(0.92f, 0.94f, 0.97f, 1f);
                    tmp.alignment = TextAlignmentOptions.MidlineLeft;
                    tmp.textWrappingMode = TextWrappingModes.Normal;
                    tmp.overflowMode = TextOverflowModes.Ellipsis;
                }
            }
        }
    }

    // ==================== INVENTORY PANEL STYLING ====================

    private void StyleInventoryPanel()
    {
        if (inventoryPanel == null) return;

        // Position lowered right near the bottom screen margin
        inventoryPanel.anchorMin = new Vector2(0.5f, 0f);
        inventoryPanel.anchorMax = new Vector2(0.5f, 0f);
        inventoryPanel.pivot = new Vector2(0.5f, 0f);
        inventoryPanel.anchoredPosition = new Vector2(0f, inventoryBottomOffset);
        inventoryPanel.sizeDelta = new Vector2(350f, 72f);

        Sprite normalSlotSprite = CreateRoundedRectSprite(80, 80, 14, slotBgColor, slotBorderColor, 2);
        Sprite activeSlotSprite = CreateRoundedRectSprite(80, 80, 14, slotBgColor, slotActiveBorderColor, 3);

        int count = Mathf.Min(4, inventoryPanel.childCount);
        for (int i = 0; i < count; i++)
        {
            Transform child = inventoryPanel.GetChild(i);
            Image img = child.GetComponent<Image>();
            if (img != null)
            {
                slotImages[i] = img;
                img.sprite = normalSlotSprite;
                img.color = Color.white; // Tint is in the sprite
                img.type = Image.Type.Sliced;

                // Add slot number label [ 1 ], [ 2 ], etc.
                Transform numObj = child.Find("SlotNum");
                if (numObj == null)
                {
                    GameObject go = new GameObject("SlotNum");
                    go.transform.SetParent(child, false);
                    TextMeshProUGUI numTMP = go.AddComponent<TextMeshProUGUI>();
                    numTMP.text = (i + 1).ToString();
                    numTMP.fontSize = 11;
                    numTMP.color = new Color(0.60f, 0.60f, 0.65f, 0.55f); // Clean muted gray
                    numTMP.fontStyle = FontStyles.Bold;
                    numTMP.alignment = TextAlignmentOptions.TopLeft;

                    RectTransform rt = numTMP.rectTransform;
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchoredPosition = new Vector2(8f, -6f);
                    rt.sizeDelta = new Vector2(25f, 20f);
                }

                // Add active glow overlay
                Transform glowObj = child.Find("ActiveGlow");
                if (glowObj == null)
                {
                    GameObject g = new GameObject("ActiveGlow");
                    g.transform.SetParent(child, false);
                    Image glowImg = g.AddComponent<Image>();
                    glowImg.sprite = activeSlotSprite;
                    glowImg.color = new Color(1f, 1f, 1f, 0f); // Hidden initially
                    glowImg.type = Image.Type.Sliced;
                    glowImg.raycastTarget = false;

                    RectTransform grt = glowImg.rectTransform;
                    grt.anchorMin = Vector2.zero;
                    grt.anchorMax = Vector2.one;
                    grt.offsetMin = Vector2.zero;
                    grt.offsetMax = Vector2.zero;

                    slotActiveGlows[i] = glowImg;
                }
            }
        }

        // Setup TapeIcon in Slot 1 (child index 0)
        if (inventoryPanel.childCount > 0)
        {
            Transform slot1 = inventoryPanel.GetChild(0);
            Transform tapeObj = slot1.Find("TapeIcon");
            if (tapeObj == null)
            {
                GameObject newTape = new GameObject("TapeIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                newTape.transform.SetParent(slot1, false);
                tapeObj = newTape.transform;

                RectTransform rt = newTape.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(46f, 46f);

                Image img = newTape.GetComponent<Image>();
                img.preserveAspect = true;
                img.sprite = LoadTapeSprite();
                img.color = Color.white;

                newTape.SetActive(false); // Inactive until tape picked up
            }
            else
            {
                Image img = tapeObj.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = LoadTapeSprite();
                    img.preserveAspect = true;
                    img.color = Color.white;
                }
            }

            if (GameManager.instance != null && GameManager.instance.tapeUIIcon == null)
            {
                GameManager.instance.tapeUIIcon = tapeObj.gameObject;
            }
        }

        // Ensure Slot 2 KeyIcon has preserveAspect
        if (inventoryPanel.childCount > 1)
        {
            Transform slot2 = inventoryPanel.GetChild(1);
            Transform keyObj = slot2.Find("KeyIcon");
            if (keyObj != null)
            {
                Image keyImg = keyObj.GetComponent<Image>();
                if (keyImg != null) keyImg.preserveAspect = true;
                RectTransform krt = keyObj.GetComponent<RectTransform>();
                if (krt != null) krt.sizeDelta = new Vector2(46f, 46f);
            }
        }
    }

    // ==================== ROBLOX-STYLE PROXIMITY PROMPT ====================

    private void CreateProximityPrompt()
    {
        if (mainCanvas == null) return;

        // Container
        promptRoot = new GameObject("RobloxProximityPrompt", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        promptRoot.transform.SetParent(mainCanvas.transform, false);
        promptRoot.transform.SetAsLastSibling();

        promptCanvasGroup = promptRoot.GetComponent<CanvasGroup>();
        promptCanvasGroup.alpha = 0f;
        promptCanvasGroup.interactable = false;
        promptCanvasGroup.blocksRaycasts = false;

        promptRect = promptRoot.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0.5f);
        promptRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptRect.pivot = new Vector2(0.5f, 0.5f);
        promptRect.anchoredPosition = new Vector2(0f, -115f); // Below crosshair
        promptRect.sizeDelta = new Vector2(190f, 44f);

        // Pill background
        promptContainerBg = promptRoot.GetComponent<Image>();
        promptContainerBg.sprite = CreateRoundedRectSprite(190, 44, 22, promptBgColor, promptBorderColor, 2);
        promptContainerBg.color = Color.white;
        promptContainerBg.type = Image.Type.Sliced;
        promptContainerBg.raycastTarget = false;

        // Keycap Badge: [ E ]
        GameObject keyBadgeObj = new GameObject("KeycapBadge", typeof(RectTransform), typeof(Image));
        keyBadgeObj.transform.SetParent(promptRoot.transform, false);

        promptKeyBg = keyBadgeObj.GetComponent<Image>();
        promptKeyBg.sprite = CreateRoundedRectSprite(34, 34, 8, new Color(0.14f, 0.14f, 0.16f, 0.95f), new Color(0.40f, 0.40f, 0.44f, 0.65f), 2);
        promptKeyBg.color = Color.white;
        promptKeyBg.type = Image.Type.Sliced;
        promptKeyBg.raycastTarget = false;

        RectTransform keyRt = keyBadgeObj.GetComponent<RectTransform>();
        keyRt.anchorMin = new Vector2(0f, 0.5f);
        keyRt.anchorMax = new Vector2(0f, 0.5f);
        keyRt.pivot = new Vector2(0f, 0.5f);
        keyRt.anchoredPosition = new Vector2(8f, 0f);
        keyRt.sizeDelta = new Vector2(34f, 34f);

        // Letter "E" inside Keycap
        GameObject keyTextObj = new GameObject("KeyText", typeof(RectTransform), typeof(TextMeshProUGUI));
        keyTextObj.transform.SetParent(keyBadgeObj.transform, false);
        promptKeyText = keyTextObj.GetComponent<TextMeshProUGUI>();
        promptKeyText.text = "E";
        promptKeyText.fontSize = promptKeyFontSize;
        promptKeyText.fontStyle = FontStyles.Bold;
        promptKeyText.color = new Color(0.95f, 0.95f, 0.98f, 1f); // Crisp clean white, NO NEON
        promptKeyText.alignment = TextAlignmentOptions.Center;
        promptKeyText.raycastTarget = false;

        RectTransform ktRt = keyTextObj.GetComponent<RectTransform>();
        ktRt.anchorMin = Vector2.zero;
        ktRt.anchorMax = Vector2.one;
        ktRt.offsetMin = Vector2.zero;
        ktRt.offsetMax = Vector2.zero;

        // Action Label (e.g. "OPEN DRAWER", "PICK UP TAPE")
        GameObject actionTextObj = new GameObject("ActionText", typeof(RectTransform), typeof(TextMeshProUGUI));
        actionTextObj.transform.SetParent(promptRoot.transform, false);
        promptActionText = actionTextObj.GetComponent<TextMeshProUGUI>();
        promptActionText.text = "INTERACT";
        promptActionText.fontSize = promptActionFontSize;
        promptActionText.fontStyle = FontStyles.Bold;
        promptActionText.color = new Color(0.92f, 0.92f, 0.95f, 1f);
        promptActionText.alignment = TextAlignmentOptions.MidlineLeft;
        promptActionText.raycastTarget = false;

        RectTransform atRt = actionTextObj.GetComponent<RectTransform>();
        atRt.anchorMin = new Vector2(0f, 0f);
        atRt.anchorMax = new Vector2(1f, 1f);
        atRt.pivot = new Vector2(0f, 0.5f);
        atRt.offsetMin = new Vector2(50f, 4f);
        atRt.offsetMax = new Vector2(-12f, -4f);
    }

    private void CreateCrosshair()
    {
        if (mainCanvas == null) return;

        crosshairRoot = new GameObject("ModernCrosshair", typeof(RectTransform), typeof(Image));
        crosshairRoot.transform.SetParent(mainCanvas.transform, false);

        RectTransform rt = crosshairRoot.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(8f, 8f);

        crosshairDot = crosshairRoot.GetComponent<Image>();
        crosshairDot.sprite = CreateCircleSprite(16, Color.white);
        crosshairDot.color = new Color(1f, 1f, 1f, 0.65f);
        crosshairDot.raycastTarget = false;
    }

    // ==================== UPDATE & PROMPT RAYCAST ====================

    void Update()
    {
        UpdateInventorySlotHighlights();
        HandleInteractionRaycast();
        AnimatePromptTransition();
    }

    private void HandleInteractionRaycast()
    {
        if (playerCam == null) playerCam = Camera.main;
        if (playerCam == null) return;

        // Hide prompt if player has escaped or is in victory screen
        if (RoomStateManager.instance != null && RoomStateManager.instance.isEscaped)
        {
            targetPromptAlpha = 0f;
            return;
        }

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance);
        if (hits == null || hits.Length == 0)
        {
            hits = Physics.SphereCastAll(ray, 0.35f, interactDistance);
        }

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            Interactable targetInteractable = null;
            foreach (var h in hits)
            {
                Interactable it = h.collider.GetComponentInParent<Interactable>();
                if (it != null && it.gameObject.activeInHierarchy && it.enabled)
                {
                    targetInteractable = it;
                    break;
                }
            }

            if (targetInteractable != null)
            {
                string actionText = GetActionTextForInteractable(targetInteractable);
                if (promptActionText != null) promptActionText.text = actionText;

                // Adjust pill width dynamically to fit action text
                float calculatedWidth = Mathf.Max(180f, 72f + (actionText.Length * 9.5f));
                if (promptRect != null) promptRect.sizeDelta = new Vector2(calculatedWidth, 44f);

                targetPromptAlpha = 1f;
                targetPromptScale = 1f;

                // Expand crosshair dot slightly
                if (crosshairDot != null)
                {
                    crosshairDot.rectTransform.sizeDelta = Vector2.Lerp(crosshairDot.rectTransform.sizeDelta, new Vector2(14f, 14f), Time.deltaTime * 12f);
                    crosshairDot.color = new Color(1f, 1f, 1f, 0.95f); // Clean white, NO NEON
                }
                return;
            }
        }

        // Nothing interactable aimed at
        targetPromptAlpha = 0f;
        targetPromptScale = 0.85f;

        if (crosshairDot != null)
        {
            crosshairDot.rectTransform.sizeDelta = Vector2.Lerp(crosshairDot.rectTransform.sizeDelta, new Vector2(8f, 8f), Time.deltaTime * 12f);
            crosshairDot.color = new Color(1f, 1f, 1f, 0.65f);
        }
    }

    private string GetActionTextForInteractable(Interactable interactable)
    {
        string typeName = interactable.GetType().Name;
        string objName = interactable.gameObject.name.ToLower();

        if (interactable is EvidenceItem ev && !string.IsNullOrEmpty(ev.promptActionText))
        {
            return ev.promptActionText;
        }

        if (interactable is Door)
        {
            return PlayerController.hasKey ? "UNLOCK EXIT DOOR" : "DOOR (LOCKED)";
        }
        if (interactable is Drawer)
        {
            return "OPEN / CLOSE DRAWER";
        }
        if (interactable is TapePlayer)
        {
            return PlayerController.hasTape ? "INSERT CASSETTE" : "EXAMINE CASSETTE DECK";
        }
        if (interactable is TapePickup tp)
        {
            return $"PICK UP TAPE {tp.tapeNumber}";
        }
        if (interactable is KeyPickup)
        {
            return "PICK UP WORN KEY";
        }
        if (interactable is DeskInvestigation)
        {
            return "EXAMINE DESK";
        }

        if (objName.Contains("door")) return "OPEN DOOR";
        if (objName.Contains("drawer")) return "OPEN DRAWER";
        if (objName.Contains("tape")) return "PICK UP TAPE";
        if (objName.Contains("key")) return "PICK UP KEY";

        return "INTERACT";
    }

    private void AnimatePromptTransition()
    {
        if (promptCanvasGroup == null) return;

        currentPromptAlpha = Mathf.MoveTowards(currentPromptAlpha, targetPromptAlpha, Time.deltaTime * 8f);
        promptCanvasGroup.alpha = currentPromptAlpha;

        currentPromptScale = Mathf.MoveTowards(currentPromptScale, targetPromptScale, Time.deltaTime * 6f);
        if (promptRect != null) promptRect.localScale = Vector3.one * currentPromptScale;
    }

    private void UpdateInventorySlotHighlights()
    {
        // Slot 1 (Tape)
        if (slotActiveGlows[0] != null)
        {
            float targetAlpha = PlayerController.hasTape ? 1f : 0f;
            Color c = slotActiveGlows[0].color;
            c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * 6f);
            slotActiveGlows[0].color = c;
        }

        // Slot 2 (Key)
        if (slotActiveGlows[1] != null)
        {
            float targetAlpha = PlayerController.hasKey ? 1f : 0f;
            Color c = slotActiveGlows[1].color;
            c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * 6f);
            slotActiveGlows[1].color = c;
        }

        // Automatically ensure icons reflect player state
        if (GameManager.instance != null)
        {
            if (GameManager.instance.tapeUIIcon != null)
            {
                if (PlayerController.hasTape && !GameManager.instance.tapeUIIcon.activeSelf)
                {
                    GameManager.instance.tapeUIIcon.SetActive(true);
                }
                else if (!PlayerController.hasTape && GameManager.instance.tapeUIIcon.activeSelf)
                {
                    GameManager.instance.tapeUIIcon.SetActive(false);
                }
            }

            if (GameManager.instance.keyUIIcon != null)
            {
                if (PlayerController.hasKey && !GameManager.instance.keyUIIcon.activeSelf)
                {
                    GameManager.instance.keyUIIcon.SetActive(true);
                }
                else if (!PlayerController.hasKey && GameManager.instance.keyUIIcon.activeSelf)
                {
                    GameManager.instance.keyUIIcon.SetActive(false);
                }
            }
        }
    }

    private Sprite LoadTapeSprite()
    {
        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (var s in allSprites)
        {
            if (s.name.ToLower().Contains("tape")) return s;
        }

        string[] paths = new string[]
        {
            System.IO.Path.Combine(Application.dataPath, "TapeIcon.png"),
            System.IO.Path.Combine(Application.dataPath, "Models", "TapeIcon.png")
        };

        foreach (string p in paths)
        {
            if (System.IO.File.Exists(p))
            {
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(p);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    }
                }
                catch { }
            }
        }
        return null;
    }

    // ==================== PROCEDURAL SPRITE GENERATION ====================

    private Sprite CreateRoundedRectSprite(int w, int h, int radius, Color fillColor, Color borderColor, int borderWidth)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int cornerX = Mathf.Min(x, w - 1 - x);
                int cornerY = Mathf.Min(y, h - 1 - y);

                if (cornerX < radius && cornerY < radius)
                {
                    float dist = Vector2.Distance(new Vector2(cornerX, cornerY), new Vector2(radius, radius));
                    if (dist > radius + 0.5f)
                    {
                        tex.SetPixel(x, y, clear);
                    }
                    else if (dist > radius - borderWidth)
                    {
                        tex.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fillColor);
                    }
                }
                else
                {
                    if (x < borderWidth || x >= w - borderWidth || y < borderWidth || y >= h - borderWidth)
                    {
                        tex.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fillColor);
                    }
                }
            }
        }

        tex.Apply();
        // 9-slice borders so it can stretch cleanly
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    private Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float radius = size * 0.5f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                if (d <= radius)
                {
                    tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
