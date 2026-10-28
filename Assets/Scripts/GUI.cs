using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GUI : MonoBehaviour
{
    [Space]
    [Header("Referencias externas:")]
    [Space]
    [SerializeField] controlPID control;
    [SerializeField] GameObject book;


    [Space]
    [Header("Referencias internas:")]
    [Space]
    [SerializeField] GameObject grafica;
    [SerializeField] GameObject p_graficas;
    [SerializeField] GameObject p_control;
    [Space]
    [SerializeField] GameObject ob_sliderLimit;
    [SerializeField] GameObject anguloInicial;
    [Space]
    [SerializeField] Image b_play;
    [SerializeField] Image b_pause;
    [Space]
    [SerializeField] GameObject ob_max_min;
    [SerializeField] GameObject ob_max_min_control;


    // Slider_ang (Dentro de AnguloInicial) cuando emite evento OnValueChanged(), se conecta a ControlPID (CambiarAnguloInicial())
    // Slider_limF (Dentro de LimiteFuerza) cuando emite evento OnValueChanged(), se conecta a ControlPID (CambiarLimiteFuerza())
    // btn_PERTURBACION1 y btn_PERTURBACION2 (Dentro de elementos_control), se conectan a ControlPID (AñadirPerturbacion1() y AñadirPerturbacion2())

    bool reanudarAlOcultarInstrucciones = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Referencias externas al GUI
        control = GameObject.FindGameObjectWithTag("controlPID").GetComponent<controlPID>();

        book = GameObject.Find("INSTRUCCIONES");

        PausarSimulacion();

        if (book != null)
        {
            book.SetActive(false);
        }
        
                
        ob_sliderLimit.GetComponent<Slider>().enabled = false;
        ob_sliderLimit.GetComponent<Slider>().interactable = false;
        ob_sliderLimit.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        ob_sliderLimit.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0f);
        ob_sliderLimit.transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(false);

        if (grafica != null)
        {
            grafica.GetComponent<Window_Graph>().DibujaGrafica();
        }
            

        b_play.color = new Color(1, 1, 1, .25f);
        b_pause.color = new Color(1, 1, 1, 1f);
    }
        

    public void ReiniciarSimulacion()
    {
        // Recarga la escena ACTUAL (antes siempre cargaba "InvertedPendulum", también desde los ejercicios)
        Scene actual = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        // En el editor se carga por ruta: funciona aunque la escena no esté en la lista de escenas del build
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(actual.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        // En un ejecutable la escena tiene que estar incluida en el build
        SceneManager.LoadScene(actual.buildIndex);
#endif
    }

    public void PausarSimulacion()
    {
        GameObject[] componentes = GameObject.FindGameObjectsWithTag("componentePendulo");
        foreach (GameObject componente in componentes)
        {
            componente.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
        control.estado = Estado.Pausa;
        anguloInicial.GetComponent<TextMeshProUGUI>().text = "\u0398<sub>inicial</sub> =";
        b_play.color = new Color(1, 1, 1, .25f);
        b_pause.color = new Color(1, 1, 1, 1f);
    }

    public void ReanudarSimulacion()
    {
        GameObject[] componentes = GameObject.FindGameObjectsWithTag("componentePendulo");
        foreach (GameObject componente in componentes)
        {
            componente.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
        control.estado = Estado.Activo;
        anguloInicial.GetComponent<TextMeshProUGUI>().text = "\u0398<sub>pert</sub> =";
        b_play.color = new Color(1, 1, 1, 1f);
        b_pause.color = new Color(1, 1, 1, .25f);

    }

    public void LimiteFuerza()
    {
        Toggle t = GameObject.Find("limFuerzaON").GetComponent<Toggle>();
       
        if (t.isOn)
        {
            ob_sliderLimit.GetComponent<Slider>().enabled = true;
            ob_sliderLimit.GetComponent<Slider>().interactable = true;
            ob_sliderLimit.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            ob_sliderLimit.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            ob_sliderLimit.transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(true);
            ob_sliderLimit.transform.GetChild(3).gameObject.SetActive(true);
        }
        else
        {
            ob_sliderLimit.GetComponent<Slider>().enabled = false;
            ob_sliderLimit.GetComponent<Slider>().interactable = false;
            ob_sliderLimit.transform.GetChild(0).GetComponent<Image>().color = new Color(1,1,1,0.5f);
            ob_sliderLimit.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0f);
            ob_sliderLimit.transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(false);
            ob_sliderLimit.transform.GetChild(3).gameObject.SetActive(false);
        }
    }

    public void Instrucciones()
    {
        book.SetActive(!book.activeSelf);
        if (book.activeSelf)
        {
            if (control.estado == Estado.Activo)
            {
                reanudarAlOcultarInstrucciones = true;
            }
            PausarSimulacion();
        }
        else 
        {
            if (reanudarAlOcultarInstrucciones) 
            { 
                ReanudarSimulacion();
            }
            reanudarAlOcultarInstrucciones = false;
           
        }
    }

    public void MaxMinGraph()
    {
        if (ob_max_min.transform.localScale.y == 1)
        {
            ob_max_min.transform.localScale = new Vector3(1, -1, 1);
            RectTransform g = p_graficas.GetComponent<RectTransform>();
            g.sizeDelta = new Vector2(g.sizeDelta.x, 40);
            g.anchoredPosition = new Vector2(g.anchoredPosition.x, 30);
            p_graficas.transform.GetChild(1).localScale = new Vector3(1, 0, 1);
            p_graficas.transform.GetChild(2).gameObject.SetActive(false);
        }
        else
        {
            ob_max_min.transform.localScale = new Vector3(1, 1, 1);
            RectTransform g = p_graficas.GetComponent<RectTransform>();
            g.sizeDelta = new Vector2(g.sizeDelta.x, 420);
            g.anchoredPosition = new Vector2(g.anchoredPosition.x, 220);
            p_graficas.transform.GetChild(1).localScale = new Vector3(1, 1, 1);
            p_graficas.transform.GetChild(2).gameObject.SetActive(true);
        }
        //ReanudarSimulacion();
    }

    public void MaxMinControl()
    {
        if (ob_max_min_control.transform.localScale.y == 1)
        {
            ob_max_min_control.transform.localScale = new Vector3(1, -1, 1);
            RectTransform g = p_control.GetComponent<RectTransform>();
            g.sizeDelta = new Vector2(g.sizeDelta.x, 40);
            g.anchoredPosition = new Vector2(g.anchoredPosition.x, 30);
            p_control.transform.GetChild(1).gameObject.SetActive(false);
            //p_graficas.transform.GetChild(2).gameObject.SetActive(false);
        }
        else
        {
            ob_max_min_control.transform.localScale = new Vector3(1, 1, 1);
            RectTransform g = p_control.GetComponent<RectTransform>();
            g.sizeDelta = new Vector2(g.sizeDelta.x, 420);
            g.anchoredPosition = new Vector2(g.anchoredPosition.x, 220);
            p_control.transform.GetChild(1).gameObject.SetActive(true);
            //p_graficas.transform.GetChild(2).gameObject.SetActive(true);
        }

    }

    public void Salir()
    {
        Application.Quit();
    }

}
