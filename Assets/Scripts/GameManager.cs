using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    //BARRAS DE VIDA POMPOMPURIN
    public int maxHealth = 100; 
    public int currentHealth;  
    private float displayHealth; 

    private float smoothSpeed = 3f; 
    public Image healthPompompurinBar;
    public Image healtCheedorBar; 

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
        currentHealth = maxHealth;
        displayHealth = (float)maxHealth; 
        UpdateUI();
        PlayMusicByState(GameState.Menu);
    }

    void Update()
    {
        if (!Mathf.Approximately(displayHealth, currentHealth))
        {
            displayHealth = Mathf.Lerp(displayHealth, currentHealth, Time.deltaTime * smoothSpeed);
            UpdateUI();
        }
    }

    //JUEGO: BARRAS VIDA, CANDYCOINDS, VIDA CAMEMI, CHEEDOR, RATBOOT
    void UpdateUI()
    {
        if (healthPompompurinBar != null)
        {
            healthPompompurinBar.fillAmount = displayHealth / maxHealth; 
        }

    }

    public void TakeDamage(int damage)
    {
        PompompurinController player = FindObjectOfType<PompompurinController>();
        if (player != null) player.TakeDamage(damage);  // Para el animator "Die"

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Sonido de daño
        // PlaySfx(2); 

        if (currentHealth <= 0)
            Die();
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
