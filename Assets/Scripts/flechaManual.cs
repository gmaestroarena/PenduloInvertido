using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class flechaManual : MonoBehaviour
{
    public Vector3 fuerza;
    public Slider sliderFuerza;

    private LineRenderer linea;
    

    void Awake()
    {
        linea = GetComponent<LineRenderer>();
        linea.positionCount = 7;
        linea.useWorldSpace = true;
        
    }

    void Update()
    {
        fuerza = new Vector3(1,0,0)* sliderFuerza.value;

        Vector3 origen = transform.parent.position;
        Vector3 destino = origen + (Vector3)fuerza*.1f;
        

        if (Mathf.Abs(fuerza.magnitude) > 3f)
        {
            linea.SetPosition(0, origen);
            linea.SetPosition(1, destino - new Vector3(.1f, 0, 0) * Mathf.Sign(fuerza.x));
            linea.SetPosition(2, destino - new Vector3(.2f, 0, 0) * Mathf.Sign(fuerza.x) + new Vector3(0, .1f, 0));
            linea.SetPosition(3, destino);
            linea.SetPosition(4, destino - new Vector3(.2f, 0, 0) * Mathf.Sign(fuerza.x) - new Vector3(0, .1f, 0));
            linea.SetPosition(5, destino - new Vector3(.1f, 0, 0) * Mathf.Sign(fuerza.x));
            linea.SetPosition(6, origen);
        }
        else
        {
            linea.SetPosition(0, origen);
            linea.SetPosition(1, destino);
            linea.SetPosition(2, origen);
            linea.SetPosition(3, destino);
            linea.SetPosition(4, origen);
            linea.SetPosition(5, destino);
            linea.SetPosition(6, origen);
            
        }
        
    }
}
