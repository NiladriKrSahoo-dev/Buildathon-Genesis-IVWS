using UnityEngine;

public class Door : Interactable
{
    public RoomStateManager roomManager;

    public override void Interact()
    {
        base.Interact();
        if (roomManager != null)
        {
            roomManager.SwitchLayer();
        }
    }
}