using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class IntroScreenManager : MonoBehaviour
{
    [Header("Intro Timing")]
    public float blackHoldDuration = 1.0f;      // Seconds of pure black before logo appears
    public float logoFadeInDuration = 1.5f;      // Logo fades in over this time
    public float logoHoldDuration = 2.5f;        // Logo stays fully visible
    public float taglineFadeInDuration = 0.8f;   // Tagline fades in after logo
    public float taglineHoldDuration = 1.5f;     // Tagline stays visible
    public float fadeOutDuration = 2.0f;         // Everything fades out to reveal game

    [Header("Custom Logo (Optional)")]
    public Sprite customLogoSprite;              // Drag a sprite here to override the Resources logo

    private Canvas introCanvas;
    private Image blackOverlay;
    private Image logoImage;
    private TextMeshProUGUI taglineText;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        CreateIntroUI();
        LockPlayer(true);
        StartCoroutine(PlayIntroSequence());
    }

    private void CreateIntroUI()
    {
        // Create a dedicated Canvas that renders on top of everything
        GameObject canvasObj = new GameObject("IntroScreenCanvas");
        canvasObj.transform.SetParent(transform, false);
        introCanvas = canvasObj.AddComponent<Canvas>();
        introCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        introCanvas.sortingOrder = 9999; // Always on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        // Full-screen black overlay
        GameObject blackObj = new GameObject("BlackOverlay");
        blackObj.transform.SetParent(canvasObj.transform, false);
        blackOverlay = blackObj.AddComponent<Image>();
        blackOverlay.color = new Color(0.08f, 0.06f, 0.06f, 1f); // Near-black with slight warmth
        RectTransform blackRect = blackOverlay.GetComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero;
        blackRect.anchorMax = Vector2.one;
        blackRect.offsetMin = Vector2.zero;
        blackRect.offsetMax = Vector2.zero;

        // SINS Logo Image
        GameObject logoObj = new GameObject("SinsLogo");
        logoObj.transform.SetParent(canvasObj.transform, false);
        logoImage = logoObj.AddComponent<Image>();
        logoImage.preserveAspect = true;

        // Load logo: try custom sprite first, then Resources
        Sprite logoSprite = customLogoSprite;
        if (logoSprite == null)
        {
            Texture2D logoTex = Resources.Load<Texture2D>("SinsLogo");
            if (logoTex != null)
            {
                logoSprite = Sprite.Create(logoTex, new Rect(0, 0, logoTex.width, logoTex.height), new Vector2(0.5f, 0.5f));
            }
        }

        if (logoSprite != null)
        {
            logoImage.sprite = logoSprite;
        }
        else
        {
            // Fallback: just use white with transparency
            logoImage.color = Color.clear;
        }

        RectTransform logoRect = logoImage.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoRect.pivot = new Vector2(0.5f, 0.5f);
        logoRect.sizeDelta = new Vector2(550, 550); // Big centered logo
        logoRect.anchoredPosition = new Vector2(0f, 30f); // Slightly above center

        Color logoColor = logoImage.color;
        logoColor.a = 0f; // Start invisible
        logoImage.color = logoColor;

        // Tagline text below logo
        GameObject tagObj = new GameObject("Tagline");
        tagObj.transform.SetParent(canvasObj.transform, false);
        taglineText = tagObj.AddComponent<TextMeshProUGUI>();
        taglineText.text = "WHAT  HAPPENED  IN  THIS  ROOM?";
        taglineText.fontSize = 22f;
        taglineText.fontStyle = FontStyles.SmallCaps;
        taglineText.characterSpacing = 8f;
        taglineText.color = new Color(0.65f, 0.20f, 0.18f, 0f); // Deep blood red, invisible initially
        taglineText.alignment = TextAlignmentOptions.Center;

        RectTransform tagRect = taglineText.GetComponent<RectTransform>();
        tagRect.anchorMin = new Vector2(0.5f, 0.5f);
        tagRect.anchorMax = new Vector2(0.5f, 0.5f);
        tagRect.pivot = new Vector2(0.5f, 0.5f);
        tagRect.sizeDelta = new Vector2(800, 50);
        tagRect.anchoredPosition = new Vector2(0f, -220f); // Below the logo
    }

    private IEnumerator PlayIntroSequence()
    {
        // Phase 1: Hold on pure black
        yield return new WaitForSeconds(blackHoldDuration);

        // Phase 2: Fade in logo
        float elapsed = 0f;
        while (elapsed < logoFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / logoFadeInDuration);
            // Smooth ease-in curve
            float alpha = t * t * (3f - 2f * t);
            Color c = logoImage.color;
            c.a = alpha;
            logoImage.color = c;
            yield return null;
        }
        SetAlpha(logoImage, 1f);

        // Phase 3: Hold logo
        yield return new WaitForSeconds(logoHoldDuration);

        // Phase 4: Fade in tagline
        elapsed = 0f;
        while (elapsed < taglineFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / taglineFadeInDuration);
            Color c = taglineText.color;
            c.a = t;
            taglineText.color = c;
            yield return null;
        }

        // Phase 5: Hold tagline
        yield return new WaitForSeconds(taglineHoldDuration);

        // Phase 6: Fade out EVERYTHING to reveal the game
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            // Smooth ease-out
            float alpha = 1f - (t * t);
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = 0f;

        // Unlock the player and destroy the intro UI
        LockPlayer(false);

        // Notify GameManager that intro is done
        if (GameManager.instance != null)
        {
            GameManager.instance.OnIntroFinished();
        }

        Destroy(introCanvas.gameObject);
    }

    private void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    private void LockPlayer(bool locked)
    {
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            pc.enabled = !locked;
        }

        // Also lock cursor
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
