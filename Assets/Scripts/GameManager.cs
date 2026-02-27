using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ==========================================
    // BARRAS DE ESTADO (UI)
    // ==========================================
    [Header("UI Bars")]
    public Image healthPompompurinBar;
    public Image healtCheedorBar;
    public Image healthGuardianBar;
    public Image healthCamemiBar;
    public Image candyCoinsBar;
    public TextMeshProUGUI maxMessageText;

    // ==========================================
    // RECURSOS Y COLECCIONABLES
    // ==========================================
    [Header("Resources")]
    public int maxCandies = 30;
    private int currentCandies = 0;

    // ==========================================
    // GUARDIANES Y ALIADOS
    // ==========================================
    [Header("Guardian Ally Counter")]
    public TextMeshProUGUI guardianAllyCounterText;
    public string counterPrefix = "x0";
    public bool debugCounterLogs = true;
    private int guardianAllyCount = 0;

    [Header("Ally Timer HUD")]
    public GameObject allyTimerContainer;
    public TextMeshProUGUI allyTimerText;

    // ==========================================
    // AUDIO
    // ==========================================
    [Header("Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioCollectionSO musicCollection;
    public AudioClip[] sfxCollection;
    public AudioMixer audioMixer;

    // ==========================================
    // DATOS Y REFERENCIAS INTERNAS
    // ==========================================
    [Header("Save Data")]
    public Data gameData;

    public enum GameState { Menu, Gameplay, Pausa, Combat }

    private VidaJugador vidaJugador;
    private CamemiController camemiController;
    private float smoothSpeed = 3f;

    // ==========================================
    // CICLO DE VIDA (Unity Events)
    // ==========================================
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
        HideCamemiHealthBar();
        UpdateGuardianAllyCounterUI();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConnectReferences();
    }

    // ==========================================
    // CONEXIÓN DE REFERENCIAS
    // ==========================================
    void ConnectReferences()
    {
        // Jugador
        VidaJugador foundVida = FindAnyObjectByType<VidaJugador>();
        if (foundVida != null && foundVida != vidaJugador)
        {
            if (vidaJugador != null)
            {
                vidaJugador.OnVidaChanged -= UpdateHealthBarPlayer;
                vidaJugador.OnPlayerDead -= Die;
            }

            vidaJugador = foundVida;
            vidaJugador.OnVidaChanged += UpdateHealthBarPlayer;
            vidaJugador.OnPlayerDead += Die;

            UpdateHealthBarPlayer(vidaJugador.vidaActual, vidaJugador.vidaMaxima);
            Debug.Log("[GameManager] VidaJugador conectada.");
        }

        // Guardián
        GuardianController guardian = FindAnyObjectByType<GuardianController>();
        if (guardian != null)
        {
            guardian.OnVidaChanged += UpdateHealthBarGuardian;
            Debug.Log("[GameManager] GuardianController conectado.");
        }

        // Camemi
        CamemiController camemi = FindAnyObjectByType<CamemiController>();
        if (camemi != null && camemi != camemiController)
        {
            camemiController = camemi;
            camemiController.OnVidaChanged += UpdateHealthBarCamemi;
            UpdateHealthBarCamemi(camemiController.VidaActual, camemiController.VidaMax);
            HideCamemiHealthBar();
            Debug.Log("[GameManager] CamemiController conectado.");
        }
    }

    // ==========================================
    // GESTIÓN DE UI Y BARRAS DE VIDA
    // ==========================================
    void UpdateHealthBarPlayer(int vidaActual, int vidaMaxima)
    {
        if (healthPompompurinBar != null)
            healthPompompurinBar.fillAmount = (float)vidaActual / vidaMaxima;
    }

    public void UpdateHealthBarGuardian(int vidaActual, int vidaMaxima)
    {
        if (healthGuardianBar != null)
        {
            healthGuardianBar.transform.parent.gameObject.SetActive(true);
            healthGuardianBar.fillAmount = (float)vidaActual / vidaMaxima;
        }
    }

    void UpdateHealthBarCamemi(int vidaActual, int vidaMaxima)
    {
        if (healthCamemiBar != null)
        {
            healthCamemiBar.transform.parent.gameObject.SetActive(true);
            healthCamemiBar.fillAmount = (float)vidaActual / vidaMaxima;
        }
    }

    public void HideGuardianHealthBar()
    {
        if (healthGuardianBar != null)
            healthGuardianBar.transform.parent.gameObject.SetActive(false);
    }

    public void HideCamemiHealthBar()
    {
        if (healthCamemiBar != null)
            healthCamemiBar.transform.parent.gameObject.SetActive(false);
    }

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

    void HideAllyTimerUI()
    {
        if (allyTimerContainer != null) allyTimerContainer.SetActive(false);
    }

    // ==========================================
    // LÓGICA DE JUEGO Y DAÑO
    // ==========================================
    public void TakeDamage(int damage)
    {
        if (vidaJugador != null)
            vidaJugador.RecibirDaño(damage);
    }

    void Die()
    {
        Debug.Log("Game Over");
    }

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

    // ==========================================
    // CONTADOR DE GUARDIANES
    // ==========================================
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

    // ==========================================
    // AUDIO
    // ==========================================
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

    // ==========================================
    // SISTEMA DE GUARDADO (Save/Load)
    // ==========================================
    public void SaveToData()
    {
        if (vidaJugador != null)
        {
            gameData.playerHealth = vidaJugador.vidaActual;
            gameData.playerMaxHealth = vidaJugador.vidaMaxima;

            PompompurinController pompom = vidaJugador.GetComponent<PompompurinController>();
            if (pompom != null)
            {
                gameData.playerPositionX = pompom.transform.position.x;
                gameData.playerPositionY = pompom.transform.position.y;
                gameData.playerPositionZ = pompom.transform.position.z;
            }
        }

        gameData.currentCandies = currentCandies;
        gameData.guardianAllyCount = guardianAllyCount;

        GuardianController[] guardianes = FindObjectsByType<GuardianController>(FindObjectsSortMode.None);
        for (int i = 0; i < Mathf.Min(guardianes.Length, gameData.guardians.Length); i++)
            gameData.guardians[i] = guardianes[i].GetSaveData();

        audioMixer.GetFloat("GeneralVolume", out gameData.masterVolume);
        audioMixer.GetFloat("MusicVolume", out gameData.musicVolume);
        audioMixer.GetFloat("SFXVolume", out gameData.sfxVolume);

        gameData.lastScene = SceneManager.GetActiveScene().name;
        InventoryManager.instance?.SaveToData();

        Debug.Log("[GameManager] Partida guardada.");
    }

    public void LoadFromData()
    {
        if (vidaJugador != null)
        {
            vidaJugador.vidaMaxima = gameData.playerMaxHealth;
            vidaJugador.vidaActual = gameData.playerHealth;
            vidaJugador.NotificarCambio();

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

        currentCandies = gameData.currentCandies;
        UpdateCandyBar();

        // ── Estado guardianes ─────────────────────────────────────
        GuardianController[] guardianes = FindObjectsByType<GuardianController>(FindObjectsSortMode.None);
        for (int i = 0; i < Mathf.Min(guardianes.Length, gameData.guardians.Length); i++)
        {
            if (gameData.guardians[i] != null)
                guardianes[i].LoadSaveData(gameData.guardians[i]);
        }

        // ── Contador guardianes 
        guardianAllyCount = 0;
        foreach (var g in guardianes)
        {
            if (g.CompareTag("GuardianAlly"))
            {
                guardianAllyCount++;
            }
        }
        UpdateGuardianAllyCounterUI();

        Musicvolume(gameData.musicVolume);
        SFXVolume(gameData.sfxVolume);
        MasterVolume(gameData.masterVolume);

        InventoryManager.instance?.LoadFromData();

        Debug.Log("[GameManager] Partida cargada.");
    }
}