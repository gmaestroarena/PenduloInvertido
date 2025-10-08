using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dibuja un anillo (círculo con grosor) en el sistema UI sin texturas.
/// </summary>
[ExecuteAlways]
public class UICircle : Graphic
{
    [Range(3, 256)]
    public int segments = 128;

    [Range(0f, 1f)]
    public float fillPercent = 1f; // 1 = círculo completo

    [Range(0f, 1f)]
    public float thickness = 0.1f; // grosor relativo al radio

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float outerRadius = rectTransform.rect.width * 0.5f;
        float innerRadius = outerRadius * (1f - thickness);
        float angleStep = 360f * fillPercent / segments;

        Vector3 center = Vector3.zero;
        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 outer = new Vector3(cos * outerRadius, sin * outerRadius);
            Vector3 inner = new Vector3(cos * innerRadius, sin * innerRadius);

            UIVertex v1 = UIVertex.simpleVert;
            v1.color = color;
            v1.position = outer;

            UIVertex v2 = UIVertex.simpleVert;
            v2.color = color;
            v2.position = inner;

            vh.AddVert(v1);
            vh.AddVert(v2);
        }

        // Crear triángulos
        for (int i = 0; i < segments * 2; i += 2)
        {
            vh.AddTriangle(i, i + 1, i + 3);
            vh.AddTriangle(i, i + 3, i + 2);
        }
    }
}

