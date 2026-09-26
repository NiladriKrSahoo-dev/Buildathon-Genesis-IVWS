using UnityEngine;
using System.Collections;
using TMPro;

public class EvidenceItem : Interactable
{
    [Header("Clue Details")]
    public string clueId;
    public string promptActionText = "EXAMINE EVIDENCE";
    [TextArea(2, 4)]
    public string discoverySubtitle;

    [Header("Inspection Audio")]
    public AudioClip inspectSound;
    public AudioClip voiceoverAudio;

    private bool hasBeenExamined = false;
    private Coroutine captionCoroutine;
    private TextMeshProUGUI captionUI;

    public override void Interact()
    {
        base.Interact();

        if (!hasBeenExamined)
        {
            hasBeenExamined = true;
            EvidenceManager.guiltScore++;
            Debug.Log($"EvidenceItem: Examined '{clueId}'. Current Guilt Score: {EvidenceManager.guiltScore}");
        }

        PlayInspectAudio();
        ShowSubtitle(discoverySubtitle);
    }

    private void PlayInspectAudio()
    {
        AudioSource src = GetComponent<AudioSource>();
        if (src == null)
        {
            src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 0f;
            src.playOnAwake = false;
        }

        if (inspectSound != null)
        {
            src.PlayOneShot(inspectSound, 0.5f);
        }
        else
        {
            AudioClip chime = CreateDiscoveryChime();
            src.PlayOneShot(chime, 0.35f);
        }
    }

    private static AudioClip cachedChime;
    private AudioClip CreateDiscoveryChime()
    {
        if (cachedChime != null) return cachedChime;

        int sampleRate = 44100;
        float duration = 0.6f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-5.5f * t); // smooth decay
            // Dual frequency chime (E5: 659.25Hz, B5: 987.77Hz)
            float wave = (Mathf.Sin(2f * Mathf.PI * 659.25f * t) * 0.6f) +
                         (Mathf.Sin(2f * Mathf.PI * 987.77f * t) * 0.4f);
            samples[i] = wave * envelope * 0.25f;
        }

        cachedChime = AudioClip.Create("DiscoveryChime", sampleCount, 1, sampleRate, false);
        cachedChime.SetData(samples, 0);
        return cachedChime;
    }

    private void ShowSubtitle(string text)
    {
        if (captionUI == null)
        {
            TapePlayer tp = FindAnyObjectByType<TapePlayer>();
            if (tp != null && tp.captionUI != null)
            {
                captionUI = tp.captionUI;
            }
            else
            {
                TextMeshProUGUI[] all = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
                foreach (var t in all)
                {
                    string n = t.gameObject.name.ToLower();
                    if (n.Contains("caption") || n.Contains("subtitle"))
                    {
                        captionUI = t;
                        break;
                    }
                }
            }
        }

        if (captionUI != null)
        {
            Canvas parentCanvas = captionUI.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null && !parentCanvas.gameObject.activeSelf) parentCanvas.gameObject.SetActive(true);

            if (captionCoroutine != null) StopCoroutine(captionCoroutine);
            captionCoroutine = StartCoroutine(SubtitleRoutine(text));
        }
    }

    private IEnumerator SubtitleRoutine(string text)
    {
        captionUI.gameObject.SetActive(true);
        captionUI.fontSize = 19f;
        captionUI.fontStyle = FontStyles.Normal;
        captionUI.color = Color.white;
        captionUI.text = text;

        float displayTime = 5.5f;
        AudioClip vo = voiceoverAudio;
        if (vo == null)
        {
            if (clueId == "whiskey_bottle") vo = Resources.Load<AudioClip>("Voiceovers/VO_Whiskey");
            else if (clueId == "torn_photo") vo = Resources.Load<AudioClip>("Voiceovers/VO_TornPhoto");
            else if (clueId == "diary_page") vo = Resources.Load<AudioClip>("Voiceovers/VO_Diary");
        }
        if (vo != null) displayTime = Mathf.Max(vo.length + 0.3f, 4.0f);

        yield return new WaitForSeconds(displayTime);

        captionUI.text = "";
        captionUI.gameObject.SetActive(false);
    }
}
