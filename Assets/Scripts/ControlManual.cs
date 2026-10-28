using UnityEngine;
using UnityEngine.UI;

public class ControlManual : MonoBehaviour
{
    public Rigidbody2D carro;
    public Slider slider_Fuerza;
    public float fuerzaMax = 2;   // N por unidad del deslizador (deslizador ±100 -> ±200 N, mismo rango que el límite del PID)
    public Vector2 direccion = Vector2.right;

    [Header("Retorno del deslizador")]
    [Tooltip("Si está activo, el deslizador vuelve a 0 al soltarlo (como un joystick con muelle).")]
    public bool volverACeroAlSoltar = true;
    [Tooltip("Tiempo (s) que tarda en volver desde el extremo hasta 0. Con 0 vuelve de forma instantánea.")]
    [Min(0f)] public float tiempoRetorno = 0.25f;

    private SliderRetornoCero retorno;

    void Start()
    {
        retorno = slider_Fuerza.GetComponent<SliderRetornoCero>();
        if (retorno == null)
        {
            retorno = slider_Fuerza.gameObject.AddComponent<SliderRetornoCero>();
        }
    }

    void Update()
    {
        // La configuración se puede cambiar en el Inspector durante la ejecución
        retorno.activo = volverACeroAlSoltar;
        retorno.tiempoRetorno = tiempoRetorno;
    }

    // Las fuerzas se aplican en FixedUpdate: Unity borra las fuerzas acumuladas tras cada paso de física,
    // así que aplicarlas aquí garantiza que la fuerza actúa en todos los pasos, sea cual sea la tasa de FPS.
    // (No hace falta poner totalForce a cero: AddForce solo afecta al paso de física siguiente.)
    void FixedUpdate()
    {
        carro.AddForce(direccion.normalized * slider_Fuerza.value * fuerzaMax, ForceMode2D.Force);
    }
}
