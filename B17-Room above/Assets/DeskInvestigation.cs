using UnityEngine;
using TMPro;
using System.Collections;

public class DeskInvestigation : Interactable
{
    private Coroutine captionRoutine;

    public override void Interact()
    {
        base.Interact();
        // If player already has the key, no need to show missing keys message
        if (PlayerController.hasKey) return;

        TapePlayer tp = FindAnyObjectByType<TapePlayer>();
        if (tp != null && tp.captionUI != null)
        {
            if (captionRoutine != null) StopCoroutine(captionRoutine);
            captionRoutine = StartCoroutine(ShowDeskCaption(tp.captionUI));
        }
    }

    private IEnumerator ShowDeskCaption(TextMeshProUGUI captionUI)
    {
        captionUI.gameObject.SetActive(true);
        captionUI.fontSize = 19f;
        captionUI.fontStyle = FontStyles.Normal;
        captionUI.color = Color.white;
        captionUI.text = "The cassette deck is empty... I need to find Tape 2. It must be inside the desk drawer.";

        yield return new WaitForSeconds(4.0f);
        captionUI.text = "";
        captionUI.gameObject.SetActive(false);
    }
}
