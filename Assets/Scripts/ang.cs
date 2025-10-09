using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ang : MonoBehaviour
{
    [SerializeField] Transform barra;
    public Sector2D sector2D;
    
       

    // Update is called once per frame
    void Update()
    {
        float theta = Mathf.RoundToInt(-barra.rotation.z * 360 / Mathf.PI);
        GetComponent<TextMeshPro>().text = theta.ToString()+"º";
        if (Mathf.Abs(theta) < 5)
        {
            GetComponent<TextMeshPro>().text = "";
        }
        GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, (sector2D.startAngle + sector2D.endAngle)/2 - 90);
        float cos_a = Mathf.Cos(GetComponent<RectTransform>().eulerAngles.z*Mathf.PI/180);
        float sin_a = Mathf.Sin(GetComponent<RectTransform>().eulerAngles.z * Mathf.PI / 180);
        GetComponent<RectTransform>().anchoredPosition = transform.up * 15;
        
    }
}
