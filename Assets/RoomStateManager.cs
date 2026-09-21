using UnityEngine;

public class RoomStateManager : MonoBehaviour
{
    public enum Layer { Present, Past }
    public Layer currentLayer = Layer.Present;

    public GameObject lampBroken;
    public GameObject lampIntact;
    public GDTClock wallClock;

    private bool lampWasFixed = false;

    void Start()
    {
        UpdateRoom();
    }

    public void SwitchLayer()
    {
        currentLayer = (currentLayer == Layer.Present) ? Layer.Past : Layer.Present;
        UpdateRoom();
    }

    public void FixLampInPast()
    {
        lampWasFixed = true;
        UpdateRoom();
    }

    void UpdateRoom()
    {
        if (currentLayer == Layer.Present)
        {
            if (wallClock != null) wallClock.SetTime(10, 50);
            if (lampBroken != null) lampBroken.SetActive(!lampWasFixed);
            if (lampIntact != null) lampIntact.SetActive(lampWasFixed);
        }
        else
        {
            if (wallClock != null) wallClock.SetTime(10, 40);
            if (lampBroken != null) lampBroken.SetActive(false);
            if (lampIntact != null) lampIntact.SetActive(true);
        }
    }
}