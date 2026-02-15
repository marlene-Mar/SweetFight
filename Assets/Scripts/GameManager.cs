using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    private float smoothSpeed = 3f; 
    public Image healthPompompurinBar;
    public Image healtCheedorBar;

    private VidaJugador vidaJugador;

    //AUDIO
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioCollectionSO musicCollection;
    public AudioClip[] sfxCollection;
    public AudioMixer audioMixer;

    public enum GameState { Menu, Gameplay, Pausa, Combat }
    public static GameManager Instance { get; private set; }

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

    void Start()
    {
        PlayMusicByState(GameState.Menu);

        vidaJugador = FindObjectOfType<VidaJugador>();

        if (vidaJugador != null)
        {
            vidaJugador.OnVidaChanged += UpdateHealthBar;
            vidaJugador.OnPlayerDead += Die;
        }
    }


    void UpdateHealthBar(int vidaActual, int vidaMaxima)
    {
        if (healthPompompurinBar != null)
        {
            healthPompompurinBar.fillAmount =
                (float)vidaActual / vidaMaxima;
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
        //Time.timeScale = 0f; // Pausa el juego
    }

    //AUDIO
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
