using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema central de audio para efectos (SFX) y música de fondo (BGM).
/// Maneja transiciones musicales automáticas y soporta generación de sonidos de reserva.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    [Header("Música de Fondo Principal")]
    [Tooltip("Pista de música continua que sonará en bucle desde que inicia el juego y persistirá entre niveles.")]
    public AudioClip backgroundMusic;

    [Header("Música de Combate contra el Jefe (Opcional)")]
    [Tooltip("Pista opcional para el Boss (Nivel 3). Si se deja vacía, continuará sonando la música de fondo sin cortarse.")]
    public AudioClip bossMusic;

    [Header("Volúmenes Maestros")]
    [Range(0f, 1f)] public float musicVolume = 0.65f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    [Header("Fuentes de Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [System.Serializable]
    public struct SoundEffect
    {
        public string name;
        public AudioClip clip;
    }

    [Header("Listas de Audio Adicionales")]
    public List<SoundEffect> musicTracks = new List<SoundEffect>();
    public List<SoundEffect> soundEffects = new List<SoundEffect>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioSources();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        EnsureAudioSources();
        if (backgroundMusic != null && !musicSource.isPlaying)
        {
            PlayBackgroundMusic(backgroundMusic);
        }
    }

    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = sfxVolume;
        }
    }

    /// <summary>
    /// Inicia o mantiene la reproducción de la música de fondo en bucle continuo sin reiniciarla si ya está sonando.
    /// </summary>
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (clip == null) return;
        EnsureAudioSources();

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return; // Ya está sonando esta misma música, mantenerla continua
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void PlayMusic(string trackName)
    {
        EnsureAudioSources();

        // 1. Caso Boss: Si hay bossMusic asignada, se reproduce. Si no, dejamos continuar la música de fondo actual
        if (trackName.Equals("BossMusic", System.StringComparison.OrdinalIgnoreCase))
        {
            if (bossMusic != null)
            {
                if (musicSource.clip == bossMusic && musicSource.isPlaying) return;
                musicSource.clip = bossMusic;
                musicSource.loop = true;
                musicSource.volume = musicVolume;
                musicSource.Play();
                return;
            }
            else if (backgroundMusic != null)
            {
                if (musicSource.clip == backgroundMusic && musicSource.isPlaying) return;
                PlayBackgroundMusic(backgroundMusic);
                return;
            }
        }

        // 2. Si hay música de fondo asignada, se mantiene continua para cualquier nivel o menú
        if (backgroundMusic != null)
        {
            if (musicSource.clip == backgroundMusic && musicSource.isPlaying)
            {
                return; // Continuidad total sin cortes
            }
            PlayBackgroundMusic(backgroundMusic);
            return;
        }

        // 3. Fallback: buscar en la lista de pistas configuradas
        foreach (var track in musicTracks)
        {
            if (track.name.Equals(trackName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (musicSource.clip == track.clip && musicSource.isPlaying) return;

                musicSource.clip = track.clip;
                musicSource.loop = true;
                musicSource.volume = musicVolume;
                musicSource.Play();
                return;
            }
        }
    }

    public void PlaySFX(string effectName)
    {
        EnsureAudioSources();

        foreach (var sfx in soundEffects)
        {
            if (sfx.name.Equals(effectName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (sfx.clip != null)
                {
                    sfxSource.PlayOneShot(sfx.clip, sfxVolume);
                    return;
                }
            }
        }
    }

    public void PlaySFXClip(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        EnsureAudioSources();
        sfxSource.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volumeScale));
    }

    private List<AudioSource> pausedSources = new List<AudioSource>();
    private float prePauseMusicVolume = -1f;

    /// <summary>
    /// Pausa todos los efectos de sonido y reduce el volumen de la música de fondo un 50%.
    /// </summary>
    public void OnGamePaused()
    {
        EnsureAudioSources();

        // 1. Reducir el volumen de la música de fondo al 50%
        if (musicSource != null)
        {
            if (prePauseMusicVolume < 0f) prePauseMusicVolume = musicSource.volume;
            musicSource.volume = prePauseMusicVolume * 0.5f;
        }

        // 2. Pausar la fuente de SFX del AudioManager
        if (sfxSource != null && sfxSource.isPlaying)
        {
            sfxSource.Pause();
        }

        // 3. Pausar todos los demás AudioSources activos en la escena (enemigos, jefe, pasos, jugador)
        pausedSources.Clear();
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource src in allSources)
        {
            if (src == null || src == musicSource) continue;

            if (src.isPlaying)
            {
                src.Pause();
                pausedSources.Add(src);
            }
        }
    }

    /// <summary>
    /// Restaura el volumen de la música de fondo al 100% y reanuda los efectos de sonido pausados.
    /// </summary>
    public void OnGameResumed()
    {
        EnsureAudioSources();

        // 1. Restaurar volumen de música al 100%
        if (musicSource != null)
        {
            musicSource.volume = prePauseMusicVolume > 0f ? prePauseMusicVolume : musicVolume;
            prePauseMusicVolume = -1f;
        }

        // 2. Reanudar fuente de SFX central
        if (sfxSource != null)
        {
            sfxSource.UnPause();
        }

        // 3. Reanudar los AudioSources que se habían pausado
        foreach (AudioSource src in pausedSources)
        {
            if (src != null)
            {
                src.UnPause();
            }
        }
        pausedSources.Clear();
    }
}
