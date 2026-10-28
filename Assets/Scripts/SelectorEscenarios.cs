using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Botón que abre un menú para saltar a cualquiera de los pasos de la práctica. Toda la interfaz se construye por código.
//  - JuntoAlManual: botón con icono en la barra superior, junto al del manual (Ejercicio3 e InvertedPendulum).
//  - EsquinaInferiorIzquierda: botón semitransparente con el texto "Menú" (Ejercicio0, 1 y 2, que no tienen barra).
public class SelectorEscenarios : MonoBehaviour
{
    [System.Serializable]
    public struct Escenario
    {
        public string titulo;
        public string rutaEscena;
    }

    public enum Ubicacion { JuntoAlManual, EsquinaInferiorIzquierda }

    [Tooltip("Dónde se coloca el botón que abre el menú")]
    public Ubicacion ubicacion = Ubicacion.JuntoAlManual;

    [Tooltip("Icono del botón (lista-de-tareas.png); solo se usa junto al manual")]
    public Sprite icono;

    [Tooltip("Nombre del botón del manual, junto al que se coloca el nuevo botón")]
    public string botonReferencia = "btn_INSTRUCCIONES";

    [Tooltip("Desplazamiento horizontal respecto al botón del manual (negativo = a la izquierda)")]
    public float desplazamientoX = -50f;

    public Escenario[] escenarios = new Escenario[]
    {
        new Escenario { titulo = "Paso 1: Objeto Péndulo",              rutaEscena = "Assets/Scenes/Ejercicio0.unity" },
        new Escenario { titulo = "Paso 2: Control Manual",              rutaEscena = "Assets/Scenes/Ejercicio1.unity" },
        new Escenario { titulo = "Paso 3: Control PID básico",          rutaEscena = "Assets/Scenes/Ejercicio2.unity" },
        new Escenario { titulo = "Paso 4: Añadimos gráficas y manual",  rutaEscena = "Assets/Scenes/Ejercicio3.unity" },
        new Escenario { titulo = "Paso 5: Proyecto final",              rutaEscena = "Assets/Scenes/InvertedPendulum.unity" },
    };

    private GameObject panelMenu;
    private bool reanudarAlCerrar = false;

    void Start()
    {
        AsegurarEventSystem();

        Canvas canvasInterfaz = null;
        GameObject referencia = ubicacion == Ubicacion.JuntoAlManual ? GameObject.Find(botonReferencia) : null;
        if (referencia != null)
        {
            CrearBoton(referencia.GetComponent<RectTransform>());
            canvasInterfaz = referencia.GetComponentInParent<Canvas>().rootCanvas;
        }
        else
        {
            if (ubicacion == Ubicacion.JuntoAlManual)
                Debug.LogWarning($"SelectorEscenarios: no se encuentra '{botonReferencia}'; se usa el botón de esquina");
            CrearBotonEsquina();
        }

        CrearMenu(canvasInterfaz);
        panelMenu.SetActive(false);
    }

    // ---------- Construcción de la interfaz ----------

