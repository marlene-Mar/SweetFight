using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int maxHealth = 100; // Salud máxima del jugador
    public int currentHealth; // Salud actual del jugador
    //Barras de vida de los personajes
    public Image healthPompompurinBar; 
    public Image healtCheedorBar;
    //public Image healtCamemiBar;
    //public Image healtGuardianBar;

    //public Image healtCandyCoinsBar;

    private Animator pompompurinAnimator;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

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
}
