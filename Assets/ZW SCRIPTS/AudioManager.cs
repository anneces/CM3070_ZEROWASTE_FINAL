using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sizzleSource; // Dedicated AudioSource for looping sizzle SFX

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

        // Ensure AudioSources exist
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        if (sizzleSource == null) sizzleSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        sizzleSource.loop = true;
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
        // Automatically plays the corresponding track when the scene loads
        if (scene.name == mainMenuSceneName)
        {
            PlayMainMenuTheme();
        }
        else if (scene.name == mainGameSceneName)
        {
            PlayMainGameTheme();
        }
    }

    #region Public Play Helper Functions

    /// <summary>
    /// Plays a one-shot SFX clip.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("[AudioManager] SFX clip or sfxSource is missing!");
        }
    }

    /// <summary>
    /// Plays background music seamlessly.
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    /// <summary>
    /// Stops current BGM track.
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// Starts looping sizzling SFX continuously while food is cooking.
    /// </summary>
    public void StartCookingSizzle()
    {
        if (sizzle_cooking != null && sizzleSource != null)
        {
            if (!sizzleSource.isPlaying)
            {
                sizzleSource.clip = sizzle_cooking;
                sizzleSource.Play();
            }
        }
    }

    /// <summary>
    /// Stops looping sizzling SFX when food is done or removed from the pan.
    /// </summary>
    public void StopCookingSizzle()
    {
        if (sizzleSource != null && sizzleSource.isPlaying)
        {
            sizzleSource.Stop();
        }
    }

    #endregion

    #region Direct Call Shortcuts

    public void PlayUIClick() => PlaySFX(uiclickbtn);
    public void Play3DButtonPressed() => PlaySFX(threedbutton_press_cut);
    public void PlayAlert() => PlaySFX(alert);
    public void PlayPhaseTransition() => PlaySFX(phasetransition);

    // Added PlayGrabDrop alias for Option B compatibility
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