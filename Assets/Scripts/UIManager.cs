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
    // Menú principal
    // ==============================
    public GameObject MenuInicial;
    public GameObject MenuPrincipal;
    public GameObject MenuPrincipalBase;
    public GameObject Personajes;
    public GameObject Configuracion;
    public GameObject Audio;
    public GameObject Controles;
    public GameObject canvasMenu;

    // ==============================
    // HUD
    // ==============================
    public GameObject HudPanel;
    public GameObject PausaPanel;
    public Image barraVidaP;
    public Image barraCandyCoins;
    public Image barraVidaC;
    public Image vida1Cheedor;
    public Image vida2Cheedor;
    public Image vida3Cheedor;
    public Image barraVidaGuardian;

    //==============================
    //MAPA
    //==============================
    public GameObject PanelMap;

    // ==============================
    // AUDIO
    // ==============================
    public TextMeshProUGUI musicLevelText;
    public TextMeshProUGUI sfxLevelText;
    private int musicLevel = 5;
    private int sfxLevel = 5;

    // ==============================
    // TIMER
    // ==============================
    public TextMeshProUGUI timerText;

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
        //ShowMenuPanel(Audio);
        Audio.SetActive(true);
        Controles.SetActive(false);
        Configuracion.SetActive(false);
    }

    public void GoToControles()
    {
        //ShowMenuPanel(Controles);
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
        MenuPrincipal.SetActive(false); //Oculta el menú
        MenuPrincipalBase.SetActive(false); //Oculta el menú
        HudPanel.SetActive(true); //Muestra el HUD
        PausaPanel.SetActive(false);

        gameManager.PlayMusicByState(GameManager.GameState.Gameplay);
        Time.timeScale = 1f;
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego");
        // Application.Quit();
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
            // Regresa al menú principal
            ShowMenuPanel(MenuPrincipal);
        }
        else if (configSource == ConfigSource.Game)
        {
            // Regresa al juego
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
    }

    public void ReanudarJuego()
    {
        PausaPanel.SetActive(false);
        HudPanel.SetActive(true);
        gameManager.PlayMusicByState(GameManager.GameState.Gameplay);
        Time.timeScale = 1f;
    }

    public void GuardarYSalir()
    {
        Debug.Log("Guardado");
        PausaPanel.SetActive(false); 
        HudPanel.SetActive(false);
        MenuPrincipal.SetActive(true);
        gameManager.PlayMusicByState(GameManager.GameState.Menu);
        Salir();
    }

    public void EdoMapa()
    {
        PanelMap.SetActive(!PanelMap.activeSelf);
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

    // ========== TIMER ============

    public void UpdateTimer(float time)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(time / 40);
        int seconds = Mathf.FloorToInt(time % 40);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
