using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectController : MonoBehaviour
{
    [Header("Referencias")]
    public RawImage overlayImage;   // el Raw Image del Canvas

    [Header("Defaults")]
    public Color hitColor = Color.red;
    public float duration = 0.5f;
    [Range(0f, 1f)]
    public float spread = 0.4f;

    private Material _mat;
    private Coroutine _coroutine;

    void Awake()
    {
        // Instanciar el material para no modificar el asset original
        _mat = Instantiate(overlayImage.material);
        overlayImage.material = _mat;
        _mat.SetFloat("_HitAmount", 0f);
    }

    /// <summary>
    /// Llama esto cuando el jugador recibe daño.
    /// </summary>
    public void TriggerHit(Color color, float dur = -1f, float sp = -1f)
    {
        if (dur < 0) dur = duration;
        if (sp < 0) sp = spread;

        _mat.SetColor("_HitColor", color);
        _mat.SetFloat("_Spread", sp);

        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(FadeOut(dur));
    }

    // Sobrecarga sin parámetros (usa los defaults del Inspector)
    public void TriggerHit() => TriggerHit(hitColor);

    private IEnumerator FadeOut(float dur)
    {
        _mat.SetFloat("_HitAmount", 1f);
        float elapsed = 0f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            float opacity = 1f - (t * t);   // ease-out cuadrático
            _mat.SetFloat("_HitAmount", opacity);
            yield return null;
        }

        _mat.SetFloat("_HitAmount", 0f);
        _coroutine = null;
    }
}