using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class pasaPagina : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI textElement;
    public UICircle ringElement1; // anillo dibujado proceduralmente
    public UICircle ringElement2; // anillo dibujado proceduralmente

    [Header("Duraciones (segundos)")]
    public float fadeInDuration = 1f;
    public float visibleDuration = 1f;
    public float fadeOutDuration = 1f;

    private Color textBaseColor;
    private Color ringBaseColor1;
    private Color ringBaseColor2;

    void Awake()
    {
        if (textElement == null)
            textElement = GetComponentInChildren<TextMeshProUGUI>();

        if (ringElement1 == null)
            ringElement1 = GetComponentInChildren<UICircle>();

        if (ringElement2 == null)
            ringElement2 = GetComponentInChildren<UICircle>();

        if (textElement == null || ringElement1 == null || ringElement2 == null)
        {
            Debug.LogError($"[TMPFadeInOutWithRing] Faltan referencias en {gameObject.name}");
            enabled = false;
            return;
        }

        textBaseColor = textElement.color;
        ringBaseColor1 = ringElement1.color;
        ringBaseColor2 = ringElement2.color;

        SetAlpha(0f);
    }

    void OnEnable()
    {
        ringElement1.gameObject.SetActive(true);
        ringElement2.gameObject.SetActive(true);
        StartCoroutine(FadeSequence());
    }

    private System.Collections.IEnumerator FadeSequence()
    {
        yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));  // fade in
        yield return new WaitForSeconds(visibleDuration);           // espera
        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration)); // fade out

        ringElement1.gameObject.SetActive(false);
        ringElement2.gameObject.SetActive(false);        
    }

    private System.Collections.IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float a = Mathf.Lerp(startAlpha, endAlpha, 2);
            SetAlpha(a);

            // Escalado suave del anillo para más efecto
            float scale = Mathf.Lerp(0.5f, 1f, t);
            ringElement1.rectTransform.localScale = Vector3.one * scale;
            ringElement2.rectTransform.localScale = Vector3.one * scale;

            yield return null;
        }

        SetAlpha(endAlpha);
    }

    private void SetAlpha(float alpha)
    {
        // Texto
        Color tc = textBaseColor;
        tc.a = alpha;
        textElement.color = tc;

        // Anillo
        Color rc = ringBaseColor1;
        rc.a = alpha;
        ringElement1.color = rc;
        ringElement1.SetVerticesDirty(); // fuerza redibujado
        ringElement2.color = rc;
        ringElement2.SetVerticesDirty(); // fuerza redibujado
    }
}
