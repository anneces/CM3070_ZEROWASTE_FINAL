using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1.0f;

    [Header("UI & Phase SFX")]
    public AudioClip uiclickbtn;
    public AudioClip threedbutton_press_cut;
    public AudioClip alert;
    public AudioClip phasetransition;

    [Header("Interaction & Environment SFX")]
    public AudioClip grab_drop_item;
    public AudioClip door_open_close;
    public AudioClip trashcan;
    public AudioClip purchase_cut;
    public AudioClip spoiled_food_cut;

    [Header("Cooking SFX")]
    public AudioClip sizzle_cooking;
    public AudioClip poof_cloud;

    [Header("Game Summary SFX")]
    public AudioClip mission_gradeA;
    public AudioClip mission_gradeB_C;
    public AudioClip mission_gradeF;

    [Header("Background Music Tracks")]
    public AudioClip mainmenutheme;
    public AudioClip maingametheme;

    [Header("Scene Names (Must Match Build Settings)")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainGameSceneName = "MainGame";

    // --- ALIAS PROPERTIES FOR BACKWARD COMPATIBILITY ---
    public AudioClip uiClickClip => uiclickbtn;
    public AudioClip ingredientDropClip => grab_drop_item;

    public float BGMVolume => bgmVolume;

    // Private internal AudioSource created dynamically to prevent inspector conflicts
    private AudioSource runtimeSizzleSource;

    private void Awake()
    {
        // Singleton Setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure primary AudioSources exist
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();

        // Dynamically create dedicated loop source for sizzle
        runtimeSizzleSource = gameObject.AddComponent<AudioSource>();
        runtimeSizzleSource.loop = true;

        bgmSource.loop = true;

        ApplyVolumes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName)
        {
            PlayMainMenuTheme();
        }
        else if (scene.name == mainGameSceneName)
        {
            PlayMainGameTheme();
        }
    }

    private void ApplyVolumes()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (runtimeSizzleSource != null) runtimeSizzleSource.volume = sfxVolume;
    }

    #region Volume Control Functions

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        if (runtimeSizzleSource != null)
        {
            runtimeSizzleSource.volume = sfxVolume;
        }
    }

    #endregion

    #region Public Play Helper Functions

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            Debug.LogWarning("[AudioManager] SFX clip or sfxSource is missing!");
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void StartCookingSizzle()
    {
        if (sizzle_cooking != null && runtimeSizzleSource != null)
        {
            if (!runtimeSizzleSource.isPlaying)
            {
                runtimeSizzleSource.clip = sizzle_cooking;
                runtimeSizzleSource.volume = sfxVolume;
                runtimeSizzleSource.Play();
            }
        }
    }

    public void StopCookingSizzle()
    {
        if (runtimeSizzleSource != null && runtimeSizzleSource.isPlaying)
        {
            runtimeSizzleSource.Stop();
        }
    }

    #endregion

    #region Direct Call Shortcuts

    public void PlayUIClick() => PlaySFX(uiclickbtn);
    public void Play3DButtonPressed() => PlaySFX(threedbutton_press_cut);
    public void PlayAlert() => PlaySFX(alert);
    public void PlayPhaseTransition() => PlaySFX(phasetransition);

    public void PlayGrabDrop() => PlaySFX(grab_drop_item);
    public void PlayGrabDropItem() => PlaySFX(grab_drop_item);

    public void PlayDoorOpenClose() => PlaySFX(door_open_close);
    public void PlayTrashCan() => PlaySFX(trashcan);
    public void PlayPurchase() => PlaySFX(purchase_cut);
    public void PlaySpoiledFood() => PlaySFX(spoiled_food_cut);
    public void PlayCookingSizzle() => StartCookingSizzle();
    public void PlayPoofCloud() => PlaySFX(poof_cloud);

    public void PlayGradeA() => PlaySFX(mission_gradeA);
    public void PlayGradeBC() => PlaySFX(mission_gradeB_C);
    public void PlayGradeF() => PlaySFX(mission_gradeF);

    public void PlayMainMenuTheme() => PlayBGM(mainmenutheme);
    public void PlayMainGameTheme() => PlayBGM(maingametheme);

    #endregion
}