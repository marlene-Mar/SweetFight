using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    // JUEGO
    public int maxHealth = 100; // Salud máxima del jugador
    public int currentHealth; // Salud actual del jugador
    //Barras de vida de los personajes
    public Image healthPompompurinBar; 
    public Image healtCheedorBar;
    //public Image healtCamemiBar;
    //public Image healtGuardianBar;

    //public Image healtCandyCoinsBar;

    private Animator pompompurinAnimator;


    //AUDIO
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioCollectionSO musicCollection;
    public AudioClip[] sfxCollection;
    public AudioMixer audioMixer;

    //private int intensityIndex;

    public enum GameState { Menu, Gameplay, Pausa, Combat }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
        PlayMusicByState(GameState.Menu);
    }

    //JUEGO: BARRAS DE VIDA
    void UpdateUI()
    {
        if (healthPompompurinBar != null)
        {
            healthPompompurinBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    void Die(){
        Debug.Log("Game Over");
    }


    //AUDIO: MUSICA

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
