using UnityEngine;
using System.Collections;

public class EerieMusicManager : MonoBehaviour
{
    public static EerieMusicManager instance;

    [Header("Music Settings")]
    [Tooltip("Optional custom music clip. If left empty, an atmospheric horror drone will be procedurally synthesized.")]
    public AudioClip customMusicClip;

    [Range(0f, 1f)] public float normalVolume = 0.32f;
    [Range(0f, 1f)] public float duckedVolume = 0.08f;
    [Range(0f, 1f)] public float nightmareVolume = 0.42f;
    public bool playOnStart = true;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;
    private bool isNightmare = false;
    private bool isFadingOut = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        SetupAudioSource();
    }

    void Start()
    {
        if (playOnStart)
        {
            PlayMusic();
        }
    }

    public void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f; // 2D global background ambient music
        audioSource.volume = normalVolume;

        if (customMusicClip == null)
        {
            customMusicClip = Resources.Load<AudioClip>("EerieMusic");
            if (customMusicClip == null) customMusicClip = Resources.Load<AudioClip>("Audio/EerieMusic");
            if (customMusicClip == null) customMusicClip = CreateEerieAmbientMusicClip();
        }

        audioSource.clip = customMusicClip;
    }

    public void PlayMusic()
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void DuckVolume(bool duck)
    {
        if (isFadingOut || audioSource == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        float target = duck ? duckedVolume : (isNightmare ? nightmareVolume : normalVolume);
        fadeCoroutine = StartCoroutine(FadeVolume(target, 1.2f));
    }

    // Called when the room collapses into the cold, horrifying crime scene
    public void TransitionToNightmare()
    {
        if (isNightmare || isFadingOut || audioSource == null) return;
        isNightmare = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        StartCoroutine(NightmareTransitionRoutine());
    }

    private IEnumerator NightmareTransitionRoutine()
    {
        float elapsed = 0f;
        float duration = 4.0f;
        float startPitch = audioSource.pitch;
        float targetPitch = 0.72f; // Deep, slow, pitch-shifted subterranean dread
        float startVol = audioSource.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            audioSource.pitch = Mathf.Lerp(startPitch, targetPitch, t);
            audioSource.volume = Mathf.Lerp(startVol, nightmareVolume, t);
            yield return null;
        }

        audioSource.pitch = targetPitch;
        audioSource.volume = nightmareVolume;
    }

    public void FadeOut(float duration = 2.5f)
    {
        if (isFadingOut || audioSource == null) return;
        isFadingOut = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutAndStop(duration));
    }

    private IEnumerator FadeVolume(float targetVol, float duration)
    {
        float startVol = audioSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, targetVol, elapsed / duration);
            yield return null;
        }
        audioSource.volume = targetVol;
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        float startVol = audioSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();
    }

    // Procedural Dark Ambient Horror Drone Composition
    // Combines deep sub-bass pulsing, eerie diminished intervals, and ghostly high resonance
    private AudioClip CreateEerieAmbientMusicClip()
    {
        int sampleRate = 44100;
        float length = 24.0f; // 24-second seamless loop
        int totalSamples = (int)(sampleRate * length);
        float[] samples = new float[totalSamples];

        // Harmonic tension tones (D minor / G# diminished tension: G1=49Hz, D2=73.4Hz, G#2=103.8Hz, D3=146.8Hz)
        float baseFreq = 49.0f;       // Sub-bass root
        float fifthFreq = 73.42f;     // Low fifth
        float tritoneFreq = 103.83f;  // Dissonant devil's interval (Tritone dread)
        float highHarmonic = 587.33f; // Distant weeping glass overtone (D5)

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;

            // 1. Slow ominous breathing envelope (0.04 Hz = 25s cycle)
            float breath = 0.70f + 0.30f * Mathf.Sin(2f * Mathf.PI * 0.083f * t);

            // 2. Sub-bass visceral drone with subtle binaural beating (slow detune)
            float subDrone = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.45f +
                             Mathf.Sin(2f * Mathf.PI * (baseFreq + 0.35f) * t) * 0.35f;

            // 3. Low hollow pad with harmonic swell
            float fifthDrone = Mathf.Sin(2f * Mathf.PI * fifthFreq * t) * 0.25f;
            float tritoneSwell = Mathf.Sin(2f * Mathf.PI * tritoneFreq * t) * (0.18f * breath);

            // 4. Ghostly glass overtone (drifts in and out like a cold chill)
            float glassPulse = Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * 0.125f * t));
            float glassShimmer = Mathf.Sin(2f * Mathf.PI * highHarmonic * t) * (0.04f * glassPulse);

            // 5. Air pressure wind rumble (filtered noise)
            float noise = (Random.value * 2f - 1f) * 0.025f;

            // Combine layers
            float rawSample = (subDrone * 0.50f) + (fifthDrone * 0.25f) + (tritoneSwell * 0.20f) + glassShimmer + noise;

            // Apply master warm saturation curve to avoid any clipping
            samples[i] = Mathf.Clamp(rawSample, -0.95f, 0.95f);
        }

        // Apply 1.0 second crossfade at loop boundaries for 100% seamless, clickless looping
        int crossfadeSamples = (int)(sampleRate * 1.0f);
        for (int i = 0; i < crossfadeSamples; i++)
        {
            float factor = (float)i / crossfadeSamples;
            int tailIndex = totalSamples - crossfadeSamples + i;
            float blended = (samples[i] * factor) + (samples[tailIndex] * (1f - factor));
            samples[i] = blended;
            samples[tailIndex] = blended;
        }

        AudioClip clip = AudioClip.Create("EerieAmbientMusic", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
