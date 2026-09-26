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

    // Procedural Melodic Horror Composition:
    // Pure musical notes (haunting music box & dark piano) over shifting cello chord pads, with ZERO static hiss or noise.
    private AudioClip CreateEerieAmbientMusicClip()
    {
        int sampleRate = 44100;
        float length = 24.0f; // 24-second seamless musical loop
        int totalSamples = (int)(sampleRate * length);
        float[] samples = new float[totalSamples];

        // 16-note haunting psychological horror melody (C minor / G harmonic minor)
        // Evokes deep mystery, sadness, and dread
        float[] melodyNotes = new float[] {
            261.63f, // C4
            311.13f, // Eb4
            392.00f, // G4
            523.25f, // C5 (high chilling bell)
            493.88f, // B4 (eerie harmonic minor leading tone)
            415.30f, // Ab4 (sorrowful drop)
            392.00f, // G4 (restless hold)
            311.13f, // Eb4
            349.23f, // F4
            415.30f, // Ab4
            392.00f, // G4
            311.13f, // Eb4
            293.66f, // D4
            349.23f, // F4
            246.94f, // B3 (dark suspense)
            261.63f  // C4 (resolves into loop start)
        };

        float noteDuration = length / melodyNotes.Length; // 1.5 seconds per musical note

        // Chord progression bass roots (C minor -> Ab major -> F minor -> G suspended)
        float[] chordRoots = new float[] { 65.41f, 51.91f, 43.65f, 49.00f }; // C2, Ab1, F1, G1
        float[] chordFifths = new float[] { 98.00f, 77.78f, 65.41f, 73.42f }; // G2, Eb2, C2, D2
        float[] chordTertiaries = new float[] { 155.56f, 130.81f, 103.83f, 123.47f }; // Eb3, C3, Ab2, B2
        float barDuration = length / chordRoots.Length; // 6.0 seconds per chord

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;

            // --- 1. MELODIC MUSIC NOTES (Music Box / Dark Piano) ---
            int noteIndex = (int)(t / noteDuration) % melodyNotes.Length;
            float noteT = t % noteDuration;
            float noteFreq = melodyNotes[noteIndex];

            // Natural acoustic bell/piano envelope: fast smooth attack (20ms), gentle exponential decay
            float noteAttack = Mathf.Clamp01(noteT / 0.025f);
            float noteDecay = Mathf.Exp(-2.1f * noteT);
            float noteEnv = noteAttack * noteDecay;

            // Rich musical harmonics (fundamental + 2nd + 3rd + high glass glint)
            float primaryNote = (Mathf.Sin(2f * Mathf.PI * noteFreq * noteT) * 0.70f) +
                                (Mathf.Sin(2f * Mathf.PI * (noteFreq * 2f) * noteT) * 0.25f) +
                                (Mathf.Sin(2f * Mathf.PI * (noteFreq * 3f) * noteT) * 0.12f) +
                                (Mathf.Sin(2f * Mathf.PI * (noteFreq * 4.01f) * noteT) * 0.06f);

            float melodicSignal = primaryNote * noteEnv;

            // Acoustic Room Echo (simulated 320ms delay on melody notes for lush room reverb)
            if (noteT > 0.32f)
            {
                float echoT = noteT - 0.32f;
                float echoEnv = Mathf.Exp(-2.5f * echoT) * 0.28f;
                melodicSignal += (Mathf.Sin(2f * Mathf.PI * noteFreq * echoT) * 0.6f +
                                  Mathf.Sin(2f * Mathf.PI * (noteFreq * 2f) * echoT) * 0.2f) * echoEnv;
            }

            // --- 2. WARM CELLO & ORGAN CHORD PADS (Pure tones, zero static) ---
            int barIndex = (int)(t / barDuration) % chordRoots.Length;
            int nextBarIndex = (barIndex + 1) % chordRoots.Length;
            float barT = (t % barDuration) / barDuration;
            float crossBlend = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((barT - 0.75f) / 0.25f)); // Smooth 1.5s crossfade between chords

            float rootF = Mathf.Lerp(chordRoots[barIndex], chordRoots[nextBarIndex], crossBlend);
            float fifthF = Mathf.Lerp(chordFifths[barIndex], chordFifths[nextBarIndex], crossBlend);
            float tertF = Mathf.Lerp(chordTertiaries[barIndex], chordTertiaries[nextBarIndex], crossBlend);

            // Slow respiratory breathing swell
            float padSwell = 0.70f + 0.30f * Mathf.Sin(2f * Mathf.PI * (1f / barDuration) * t);

            float padRoot = (Mathf.Sin(2f * Mathf.PI * rootF * t) + Mathf.Sin(2f * Mathf.PI * (rootF + 0.35f) * t)) * 0.5f;
            float padFifth = Mathf.Sin(2f * Mathf.PI * fifthF * t);
            float padTert = Mathf.Sin(2f * Mathf.PI * tertF * t);

            float padSignal = ((padRoot * 0.45f) + (padFifth * 0.35f) + (padTert * 0.25f)) * padSwell;

            // --- 3. COMBINE LAYERS (100% PURE MUSICAL SOUND, NO NOISE) ---
            float rawSample = (melodicSignal * 0.65f) + (padSignal * 0.35f);

            // Soft musical saturation
            samples[i] = Mathf.Clamp(rawSample, -1.0f, 1.0f);
        }

        // Apply 1.2 second crossfade at loop boundaries for 100% seamless, clickless looping
        int crossfadeSamples = (int)(sampleRate * 1.2f);
        for (int i = 0; i < crossfadeSamples; i++)
        {
            float factor = (float)i / crossfadeSamples;
            int tailIndex = totalSamples - crossfadeSamples + i;
            float blended = (samples[i] * factor) + (samples[tailIndex] * (1f - factor));
            samples[i] = blended;
            samples[tailIndex] = blended;
        }

        // Normalize audio to 94% peak headroom for crystal-clear, loud, pristine playback
        float maxAmp = 0.001f;
        for (int i = 0; i < totalSamples; i++)
        {
            float abs = Mathf.Abs(samples[i]);
            if (abs > maxAmp) maxAmp = abs;
        }
        float boost = 0.94f / maxAmp;
        for (int i = 0; i < totalSamples; i++)
        {
            samples[i] = Mathf.Clamp(samples[i] * boost, -0.98f, 0.98f);
        }

        AudioClip clip = AudioClip.Create("EerieMelodicHorrorTheme", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
