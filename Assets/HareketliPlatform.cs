using UnityEngine;

public class HareketliPlatform : MonoBehaviour
{
    public Transform noktaA;
    public Transform noktaB;
    public float hiz = 3f;

    private Transform hedefNokta;

    void Start()
    {
        hedefNokta = noktaB;
    }

    void FixedUpdate()
    {
        // Platformu yürüt
        transform.position = Vector2.MoveTowards(transform.position, hedefNokta.position, hiz * Time.fixedDeltaTime);

        if (Vector2.Distance(transform.position, hedefNokta.position) < 0.1f)
        {
            hedefNokta = (hedefNokta == noktaB) ? noktaA : noktaB;
        }
    }

    // ÇARPIŞMA ANINDA (Direkt Hiyerarşideki İsmine Bakıyoruz)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Eğer çarpan objenin Hiyerarşideki adı tam olarak "Player" ise:
        if (collision.gameObject.name == "Player")
        {
            collision.transform.SetParent(transform);
        }
    }

    // PLATFORMDAN AYRILINCA
    // PLATFORMDAN AYRILINCA
    private void OnCollisionExit2D(Collision2D collision)
    {
        // Unity sahneyi kapatırken hata vermesin diye "Obje aktif mi?" kontrolü ekledik
        if (gameObject.activeInHierarchy && collision.gameObject.name == "Player")
        {
            collision.transform.SetParent(null);
        }
    }
}