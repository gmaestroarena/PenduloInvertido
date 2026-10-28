using TMPro;
using UnityEngine;


public enum Estado { Pausa, Activo };
public enum Variable { Theta, F_Mando };


public class controlPID : MonoBehaviour
{
    

    [Space]
    [Header("Parámetros Control PID:")]
    [Space]
    [SerializeField] public Estado estado = Estado.Pausa;
    [Space]
    [SerializeField][Range(0,100)] private float Kp = 1f;
    [SerializeField][Range(0, 20)] private float Ki = 1f;
    [SerializeField][Range(0, 20)] private float Kd = 1f;
    [Header("Protección (mejora de InvertedPendulum):")]
    [Tooltip("Si está activa, el control se desconecta (mando = 0) cuando la barra se inclina más de 'anguloMaxControl': el péndulo se da por perdido y oscila libremente, protegiendo al carro de chocar contra los topes y romper el péndulo. Desactivada en Ejercicio2 y Ejercicio3; activada en InvertedPendulum.")]
    [SerializeField] private bool desconectarFueraDeRango = false;
    [Tooltip("Ángulo (grados) a partir del cual se desconecta el control si la protección está activa.")]
    [SerializeField][Range(0, 180)] private float anguloMaxControl = 90f;
    [Space]
    [Header("Parámetros:")]
    [Space]
    [SerializeField] private float Masa_M = 10f;
    [SerializeField] private float masa_m = 5f;
    [SerializeField] private float l_barra = 1f;
    [SerializeField] private float g = 9.8f;
    [Space]
    [Header("Variables:")]
    [Space]
    [SerializeField] public float referencia = 0f;
    [SerializeField] public float theta = 0f;
    [SerializeField] public float mando_fx = 0f;
    [SerializeField][Range(0, 100)] public float limite_mando_fx = 50f;

    [Space]
    [Header("Referencias internas:")]
    [Space]
    [SerializeField] private Rigidbody2D rg2D_carro;
    [SerializeField] private GameObject go_barra;
    [SerializeField] private GameObject go_esfera;

    // Factor de escala de las ganancias. La versión anterior usaba 10 * rotation.z, y rotation.z = sin(theta/2) ~ theta/2,
    // así que la ganancia efectiva era ~5 por radián. Con 5 se mantiene el mismo comportamiento para ángulos pequeños
    // y los valores de Kp, Ki y Kd de los ejercicios siguen siendo válidos.
    private const float ESCALA_GANANCIAS = 5f;

    private Rigidbody2D rb_barra;
    private BolaPendulo bola;
    private float integracion = 0.0f;
    private bool mostrandoPerturbación = false;
    private float timer = 0.8f;

    private UnityEngine.UI.Slider sl_Kp;
    private UnityEngine.UI.Slider sl_Ki;
    private UnityEngine.UI.Slider sl_Kd;
    private UnityEngine.UI.Slider sl_limitF;
    private UnityEngine.UI.Slider sl_angInicial;
    private TextMeshProUGUI lbl_fuerza;
    private UnityEngine.UI.Toggle limiteF_ON;

    private void OnValidate()
    {
        // Aplica rotación en el eje Z (2D)
        go_barra.transform.rotation = Quaternion.Euler(0f, 0f, theta);
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        limiteF_ON = GameObject.Find("limFuerzaON").GetComponent<UnityEngine.UI.Toggle>();
        
        sl_Kp = GameObject.Find("Slider_Kp").GetComponent<UnityEngine.UI.Slider>();
        sl_Ki = GameObject.Find("Slider_Ki").GetComponent<UnityEngine.UI.Slider>();
        sl_Kd = GameObject.Find("Slider_Kd").GetComponent<UnityEngine.UI.Slider>();
        sl_limitF = GameObject.Find("Slider_limF").GetComponent<UnityEngine.UI.Slider>();
        sl_angInicial = GameObject.Find("Slider_ang").GetComponent<UnityEngine.UI.Slider>();
        lbl_fuerza = GameObject.Find("FuerzaMando").GetComponent<TextMeshProUGUI>();
        limiteF_ON.isOn = false;

        rb_barra = go_barra.GetComponent<Rigidbody2D>();
        bola = FindAnyObjectByType<BolaPendulo>();
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        
        rg2D_carro.totalForce = Vector3.zero;
        lbl_fuerza.text = mando_fx.ToString("#0.#") + " N";
        if (estado == Estado.Activo)
        {
            if (go_barra.GetComponent<HingeJoint2D>() != null)
            {
                // Implementamos Control PID Completo con los parámetros Kp, Ki y Kd.
                // Primero se calcula el mando con el estado actual y después se aplica (sin retardo de un paso).
                Control_PID();
                rg2D_carro.AddForce(new Vector3(1, 0, 0) * mando_fx);
            }
            else 
            {
                mando_fx = 0f;
                rg2D_carro.transform.Find("SectorCircular").gameObject.SetActive(false);
                rg2D_carro.transform.Find("ejeY").gameObject.SetActive(false);
            }


               

                
        }
                      
        //theta = mando_fx / (Masa_M * l_barra * Time.deltaTime * Time.deltaTime - (Masa_M + masa_m) * g);
    }

