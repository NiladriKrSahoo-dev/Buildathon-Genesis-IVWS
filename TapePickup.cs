using UnityEngine;

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
        gameObject.SetActive(false); 
    }
}