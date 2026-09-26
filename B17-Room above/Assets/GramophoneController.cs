using UnityEngine;
using System.Collections;

public class GramophoneController : MonoBehaviour
{
    public static GramophoneController instance;

    [Header("Audio Settings")]
    public AudioClip gramophoneMusic;
    [Range(0f, 1f)] public float normalVolume = 0.40f;
    [Range(0f, 1f)] public float duckedVolume = 0.08f;

    private AudioSource audioSource;
    private Coroutine duckCoroutine;
    private bool isWindingDown = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SetupAudioSource();
        PlayGramophone();
    }

    public void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 1.0f; // Full 3D spatial audio
        audioSource.minDistance = 2.0f;
        audioSource.maxDistance = 16.0f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.volume = normalVolume;

        if (gramophoneMusic == null)
        {
            gramophoneMusic = Resources.Load<AudioClip>("GramophoneMusic");
            if (gramophoneMusic == null) gramophoneMusic = Resources.Load<AudioClip>("Audio/GramophoneMusic");
            if (gramophoneMusic == null) gramophoneMusic = Resources.Load<AudioClip>("Music/GramophoneMusic");
            if (gramophoneMusic == null) gramophoneMusic = CreateVintageGramophoneClip();
        }
        audioSource.clip = gramophoneMusic;
    }

    public void PlayGramophone()
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopGramophone()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(WindDownAndStop());
        }
    }

    public void DuckVolume(bool duck)
    {
        if (isWindingDown || audioSource == null) return;
        if (duckCoroutine != null) StopCoroutine(duckCoroutine);
        duckCoroutine = StartCoroutine(FadeVolume(duck ? duckedVolume : normalVolume, 1.2f));
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

    private IEnumerator WindDownAndStop()
    {
        isWindingDown = true;
        float startPitch = audioSource.pitch;
        float startVol = audioSource.volume;
        float elapsed = 0f;
        float duration = 2.5f;

        // Slow down the needle like a vintage phonograph grinding to a halt
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            audioSource.pitch = Mathf.Lerp(startPitch, 0.25f, t);
            audioSource.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }

        audioSource.Stop();
        audioSource.pitch = 1f;
    }

    // Procedural vintage 1920s music box melody with needle vinyl crackle
    private AudioClip CreateVintageGramophoneClip()
    {
        int sampleRate = 44100;
        float length = 16f; // 16-second looping vintage melody
        int totalSamples = (int)(sampleRate * length);
        float[] samples = new float[totalSamples];

        // Minor key vintage melody (A minor: A3, C4, E4, B3, G#3, A3)
        float[] notes = new float[] { 220.0f, 261.63f, 329.63f, 246.94f, 207.65f, 220.0f, 293.66f, 261.63f };
        float noteDuration = 2.0f;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = (int)(t / noteDuration) % notes.Length;
            float noteFreq = notes[noteIndex];
            float noteT = t % noteDuration;

            // Bell / music box harmonic envelope
            float env = Mathf.Exp(-3.2f * noteT);
            float tone = (Mathf.Sin(2f * Mathf.PI * noteFreq * noteT) * 0.6f) +
                         (Mathf.Sin(2f * Mathf.PI * (noteFreq * 2f) * noteT) * 0.25f) +
                         (Mathf.Sin(2f * Mathf.PI * (noteFreq * 3f) * noteT) * 0.15f);

            // Clean vintage bell / music box tone (no static hiss or clicks)
            samples[i] = (tone * env * 0.50f);
        }

        AudioClip clip = AudioClip.Create("VintageGramophoneMelody", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
