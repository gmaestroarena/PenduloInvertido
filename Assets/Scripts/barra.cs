using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.CompilerServices;

public class barra : MonoBehaviour
{
    public Canvas canvas;              // Asigna el Canvas de la UI
    
    public float growDuration = 1f;    // Duración de la animación de crecimiento
    public float waitAfter = 4f;       // Espera antes de llamar a f()
    public float fadeDuration = 2f;  // Duración del fade out

    [SerializeField] GUI GUI_script;

    private Text messageText;
    private RectTransform rect;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        
        
        // Crear el texto hijo del canvas
        GameObject textGO = new GameObject("BreakText", typeof(Text));
        textGO.transform.SetParent(canvas.transform, false);

        messageText = textGO.GetComponent<Text>();
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.fontSize = 60;
        messageText.color = Color.yellow;
        
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.resizeTextForBestFit = true;
        messageText.resizeTextMinSize = 10;
        messageText.resizeTextMaxSize = 60;

        rect = messageText.rectTransform;
        // Ocupa todo el Canvas (sin recortes)
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(260f, 650f);
        rect.offsetMax = new Vector2(-260f, -260f);
        rect.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void OnJointBreak2D(Joint2D broken)
    {
        Debug.Log($"💥Se rompió la articulación (HingeJoint) {broken.name}-{broken.connectedBody.name}");
        StartCoroutine(ShowMessage());
    }


    private IEnumerator ShowMessage()
    {
        messageText.text = "Se rompió la articulación BARRA / CARRO.\nEl Control PID se ha estropeado...";
        rect.localScale = Vector3.zero;

        float t = 0f;
        while (t < growDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / growDuration);
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, eased);
            yield return null;
        }


        // 🔹 Esperar antes del fade out
        yield return new WaitForSeconds(waitAfter);

        // 🔹 Fade out (desvanecer)
        t = 0f;
        Color startColor = messageText.color;
        Image cielo = canvas.transform.Find("Cielo").GetComponent<Image>();
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            float oscurecimiento = Mathf.Lerp(1f, 0f, t / fadeDuration);
            cielo.color = new Color(oscurecimiento, oscurecimiento, oscurecimiento,1-oscurecimiento+.125f);
            messageText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // 🔹 Esperar antes del fade out
        yield return new WaitForSeconds(waitAfter);

        // Ocultar completamente por si acaso
        messageText.color = new Color(startColor.r, startColor.g, startColor.b, 0f);

        f();
    }

    private void f()
    {
        Debug.Log("🔧 Se llamó a f()");
        GUI_script.ReiniciarSimulacion();
    }
}
