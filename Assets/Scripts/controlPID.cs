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

    private float theta_anterior = 0f;
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
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        rg2D_carro.totalForce = Vector3.zero;
        lbl_fuerza.text = mando_fx.ToString("#0.#") + " N";
        if (estado == Estado.Activo)
        {
            rg2D_carro.AddForce(new Vector3(1, 0, 0) * mando_fx);
            // Implementamos Control PID Completo con los parámetros Kp, Ki y Kd
            Control_PID();
            theta_anterior = go_barra.transform.rotation.z;
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

        float error = referencia - go_barra.transform.rotation.z;
        mando_fx = Kp * error;
        if (limiteF_ON.isOn)
        {
            mando_fx = Mathf.Sign(mando_fx)*Mathf.Min(mando_fx, limite_mando_fx);
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

        float error = referencia - go_barra.transform.rotation.z;
        integracion += error*Time.deltaTime;
        mando_fx = (10*Kp * error + integracion * 10*Ki);
        if (limiteF_ON.isOn)
        {
            mando_fx = Mathf.Sign(mando_fx) * Mathf.Min(mando_fx, limite_mando_fx);
        }
    }

    void Control_PID()
    {
        float error = referencia - go_barra.transform.rotation.z;
        integracion += error * Time.deltaTime;
        float derivada_theta = -(go_barra.transform.rotation.z - theta_anterior) / Time.deltaTime;
        mando_fx = (10 * Kp * error + integracion * 10 * Ki + derivada_theta * 10 * Kd);
        if (limiteF_ON.isOn)
        {
            mando_fx = Mathf.Sign(mando_fx) * Mathf.Min(Mathf.Abs(mando_fx), limite_mando_fx);
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
