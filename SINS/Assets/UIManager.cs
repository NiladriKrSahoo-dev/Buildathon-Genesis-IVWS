using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;
    public Image inventorySlot1;

    public void UpdateObjective(string text)
    {
        if (objectiveText != null) objectiveText.text = text;
        if (GameManager.instance != null) GameManager.instance.SetObjective(text);
    }

    public void AddItemToInventory()
    {
        if (inventorySlot1 != null) inventorySlot1.color = Color.white; 
    }
}