using UnityEngine;
using System.Collections;

public class EerieMusicManager : MonoBehaviour
{
    public static EerieMusicManager instance;

    [Header("Music Settings")]
    [Tooltip("Optional custom music clip. If left empty, an atmospheric horror drone will be procedurally synthesized.")]
    public AudioClip customMusicClip;

    [Range(0f, 1f)] public float normalVolume = 0.75f;
    [Range(0f, 1f)] public float duckedVolume = 0.25f;
    [Range(0f, 1f)] public float nightmareVolume = 0.88f;
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

        // Auto-upgrade volumes if upgrading from previous lower defaults
        if (normalVolume < 0.70f) normalVolume = 0.75f;
        if (duckedVolume < 0.20f) duckedVolume = 0.25f;
        if (nightmareVolume < 0.80f) nightmareVolume = 0.88f;

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
            audioSource.volume = normalVolume;
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
    // Carefully EQ'd with rich low-mid harmonics so it translates loudly and powerfully on laptop speakers and headphones
    private AudioClip CreateEerieAmbientMusicClip()
    {
        int sampleRate = 44100;
        float length = 24.0f; // 24-second seamless loop
        int totalSamples = (int)(sampleRate * length);
        float[] samples = new float[totalSamples];

        // Fundamental horror frequencies:
        // D minor / G# diminished tension with rich mid-range harmonics for speaker clarity
        float bassSub = 58.27f;       // Bb1 sub-bass
        float bassFundamental = 77.78f; // Eb2 - warm mid-bass audible on laptop speakers
        float fifthFreq = 116.54f;    // Bb2 - cello-like body
        float tritoneFreq = 164.81f;  // E3 - ominous devil's tritone dissonance
        float upperPad = 233.08f;     // Bb3 - cold cinematic string pad
        float highHarmonic = 659.25f; // E5 - distant ghostly glass overtone

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;

            // 1. Slow, hypnotic respiratory swelling (0.07 Hz = 14s swell cycle)
            float breath = 0.65f + 0.35f * Mathf.Sin(2f * Mathf.PI * 0.071f * t);
            float secondaryBreath = 0.75f + 0.25f * Mathf.Cos(2f * Mathf.PI * 0.042f * t);

            // 2. Heavy bass drone (sub + speaker-audible fundamental with binaural detune)
            float subDrone = Mathf.Sin(2f * Mathf.PI * bassSub * t) * 0.40f;
            float midBassDrone = (Mathf.Sin(2f * Mathf.PI * bassFundamental * t) + 
                                  Mathf.Sin(2f * Mathf.PI * (bassFundamental + 0.45f) * t)) * 0.50f;

            // 3. Haunting harmonic chord pads (swelling dissonant tension)
            float fifthDrone = Mathf.Sin(2f * Mathf.PI * fifthFreq * t) * 0.40f;
            float tritoneSwell = Mathf.Sin(2f * Mathf.PI * tritoneFreq * t) * (0.35f * breath);
            float stringPad = Mathf.Sin(2f * Mathf.PI * upperPad * t) * (0.25f * secondaryBreath);

            // 4. Ghostly glass overtone (drifts in and out like cold breathing)
            float glassPulse = Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * 0.125f * t));
            float glassShimmer = Mathf.Sin(2f * Mathf.PI * highHarmonic * t) * (0.12f * glassPulse);

            // 5. Ominous air texture
            float noise = (Random.value * 2f - 1f) * 0.035f;

            // Sum layers with rich musical balance
            float raw = (subDrone * 0.35f) + 
                        (midBassDrone * 0.45f) + 
                        (fifthDrone * 0.35f) + 
                        (tritoneSwell * 0.35f) + 
                        (stringPad * 0.25f) + 
                        glassShimmer + noise;

            // Apply soft analog-style saturation
            samples[i] = Mathf.Clamp(raw, -1.0f, 1.0f);
        }

        // Seamless loop crossfade (1.2s at both boundaries)
        int crossfadeSamples = (int)(sampleRate * 1.2f);
        for (int i = 0; i < crossfadeSamples; i++)
        {
            float factor = (float)i / crossfadeSamples;
            int tailIndex = totalSamples - crossfadeSamples + i;
            float blended = (samples[i] * factor) + (samples[tailIndex] * (1f - factor));
            samples[i] = blended;
            samples[tailIndex] = blended;
        }

        // Audio Normalization: Boost waveform to 92% full scale
        float maxAmp = 0.001f;
        for (int i = 0; i < totalSamples; i++)
        {
            float abs = Mathf.Abs(samples[i]);
            if (abs > maxAmp) maxAmp = abs;
        }
        float boost = 0.92f / maxAmp;
        for (int i = 0; i < totalSamples; i++)
        {
            samples[i] = Mathf.Clamp(samples[i] * boost, -0.98f, 0.98f);
        }

        AudioClip clip = AudioClip.Create("EerieAmbientMusic", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
