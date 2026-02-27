using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private GameManager gameManager;

    // ==============================
    // Menu principal
    // ==============================
    public GameObject MenuInicial;
    public GameObject MenuPrincipal;
    public GameObject MenuPrincipalBase;
    public GameObject Personajes;
    public GameObject Configuracion;
    public GameObject Audio;
    public GameObject Controles;
    public GameObject canvasMenu;
    public GameObject timerContainer;

    // ==============================
    // HUD
    // ==============================
    public GameObject HudPanel;
    public GameObject PausaPanel;
    private bool isGamePaused = false;
    public GameObject PanelCreditos;

    //==============================
    //MAPA
    //==============================
    public GameObject PanelMap;

    // ==============================
    // AUDIO
    // ==============================
    public TextMeshProUGUI musicLevelText;
    public TextMeshProUGUI sfxLevelText;
    private int musicLevel = 3;
    private int sfxLevel = 3;

    // ==============================
    // TIMER GUARDIÁN
    // ==============================
    public TextMeshProUGUI timerText;
    public GameObject timerContainerAliado;
    public TextMeshProUGUI timerTextAliado;

    // ==============================
    // TIMER CAMEMI  ← NUEVO
    // ==============================
    public GameObject timerContainerCamemi;    // GameObject padre del timer de Camemi (arrastra desde el Inspector)
    public TextMeshProUGUI timerTextCamemi;    // TMP independiente del timer de Camemi (arrastra desde el Inspector)

    private ConfigSource configSource;

    public enum ConfigSource
    {
        Menu,
        Game
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Estado inicial
        MenuInicial.SetActive(true);

        MenuPrincipalBase.SetActive(false);
        MenuPrincipal.SetActive(false);
        Personajes.SetActive(false);
        Configuracion.SetActive(false);
        Audio.SetActive(false);
        Controles.SetActive(false);

        canvasMenu.SetActive(true);

        HudPanel.SetActive(false);
        PausaPanel.SetActive(false);
        PanelMap.SetActive(false);

        // Asegurarse de que los timers estén ocultos al inicio
        if (timerContainer != null) timerContainer.SetActive(false);
        if (timerContainerCamemi != null) timerContainerCamemi.SetActive(false);

        gameManager = FindFirstObjectByType<GameManager>();
        gameManager.PlayMusicByState(GameManager.GameState.Menu);

        if (musicLevelText != null) musicLevelText.text = musicLevel.ToString();
        if (sfxLevelText != null) sfxLevelText.text = sfxLevel.ToString();

        Time.timeScale = 0f;
    }

    private void Update()
    {
        // ENTER para pasar del menú inicial al principal
        if (MenuInicial.activeSelf &&
            Keyboard.current != null &&
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            GoToMenuPrincipal();
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (HudPanel.activeSelf || PausaPanel.activeSelf)
            {
                if (isGamePaused)
                {
                    ReanudarJuego();
                    isGamePaused = false;
                }
                else
                {
                    PausarJuego();
                    isGamePaused = true;
                }
            }
        }
    }

    // ======================
    // MÉTODO CENTRAL
    // ======================
    public void ShowMenuPanel(GameObject panelToShow)
    {
        MenuPrincipal.SetActive(false);
        Personajes.SetActive(false);
        Configuracion.SetActive(false);
        Audio.SetActive(false);
        Controles.SetActive(false);

        panelToShow.SetActive(true);
    }

    // ======================
    // NAVEGACIÓN PRINCIPAL
    // ======================
    public void GoToMenuPrincipal()
    {
        MenuInicial.SetActive(false);
        MenuPrincipalBase.SetActive(true);
        ShowMenuPanel(MenuPrincipal);
    }

    public void GoToPersonajes()
    {
        ShowMenuPanel(Personajes);
    }

    public void GoToConfiguracion()
    {
        configSource = ConfigSource.Menu;
        ShowMenuPanel(Configuracion);
    }

    public void GoToAudio()
    {
        Audio.SetActive(true);
        Controles.SetActive(false);
        Configuracion.SetActive(false);
    }

    public void GoToControles()
    {
        Controles.SetActive(true);
        Audio.SetActive(false);
        Configuracion.SetActive(false);
    }

    public void GoToHome()
    {
        ShowMenuPanel(MenuPrincipal);
    }

    // ======================
    // JUEGO
    // ======================
    public void Jugar()
    {
        Debug.Log("Iniciar o regresar al juego");
        MenuPrincipal.SetActive(false);
        MenuPrincipalBase.SetActive(false);
        HudPanel.SetActive(true);
        PausaPanel.SetActive(false);

        HideTimer();
        HideTimerCamemi();
        if (gameManager != null && gameManager.allyTimerContainer != null)
            gameManager.allyTimerContainer.SetActive(false);

        gameManager.PlayMusicByState(GameManager.GameState.Gameplay);
        Time.timeScale = 1f;
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
    }

    public void RegresarAlMenu()
    {
        Debug.Log("Regresando al menú principal");
        ShowMenuPanel(MenuPrincipal);
        gameManager.PlayMusicByState(GameManager.GameState.Menu);
    }

    public void AbrirConfiguracionDesdeJuego()
    {
        configSource = ConfigSource.Game;
        Configuracion.SetActive(true);
        PausaPanel.SetActive(false);
    }

    public void CerrarConfiguracion()
    {
        if (configSource == ConfigSource.Menu)
        {
            ShowMenuPanel(MenuPrincipal);
        }
        else if (configSource == ConfigSource.Game)
        {
            Configuracion.SetActive(false);
            PausaPanel.SetActive(true);
            HudPanel.SetActive(true);
            Time.timeScale = 1f;
        }
    }

    public void VolverAConfiguracion()
    {
        Audio.SetActive(false);
        Controles.SetActive(false);
        Configuracion.SetActive(true);
    }

    public void PausarJuego()
    {
        if (PausaPanel.activeSelf) return;
        PausaPanel.SetActive(true);
        gameManager.PlayMusicByState(GameManager.GameState.Pausa);
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    public void ReanudarJuego()
    {
        PausaPanel.SetActive(false);
        HudPanel.SetActive(true);
        gameManager.PlayMusicByState(GameManager.GameState.Gameplay);
        Time.timeScale = 1f;
        isGamePaused = false;
    }

    public void GuardarYSalir()
    {
        Debug.Log("Guardado");
        PausaPanel.SetActive(false);
        HudPanel.SetActive(false);
        MenuPrincipal.SetActive(true);
        gameManager.PlayMusicByState(GameManager.GameState.Menu);
        SaveSystem.Instance.Save();
        Salir();
    }

    public void EdoMapa()
    {
        PanelMap.SetActive(!PanelMap.activeSelf);
    }

    public void FinDelJuego()
    {
        HudPanel.SetActive(false);
        PausaPanel.SetActive(false);
        if (timerContainer != null) timerContainer.SetActive(false);
        if (timerContainerCamemi != null) timerContainerCamemi.SetActive(false);

    }

    public void MostrarCreditos()
    {
        if (PanelCreditos != null)
        {
            MenuPrincipalBase.SetActive(false);
            HudPanel.SetActive(false);
            PanelCreditos.SetActive(true);

            Time.timeScale = 0f;
        }
    }

    public void TerminarJuegoDesdeCreditos()
    {
        if (PanelCreditos != null)
            PanelCreditos.SetActive(false);

        // Resetear y borrar guardado
        SaveSystem.Instance?.DeleteSave();
        ResetearJuegoCompleto();

        MenuPrincipalBase.SetActive(true);
        ShowMenuPanel(MenuPrincipal);
        gameManager.PlayMusicByState(GameManager.GameState.Menu);
        Time.timeScale = 1f;

    }

    public void ResetearJuegoCompleto()
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.ForceCloseDialogue();
        }

        CombatManager combatManager = FindFirstObjectByType<CombatManager>();
        if (combatManager != null) combatManager.EndCombat(false);

        CamemiController camemi = FindFirstObjectByType<CamemiController>();
        if (camemi != null) camemi.ResetCamemi();
        

        PompompurinController pompom = FindFirstObjectByType<PompompurinController>();
        VidaJugador vida = FindFirstObjectByType<VidaJugador>();

        if (vida != null)
        {
            vida.vidaActual = vida.vidaMaxima;
            vida.NotificarCambio();
        }

        if (pompom != null)
        {
            pompom.isDead = false;
            pompom.inCombat = false;
            pompom.isAttacking = false;
            pompom.enabled = true;

            CharacterController cc = pompom.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            pompom.transform.position = pompom.spawnPosition;
            pompom.transform.rotation = pompom.spawnRotation;
            if (cc != null) cc.enabled = true;

            Animator anim = pompom.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("Die", false);
                anim.SetBool("Combat", false);
                anim.SetBool("Jump", false);
                anim.SetBool("IsRun", false);
                anim.SetFloat("Speed", 0f);
                anim.SetFloat("life", vida != null ? vida.vidaMaxima : 100f);
            }
        }

        // Guardianes — volver a spawnear desde cero
        GuardianSpawner spawner = FindFirstObjectByType<GuardianSpawner>();
        spawner?.SpawnGuardians();

        // Candies y flanes — volver a spawnear
        SimpleObjectSpawner objectSpawner = FindFirstObjectByType<SimpleObjectSpawner>();
        if (objectSpawner != null)
        {
            // Destruir los que quedaron en escena
            foreach (var obj in GameObject.FindGameObjectsWithTag("Flan"))
                Destroy(obj);
            foreach (var obj in GameObject.FindGameObjectsWithTag("Candy"))
                Destroy(obj);

            objectSpawner.RespawnAll();
        }

        // Inventario
        InventoryManager.instance?.ResetInventory();

        // Contador guardianes
        GameManager.Instance?.ResetGuardianCounter();
    }

    public void MuerteJugador()
    {
        HudPanel.SetActive(false);
        PausaPanel.SetActive(false);
        if (timerContainer != null) timerContainer.SetActive(false);
        if (timerContainerCamemi != null) timerContainerCamemi.SetActive(false);

        Time.timeScale = 1f;
        gameManager.PlayMusicByState(GameManager.GameState.Menu);

        ResetearJuegoCompleto();

        MenuPrincipalBase.SetActive(true);
        ShowMenuPanel(MenuPrincipal);
    }

    // ========== TIMER GUARDIÁN ==========

    public void ShowTimer()
    {
        if (timerContainer != null) timerContainer.SetActive(true);
    }

    public void HideTimer()
    {
        if (timerContainer != null) timerContainer.SetActive(false);
    }

    public void UpdateTimer(float time)
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(time / 40);
        int seconds = Mathf.FloorToInt(time % 40);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // ========== TIMER CAMEMI ==========

    public void ShowTimerCamemi()
    {
        if (timerContainerCamemi != null) timerContainerCamemi.SetActive(true);
    }

    public void HideTimerCamemi()
    {
        if (timerContainerCamemi != null) timerContainerCamemi.SetActive(false);
    }

    public void UpdateTimerCamemi(float time)
    {
        if (timerTextCamemi == null) return;     
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerTextCamemi.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // =========AUDIO=========

    public void IncreaseMusic() => ChangeMusicVolume(1);
    public void DecreaseMusic() => ChangeMusicVolume(-1);
    public void IncreaseSFX() => ChangeSFXVolume(1);
    public void DecreaseSFX() => ChangeSFXVolume(-1);

    private void ChangeMusicVolume(int amount)
    {
        musicLevel = Mathf.Clamp(musicLevel + amount, 0, 10);
        if (musicLevelText != null) musicLevelText.text = musicLevel.ToString();

        float volumeNormalized = Mathf.Max(musicLevel / 10f, 0.0001f);
        gameManager.Musicvolume(LinearToLog(volumeNormalized));
    }

    private void ChangeSFXVolume(int amount)
    {
        sfxLevel = Mathf.Clamp(sfxLevel + amount, 0, 10);
        if (sfxLevelText != null) sfxLevelText.text = sfxLevel.ToString();

        float volumeNormalized = Mathf.Max(sfxLevel / 10f, 0.0001f);
        gameManager.SFXVolume(LinearToLog(volumeNormalized));
    }

    private float LinearToLog(float value)
    {
        return Mathf.Log10(value) * 20;
    }

    // ========== CONTINUAR PARTIDA ==========
    public void ContinuarPartida()
    {
        // Revisamos si existe un archivo de guardado
        if (SaveSystem.Instance != null && SaveSystem.Instance.SaveExists())
        {
            Debug.Log("Cargando partida guardada...");
            MenuPrincipal.SetActive(false);
            MenuPrincipalBase.SetActive(false);
            HudPanel.SetActive(true);
            PausaPanel.SetActive(false);

            HideTimer();
            HideTimerCamemi();
            if (gameManager != null && gameManager.allyTimerContainer != null)
                gameManager.allyTimerContainer.SetActive(false);

            gameManager.PlayMusicByState(GameManager.GameState.Gameplay);
            Time.timeScale = 1f;

            // Llamamos a nuestro nuevo sistema de carga asíncrono
            SaveSystem.Instance.Load();
        }
        else
        {
            Debug.LogWarning("No hay partida guardada. Iniciando juego nuevo...");
            Jugar(); // Si no hay partida, lo mandamos a un juego nuevo normal
        }
    }

    public void ToggleInventory()
    {
        InventoryManager.instance?.ToggleInventory();
    }
}