using UnityEngine;

public class dibuja_ejes : MonoBehaviour
{
    public UILineRendererLite line;
    public Vector2 A;
    public Vector2 B;
    public bool linea_discontinua;

    void Start()
    {
        if (linea_discontinua)
        {
            line.SetDashedLine(A, B, line.dashLength, line.gapLength);
        }
        else
        {
            line.SetLine(A, B);
        }
            
        
    }
}
