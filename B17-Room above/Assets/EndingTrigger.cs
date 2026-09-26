using UnityEngine;

public class EndingTrigger : Interactable
{
    [Header("Ending Setup")]
    public RoomStateManager roomManager;
    public GameObject objectiveTextUI; 

    public override void Interact()
    {
        base.Interact();
        
        if (roomManager != null)
        {
            // This now perfectly matches the new flash sequence in RoomStateManager
            roomManager.TriggerEndingSequence();
        }

        if (objectiveTextUI != null)
        {
            objectiveTextUI.SetActive(false);
        }
    }
}