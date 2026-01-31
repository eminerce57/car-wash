using UnityEngine;

/// <summary>
/// Ses Yöneticisi - Singleton
/// Tüm oyun seslerini yönetir
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [Header("Ses Kaynakları")]
    public AudioSource sfxSource;        // Efekt sesleri için
    public AudioSource loopSource;       // Döngülü sesler için (yıkama vs)
    public AudioSource musicSource;      // Arka plan müziği için
    
    [Header("Ses Klipleri - Efektler")]
    public AudioClip coinSound;          // Para kazanma sesi
    public AudioClip washingSound;       // Yıkama sesi (döngü)
    public AudioClip carHornSound;       // Korna sesi
    public AudioClip buttonClickSound;   // Buton tıklama
    public AudioClip unlockSound;        // Yeni şey açma sesi
    public AudioClip upgradeSound;       // Upgrade sesi
    
    [Header("Arka Plan Müziği")]
    public AudioClip backgroundMusic;    // Ana müzik
    public bool playMusicOnStart = true; // Başlangıçta müzik çalsın mı?
    
    [Header("Ses Ayarları")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;
    
    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // AudioSource yoksa ekle
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        if (loopSource == null)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;
        }
        
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;  // Müzik sürekli dönsün
        }
    }
    
    private void Start()
    {
        // Başlangıçta müzik çal
        if (playMusicOnStart && backgroundMusic != null)
        {
            PlayMusic();
        }
    }
    
    /// <summary>
    /// Tek seferlik ses çal
    /// </summary>
    public void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }
    
    /// <summary>
    /// Para kazanma sesi
    /// </summary>
    public void PlayCoinSound()
    {
        PlaySound(coinSound);
    }
    
    /// <summary>
    /// Buton tıklama sesi
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }
    
    /// <summary>
    /// Unlock sesi
    /// </summary>
    public void PlayUnlockSound()
    {
        PlaySound(unlockSound);
    }
    
    /// <summary>
    /// Upgrade sesi
    /// </summary>
    public void PlayUpgradeSound()
    {
        PlaySound(upgradeSound);
    }
    
    /// <summary>
    /// Korna sesi
    /// </summary>
    public void PlayHornSound()
    {
        PlaySound(carHornSound, 0.5f);
    }
    
    /// <summary>
    /// Yıkama sesini başlat (döngü)
    /// </summary>
    public void StartWashingSound()
    {
        if (washingSound == null || loopSource == null) return;
        
        if (!loopSource.isPlaying)
        {
            loopSource.clip = washingSound;
            loopSource.volume = sfxVolume * 0.7f;
            loopSource.Play();
        }
    }
    
    /// <summary>
    /// Yıkama sesini durdur
    /// </summary>
    public void StopWashingSound()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }
    
    /// <summary>
    /// Arka plan müziğini çal
    /// </summary>
    public void PlayMusic()
    {
        if (backgroundMusic == null || musicSource == null) return;
        
        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }
    
    /// <summary>
    /// Arka plan müziğini durdur
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    /// <summary>
    /// Müzik çalıyor mu?
    /// </summary>
    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }
    
    /// <summary>
    /// Tüm sesleri kapat/aç
    /// </summary>
    public void SetMute(bool mute)
    {
        if (sfxSource != null) sfxSource.mute = mute;
        if (loopSource != null) loopSource.mute = mute;
        if (musicSource != null) musicSource.mute = mute;
    }
    
    /// <summary>
    /// Efekt ses seviyesini ayarla
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.volume = sfxVolume * 0.7f;
        }
    }
    
    /// <summary>
    /// Müzik ses seviyesini ayarla
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
}
