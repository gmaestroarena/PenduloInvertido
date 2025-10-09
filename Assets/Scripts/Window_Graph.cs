using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Graph : MonoBehaviour
{


    [SerializeField] private Sprite circuloSpriteVerde;
    [SerializeField] private Sprite circuloSpriteRojo;
    private Variable variable;
    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private List<GameObject> gameObjectList_fuerza;
    private List<GameObject> gameObjectList_angulo;
    
    private GameObject go_barra;
    private controlPID script_control;
    
    public List<float> valores_fuerza;
    public List<float> valores_angulo;
    public List<Vector2> posiciones_fuerza;
    public List<Vector2> posiciones_angulo;

    private bool graficaFuerzaVisible = true;
    private bool graficaAnguloVisible = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        script_control = GameObject.FindGameObjectWithTag("controlPID").GetComponent<controlPID>();
        go_barra = GameObject.Find("barra");

        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();

        gameObjectList_fuerza = new List<GameObject>();
        gameObjectList_angulo = new List<GameObject>();

        posiciones_fuerza = new List<Vector2>();
        posiciones_angulo = new List<Vector2>();

        //Necesita conectarse a ControlPID y Barra para capturar los valores en cada intervalo de muestreo para actualizar variables de las dos gráficas


    }

    private GameObject CrearCirculo(Variable v, Vector2 anchoredPosition)
    {
        GameObject go = new GameObject("circulo", typeof(Image));
        go.transform.SetParent(graphContainer, false);
        if (v == Variable.F_Mando) 
        {
            go.GetComponent<Image>().sprite = circuloSpriteVerde;
        }
        else
        {
               go.GetComponent<Image>().sprite = circuloSpriteRojo;
        }
            
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(3, 3);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        return go;
    }

    public void MostrarGrafica(List<float> valores_fuerza, List<float> valores_angulo, int maxNumValoresVisibles = -1)
    {
        if (maxNumValoresVisibles <= 0)
        {
            maxNumValoresVisibles = valores_fuerza.Count;
        }

        maxNumValoresVisibles = (maxNumValoresVisibles-1)*2;

        foreach (GameObject gameObject in gameObjectList_fuerza)
        {
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in gameObjectList_angulo)
        {
            Destroy(gameObject);
        }

        gameObjectList_fuerza.Clear();
        gameObjectList_angulo.Clear();

        float anchura_graph = graphContainer.sizeDelta.x-10;
        float altura_graph = graphContainer.sizeDelta.y;
        


        float yMax = 200;
        float yMin = -200;

        float yDif = yMax - yMin;
        if (yDif <= 0)
        {
            yDif = 5f;
        }
        
        //yMax = yMax + yDif * 0.2f;
        //yMin = yMin - yDif * 0.2f;
        
        // Si queremos que la gráfica tenga como valor más bajo el 0
        //yMin = 0f;

        float xSize = anchura_graph / (maxNumValoresVisibles+3);

        int xIndex = 0;

        

        // Puntos de la gráfica FUERZA
       
        posiciones_fuerza.Clear();
        if (graficaFuerzaVisible)
        {
            for (int i = Mathf.Max(valores_fuerza.Count - maxNumValoresVisibles, 0) + 1; i < valores_fuerza.Count; i++)
            {
                float pos_x = 35 + xSize + xIndex * xSize;
                float pos_y = valores_fuerza[i] * 0.9f;
                pos_y = altura_graph/2.0f + Mathf.Clamp(pos_y, -180, 180);
                //GameObject go_circulo = CrearCirculo(Variable.F_Mando, new Vector2(pos_x, pos_y));
                //gameObjectList_fuerza.Add(go_circulo);
                posiciones_fuerza.Add(new Vector2(pos_x, pos_y));
                
                if (xIndex != 0)
                {
                    GameObject go_conector = ConectorPuntos(Variable.F_Mando, posiciones_fuerza[xIndex], posiciones_fuerza[xIndex - 1]);
                    gameObjectList_fuerza.Add(go_conector);
                }
                xIndex += 1;
            }
            if (valores_fuerza.Count > 300)
            {
                valores_fuerza.RemoveAt(0);
            }
        }
        

        // Puntos de la gráfica ANGULO
        
        xIndex = 0;
        posiciones_angulo.Clear();
        if (graficaAnguloVisible)
        {
            for (int i = Mathf.Max(valores_angulo.Count - maxNumValoresVisibles, 0) + 1; i < valores_angulo.Count; i++)
            {
                float pos_x = 35 + xSize + xIndex * xSize;
                float pos_y = -valores_angulo[i] * 800 / Mathf.PI;
                pos_y = altura_graph / 2.0f + Mathf.Clamp(pos_y, -180, 180);
                //GameObject go_circulo = CrearCirculo(Variable.Theta, new Vector2(pos_x, pos_y));
                //gameObjectList_angulo.Add(go_circulo);
                posiciones_angulo.Add(new Vector2(pos_x, pos_y));
     
                if (xIndex != 0)
                {
                    GameObject go_conector = ConectorPuntos(Variable.Theta, posiciones_angulo[xIndex], posiciones_angulo[xIndex - 1]);
                    gameObjectList_angulo.Add(go_conector);
                }
                xIndex += 1;
            }
            if (valores_angulo.Count > 300)
            {
                valores_angulo.RemoveAt(0);
            }
        }
    }

    private GameObject ConectorPuntos(Variable v, Vector2 dotPosA, Vector2 dotPosB)
    {
        GameObject go = new GameObject("segmento", typeof(Image));
        go.transform.SetParent(graphContainer, false);
        if (v == Variable.F_Mando)
        {
            go.GetComponent<Image>().color = new Color(0, 1, 0, 1);
        }
        else
        {
            go.GetComponent<Image>().color = new Color(1, 0, 0, 1);
        }
            
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        Vector2 dir = (dotPosB - dotPosA).normalized;
        float distancia = Vector2.Distance(dotPosA, dotPosB);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(distancia, 2);
        rectTransform.anchoredPosition = dotPosA + dir * distancia * 0.5f; 
        rectTransform.localEulerAngles = new Vector3(0,0,Vector2.SignedAngle(new Vector2(1,0),dir));
        return go;
    }

    public void actualizaListaAleatoria(int nPuntos, int nPuntosVisibles)
    {
        valores_fuerza.Clear();
        valores_angulo.Clear();
        for (int i = 0; i < nPuntos; i++)
        {
            valores_fuerza.Add(UnityEngine.Random.Range(-500, 500));
            valores_angulo.Add(UnityEngine.Random.Range(-500, 500));
        }
        MostrarGrafica(valores_fuerza, valores_angulo, nPuntosVisibles);
    }

    public void actualizaListasReales(int nPuntosVisibles)
    {
        valores_fuerza.Add(script_control.mando_fx);
        valores_angulo.Add(go_barra.transform.rotation.z);
        MostrarGrafica(valores_fuerza, valores_angulo, nPuntosVisibles);        
    }

    public void ResetGrafica()
    {
        valores_angulo.Clear();
        valores_fuerza.Clear();
    }

    public void DibujaGrafica()
    {
        StartCoroutine(MyPeriodicFunction(0.1f, 150, 150));
    }

    public void VisibilidadFuerza()
    {
        graficaFuerzaVisible = !graficaFuerzaVisible;
    }

    public void VisibilidadAngulo()
    {
        graficaAnguloVisible = !graficaAnguloVisible;
    }

    IEnumerator MyPeriodicFunction( float intervalo, int nPuntos, int nPuntosVisibles)
    {
        valores_fuerza.Clear();
        valores_angulo.Clear();
        while (true)
        {
            // Your function logic here
            //actualizaListaAleatoria(nPuntos, nPuntosVisibles);
            if (script_control.estado != Estado.Pausa)
            {
                actualizaListasReales(nPuntosVisibles);
            }
                
            yield return new WaitForSeconds(intervalo); // Wait for the specified interval
        }
    }
}
