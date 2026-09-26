using UnityEngine;

public class TickingClock : MonoBehaviour
{
    public Transform hourHand;
    public Transform minuteHand;
    
    [Header("Time Jump Audio")]
    public AudioSource audioSource;
    public AudioClip windingSound;
    
    public int hour = 10;
    public int minute = 0;
    
    private float timer = 0f;
    private bool isSpinning = false;
    private int targetHour = 11;

    void Awake()
    {
        // If modern ClockController is active on this clock, disable this script to avoid fighting hand rotations
        if (GetComponent<ClockController>() != null)
        {
            enabled = false;
            return;
        }
    }

    void Start()
    {
        if (GetComponent<ClockController>() != null)
        {
            enabled = false;
            return;
        }

        // Automatically grabs the AudioSource if you forget to link it
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // If spinning, time moves 100x faster
        float speed = isSpinning ? 100f : 1f;
        timer += Time.deltaTime * speed;

        if (timer >= 1f) 
        {
            timer = 0f;
            minute++;
            
            if (minute >= 60) 
            { 
                minute = 0; 
                hour++; 
                if (hour >= 24) hour = 0;
                
                // Stop the spin exactly when we hit the target hour
                if (isSpinning && hour == targetHour) 
                {
                    isSpinning = false;
                    if (audioSource != null) audioSource.Stop(); // Stop the winding sound
                }
            }
            
            float hourAngle = (hour % 12) * 30f + (minute * 0.5f);
            float minuteAngle = minute * 6f;
            
            // Removed the minus signs here so the hands spin clockwise
            if (hourHand != null) hourHand.localRotation = Quaternion.Euler(0, 0, hourAngle);
            if (minuteHand != null) minuteHand.localRotation = Quaternion.Euler(0, 0, minuteAngle);
        }
    }
    
    public void FastForwardTo(int target)
    {
        targetHour = target;
        isSpinning = true;
        
        // Play the scary winding sound
        if (audioSource != null && windingSound != null)
        {
            audioSource.clip = windingSound;
            audioSource.Play();
        }
    }
}