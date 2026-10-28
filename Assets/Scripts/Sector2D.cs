using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(LineRenderer))]
public class Sector2D : MonoBehaviour
{
    [Header("Sector Settings")]
    public float radius = 5f;
    public float startAngle = 0f;  // grados
    public float endAngle = 90f;   // grados
    [Range(3, 256)] public int segments = 64;
    [Range(0f, 1f)] public float alpha = 0.5f;
    public Color baseColor = Color.yellow;

    [Header("Outline Settings")]
    public bool drawArc = true;
    public bool drawRadii = false;
    public Color outlineColor = Color.white;
    public float outlineWidth = 0.05f;

    // internas
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private LineRenderer lineRenderer;

    // Init seguro: crea/obtiene componentes y el mesh si hace falta.
    void InitIfNeeded()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

        // Preferimos sharedMesh en editor para que se vea en Scene; en play usamos sharedMesh también es OK
        if (mf.sharedMesh == null)
        {
            mesh = new Mesh();
            mesh.name = $"Sector2D_Mesh_{GetEntityId()}";
          mf.sharedMesh = mesh;
        }
        else
        {
            mesh = mf.sharedMesh;
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (meshRenderer.sharedMaterial == null)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            meshRenderer.sharedMaterial = mat;
        }

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) lineRenderer = gameObject.AddComponent<LineRenderer>();
        if (lineRenderer.sharedMaterial == null)
        {
            lineRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        // Ajustes seguros por defecto
        meshRenderer.sortingLayerName = meshRenderer.sortingLayerName ?? "Default";
        meshRenderer.sortingOrder = 10;

        lineRenderer.useWorldSpace = false;
        lineRenderer.numCapVertices = 0;
        lineRenderer.numCornerVertices = 0;
        lineRenderer.widthMultiplier = outlineWidth;
        lineRenderer.startWidth = outlineWidth;
        lineRenderer.endWidth = outlineWidth;
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;
        lineRenderer.sortingLayerName = meshRenderer.sortingLayerName;
        lineRenderer.sortingOrder = meshRenderer.sortingOrder + 1;
    }

    void Awake()
    {
        InitIfNeeded();
        GenerateSector();
    }

    void OnValidate()
    {
        // En editor, puede llamarse antes de Awake — inicializamos si hace falta y regeneramos.
        InitIfNeeded();
        // Solo regeneramos si el mesh existe
        if (mesh != null)
            GenerateSector();
    }

    // Llamable desde otros scripts; protege si mesh == null
    public void GenerateSector()
    {
        if (mesh == null)
        {
            InitIfNeeded();
            if (mesh == null) return;
        }

        // seguridad: válidas
        if (segments < 3) segments = 3;
        if (radius <= 0f) radius = 0.01f;

        mesh.Clear();

        int vertCount = segments + 2;
        Vector3[] vertices = new Vector3[vertCount];
        int[] triangles = new int[segments * 3];
        Color[] colors = new Color[vertCount];

        Color c = baseColor;
        c.a = alpha;

        vertices[0] = Vector3.zero;
        colors[0] = c;

        float startRad = startAngle * Mathf.Deg2Rad;
        float endRad = endAngle * Mathf.Deg2Rad;
        float step = (endRad - startRad) / segments;

        // --- Generar vértices del sector ---
        for (int i = 0; i <= segments; i++)
        {
            float a = startRad + i * step;
            vertices[i + 1] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
            colors[i + 1] = c;
        }

        // --- Triángulos del relleno ---
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // material color (si usas Sprites/Default, el tint funciona)
        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            meshRenderer.sharedMaterial.color = c;

        // actualizar línea (arco solamente)
        DrawOutlineArc(vertices);
    }

    private void DrawOutlineArc(Vector3[] vertices)
    {
        if (lineRenderer == null) return;

        if (!drawArc && !drawRadii)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // arco = vertices[1] .. vertices[segments+1]
        if (drawArc)
        {
            int count = vertices.Length - 1; // segments+1
            lineRenderer.positionCount = count;
            // copiar arc points directamente (no cerrar), en local space (useWorldSpace=false)
            for (int i = 0; i < count; i++)
                lineRenderer.SetPosition(i, vertices[i + 1]);
            lineRenderer.startColor = outlineColor;
            lineRenderer.endColor = outlineColor;
            lineRenderer.widthMultiplier = outlineWidth;
        }
        else
        {
            lineRenderer.positionCount = 0;
        }

        // radii: dibujamos con Debug.DrawLine en editor/play o (si quieres visual real) podrías crear
        // un LineRenderer adicional para cada radio. Por simplicidad ahora usamos Debug.DrawLine
        if (drawRadii)
        {
            // radii en world (transform.TransformPoint)
            Vector3 worldCenter = transform.position;
            Vector3 worldA = transform.TransformPoint(vertices[1]);
            Vector3 worldB = transform.TransformPoint(vertices[vertices.Length - 1]);
            Debug.DrawLine(worldCenter, worldA, outlineColor);
            Debug.DrawLine(worldCenter, worldB, outlineColor);
        }
    }
}
