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

    [Header("Ally Timer HUD")]
    public GameObject allyTimerContainer;
    public TextMeshProUGUI allyTimerText;            
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
        SceneManager.sceneLoaded += OnSceneLoaded;
        GuardianController.OnAllyTimerUpdated += UpdateAllyTimerUI;
        GuardianController.OnAllyTimerEnded += HideAllyTimerUI;
        GuardianController.OnGuardianBecameAlly += HideGuardianHealthBar;
        GuardianController.OnGuardianLeftAlly += HideGuardianHealthBar;
    }

    void OnDisable()
    {
        GuardianController.OnGuardianBecameAlly -= IncrementGuardianCounter;
        GuardianController.OnGuardianLeftAlly -= DecrementGuardianCounter;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GuardianController.OnAllyTimerUpdated -= UpdateAllyTimerUI;
        GuardianController.OnAllyTimerEnded -= HideAllyTimerUI;
        GuardianController.OnGuardianBecameAlly -= HideGuardianHealthBar;
        GuardianController.OnGuardianLeftAlly -= HideGuardianHealthBar;
    }

    void Start()
    {
        PlayMusicByState(GameState.Menu);
        ConnectReferences();
        UpdateCandyBar();
        if (maxMessageText != null) maxMessageText.gameObject.SetActive(false);
        if (allyTimerContainer != null) allyTimerContainer.SetActive(false);

        HideGuardianHealthBar();
        UpdateGuardianAllyCounterUI();
    }

    // Se llama automáticamente al cargar una escena nueva (por DontDestroyOnLoad)
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConnectReferences();
    }

    // Actualiza el UI del timer de aliado con el tiempo restante y total
    void UpdateAllyTimerUI(float remaining, float total)
    {
        if (allyTimerContainer != null) allyTimerContainer.SetActive(true);
        if (allyTimerText != null)
        {
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            allyTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void HideAllyTimerUI()  // ← solo una versión
    {
        if (allyTimerContainer != null) allyTimerContainer.SetActive(false);
    }

    public void HideGuardianHealthBar()
    {
        if (healthGuardianBar != null)
        {
            // Apaga el contenedor padre (para que también se oculte el fondo y el marco de la barra)
            healthGuardianBar.transform.parent.gameObject.SetActive(false);
        }
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
        {
            // Activa el contenedor padre si estaba desactivado
            healthGuardianBar.transform.parent.gameObject.SetActive(true);
            healthGuardianBar.fillAmount = (float)vidaActual / vidaMaxima;
        }
    }

    void UpdateHealthBarCamemi(int vidaActual, int vidaMaxima)
    {
        if (healthCamemiBar != null)
        {
            // Activar el contenedor padre si está desactivado
            healthCamemiBar.transform.parent.gameObject.SetActive(true);
            healthCamemiBar.fillAmount = (float)vidaActual / vidaMaxima;
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


    [Header("Save Data")]
    public Data gameData;

    public void SaveToData()
    {
        // ── Player ────────────────────────────────────────────────
        if (vidaJugador != null)
        {
            gameData.playerHealth = vidaJugador.vidaActual;
            gameData.playerMaxHealth = vidaJugador.vidaMaxima;

            // ✅ Posición del jugador, no del GameManager
            PompompurinController pompom = vidaJugador.GetComponent<PompompurinController>();
            if (pompom != null)
            {
                gameData.playerPositionX = pompom.transform.position.x;
                gameData.playerPositionY = pompom.transform.position.y;
                gameData.playerPositionZ = pompom.transform.position.z;
            }
        }

        // ── Candies ───────────────────────────────────────────────
        gameData.currentCandies = currentCandies;
        gameData.guardianAllyCount = guardianAllyCount;

        // ── Guardianes ────────────────────────────────────────────
        GuardianController[] guardianes = FindObjectsByType<GuardianController>(FindObjectsSortMode.None);
        for (int i = 0; i < Mathf.Min(guardianes.Length, gameData.guardians.Length); i++)
            gameData.guardians[i] = guardianes[i].GetSaveData();

        // ── Audio ─────────────────────────────────────────────────
        audioMixer.GetFloat("GeneralVolume", out gameData.masterVolume);
        audioMixer.GetFloat("MusicVolume", out gameData.musicVolume);
        audioMixer.GetFloat("SFXVolume", out gameData.sfxVolume);

        // ── Escena ────────────────────────────────────────────────
        gameData.lastScene = SceneManager.GetActiveScene().name;

        // ── Inventario ────────────────────────────────────────────
        InventoryManager.instance?.SaveToData();

        Debug.Log("[GameManager] Partida guardada.");
    }

    public void LoadFromData()
    {
        // ── Player vida ───────────────────────────────────────────
        if (vidaJugador != null)
        {
            vidaJugador.vidaMaxima = gameData.playerMaxHealth;
            vidaJugador.vidaActual = gameData.playerHealth;
            vidaJugador.NotificarCambio();

            // ✅ Restaurar posición del jugador
            PompompurinController pompom = vidaJugador.GetComponent<PompompurinController>();
            if (pompom != null)
            {
                CharacterController cc = pompom.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                pompom.transform.position = new Vector3(
                    gameData.playerPositionX,
                    gameData.playerPositionY,
                    gameData.playerPositionZ
                );

                if (cc != null) cc.enabled = true;
            }
        }

        // ── Candies ───────────────────────────────────────────────
        currentCandies = gameData.currentCandies;
        UpdateCandyBar();

        // ── Contador guardianes ───────────────────────────────────
        guardianAllyCount = gameData.guardianAllyCount;
        UpdateGuardianAllyCounterUI();

        // ── Estado guardianes ─────────────────────────────────────
        GuardianController[] guardianes = FindObjectsByType<GuardianController>(FindObjectsSortMode.None);
        for (int i = 0; i < Mathf.Min(guardianes.Length, gameData.guardians.Length); i++)
        {
            if (gameData.guardians[i] != null)
                guardianes[i].LoadSaveData(gameData.guardians[i]);
        }

        // ── Audio ─────────────────────────────────────────────────
        Musicvolume(gameData.musicVolume);
        SFXVolume(gameData.sfxVolume);
        MasterVolume(gameData.masterVolume);

        // ── Inventario ────────────────────────────────────────────
        InventoryManager.instance?.LoadFromData();

        Debug.Log("[GameManager] Partida cargada.");
    }
}