using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClip intermissionBGM;
    [SerializeField] private AudioClip combatBGM;

    [Header("Individual Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float intermissionVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float combatVolume = 1f;

    [Header("Audio Source Declarations")]
    [SerializeField] private AudioSource intermissionAudioSource;
    [SerializeField] private AudioSource combatAudioSource;

    [Header("Mixer Output (Optional)")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;

    [Header("Crossfade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    private AudioSource activeSource;
    private SpawnHandler spawnHandler;
    private bool wasInIntermission = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (intermissionAudioSource == null)
        {
            intermissionAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (combatAudioSource == null)
        {
            combatAudioSource = gameObject.AddComponent<AudioSource>();
        }

        ConfigureAudioSource(intermissionAudioSource);
        ConfigureAudioSource(combatAudioSource);

        activeSource = intermissionAudioSource;
    }

    private void ConfigureAudioSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        if (bgmMixerGroup != null)
        {
            source.outputAudioMixerGroup = bgmMixerGroup;
        }
    }

    private void Start()
    {
        FindSpawnHandler();

        if (intermissionBGM != null && intermissionAudioSource != null)
        {
            intermissionAudioSource.clip = intermissionBGM;
            intermissionAudioSource.volume = intermissionVolume;
            intermissionAudioSource.Play();
        }

        if (combatBGM != null && combatAudioSource != null)
        {
            combatAudioSource.clip = combatBGM;
            combatAudioSource.volume = 0f;
        }
    }

    private void Update()
    {
        if (spawnHandler == null)
        {
            FindSpawnHandler();
            if (spawnHandler == null) return;
        }

        bool isIntermission = spawnHandler.IsIntermission;

        if (isIntermission != wasInIntermission)
        {
            wasInIntermission = isIntermission;

            if (isIntermission)
            {
                SwitchToTrack(intermissionAudioSource, intermissionBGM, intermissionVolume);
            }
            else
            {
                SwitchToTrack(combatAudioSource, combatBGM, combatVolume);
            }
        }
    }

    private void FindSpawnHandler()
    {
        spawnHandler = FindObjectOfType<SpawnHandler>();
    }

    public void SwitchToTrack(AudioSource targetSource, AudioClip newClip, float targetVolume)
    {
        if (targetSource == null || newClip == null) return;

        AudioSource fadingOutSource = activeSource;
        activeSource = targetSource;

        if (activeSource.clip != newClip)
        {
            activeSource.clip = newClip;
        }

        if (!activeSource.isPlaying)
        {
            activeSource.Play();
        }

        StopAllCoroutines();
        StartCoroutine(CrossfadeRoutine(fadingOutSource, activeSource, targetVolume, fadeDuration));
    }

    private IEnumerator CrossfadeRoutine(AudioSource fadeOut, AudioSource fadeIn, float targetInVolume, float duration)
    {
        float timer = 0f;
        float startVolumeOut = fadeOut != null ? fadeOut.volume : 0f;
        float startVolumeIn = fadeIn != null ? fadeIn.volume : 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (fadeOut != null)
            {
                fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
            }

            if (fadeIn != null)
            {
                fadeIn.volume = Mathf.Lerp(startVolumeIn, targetInVolume, t);
            }

            yield return null;
        }

        if (fadeOut != null)
        {
            fadeOut.volume = 0f;
            fadeOut.Stop();
        }

        if (fadeIn != null)
        {
            fadeIn.volume = targetInVolume;
        }
    }
}