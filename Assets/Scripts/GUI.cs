using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GUI : MonoBehaviour
{
    controlPID control;
    
    GameObject ob_sliderLimit;
    GameObject ob_max_min;
    GameObject ob_max_min_control;
    GameObject p_graficas;
    GameObject p_control;

    Image b_play;
    Image b_pause;

    GameObject book;

    bool reanudarAlOcultarInstrucciones = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        control = GameObject.FindGameObjectWithTag("controlPID").GetComponent<controlPID>();
        book = GameObject.Find("INSTRUCCIONES");
        b_play = GameObject.Find("btn_PLAY").GetComponent<Image>();
        b_pause = GameObject.Find("btn_PAUSE").GetComponent<Image>();
        book.SetActive(false);
        p_graficas = GameObject.Find("PanelGráficas");
        p_control = GameObject.Find("PanelControl");
        ob_sliderLimit = GameObject.Find("Slider_limF");
        ob_sliderLimit.GetComponent<Slider>().enabled = false;
        ob_sliderLimit.GetComponent<Slider>().interactable = false;
        ob_sliderLimit.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        ob_sliderLimit.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0f);
        ob_sliderLimit.transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(false);
        ob_max_min = GameObject.Find("btn_max_min");
        ob_max_min_control = GameObject.Find("btn_max_min_control");
        GameObject.Find("WindowGraph_g1").GetComponent<Window_Graph>().DibujaGrafica();
        b_play.color = new Color(1, 1, 1, .25f);
        b_pause.color = new Color(1, 1, 1, 1f);
    }
        

    public void ReiniciarSimulacion()
    {
        SceneManager.LoadScene("Escena_PenduloInvertido");
    }

    public void PausarSimulacion()
    {
        GameObject[] componentes = GameObject.FindGameObjectsWithTag("componentePendulo");
        foreach (GameObject componente in componentes)
        {
            componente.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
        control.estado = Estado.Pausa;
        GameObject.Find("AnguloInicial").GetComponent<TextMeshProUGUI>().text = "\u0398<sub>inicial</sub> =";
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
        GameObject.Find("AnguloInicial").GetComponent<TextMeshProUGUI>().text = "\u0398<sub>pert</sub> =";
        b_play.color = new Color(1, 1, 1, 1f);
        b_pause.color = new Color(1, 1, 1, .25f);

        //GameObject.Find("WindowGraph1").GetComponent<Window_Graph>().DibujaGrafica(Variable.Theta);
        //GameObject.Find("WindowGraph_g1").GetComponent<Window_Graph>().DibujaGrafica();

    }

    public void LimiteFuerza()
    {
        Toggle t = FindAnyObjectByType<Toggle>();
       
        if (t.isOn)
        {
            //ob_sliderLimit.SetActive(true);
            ob_sliderLimit.GetComponent<Slider>().enabled = true;
            ob_sliderLimit.GetComponent<Slider>().interactable = true;
            ob_sliderLimit.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            ob_sliderLimit.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            ob_sliderLimit.transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(true);
            ob_sliderLimit.transform.GetChild(3).gameObject.SetActive(true);
        }
        else
        {
            //ob_sliderLimit.SetActive(false);
            
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
