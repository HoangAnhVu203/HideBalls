using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip bgmGameplay; 

    [Header("SFX Clips")]
    public AudioClip sfxMergeBall;
    public AudioClip sfxWin;
    public AudioClip sfxLose;
    public AudioClip sfxButton;

    [Space(10)]
    [Tooltip("Âm khi click vào khối (ClickFall)")]
    public AudioClip sfxClickBlock;

    [Tooltip("Âm khi click vào ball Spine (ClickFallSpine)")]
    public AudioClip sfxClickSpine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PlayMusic(bgmGameplay);
    }

    // ================== CORE ==================

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (!IsSoundOn()) return;
        if (sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (!IsMusicOn()) return;
        if (musicSource == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // ================== HELPER GAMEPLAY ==================

    public void PlayMergeBall() => PlaySFX(sfxMergeBall);
    public void PlayWin()       => PlaySFX(sfxWin);
    public void PlayLose()      => PlaySFX(sfxLose);
    public void PlayButton()    => PlaySFX(sfxButton);

    // click block thường (ClickFall)
    public void PlayClickBlock() => PlaySFX(sfxClickBlock);

    // click ball spine (ClickFallSpine)
    public void PlayClickSpine() => PlaySFX(sfxClickSpine);

    // ================== SETTING FLAGS ==================

    public static bool IsSoundOn()     => PlayerPrefs.GetInt("SOUND_ON", 1) == 1;
    public static bool IsMusicOn()     => PlayerPrefs.GetInt("MUSIC_ON", 1) == 1;
    public static bool IsVibrationOn() => PlayerPrefs.GetInt("VIBRATION_ON", 1) == 1;
    public static void SetSound(bool on)
    {
        PlayerPrefs.SetInt("SOUND_ON", on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetMusic(bool on)
    {
        PlayerPrefs.SetInt("MUSIC_ON", on ? 1 : 0);
        PlayerPrefs.Save();

        if (Instance != null && Instance.musicSource != null)
        {
            if (on)
            {
                if (!Instance.musicSource.isPlaying && Instance.bgmGameplay != null)
                    Instance.PlayMusic(Instance.bgmGameplay);
            }
            else
            {
                Instance.musicSource.Stop();
            }
        }
    }
}
