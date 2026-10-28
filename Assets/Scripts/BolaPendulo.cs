using UnityEngine;

// La bola es un Rigidbody2D dinámico unido a la barra con un FixedJoint2D. En los prefabs está colocada como HIJA de la
// barra (también dinámica) para que, en pausa, siga a la barra al cambiar el ángulo inicial.
// Problema: un Rigidbody2D dinámico hijo de otro dinámico se mueve DOS veces en cada paso (por su propia física y, además,
// arrastrado por el Transform del padre), lo que mete energía en el sistema y produce movimientos bruscos y "acelerados".
//
// Solución: al arrancar se saca la bola de la jerarquía de la barra (manteniendo su posición), y se recoloca a mano en el
// extremo de la barra cuando la barra está en pausa (Static) o cuando se aplica una perturbación.
[RequireComponent(typeof(Rigidbody2D))]
public class BolaPendulo : MonoBehaviour
{
    private Transform barra;
    private Rigidbody2D rbBarra;
    private Rigidbody2D rb;
    private Vector3 posicionLocalEnBarra;
    private Quaternion rotacionLocalEnBarra;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        barra = transform.parent;
        rbBarra = barra != null ? barra.GetComponent<Rigidbody2D>() : null;

        // Si la bola no cuelga de un Rigidbody2D (p. ej. Ejercicio0) no hay nada que hacer
        if (rbBarra == null)
        {
            enabled = false;
            return;
        }

        posicionLocalEnBarra = barra.InverseTransformPoint(transform.position);
        rotacionLocalEnBarra = Quaternion.Inverse(barra.rotation) * transform.rotation;
        transform.SetParent(barra.parent, true);
    }

    void FixedUpdate()
    {
        if (rbBarra.bodyType == RigidbodyType2D.Static) Recolocar();
    }

    void LateUpdate()
    {
        // En pausa el ángulo de la barra se cambia girando su Transform: la bola debe acompañarla
        if (rbBarra.bodyType == RigidbodyType2D.Static) Recolocar();
    }

    // Coloca la bola en el extremo de la barra (posición, giro y velocidad coherentes con la barra)
    public void Recolocar()
    {
        if (!enabled) return;

        Vector3 p = barra.TransformPoint(posicionLocalEnBarra);
        Quaternion q = barra.rotation * rotacionLocalEnBarra;
        transform.SetPositionAndRotation(p, q);
        rb.position = p;
        rb.rotation = q.eulerAngles.z;

        if (rbBarra.bodyType == RigidbodyType2D.Static)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            rb.linearVelocity = rbBarra.GetPointVelocity(p);
            rb.angularVelocity = rbBarra.angularVelocity;
        }
    }
}
