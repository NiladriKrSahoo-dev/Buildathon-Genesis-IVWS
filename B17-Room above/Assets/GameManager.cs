using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Text")]
    public TextMeshProUGUI objectiveText;
    public string currentObjective;

    [Header("Inventory Slots")]
    public GameObject tapeUIIcon;
    public GameObject keyUIIcon;

    void Awake()
    {
        instance = this; 
        EnsureObjectiveText();
        EnsureTapeIcon();
        if (GetComponent<ModernHUDManager>() == null)
        {
            gameObject.AddComponent<ModernHUDManager>();
        }
        if (GetComponent<EvidenceManager>() == null)
        {
            gameObject.AddComponent<EvidenceManager>();
        }
        if (GetComponent<IntroScreenManager>() == null)
        {
            gameObject.AddComponent<IntroScreenManager>();
        }
    }

    void Start()
    {
        EnsureObjectiveText();
        EnsureTapeIcon();
        EnsureGramophone();
        if (objectiveText != null && (string.IsNullOrEmpty(objectiveText.text) || objectiveText.text.ToLower().Contains("tv")))
        {
            SetObjective("Find and listen to the cassette tape.");
        }
    }

    private void EnsureGramophone()
    {
        GameObject gramo = GameObject.Find("gramophone");
        if (gramo == null)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name.ToLower().Contains("gramophone") && go.scene.isLoaded)
                {
                    gramo = go;
                    break;
                }
            }
        }
        if (gramo != null && gramo.GetComponent<GramophoneController>() == null)
        {
            gramo.AddComponent<GramophoneController>();
        }
    }

    public void EnsureTapeIcon()
    {
        if (tapeUIIcon != null) return;

        // Look for existing TapeIcon in scene
        GameObject existing = GameObject.Find("TapeIcon");
        if (existing != null)
        {
            tapeUIIcon = existing;
            return;
        }

        // Try to find under Slot 1 of InventoryPanel
        GameObject inv = GameObject.Find("InventoryPanel");
        if (inv != null && inv.transform.childCount > 0)
        {
            Transform slot1 = inv.transform.GetChild(0);
            Transform child = slot1.Find("TapeIcon");
            if (child != null)
            {
                tapeUIIcon = child.gameObject;
                return;
            }

            // Create TapeIcon inside Slot 1
            GameObject newTape = new GameObject("TapeIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            newTape.transform.SetParent(slot1, false);

            RectTransform rt = newTape.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(48f, 48f);

            UnityEngine.UI.Image img = newTape.GetComponent<UnityEngine.UI.Image>();
            img.preserveAspect = true;
            img.sprite = LoadTapeSprite();
            img.color = Color.white;

            newTape.SetActive(false);
            tapeUIIcon = newTape;
        }
    }

    private Sprite LoadTapeSprite()
    {
        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (var s in allSprites)
        {
            if (s.name.ToLower().Contains("tape")) return s;
        }

        string p = System.IO.Path.Combine(Application.dataPath, "TapeIcon.png");
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
        return null;
    }

    public void EnsureObjectiveText()
    {
        if (objectiveText == null)
        {
            // Search active and inactive UI text objects
            TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var t in allTexts)
            {
                if (t.gameObject.name.ToLower().Contains("objective"))
                {
                    objectiveText = t;
                    break;
                }
            }

            if (objectiveText == null)
            {
                UIManager ui = FindAnyObjectByType<UIManager>();
                if (ui != null && ui.objectiveText != null) objectiveText = ui.objectiveText;
            }
        }

        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(true);
            objectiveText.color = new Color(0.95f, 0.96f, 0.98f, 1f);
            
            Canvas parentCanvas = objectiveText.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
            {
                parentCanvas.gameObject.SetActive(true);
            }
        }
    }

    public void SetObjective(string newObjective)
    {
        currentObjective = newObjective;
        EnsureObjectiveText();
        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(true);
            objectiveText.text = newObjective;
        }
    }

    public void CompleteObjective()
    {
        EnsureObjectiveText();
        if (objectiveText != null)
        {
            string cur = objectiveText.text;
            if (cur.Contains("\n"))
            {
                string[] parts = cur.Split('\n');
                objectiveText.text = $"<size=68%><color=#44FF88><b>MISSION COMPLETED</b></color></size>\n<size=92%><s>{parts[parts.Length - 1]}</s></size>";
            }
            else
            {
                objectiveText.text = $"<size=68%><color=#44FF88><b>MISSION COMPLETED</b></color></size>\n<size=92%><s>{cur}</s></size>";
            }
        }
    }

    public void ShowTapeInInventory()
    {
        EnsureTapeIcon();
        if (tapeUIIcon != null) tapeUIIcon.SetActive(true);
    }

    public void HideTapeInInventory()
    {
        EnsureTapeIcon();
        if (tapeUIIcon != null) tapeUIIcon.SetActive(false);
    }

    public void ShowKeyInInventory()
    {
        if (keyUIIcon != null) keyUIIcon.SetActive(true);
    }

    public void OnIntroFinished()
    {
        Debug.Log("GameManager: Intro finished, game begins.");
        SetObjective("Find and listen to the cassette tape.");
    }
}