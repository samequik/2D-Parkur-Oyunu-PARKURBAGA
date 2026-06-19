using UnityEngine;

/// <summary>
/// Kameranın görmediği objelerin Animator ve hareket scriptlerini kapatır.
/// Scene View kamerasını YOKSAYAR — sadece oyun kamerasına (Camera.main) bakar.
/// 
/// KULLANIM: Mor biome'daki tüm animasyonlu/hareketli objelere ekle.
/// </summary>
public class PerformansOptimizer : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Kamera kenarından ne kadar fazla marj bırakılsın (erken açılma için)")]
    public float kenarMarji = 3.5f; // %350 marj — ekranın 3.5 katı uzaklıktan aktif olur
    
    [Tooltip("Kontrol sıklığı (saniye). Düşük = daha hassas ama daha pahalı")]
    public float kontrolSikligi = 0.25f; // Saniyede 4 kez kontrol (her frame değil!)

    // Bileşenler
    private Animator animator;
    private HareketliTuzak hareketliTuzak;
    private HareketliPlatform hareketliPlatform;
    private AsansorPlatform asansorPlatform;
    private Camera anaKamera;
    
    // Durum
    private bool aktifMi = true;

    void Start()
    {
        // Inspector'daki eski değerleri ezme — minimum marj 5.0
        if (kenarMarji < 3.5f)
            kenarMarji = 3.5f;
        
        // Bileşenleri bul
        animator = GetComponent<Animator>();
        hareketliTuzak = GetComponent<HareketliTuzak>();
        hareketliPlatform = GetComponent<HareketliPlatform>();
        asansorPlatform = GetComponent<AsansorPlatform>();
        anaKamera = Camera.main;
        
        // Her frame yerine belirli aralıklarla kontrol et (performans için)
        InvokeRepeating(nameof(GorünürlükKontrol), 0f, kontrolSikligi);
    }

    void GorünürlükKontrol()
    {
        if (anaKamera == null)
        {
            anaKamera = Camera.main;
            if (anaKamera == null) return;
        }

        // Objenin pozisyonunu kameranın viewport'una çevir
        // Viewport: (0,0) = sol alt, (1,1) = sağ üst
        Vector3 viewportPoz = anaKamera.WorldToViewportPoint(transform.position);
        
        // Z < 0 ise obje kameranın arkasında
        bool gorunuyorMu = viewportPoz.z > 0 
            && viewportPoz.x > -kenarMarji 
            && viewportPoz.x < 1f + kenarMarji 
            && viewportPoz.y > -kenarMarji 
            && viewportPoz.y < 1f + kenarMarji;

        // Durum değiştiyse güncelle
        if (gorunuyorMu && !aktifMi)
        {
            BileşenleriAç();
        }
        else if (!gorunuyorMu && aktifMi)
        {
            BileşenleriKapat();
        }
    }

    void BileşenleriKapat()
    {
        aktifMi = false;
        
        if (animator != null) animator.enabled = false;
        if (hareketliTuzak != null) hareketliTuzak.enabled = false;
        if (hareketliPlatform != null) hareketliPlatform.enabled = false;
        if (asansorPlatform != null) asansorPlatform.enabled = false;
    }

    void BileşenleriAç()
    {
        aktifMi = true;
        
        if (animator != null) animator.enabled = true;
        if (hareketliTuzak != null) hareketliTuzak.enabled = true;
        if (hareketliPlatform != null) hareketliPlatform.enabled = true;
        if (asansorPlatform != null) asansorPlatform.enabled = true;
    }

    // Editor'de görsel debug — Scene view'da hangi objeler aktif/pasif görürsün
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = aktifMi ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
