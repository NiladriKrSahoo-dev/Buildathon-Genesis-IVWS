using UnityEngine;
using TMPro;
using System.Collections;

public class TapePickup : Interactable
{
    [Header("Tape Configuration")]
    public int tapeNumber = 1;

    public override void Interact()
    {
        base.Interact();

        if (tapeNumber == 1)
        {
            PlayerController.hasTape1 = true;
        }
        else if (tapeNumber == 2)
        {
            PlayerController.hasTape2 = true;
        }
        else
        {
            PlayerController.hasTape = true;
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.ShowTapeInInventory();
            GameManager.instance.SetObjective($"Insert Tape {tapeNumber} into the cassette player.");
        }

        if (tapeNumber == 2)
        {
            TapePlayer tp = FindAnyObjectByType<TapePlayer>();
            if (tp != null && tp.captionUI != null)
            {
                tp.StartCoroutine(ShowTapeFoundSubtitle(tp.captionUI));
            }
        }

        gameObject.SetActive(false); 
    }

    private IEnumerator ShowTapeFoundSubtitle(TextMeshProUGUI captionUI)
    {
        Canvas parentCanvas = captionUI.GetComponentInParent<Canvas>(true);
        if (parentCanvas != null && !parentCanvas.gameObject.activeSelf) parentCanvas.gameObject.SetActive(true);

        captionUI.gameObject.SetActive(true);
        captionUI.text = "[Tape 2 Found: A hidden recording from that night...]";
        yield return new WaitForSeconds(3.5f);
        captionUI.text = "";
        captionUI.gameObject.SetActive(false);
    }
}