using UnityEngine;

public class MusicManager : MonoBehaviour
{ 
    public static MusicManager Instance { get; private set; }
 
    [Header("Фонова музика")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float backgroundVolume = 0.5f;
 
    [Header("Звукові ефекти")]
    public AudioClip coinSound;
    public AudioClip growSound;
    public AudioClip lifeSound;
    public AudioClip deathSound;
    public AudioClip victorySound;
    public AudioClip jumpSound;
 
    [Header("Гучність ефектів")]
    [Range(0f, 1f)] public float coinVolume    = 1f;
    [Range(0f, 1f)] public float growVolume    = 1f;
    [Range(0f, 1f)] public float lifeVolume    = 1f;
    [Range(0f, 1f)] public float deathVolume   = 1f;
    [Range(0f, 1f)] public float victoryVolume = 1f;
    [Range(0f, 1f)] public float jumpVolume    = 1f;


 
    private AudioSource bgSource;
    private AudioSource sfxSource;
 
    private void Awake()
    {
        if (Instance != null) { DestroyImmediate(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
 
        bgSource = GetComponent<AudioSource>();
        if (bgSource == null)
            bgSource = gameObject.AddComponent<AudioSource>();

        bgSource.loop        = true;
        bgSource.playOnAwake = false;
 
        sfxSource            = gameObject.AddComponent<AudioSource>();
        sfxSource.loop       = false;
        sfxSource.playOnAwake = false;
    }

    private void Start()
    {
        PlayBackground();
    }
 
    public void PlayBackground()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("MusicManager: фонова музика не призначена!");
            return;
        }
        bgSource.clip   = backgroundMusic;
        bgSource.volume = backgroundVolume;
        bgSource.Play();
    }

    public void StopBackground() => bgSource.Stop();
 
    public void PlayCoin()    => PlaySFX(coinSound,    coinVolume);
    public void PlayGrow()    => PlaySFX(growSound,    growVolume);
    public void PlayLife()    => PlaySFX(lifeSound,    lifeVolume);
    public void PlayDeath()   => PlaySFX(deathSound,   deathVolume);
    public void PlayVictory() => PlaySFX(victorySound, victoryVolume);
    public void PlayJump()    => PlaySFX(jumpSound,    jumpVolume);

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null)
        {
             return;
        }
        sfxSource.PlayOneShot(clip, volume);
    }
}