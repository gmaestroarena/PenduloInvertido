using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UILineRendererLite : Graphic
{
    [Header("General")]
    public float thickness = 2f;

    [Header("Dashed line")]
    public bool dashed = false;
    public float dashLength = 10f;
    public float gapLength = 5f;

    private List<(Vector2, Vector2)> segments = new();

    // -------------------------------------------------------------
    public void Clear()
    {
        segments.Clear();
        SetVerticesDirty();
    }

    // -------------------------------------------------------------
    public void SetLine(Vector2 start, Vector2 end)
    {
        Clear();
        dashed = false;
        segments.Add((start, end));
        SetVerticesDirty();
    }

    public void SetDashedLine(Vector2 start, Vector2 end, float dash, float gap)
    {
        Clear();
        dashed = true;
        dashLength = dash;
        gapLength = gap;
        segments = GenerateDashes(start, end, dash, gap);
        SetVerticesDirty();
    }

    // -------------------------------------------------------------
    public void SetPolygon(Vector2[] points, bool loop = true)
    {
        Clear();
        if (points == null || points.Length < 2)
            return;

        for (int i = 0; i < points.Length - 1; i++)
            segments.Add((points[i], points[i + 1]));

        if (loop)
            segments.Add((points[^1], points[0]));

        if (dashed)
            segments = GenerateDashedSegments(segments, dashLength, gapLength);

        SetVerticesDirty();
    }

    // -------------------------------------------------------------
    public void SetCircle(Vector2 center, float radius, int resolution = 64)
    {
        Clear();
        if (resolution < 3) resolution = 3;

        Vector2[] pts = new Vector2[resolution];
        float step = Mathf.PI * 2f / resolution;

        for (int i = 0; i < resolution; i++)
        {
            float a = i * step;
            pts[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }

        SetPolygon(pts, true);
    }

    // -------------------------------------------------------------
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        float half = thickness * 0.5f;

        foreach (var (a, b) in segments)
            DrawSegment(vh, a, b, half);
    }

    // -------------------------------------------------------------
    private void DrawSegment(VertexHelper vh, Vector2 a, Vector2 b, float half)
    {
        Vector2 dir = (b - a).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * half;

        int idx = vh.currentVertCount;
        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        v.position = a - normal; vh.AddVert(v);
        v.position = a + normal; vh.AddVert(v);
        v.position = b + normal; vh.AddVert(v);
        v.position = b - normal; vh.AddVert(v);

        vh.AddTriangle(idx + 0, idx + 1, idx + 2);
        vh.AddTriangle(idx + 2, idx + 3, idx + 0);
    }

    // -------------------------------------------------------------
    private List<(Vector2, Vector2)> GenerateDashes(Vector2 a, Vector2 b, float dash, float gap)
    {
        var list = new List<(Vector2, Vector2)>();
        Vector2 dir = (b - a).normalized;
        float dist = Vector2.Distance(a, b);
        float cur = 0f;
        bool draw = true;

        while (cur < dist)
        {
            float len = draw ? dash : gap;
            float seg = Mathf.Min(len, dist - cur);
            if (draw)
                list.Add((a + dir * cur, a + dir * (cur + seg)));
            cur += seg;
            draw = !draw;
        }
        return list;
    }

    private List<(Vector2, Vector2)> GenerateDashedSegments(List<(Vector2, Vector2)> src, float dash, float gap)
    {
        var list = new List<(Vector2, Vector2)>();
        foreach (var (a, b) in src)
            list.AddRange(GenerateDashes(a, b, dash, gap));
        return list;
    }
}