    private void Update()
    {
        if (mostrandoPerturbación && timer > 0.0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0.0f)
            {
                timer = 0.8f;
                mostrandoPerturbación = false;
                go_esfera.transform.GetChild(0).gameObject.SetActive(false);
                go_esfera.transform.GetChild(1).gameObject.SetActive(false);
                go_esfera.transform.GetChild(2).gameObject.SetActive(false);
            }
        }
    }

    // Ángulo de la barra respecto a la vertical, en radianes, en el intervalo (-pi, pi].
    // Positivo = giro antihorario (barra inclinada hacia la izquierda).
    // (Antes se usaba transform.rotation.z, que es la componente z del cuaternión = sin(theta/2), no el ángulo.)
    float AnguloBarra()
    {
        return Mathf.DeltaAngle(0f, go_barra.transform.eulerAngles.z) * Mathf.Deg2Rad;
    }

    // Saturación simétrica de la fuerza de mando: |mando| <= limite
    float Saturar(float mando)
    {
        return Mathf.Sign(mando) * Mathf.Min(Mathf.Abs(mando), limite_mando_fx);
    }

    // Fuera del rango de trabajo el controlador se desconecta: mando nulo y sin memoria integral.
    // Sin esto, con la barra por debajo de la horizontal el PID seguiría empujando con cientos de newtons
    // (y el signo del error salta al pasar por la vertical inferior), sacudiendo el carro y haciendo
    // oscilar la barra mucho más rápido que un péndulo libre.
    bool FueraDeRango(float error)
    {
        if (desconectarFueraDeRango && Mathf.Abs(error) > anguloMaxControl * Mathf.Deg2Rad)
        {
            mando_fx = 0f;
            integracion = 0f;
            return true;
        }
        return false;
    }

    // Reinicia la memoria del controlador (acción integral)
    public void ReiniciarControl()
    {
        integracion = 0f;
    }

    void Control_P()
    {

        // Se calcula el error:  error = referencia - angulo. La referencia es que la barra esté vertical (angulo = 0º)
        // Quiero que error tienda a cero, es decir, que el ángulo tienda a cero. Realimento negativamente para generar una fuerza de mando
        // La fuerza de mando (mando_fx) debe oponerse al movimiento horizontal del carro (intenta disminuir el ángulo en valor absoluto).
        // 
        // La fuerza será proporcional al error mediante una constante de control Kp.
        // Hemos propuesto un rango para poder probar distintos valores de Kp: Rango(Kp) = [0,200]
        // El error nunca se corregirá del todo (el estado final será un ángulo distinto de cero.
        //
        // NOTA: El control_P no se utiliza en el programa, pero lo dejamos para que se pueda entender como un paso del control PID

        float error = referencia - AnguloBarra();
        if (FueraDeRango(error)) return;
        mando_fx = ESCALA_GANANCIAS * Kp * error;
        if (limiteF_ON.isOn)
        {
            mando_fx = Saturar(mando_fx);
        }
    }

    void Control_PI()
    {

        // Se calcula el error:  error = referencia - angulo. La referencia es que la barra esté vertical (angulo = 0º)
        // Quiero que error tienda a cero, es decir, que el ángulo tienda a cero. Realimento negativamente para generar una fuerza de mando
        // La fuerza de mando (mando_fx) debe oponerse al movimiento horizontal del carro (intenta disminuir el ángulo en valor absoluto).
        // 
        // La fuerza será proporcional al error mediante una constante de control Kp. Añadiremos también una acción integral para que 
        // Hemos propuesto un rango para poder probar distintos valores de Kp: Rango(Kp) = [0,200]
        //
        // NOTA: El control_PI no se utiliza en el programa, pero lo dejamos para que se pueda entender como un paso del control PID

        float error = referencia - AnguloBarra();
        if (FueraDeRango(error)) return;
        float integracion_nueva = integracion + error * Time.deltaTime;
        float mando = ESCALA_GANANCIAS * (Kp * error + Ki * integracion_nueva);

        if (limiteF_ON.isOn && Mathf.Abs(mando) > limite_mando_fx)
        {
            // Anti-windup: si el mando está saturado no se acumula la integral
            mando_fx = Saturar(mando);
        }
        else
        {
            integracion = integracion_nueva;
            mando_fx = mando;
        }
    }

    void Control_PID()
    {
        float error = referencia - AnguloBarra();
        if (FueraDeRango(error)) return;

        // Derivada del error = -d(theta)/dt. Se toma directamente la velocidad angular de la barra (grados/s -> rad/s)
        // en lugar de derivar numéricamente: así no hay picos ("derivative kick") al arrancar ni al aplicar perturbaciones.
        float derivada_error = -rb_barra.angularVelocity * Mathf.Deg2Rad;

        float integracion_nueva = integracion + error * Time.deltaTime;
        float mando = ESCALA_GANANCIAS * (Kp * error + Ki * integracion_nueva + Kd * derivada_error);

        if (limiteF_ON.isOn && Mathf.Abs(mando) > limite_mando_fx)
        {
            // Anti-windup (integración condicional): si el mando está saturado no se acumula la integral
            mando_fx = Saturar(mando);
        }
        else
        {
            integracion = integracion_nueva;
            mando_fx = mando;
        }
    }

    public void AñadirPerturbacion(float angulo)
    {
        go_barra.transform.eulerAngles += new Vector3(0,0,angulo);
        go_esfera.transform.GetChild(2).gameObject.SetActive(true);
        go_esfera.transform.GetChild(2).GetComponent<TextMesh>().text = (Mathf.Abs((int)angulo)).ToString() + "º";
        if (angulo > 0.0f)
        {
            go_esfera.transform.GetChild(0).gameObject.SetActive(true);
            go_esfera.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            go_esfera.transform.GetChild(0).gameObject.SetActive(false);
            go_esfera.transform.GetChild(1).gameObject.SetActive(true);
        }

        go_barra.transform.rotation = Quaternion.Euler(0f, 0f, angulo);
        if (bola != null) bola.Recolocar();   // la bola ya no es hija de la barra: hay que llevarla con ella
        ReiniciarControl();
        mostrandoPerturbación = true;
        timer = 0.8f;
    }

    public void AñadirPerturbacion1()
    {
        float angulo = Random.Range(20f, 40f);
        angulo = Random.Range(0f, 1f) > 0.5f ? angulo : -angulo;
        AñadirPerturbacion(angulo);
    }

    public void AñadirPerturbacion2()
    {
        AñadirPerturbacion(theta);
    }


    public void CambiarParametrosControl()
    {
        Kp = sl_Kp.value;
        Ki = sl_Ki.value;
        Kd = sl_Kd.value;
        GameObject.Find("Kp_valor").GetComponent<TextMeshProUGUI>().text = Kp.ToString("#0.#");
        GameObject.Find("Ki_valor").GetComponent<TextMeshProUGUI>().text = Ki.ToString("#0.#");
        GameObject.Find("Kd_valor").GetComponent<TextMeshProUGUI>().text = Kd.ToString("#0.#");
    }

    public void CambiarLimiteFuerza()
    {
        limite_mando_fx = sl_limitF.value;
        GameObject.Find("lim_valor").GetComponent<TextMeshProUGUI>().text = limite_mando_fx.ToString("#0.#");

    }

    public void CambiarAnguloInicial()
    {
        theta = -sl_angInicial.value;
        GameObject.Find("ang_valor").GetComponent<TextMeshProUGUI>().text = (-theta).ToString("#0.#")+"º";
        if (estado == Estado.Pausa)
        {
            go_barra.transform.rotation = Quaternion.Euler(0f, 0f, theta);
        }
        
        GameObject.Find("txt_perturbacion_2").GetComponent<TextMeshProUGUI>().text = "\u0398<sub> pert</sub>= " + (-theta).ToString() + "º";
    }
}
