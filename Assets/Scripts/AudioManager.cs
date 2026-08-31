using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;

    private AudioClip activateClip;
    private AudioClip wrongClip;
    private AudioClip winClip;
    private AudioClip loseClip;
    private AudioClip hazardClip;
    private AudioClip clickClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        GenerateClips();
    }

    private void GenerateClips()
    {
        activateClip = CreateToneClip("Activate", new float[] { 523.25f, 659.25f, 783.99f }, 0.25f);
        wrongClip = CreateNoiseBuzzer("Wrong", 140f, 0.35f);
        winClip = CreateToneClip("Win", new float[] { 440f, 554.37f, 659.25f, 880f }, 0.6f);
        loseClip = CreateToneClip("Lose", new float[] { 400f, 350f, 300f, 220f }, 0.6f);
        hazardClip = CreateZapClip("Hazard", 0.2f);
        clickClip = CreateToneClip("Click", new float[] { 800f }, 0.05f);
    }

    public void PlayActivate() => PlayClip(activateClip, 0.8f);
    public void PlayWrong() => PlayClip(wrongClip, 0.9f);
    public void PlayWin() => PlayClip(winClip, 1.0f);
    public void PlayLose() => PlayClip(loseClip, 1.0f);
    public void PlayHazard() => PlayClip(hazardClip, 0.85f);
    public void PlayClick() => PlayClip(clickClip, 0.5f);

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    private AudioClip CreateToneClip(string clipName, float[] freqs, float duration)
    {
        int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * duration);
        float[] samples = new float[totalSamples];

        int noteLength = totalSamples / freqs.Length;
        for (int i = 0; i < totalSamples; i++)
        {
            int noteIndex = Mathf.Min(i / noteLength, freqs.Length - 1);
            float freq = freqs[noteIndex];
            float t = (float)i / sampleRate;
            float env = 1f - ((float)(i % noteLength) / noteLength);
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.4f;
        }

        AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateNoiseBuzzer(string clipName, float freq, float duration)
    {
        int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * duration);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float sq = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
            float env = 1f - ((float)i / totalSamples);
            samples[i] = sq * env * 0.35f;
        }

        AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateZapClip(string clipName, float duration)
    {
        int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * duration);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float currentFreq = Mathf.Lerp(900f, 150f, (float)i / totalSamples);
            float env = 1f - ((float)i / totalSamples);
            samples[i] = Mathf.Sin(2f * Mathf.PI * currentFreq * t) * env * 0.4f;
        }

        AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
