using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Text")]
    public TextMeshProUGUI objectiveText;

    [Header("Inventory Slots")]
    public GameObject tapeUIIcon;
    public GameObject keyUIIcon;

    void Awake()
    {
        instance = this; 
        EnsureObjectiveText();
    }

    void Start()
    {
        EnsureObjectiveText();
        if (string.IsNullOrEmpty(objectiveText != null ? objectiveText.text : ""))
        {
            SetObjective("Find and listen to the cassette tape.");
        }
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
                UIManager ui = FindObjectOfType<UIManager>();
                if (ui != null && ui.objectiveText != null) objectiveText = ui.objectiveText;
            }
        }

        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(true);
            // Ensure readable dark color on light/white panels
            objectiveText.color = new Color(0.12f, 0.12f, 0.15f, 1f);
            
            Canvas parentCanvas = objectiveText.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
            {
                parentCanvas.gameObject.SetActive(true);
            }
        }
    }

    public void SetObjective(string newObjective)
    {
        EnsureObjectiveText();
        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(true);
            objectiveText.color = new Color(0.12f, 0.12f, 0.15f, 1f);
            objectiveText.text = newObjective;
        }
    }

    public void CompleteObjective()
    {
        EnsureObjectiveText();
        if (objectiveText != null)
        {
            objectiveText.text = "<s>" + objectiveText.text + "</s>"; 
        }
    }

    public void ShowTapeInInventory()
    {
        if (tapeUIIcon != null) tapeUIIcon.SetActive(true);
    }

    public void ShowKeyInInventory()
    {
        if (keyUIIcon != null) keyUIIcon.SetActive(true);
    }
}