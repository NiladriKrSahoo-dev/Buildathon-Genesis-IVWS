using UnityEngine;

public class ClockController : MonoBehaviour
{
    public Transform hourHand;
    public Transform minuteHand;

    public void SetTime(int hour, int minute)
    {
        float hourAngle = (hour % 12) * 30f + (minute * 0.5f);
        float minuteAngle = minute * 6f;

        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0, 0, -hourAngle);
        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0, 0, -minuteAngle);
    }
}