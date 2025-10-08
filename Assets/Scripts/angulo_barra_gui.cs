using UnityEngine;

public class angulo_barra_gui : MonoBehaviour
{
    public Transform barraA;
    public Transform barraB;
    public Sector2D sector;
    public float radius = 2f;

    void Update()
    {
        Vector2 dirA = barraA.up;  // si las barras están orientadas con su "up"
        Vector2 dirB = barraB.up;

        float angleA = Mathf.Atan2(dirA.y, dirA.x) * Mathf.Rad2Deg;
        float angleB = Mathf.Atan2(dirB.y, dirB.x) * Mathf.Rad2Deg;

        sector.transform.position = barraA.position; // o punto común
        sector.radius = radius;
        sector.startAngle = angleA;
        sector.endAngle = angleB;
        sector.GenerateSector();
    }

}
