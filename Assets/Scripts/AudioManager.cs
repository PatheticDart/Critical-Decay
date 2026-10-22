using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer Reference")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Mixer Parameter Names")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [SerializeField] private string bgmParam = "BGMVolume";

    [Header("UI 2D SFX Centralized Players")]
    [SerializeField] private AudioSource uiAudioSourcePrimary;
    [SerializeField] private AudioSource uiAudioSourceSecondary;

    [Header("UI Click Sound Effects")]
    [SerializeField] private AudioClip defaultButtonClickSFX;
    [SerializeField] private AudioClip secondaryButtonClickSFX;

    [Header("UI Sliders (Optional Assignment per Scene)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    private const string MasterKey = "MasterVolume_Key";
    private const string SFXKey = "SFXVolume_Key";
    private const string BGMKey = "BGMVolume_Key";

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

        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        if (uiAudioSourcePrimary == null)
        {
            uiAudioSourcePrimary = gameObject.AddComponent<AudioSource>();
        }
        uiAudioSourcePrimary.spatialBlend = 0f;

        if (uiAudioSourceSecondary == null)
        {
            uiAudioSourceSecondary = gameObject.AddComponent<AudioSource>();
        }
        uiAudioSourceSecondary.spatialBlend = 0f;
    }

    private void Start()
    {
        LoadAndApplySavedVolumes();
        BindSlidersIfAvailable();
    }

    public void PlayUIClick(int soundIndex = 1, AudioClip customClip = null)
    {
        AudioClip clipToPlay = customClip;
        AudioSource targetSource = uiAudioSourcePrimary;

        if (clipToPlay == null)
        {
            if (soundIndex == 2)
            {
                clipToPlay = secondaryButtonClickSFX;
                targetSource = uiAudioSourceSecondary != null ? uiAudioSourceSecondary : uiAudioSourcePrimary;
            }
            else
            {
                clipToPlay = defaultButtonClickSFX;
                targetSource = uiAudioSourcePrimary;
            }
        }

        if (clipToPlay != null && targetSource != null)
        {
            targetSource.PlayOneShot(clipToPlay);
        }
    }

    public void LoadAndApplySavedVolumes()
    {
        float masterVal = PlayerPrefs.GetFloat(MasterKey, 0.75f);
        float sfxVal = PlayerPrefs.GetFloat(SFXKey, 0.75f);
        float bgmVal = PlayerPrefs.GetFloat(BGMKey, 0.75f);

        SetMasterVolume(masterVal);
        SetSFXVolume(sfxVal);
        SetBGMVolume(bgmVal);
    }

    public void BindSliders(Slider master, Slider sfx, Slider bgm)
    {
        masterSlider = master;
        sfxSlider = sfx;
        bgmSlider = bgm;

        BindSlidersIfAvailable();
    }

    private void BindSlidersIfAvailable()
    {
        if (masterSlider != null)
        {
            masterSlider.value = PlayerPrefs.GetFloat(MasterKey, 0.75f);
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat(SFXKey, 0.75f);
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = PlayerPrefs.GetFloat(BGMKey, 0.75f);
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        ApplyVolumeToMixer(masterParam, value);
        PlayerPrefs.SetFloat(MasterKey, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        ApplyVolumeToMixer(sfxParam, value);
        PlayerPrefs.SetFloat(SFXKey, value);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        ApplyVolumeToMixer(bgmParam, value);
        PlayerPrefs.SetFloat(BGMKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyVolumeToMixer(string parameterName, float linearValue)
    {
        if (mainMixer == null) return;

        float decibels = Mathf.Log10(linearValue) * 20f;
        mainMixer.SetFloat(parameterName, decibels);
    }
}