using UnityEngine;
using TMPro;
using System.Collections;

public class KeyPickup : Interactable
{
    public override void Interact()
    {
        base.Interact();
        PlayerController.hasKey = true;
        if (GameManager.instance != null)
        {
            GameManager.instance.ShowKeyInInventory();
            GameManager.instance.SetObjective("Unlock the door and face reality.");
        }

        // Cut the tension heartbeat into sudden dead silence
        if (RoomStateManager.instance != null)
        {
            RoomStateManager.instance.StopHeartbeat();
        }

        // Show revelation subtitle
        TapePlayer tp = FindAnyObjectByType<TapePlayer>();
        if (tp != null && tp.captionUI != null)
        {
            tp.StartCoroutine(ShowKeyFoundSubtitle(tp.captionUI, 4.5f));
        }

        gameObject.SetActive(false); 
    }

    private IEnumerator ShowKeyFoundSubtitle(TextMeshProUGUI captionUI, float duration)
    {
        Canvas parentCanvas = captionUI.GetComponentInParent<Canvas>(true);
        if (parentCanvas != null && !parentCanvas.gameObject.activeSelf) parentCanvas.gameObject.SetActive(true);

        captionUI.gameObject.SetActive(true);
        captionUI.fontSize = 19f;
        captionUI.fontStyle = FontStyles.Normal;
        captionUI.color = Color.white;
        captionUI.text = "The key was never on the desk... You locked the door from the inside.";
        yield return new WaitForSeconds(duration);
        captionUI.text = "";
        captionUI.gameObject.SetActive(false);
    }
}