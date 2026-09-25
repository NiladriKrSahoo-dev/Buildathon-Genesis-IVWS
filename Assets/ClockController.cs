using UnityEngine;
using System.Collections;

public class ClockController : MonoBehaviour
{
    [Header("Clock Hands")]
    public Transform minuteHand;
    public Transform hourHand;
    [Tooltip("Base rotation axis for the hands (usually Vector3.forward)")]
    public Vector3 rotationAxis = Vector3.forward;

    [Header("Direction")]
    [Tooltip("Checked = Clockwise / Forward (12 -> 3 -> 6 -> 9). Uncheck if you want counter-clockwise.")]
    public bool spinClockwise = true;
    
    [Header("Normal Time Settings")]
    public float timeSpeed = 30f; // How fast the hands spin normally
    public bool isTicking = true;

    [Header("Time Shift Settings (The Catalyst)")]
    [Tooltip("How fast the clock hands spin violently out of control.")]
    public float crazySpinSpeed = 1800f; 
    public float spinDuration = 8f;

    private bool isTimeShifting = false;

    void Start()
    {
        // Auto-find hand transforms among children if not assigned in Inspector
        if (minuteHand == null)
        {
            Transform found = transform.Find("Minute-Hand");
            if (found != null) minuteHand = found;
        }
        if (hourHand == null)
        {
            Transform found = transform.Find("Hour-Hand");
            if (found != null) hourHand = found;
        }
    }

    // In Unity's coordinate system, rotating clockwise around +Z requires a negative angle (or Vector3.back)
    private Vector3 GetEffectiveAxis()
    {
        Vector3 baseAxis = (rotationAxis != Vector3.zero) ? rotationAxis.normalized : Vector3.forward;
        return spinClockwise ? -baseAxis : baseAxis;
    }

    void Update()
    {
        // Normal ticking behavior
        if (isTicking && !isTimeShifting)
        {
            Vector3 axis = GetEffectiveAxis();
            if (minuteHand != null) minuteHand.Rotate(axis, timeSpeed * Time.deltaTime, Space.Self);
            // Hour hand moves 12 times slower than the minute hand
            if (hourHand != null) hourHand.Rotate(axis, (timeSpeed / 12f) * Time.deltaTime, Space.Self);
        }
    }

    // Step 1: The Catalyst - Called by TapePlayer.cs on Tape 2 climax
    public void TriggerTimeShift()
    {
        if (!isTimeShifting)
        {
            isTimeShifting = true;
            Debug.Log("ClockController: Time Shift activated! Clock hands spinning out of control.");
            StartCoroutine(SpinOutControl());
        }
    }

    // The animation that spins the clock wildly for spinDuration
    IEnumerator SpinOutControl()
    {
        float elapsed = 0f;
        Vector3 axis = GetEffectiveAxis();
        
        while (elapsed < spinDuration)
        {
            if (minuteHand != null) minuteHand.Rotate(axis, crazySpinSpeed * Time.deltaTime, Space.Self);
            if (hourHand != null) hourHand.Rotate(axis, (crazySpinSpeed / 12f) * Time.deltaTime, Space.Self);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}