using UnityEngine;

 
public class MusicManager : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS & PROPERTIES
    // ==========================================
    public static MusicManager Instance { get; private set; }

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float backgroundVolume = 0.5f;

    [Header("Sound Effects")]
    public AudioClip coinSound;
    public AudioClip growSound;
    public AudioClip lifeSound;
    public AudioClip deathSound;
    public AudioClip victorySound;
    public AudioClip jumpSound;

    [Header("Effect Volumes")]
    [Range(0f, 1f)] public float coinVolume    = 1f;
    [Range(0f, 1f)] public float growVolume    = 1f;
    [Range(0f, 1f)] public float lifeVolume    = 1f;
    [Range(0f, 1f)] public float deathVolume   = 1f;
    [Range(0f, 1f)] public float victoryVolume = 1f;
    [Range(0f, 1f)] public float jumpVolume    = 1f;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private AudioSource _bgSource;
    private AudioSource _sfxSource;

    // ==========================================
    // 3. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitAudioSources();
    }

    private void Start()
    {
        PlayBackground();
    }

    // ==========================================
    // 4. PUBLIC METHODS
    // ==========================================
    public void PlayBackground()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("MusicManager: background music clip is not assigned!");
            return;
        }

        _bgSource.clip   = backgroundMusic;
        _bgSource.volume = backgroundVolume;
        _bgSource.Play();
    }

    public void StopBackground() => _bgSource.Stop();

    public void PlayCoin()    => PlaySFX(coinSound,    coinVolume);
    public void PlayGrow()    => PlaySFX(growSound,    growVolume);
    public void PlayLife()    => PlaySFX(lifeSound,    lifeVolume);
    public void PlayDeath()   => PlaySFX(deathSound,   deathVolume);
    public void PlayVictory() => PlaySFX(victorySound, victoryVolume);
    public void PlayJump()    => PlaySFX(jumpSound,    jumpVolume);

    // ==========================================
    // 5. PRIVATE METHODS
    // ==========================================
    private void InitAudioSources()
    {
        _bgSource = GetComponent<AudioSource>();
        if (_bgSource == null)
        {
            _bgSource = gameObject.AddComponent<AudioSource>();
        }

        _bgSource.loop        = true;
        _bgSource.playOnAwake = false;

        _sfxSource             = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop        = false;
        _sfxSource.playOnAwake = false;
    }

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            return;
        }

        _sfxSource.PlayOneShot(clip, volume);
    }
}