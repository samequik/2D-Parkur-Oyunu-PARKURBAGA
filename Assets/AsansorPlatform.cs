using UnityEngine;

public class AsansorPlatform : MonoBehaviour
{
    [Header("Asansor Rotalari")]
    public Transform noktaA;
    public Transform noktaB;
    public float hiz = 3f;

    private Vector3 baslangicPozisyonu;
    private bool calisiyorMu = false;

    void Start()
    {
        // Başlangıç noktasını hafızaya al
        baslangicPozisyonu = transform.position;
    }

    void FixedUpdate()
    {
        // Üstüne basıldıysa çalış
        if (calisiyorMu)
        {
            transform.position = Vector2.MoveTowards(transform.position, noktaB.position, hiz * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            calisiyorMu = true; // Motoru çalıştır
            collision.transform.SetParent(transform); // Karakteri sabitle
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (gameObject.activeInHierarchy && collision.gameObject.name == "Player")
        {
            collision.transform.SetParent(null); // Karakteri serbest bırak
        }
    }

    // --- ÖLÜNCE ÇAĞRILACAK SIFIRLAMA KODU ---
    // --- ÖLÜNCE ÇAĞRILACAK SIFIRLAMA KODU ---
    public void AsansoruSifirla()
    {
        calisiyorMu = false; // Motoru durdur
        transform.position = baslangicPozisyonu; // Geri dön
        
        // DetachChildren satırını sildik ki asansör kıyafetlerini çıkarmasın! :)
    }
}