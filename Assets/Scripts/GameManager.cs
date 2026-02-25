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
    public Image healthCamemiBar;
    public Image candyCoinsBar;
    private VidaJugador vidaJugador;
    public TextMeshProUGUI maxMessageText;
    public int maxCandies = 30;
    private int currentCandies = 0;

    // ══════════════════════════════════════════════════════════
    //  CONTADOR DE GUARDIANES ALIADOS
    // ══════════════════════════════════════════════════════════
    [Header("Guardian Ally Counter")]
    public TextMeshProUGUI guardianAllyCounterText;
    public string counterPrefix = "x0";
    public bool debugCounterLogs = true;
    private int guardianAllyCount = 0;

    // ══════════════════════════════════════════════════════════
    //  CAMEMI
    // ══════════════════════════════════════════════════════════
    private CamemiController camemiController;

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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        GuardianController.OnGuardianBecameAlly += IncrementGuardianCounter;
        GuardianController.OnGuardianLeftAlly += DecrementGuardianCounter;
        SceneManager.sceneLoaded += OnSceneLoaded; // Re-buscar refs al cambiar escena
    }

    void OnDisable()
    {
        GuardianController.OnGuardianBecameAlly -= IncrementGuardianCounter;
        GuardianController.OnGuardianLeftAlly -= DecrementGuardianCounter;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        PlayMusicByState(GameState.Menu);
        ConnectReferences();
        UpdateCandyBar();
        if (maxMessageText != null) maxMessageText.gameObject.SetActive(false);
        UpdateGuardianAllyCounterUI();
    }

    // Se llama automáticamente al cargar una escena nueva (por DontDestroyOnLoad)
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConnectReferences();
    }

    /// <summary>Busca y suscribe todas las referencias de vida en la escena activa.</summary>
    void ConnectReferences()
    {
        // ── Jugador ───────────────────────────────────────────
        VidaJugador foundVida = FindAnyObjectByType<VidaJugador>();
        if (foundVida != null && foundVida != vidaJugador)
        {
            // Desuscribir el anterior si existía
            if (vidaJugador != null)
            {
                vidaJugador.OnVidaChanged -= UpdateHealthBarPlayer;
                vidaJugador.OnPlayerDead -= Die;
            }

            vidaJugador = foundVida;
            vidaJugador.OnVidaChanged += UpdateHealthBarPlayer;
            vidaJugador.OnPlayerDead += Die;

            // Forzar el fill inicial correcto
            UpdateHealthBarPlayer(vidaJugador.vidaActual, vidaJugador.vidaMaxima);
            Debug.Log("[GameManager] VidaJugador conectada.");
        }

        // ── Guardián ──────────────────────────────────────────
        GuardianController guardian = FindAnyObjectByType<GuardianController>();
        if (guardian != null)
        {
            guardian.OnVidaChanged += UpdateHealthBarGuardian;
            Debug.Log("[GameManager] GuardianController conectado.");
        }

        // ── Camemi ────────────────────────────────────────────
        CamemiController camemi = FindAnyObjectByType<CamemiController>();
        if (camemi != null && camemi != camemiController)
        {
            camemiController = camemi;
            // Suscribirse al evento de vida de Camemi
            camemiController.OnVidaChanged += UpdateHealthBarCamemi;

            // Forzar el fill inicial
            UpdateHealthBarCamemi(camemiController.VidaActual, camemiController.VidaMax);
            Debug.Log("[GameManager] CamemiController conectado.");
        }
    }

    // ══════════════════════════════════════════════════════════
    //  BARRAS DE VIDA
    // ══════════════════════════════════════════════════════════
    void UpdateHealthBarPlayer(int vidaActual, int vidaMaxima)
    {
        if (healthPompompurinBar != null)
            healthPompompurinBar.fillAmount = (float)vidaActual / vidaMaxima;
    }

    public void UpdateHealthBarGuardian(int vidaActual, int vidaMaxima)
    {
        if (healthGuardianBar != null)
            healthGuardianBar.fillAmount = (float)vidaActual / vidaMaxima;
    }

    void UpdateHealthBarCamemi(int vidaActual, int vidaMaxima)
    {
        if (healthCamemiBar != null)
            healthCamemiBar.fillAmount = (float)vidaActual / vidaMaxima;
    }

    public void TakeDamage(int damage)
    {
        if (vidaJugador != null)
            vidaJugador.RecibirDaño(damage);
    }

    void Die()
    {
        Debug.Log("Game Over");
    }

    // ══════════════════════════════════════════════════════════
    //  CANDY COINS
    // ══════════════════════════════════════════════════════════
    public void AddCandy(int amount)
    {
        if (currentCandies >= maxCandies) return;

        currentCandies += amount;
        UpdateCandyBar();

        if (currentCandies >= maxCandies)
        {
            currentCandies = maxCandies;
            if (maxMessageText != null)
            {
                maxMessageText.gameObject.SetActive(true);
                maxMessageText.text = "¡CandyCoins al máximo!";
            }
        }
    }

    void UpdateCandyBar()
    {
        if (candyCoinsBar != null)
            candyCoinsBar.fillAmount = (float)currentCandies / maxCandies;
    }

    // ══════════════════════════════════════════════════════════
    //  CONTADOR DE GUARDIANES ALIADOS
    // ══════════════════════════════════════════════════════════
    void IncrementGuardianCounter()
    {
        guardianAllyCount++;
        if (debugCounterLogs) Debug.Log($"[GameManager] +1 Guardián Aliado → Total: {guardianAllyCount}");
        UpdateGuardianAllyCounterUI();
    }

    void DecrementGuardianCounter()
    {
        guardianAllyCount = Mathf.Max(0, guardianAllyCount - 1);
        if (debugCounterLogs) Debug.Log($"[GameManager] -1 Guardián Aliado → Total: {guardianAllyCount}");
        UpdateGuardianAllyCounterUI();
    }

    void UpdateGuardianAllyCounterUI()
    {
        if (guardianAllyCounterText != null)
            guardianAllyCounterText.text = counterPrefix + guardianAllyCount;
    }

    public int GetGuardianAllyCount() => guardianAllyCount;

    public void ResetGuardianCounter()
    {
        guardianAllyCount = 0;
        UpdateGuardianAllyCounterUI();
        if (debugCounterLogs) Debug.Log("[GameManager] Contador de guardianes reiniciado.");
    }

    // ══════════════════════════════════════════════════════════
    //  AUDIO
    // ══════════════════════════════════════════════════════════
    public void PlayMusicByState(GameState state)
    {
        int index = 0;
        switch (state)
        {
            case GameState.Menu: index = 0; break;
            case GameState.Gameplay: index = 1; break;
            case GameState.Pausa: index = 2; break;
        }

        if (musicSource.clip == musicCollection.audioClips[index] && musicSource.isPlaying)
            return;

        musicSource.clip = musicCollection.audioClips[index];
        musicSource.Play();
    }

    public void PlaySfx(int index)
    {
        if (sfxCollection != null && index >= 0 && index < sfxCollection.Length)
            sfxSource.PlayOneShot(sfxCollection[index]);
    }

    public void StopMusic() => musicSource.Stop();

    public void Musicvolume(float volume) => audioMixer.SetFloat("MusicVolume", volume);
    public void SFXVolume(float volume) => audioMixer.SetFloat("SFXVolume", volume);
    public void MasterVolume(float volume) => audioMixer.SetFloat("GeneralVolume", volume);
}