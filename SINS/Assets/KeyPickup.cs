using UnityEngine;

public class KeyPickup : Interactable
{
    public override void Interact()
    {
        base.Interact();
        PlayerController.hasKey = true;
        if (GameManager.instance != null)
        {
            GameManager.instance.ShowKeyInInventory();
            GameManager.instance.SetObjective("Use the key to unlock the door.");
        }
        gameObject.SetActive(false); 
    }
}