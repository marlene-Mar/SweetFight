using UnityEngine;
using System.Collections;

// Clase que se encarga de controlar el efecto visual al recibir un golpe en el jugador
public class HitEffectController : MonoBehaviour
{
    [Header("Referencias")]
    public Renderer characterRenderer; 

    [Header("Configuración")]
    public Color hitColor = new Color(0.5f, 0f, 1f, 1f); 
    public float duration = 0.8f;

    private Material[] _materials;
    private Coroutine _coroutine;

    void Awake()
    {
        if (characterRenderer != null)
        {
            _materials = characterRenderer.materials;
            
            foreach(Material m in _materials)
            {
                m.SetFloat("_HitAmount", 0f);
            }
        }
    }

    // Método público para activar el efecto de golpe
    public void TriggerHit()
    {
        Debug.Log("¡Golpe recibido! Activando efectos...");
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(FadeOut(duration));
    }

    // Coroutine que maneja la transición del efecto de golpe
    private IEnumerator FadeOut(float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float intensity = 1f - (elapsed / dur);

            foreach (Material m in _materials)
            {
                if (m != null)
                {
                    m.SetFloat("_HitAmount", intensity);
                    if (m.HasProperty("_HitColor")) m.SetColor("_HitColor", hitColor);
                    if (m.HasProperty("_AuraColor")) m.SetColor("_AuraColor", hitColor);
                }
            }
            yield return null;
        }
        
        // Limpieza al final
        foreach (Material m in _materials) m.SetFloat("_HitAmount", 0f);
    }
}