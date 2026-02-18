using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private float smoothSpeed = 3f;
    public Image healthPompompurinBar;
    public Image healtCheedorBar;
    public Image healthGuardianBar;
    public Image candyCoinsBar;
    private VidaJugador vidaJugador;

    // ══════════════════════════════════════════════════════════
    //  CONTADOR DE GUARDIANES ALIADOS
    // ══════════════════════════════════════════════════════════
    [Header("Guardian Ally Counter")]
    [Tooltip("Texto que muestra el contador de guardianes aliados (TextMeshPro)")]
    public TextMeshProUGUI guardianAllyCounterText;

    [Tooltip("Alternativamente, usa UI.Text si no tienes TextMeshPro")]
    public Text guardianAllyCounterTextLegacy;

    [Tooltip("Prefijo del texto mostrado")]
    public string counterPrefix = "Guardianes Aliados: ";

    [Tooltip("Muestra logs del contador en consola")]
    public bool debugCounterLogs = true;

    private int guardianAllyCount = 0;

    // ══════════════════════════════════════════════════════════
    //  AUDIO
    // ══════════════════════════════════════════════════════════
    [Header("Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioCollectionSO musicCollection;
    public AudioClip[] sfxCollection;
    public AudioMixer audioMixer;

    public enum GameState { Menu, Gameplay, Pausa, Combat }
    public static GameManager Instance { get; private set; }

    // ══════════════════════════════════════════════════════════
    //  CICLO DE VIDA
    // ══════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //Para cambio de escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // Suscribirse a los eventos de guardianes aliados
        GuardianController.OnGuardianBecameAlly += IncrementGuardianCounter;
        GuardianController.OnGuardianLeftAlly += DecrementGuardianCounter;
    }

    void OnDisable()
    {
        // Desuscribirse para evitar memory leaks
        GuardianController.OnGuardianBecameAlly -= IncrementGuardianCounter;
        GuardianController.OnGuardianLeftAlly -= DecrementGuardianCounter;
    }

    void Start()
    {
        PlayMusicByState(GameState.Menu);

        // Configurar barra de vida del jugador
        vidaJugador = FindAnyObjectByType<VidaJugador>();
        if (vidaJugador != null)
        {
            vidaJugador.OnVidaChanged += UpdateHealthBarPlayer;
            vidaJugador.OnPlayerDead += Die;
        }

        // Configurar barra de vida del guardián
        GuardianController guardian = FindAnyObjectByType<GuardianController>();
        if (guardian != null)
        {
            guardian.OnVidaChanged += UpdateHealthBarGuardian;
        }

        // Inicializar contador de guardianes
        UpdateGuardianAllyCounterUI();
    }

    // ══════════════════════════════════════════════════════════
    //  BARRAS DE VIDA
    // ══════════════════════════════════════════════════════════
    void UpdateHealthBarPlayer(int vidaActual, int vidaMaxima)
    {
        if (healthPompompurinBar != null)
        {
            healthPompompurinBar.fillAmount = (float)vidaActual / vidaMaxima;
        }
    }

    public void TakeDamage(int damage)
    {
        if (vidaJugador != null)
            vidaJugador.RecibirDaño(damage);
    }

    void Die()
    {
        Debug.Log("Game Over");
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Time.timeScale = 0f; // Pausa el juego
    }

    public void UpdateHealthBarGuardian(int vidaActual, int vidaMaxima)
    {
        if (healthGuardianBar != null)
        {
            healthGuardianBar.fillAmount = (float)vidaActual / vidaMaxima;
        }
    }

    // ══════════════════════════════════════════════════════════
    //  CONTADOR DE GUARDIANES ALIADOS
    // ══════════════════════════════════════════════════════════
    void IncrementGuardianCounter()
    {
        guardianAllyCount++;

        if (debugCounterLogs)
            Debug.Log($"[GameManager] +1 Guardián Aliado → Total: {guardianAllyCount}");

        UpdateGuardianAllyCounterUI();
    }

    void DecrementGuardianCounter()
    {
        guardianAllyCount = Mathf.Max(0, guardianAllyCount - 1);

        if (debugCounterLogs)
            Debug.Log($"[GameManager] -1 Guardián Aliado → Total: {guardianAllyCount}");

        UpdateGuardianAllyCounterUI();
    }

    void UpdateGuardianAllyCounterUI()
    {
        string displayText = counterPrefix + guardianAllyCount;

        if (guardianAllyCounterText != null)
            guardianAllyCounterText.text = displayText;

        if (guardianAllyCounterTextLegacy != null)
            guardianAllyCounterTextLegacy.text = displayText;
    }

    /// <summary>Obtiene el número actual de guardianes aliados.</summary>
    public int GetGuardianAllyCount() => guardianAllyCount;

    /// <summary>Reinicia el contador de guardianes a 0 (útil al iniciar nueva partida).</summary>
    public void ResetGuardianCounter()
    {
        guardianAllyCount = 0;
        UpdateGuardianAllyCounterUI();

        if (debugCounterLogs)
            Debug.Log("[GameManager] Contador de guardianes reiniciado.");
    }

    // ══════════════════════════════════════════════════════════
    //  AUDIO
    // ══════════════════════════════════════════════════════════
    public void PlayMusicByState(GameState state)
    {
        int index = 0;
        switch (state)
        {
            case GameState.Menu:
                index = 0;
                break;
            case GameState.Gameplay:
                index = 1;
                break;
            case GameState.Pausa:
                index = 2;
                break;
        }

        if (musicSource.clip == musicCollection.audioClips[index] && musicSource.isPlaying)
            return;

        musicSource.clip = musicCollection.audioClips[index];
        musicSource.Play();
    }

    public void PlaySfx(int index)
    {
        if (sfxCollection != null && index >= 0 && index < sfxCollection.Length)
        {
            sfxSource.PlayOneShot(sfxCollection[index]);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void Musicvolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void SFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }

    public void MasterVolume(float volume)
    {
        audioMixer.SetFloat("GeneralVolume", volume);
    }
}