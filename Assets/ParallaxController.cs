using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [Header("Kamera Ayarı")]
    public Transform cam; 
    
    [Header("Hız Ayarları")]
    [Range(0f, 1f)] 
    public float parallaxMultiplierX = 0.5f; // Sağa-Sola kayma hızı
    
    [Range(0f, 1f)] 
    public float parallaxMultiplierY = 0.5f; // YENİ: Aşağı-Yukarı kayma hızı

    private Vector3 previousCamPos;

    void Start()
    {
        if (cam == null) cam = Camera.main.transform;
        previousCamPos = cam.position;
    }

    void LateUpdate() 
    {
        // Hem X hem de Y eksenindeki değişimi hesaplıyoruz
        float deltaX = cam.position.x - previousCamPos.x;
        float deltaY = cam.position.y - previousCamPos.y; // YENİ: Zıplama ve düşme farkı
        
        // Objemizi her iki eksende de kendi çarpanına göre kaydırıyoruz
        transform.position += new Vector3(deltaX * parallaxMultiplierX, deltaY * parallaxMultiplierY, 0);
        
        previousCamPos = cam.position;
    }
}