    void CrearBoton(RectTransform referencia)
    {
        GameObject go = new GameObject("btn_ESCENARIOS", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(referencia.parent, false);
        rt.anchorMin = referencia.anchorMin;
        rt.anchorMax = referencia.anchorMax;
        rt.pivot = referencia.pivot;
        rt.sizeDelta = referencia.sizeDelta;
        rt.anchoredPosition = referencia.anchoredPosition + new Vector2(desplazamientoX, 0f);

        Image img = go.GetComponent<Image>();
        img.sprite = icono;
        img.preserveAspect = true;

        go.GetComponent<Button>().onClick.AddListener(AbrirMenu);
    }

    // Sin EventSystem los botones de UI no reciben clics (Ejercicio0 no tiene)
    void AsegurarEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    // Canvas propio en modo Overlay (siempre por encima de la escena) con escala de referencia 1920x1080
    GameObject CrearCanvasOverlay(string nombre, int orden, Canvas referencia)
    {
        GameObject go = new GameObject(nombre, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = orden;

        CanvasScaler escala = go.GetComponent<CanvasScaler>();
        CanvasScaler escalaOriginal = referencia != null ? referencia.GetComponent<CanvasScaler>() : null;
        if (escalaOriginal != null)
        {
            escala.uiScaleMode = escalaOriginal.uiScaleMode;
            escala.referenceResolution = escalaOriginal.referenceResolution;
            escala.screenMatchMode = escalaOriginal.screenMatchMode;
            escala.matchWidthOrHeight = escalaOriginal.matchWidthOrHeight;
        }
        else
        {
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.matchWidthOrHeight = 0.5f;
        }
        return go;
    }

    // Botón semitransparente "Menú" en la esquina inferior izquierda
    void CrearBotonEsquina()
    {
        GameObject canvasBoton = CrearCanvasOverlay("BotonMenuEscenarios", 900, null);

        GameObject go = new GameObject("btn_MENU", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(canvasBoton.transform, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
        rt.anchoredPosition = new Vector2(20f, 20f);
        rt.sizeDelta = new Vector2(130f, 50f);
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        Button b = go.GetComponent<Button>();
        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1.6f, 1.6f, 1.6f, 1.6f);   // más opaco al pasar el ratón
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        b.colors = cb;
        b.onClick.AddListener(AbrirMenu);

        TextMeshProUGUI tmp = CrearTexto(rt, "Menú", 26, FontStyles.Bold, 0f);
        tmp.rectTransform.anchorMin = Vector2.zero;
        tmp.rectTransform.anchorMax = Vector2.one;
        tmp.rectTransform.offsetMin = tmp.rectTransform.offsetMax = Vector2.zero;
        tmp.color = new Color(1f, 1f, 1f, 0.85f);
    }

    void CrearMenu(Canvas canvas)
    {
        // El menú va en un Canvas propio en modo "Screen Space - Overlay": se dibuja siempre encima de todo
        // (el Canvas de la interfaz es "Screen Space - Camera" y los sprites del péndulo pueden quedar delante).
        panelMenu = CrearCanvasOverlay("MenuEscenarios", 1000, canvas);

        // Fondo semitransparente a pantalla completa: bloquea los clics al resto de la interfaz
        GameObject goFondo = new GameObject("Fondo", typeof(RectTransform), typeof(Image));
        RectTransform fondo = goFondo.GetComponent<RectTransform>();
        fondo.SetParent(panelMenu.transform, false);
        fondo.anchorMin = Vector2.zero;
        fondo.anchorMax = Vector2.one;
        fondo.offsetMin = Vector2.zero;
        fondo.offsetMax = Vector2.zero;
        goFondo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        // Caja central con los botones apilados
        GameObject caja = new GameObject("Caja", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform rtCaja = caja.GetComponent<RectTransform>();
        rtCaja.SetParent(fondo, false);
        rtCaja.anchorMin = rtCaja.anchorMax = rtCaja.pivot = new Vector2(0.5f, 0.5f);
        rtCaja.sizeDelta = new Vector2(560f, 0f);
        caja.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

        VerticalLayoutGroup vlg = caja.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 20, 24);
        vlg.spacing = 10f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        caja.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CrearTexto(rtCaja, "Selecciona el escenario", 30, FontStyles.Bold, 50f);

        string actual = SceneManager.GetActiveScene().path;
        foreach (Escenario e in escenarios)
        {
            Escenario esc = e;
            bool esActual = esc.rutaEscena == actual;
            Button b = CrearBotonMenu(rtCaja, esActual ? esc.titulo + "  (actual)" : esc.titulo, new Color(0.22f, 0.45f, 0.75f));
            b.onClick.AddListener(() => CargarEscenario(esc.rutaEscena));
        }

        CrearEspacio(rtCaja, 8f);
        Button cancelar = CrearBotonMenu(rtCaja, "Cancelar", new Color(0.45f, 0.45f, 0.45f));
        cancelar.onClick.AddListener(CerrarMenu);
    }

    Button CrearBotonMenu(RectTransform padre, string texto, Color color)
    {
        GameObject go = new GameObject("btn_" + texto, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.GetComponent<RectTransform>().SetParent(padre, false);
        go.GetComponent<Image>().color = color;
        go.GetComponent<LayoutElement>().preferredHeight = 52f;

        Button b = go.GetComponent<Button>();
        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        b.colors = cb;

        TextMeshProUGUI tmp = CrearTexto(go.GetComponent<RectTransform>(), texto, 24, FontStyles.Normal, 0f);
        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(16f, 0f);
        rt.offsetMax = new Vector2(-16f, 0f);
        return b;
    }

    TextMeshProUGUI CrearTexto(RectTransform padre, string texto, float tam, FontStyles estilo, float alto)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.GetComponent<RectTransform>().SetParent(padre, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tam;
        tmp.fontStyle = estilo;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        if (alto > 0f)
        {
            go.AddComponent<LayoutElement>().preferredHeight = alto;
        }
        return tmp;
    }

    void CrearEspacio(RectTransform padre, float alto)
    {
        GameObject go = new GameObject("Espacio", typeof(RectTransform), typeof(LayoutElement));
        go.GetComponent<RectTransform>().SetParent(padre, false);
        go.GetComponent<LayoutElement>().preferredHeight = alto;
    }

    // ---------- Comportamiento ----------

    void AbrirMenu()
    {
        // Mientras el menú está abierto, la simulación se pausa (como al abrir el manual)
        controlPID control = FindAnyObjectByType<controlPID>();
        GUI gui = FindAnyObjectByType<GUI>();
        reanudarAlCerrar = control != null && control.estado == Estado.Activo;
        if (reanudarAlCerrar && gui != null) gui.PausarSimulacion();

        panelMenu.SetActive(true);
    }

    void CerrarMenu()
    {
        panelMenu.SetActive(false);
        GUI gui = FindAnyObjectByType<GUI>();
        if (reanudarAlCerrar && gui != null) gui.ReanudarSimulacion();
        reanudarAlCerrar = false;
    }

    void CargarEscenario(string ruta)
    {
#if UNITY_EDITOR
        // En el editor se carga por ruta, aunque la escena no esté en la lista del build
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(ruta, new LoadSceneParameters(LoadSceneMode.Single));
#else
        if (!Application.CanStreamedLevelBeLoaded(ruta))
        {
            Debug.LogError($"La escena '{ruta}' no está incluida en el build (File > Build Profiles).");
            return;
        }
        SceneManager.LoadScene(ruta);
#endif
    }
}
