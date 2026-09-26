using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroScreenManager : MonoBehaviour
{
    [Header("Black Screen Fade Settings")]
    public float blackHoldDuration = 1.0f; // Seconds to stay pure black
    public float fadeOutDuration = 2.0f;    // Smooth fade out to reveal the room

    private Canvas introCanvas;
    private Image blackOverlay;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        CreateIntroUI();
        LockPlayer(true);
        StartCoroutine(FadeOutRoutine());
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

        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        // Pure black overlay
        GameObject blackObj = new GameObject("BlackOverlay");
        blackObj.transform.SetParent(canvasObj.transform, false);
        blackOverlay = blackObj.AddComponent<Image>();
        blackOverlay.color = Color.black;
        RectTransform blackRect = blackOverlay.GetComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero;
        blackRect.anchorMax = Vector2.one;
        blackRect.offsetMin = Vector2.zero;
        blackRect.offsetMax = Vector2.zero;
    }

    private IEnumerator FadeOutRoutine()
    {
        // Hold on solid black
        yield return new WaitForSeconds(blackHoldDuration);

        // Smooth fade out to reveal game world
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            // Ease-out curve
            canvasGroup.alpha = 1f - (t * t);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        // Unlock player controls
        LockPlayer(false);

        // Notify GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.OnIntroFinished();
        }

        if (introCanvas != null)
        {
            Destroy(introCanvas.gameObject);
        }
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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
