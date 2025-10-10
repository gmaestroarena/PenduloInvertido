using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ControlManual : MonoBehaviour
{
    public Rigidbody2D carro;
    public Slider slider_Fuerza;
    public float fuerzaMax = 6;
    public Vector2 direccion = Vector2.right;

    
    // Update is called once per frame
    void Update()
    {
        carro.totalForce = Vector2.zero;
        carro.AddForce(direccion.normalized * slider_Fuerza.value * fuerzaMax, ForceMode2D.Force);
    }
        
}
