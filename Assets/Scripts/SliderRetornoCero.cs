using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Hace que un Slider vuelva a 0 cuando se suelta (efecto "muelle").
// Se añade automáticamente desde ControlManual, pero también puede colocarse a mano sobre cualquier Slider.
[RequireComponent(typeof(Slider))]
public class SliderRetornoCero : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool activo = true;
    [Min(0f)] public float tiempoRetorno = 0.25f;   // s desde el extremo hasta 0 (0 = instantáneo)

    private Slider slider;
    private bool pulsado = false;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void OnPointerDown(PointerEventData eventData) { pulsado = true; }
    public void OnPointerUp(PointerEventData eventData) { pulsado = false; }

    void OnDisable() { pulsado = false; }

    void Update()
    {
        if (!activo || pulsado || slider.value == 0f) return;

        float valorCero = Mathf.Clamp(0f, slider.minValue, slider.maxValue);
        if (tiempoRetorno <= 0f)
        {
            slider.value = valorCero;
            return;
        }

        float rango = Mathf.Max(Mathf.Abs(slider.maxValue), Mathf.Abs(slider.minValue));
        float nuevo = Mathf.MoveTowards(slider.value, valorCero, rango / tiempoRetorno * Time.unscaledDeltaTime);
        // Con "Whole Numbers" activo, MoveTowards se redondearía y podría quedarse atascado: se fuerza el paso
        if (slider.wholeNumbers)
        {
            nuevo = slider.value > valorCero ? Mathf.Floor(nuevo) : Mathf.Ceil(nuevo);
        }
        slider.value = nuevo;
    }
}
