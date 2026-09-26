using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class IntroScreenManager : MonoBehaviour
{
    [Header("Black Screen Fade Settings")]
    public float fadeOutDuration = 2.0f;    // Smooth fade out to reveal the room

    [Header("WASD Movement Tutorial Prompt")]
    public bool showWASDOnStart = true;
    public Sprite customWASDSprite;        // Optional inspector override
    public float wasdDisplayDuration = 5.0f; // Max seconds before auto-fade

    private Canvas introCanvas;
    private Image blackOverlay;
    private CanvasGroup blackCanvasGroup;

    private GameObject playButtonObj;
    private Button playButton;
    private bool hasStarted = false;

    private GameObject wasdPanelObj;
    private CanvasGroup wasdCanvasGroup;
    private Image wasdImage;
    private TextMeshProUGUI wasdLabel;

    void Awake()
    {
        CreateIntroUI();
        LockPlayer(true);
        // Make sure cursor is visible so player can click Play
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(IntroAndTutorialRoutine());
    }

    void Update()
    {
        // F11 to toggle fullscreen anytime during gameplay or presentation
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleFullScreen();
        }

        // Keyboard shortcut to start game if mouse is not clicked
        if (!hasStarted && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;

        // Automatically maximize / fullscreen the game when Play is clicked
        SetFullScreen(true);

        if (playButtonObj != null)
        {
            Destroy(playButtonObj);
        }
    }

    public static void ToggleFullScreen()
    {
        SetFullScreen(!Screen.fullScreen);
    }

    public static void SetFullScreen(bool full)
    {
        Screen.fullScreen = full;
#if UNITY_EDITOR
        try
        {
            var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            if (gameViewType != null)
            {
                var gameView = UnityEditor.EditorWindow.GetWindow(gameViewType);
                if (gameView != null)
                {
                    gameView.maximized = full;
                }
            }
        }
        catch { }
#endif
    }

    private void CreateIntroUI()
    {
        // Dedicated top-level overlay canvas
        GameObject canvasObj = new GameObject("IntroFadeCanvas");
        canvasObj.transform.SetParent(transform, false);
        introCanvas = canvasObj.AddComponent<Canvas>();
        introCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        introCanvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 1. Black screen overlay
        GameObject blackObj = new GameObject("BlackOverlay");
        blackObj.transform.SetParent(canvasObj.transform, false);
        blackCanvasGroup = blackObj.AddComponent<CanvasGroup>();
        blackCanvasGroup.alpha = 1f;

        blackOverlay = blackObj.AddComponent<Image>();
        blackOverlay.color = Color.black;
        RectTransform blackRect = blackOverlay.GetComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero;
        blackRect.anchorMax = Vector2.one;
        blackRect.offsetMin = Vector2.zero;
        blackRect.offsetMax = Vector2.zero;

        // 2. Simple Center PLAY Button
        playButtonObj = new GameObject("PlayButton");
        playButtonObj.transform.SetParent(canvasObj.transform, false);

        Image btnImg = playButtonObj.AddComponent<Image>();
        btnImg.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

        playButton = playButtonObj.AddComponent<Button>();
        ColorBlock cb = playButton.colors;
        cb.normalColor = new Color(0.14f, 0.14f, 0.16f, 0.95f);
        cb.highlightedColor = new Color(0.65f, 0.15f, 0.15f, 1.0f); // Crimson red on hover
        cb.pressedColor = new Color(0.85f, 0.20f, 0.20f, 1.0f);
        cb.selectedColor = cb.normalColor;
        playButton.colors = cb;
        playButton.onClick.AddListener(StartGame);

        RectTransform btnRect = playButtonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(260, 75);
        btnRect.anchoredPosition = Vector2.zero;

        // Play Button Text
        GameObject btnTextObj = new GameObject("PlayText");
        btnTextObj.transform.SetParent(playButtonObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "P L A Y";
        btnText.fontSize = 28f;
        btnText.fontStyle = FontStyles.Bold;
        btnText.characterSpacing = 8f;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = btnTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // 3. WASD Movement Tutorial Prompt Panel (Top Left Corner, enlarged)
        wasdPanelObj = new GameObject("WASD_Tutorial_Prompt");
        wasdPanelObj.transform.SetParent(canvasObj.transform, false);
        wasdCanvasGroup = wasdPanelObj.AddComponent<CanvasGroup>();
        wasdCanvasGroup.alpha = 0f; // Start hidden

        // Sleek semi-transparent dark backing card
        Image panelBg = wasdPanelObj.AddComponent<Image>();
        panelBg.color = new Color(0.06f, 0.06f, 0.06f, 0.78f);

        RectTransform panelRect = wasdPanelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.sizeDelta = new Vector2(240, 250);
        panelRect.anchoredPosition = new Vector2(45f, -45f);

        // WASD Icon Image inside panel
        GameObject iconObj = new GameObject("WASD_Icon");
        iconObj.transform.SetParent(wasdPanelObj.transform, false);
        wasdImage = iconObj.AddComponent<Image>();
        wasdImage.preserveAspect = true;

        // Load WASD sprite: try custom override, then Resources
        Sprite loadedSprite = customWASDSprite;
        if (loadedSprite == null)
        {
            Texture2D tex = Resources.Load<Texture2D>("WASD_Tutorial");
            if (tex == null) tex = Resources.Load<Texture2D>("WASD_Raw");

            if (tex != null)
            {
                loadedSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }

        if (loadedSprite != null)
        {
            wasdImage.sprite = loadedSprite;
        }

        RectTransform iconRect = wasdImage.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(190, 165);
        iconRect.anchoredPosition = new Vector2(0f, 22f);

        // Subtitle Text: "MOVE"
        GameObject labelObj = new GameObject("WASD_Label");
        labelObj.transform.SetParent(wasdPanelObj.transform, false);
        wasdLabel = labelObj.AddComponent<TextMeshProUGUI>();
        wasdLabel.text = "MOVE";
        wasdLabel.fontSize = 22f;
        wasdLabel.fontStyle = FontStyles.Bold;
        wasdLabel.characterSpacing = 8f;
        wasdLabel.color = new Color(0.95f, 0.95f, 0.95f, 0.92f);
        wasdLabel.alignment = TextAlignmentOptions.Center;

        RectTransform labelRect = wasdLabel.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0f);
        labelRect.anchorMax = new Vector2(0.5f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.sizeDelta = new Vector2(210, 36);
        labelRect.anchoredPosition = new Vector2(0f, 14f);
    }

    private IEnumerator IntroAndTutorialRoutine()
    {
        // 1. Wait on black screen until player clicks PLAY (or presses Space/Enter)
        yield return new WaitUntil(() => hasStarted);

        // Brief 0.4s hold before fading
        yield return new WaitForSeconds(0.4f);

        // 2. Smooth fade out of black screen to reveal game world
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            blackCanvasGroup.alpha = 1f - (t * t);
            yield return null;
        }

        blackCanvasGroup.alpha = 0f;

        // Destroy black overlay
        if (blackOverlay != null)
        {
            Destroy(blackOverlay.gameObject);
        }

        // Lock cursor and unlock player controls for gameplay
        LockPlayer(false);

        // Notify GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.OnIntroFinished();
        }

        // 3. Show WASD Movement Tutorial Icon
        if (showWASDOnStart && wasdCanvasGroup != null)
        {
            yield return StartCoroutine(ShowWASDPromptRoutine());
        }

        // Clean up canvas
        if (introCanvas != null)
        {
            Destroy(introCanvas.gameObject);
        }
    }

    private IEnumerator ShowWASDPromptRoutine()
    {
        // Fade in WASD prompt smoothly
        float elapsed = 0f;
        float fadeInTime = 0.6f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            wasdCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInTime);
            yield return null;
        }
        wasdCanvasGroup.alpha = 1f;

        // Keep visible until the player moves (W, A, S, D) or max duration expires
        float timer = 0f;
        bool playerMoved = false;

        while (timer < wasdDisplayDuration && !playerMoved)
        {
            timer += Time.deltaTime;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
                Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || 
                Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
            {
                playerMoved = true;
            }

            yield return null;
        }

        // Smoothly fade out the WASD prompt
        elapsed = 0f;
        float fadeOutTime = 0.8f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            wasdCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutTime);
            yield return null;
        }

        wasdCanvasGroup.alpha = 0f;
    }

    private void LockPlayer(bool locked)
    {
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            pc.enabled = !locked;
        }

        if (locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
