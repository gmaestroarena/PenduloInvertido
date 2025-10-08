
using UnityEngine;
using UnityEngine.UI;

public class camara : MonoBehaviour
{
    [Header("References")]
    private Camera mainCamera;
    public Image backgroundImage;
    public Image skyImage;
    public Transform carrito;

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float smoothSpeed = 5f;
    public float minZoom = 6f;
    public float maxZoom = 16f;

    [Header("Background Parallax")]
    [Range(0f, 1f)] public float backgroundZoomFactor = 0.1f; // 5%

    private float targetZoom;
    private float baseZoom;
    private Vector3 baseScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        targetZoom = mainCamera.orthographicSize;
        baseZoom = targetZoom;
        baseScale = backgroundImage.rectTransform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
       

        targetZoom = Mathf.Abs(carrito.position.x) * zoomSpeed * .3f + 3f;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        
        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            targetZoom,
            Time.deltaTime * smoothSpeed
            );
        
        // Calcula el factor relativo al zoom original
        float zoomRatio = baseZoom/mainCamera.orthographicSize;

        backgroundImage.rectTransform.anchoredPosition = new Vector2(-carrito.position.x*15, 0);
        skyImage.rectTransform.anchoredPosition = new Vector2(-carrito.position.x * 5, 0);

        // Escala el fondo ligeramente en sentido contrario (para compensar)
        float backgroundScale = Mathf.Lerp(1f, zoomRatio, backgroundZoomFactor);
        //print(targetZoom);
        backgroundImage.rectTransform.localScale = baseScale * backgroundScale;
            
    }
}
