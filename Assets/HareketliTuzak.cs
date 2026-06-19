using UnityEngine;

public class HareketliTuzak : MonoBehaviour
{
    [Header("Devriye Ayarlari")]
    public Transform noktaA;
    public Transform noktaB;
    public float hiz = 5f;
    public float beklemeSuresi = 1.5f; 

    private Transform hedefNokta;
    private float beklemeSayaci;
    private bool bekliyorMu = false; // HATA BURADAYDI, BOŞLUK SİLİNDİ!

    void Start()
    {
        hedefNokta = noktaB;
    }

    void Update()
    {
        // Eğer bekleme modundaysak...
        if (bekliyorMu)
        {
            beklemeSayaci -= Time.deltaTime; // Geriye doğru say
            if (beklemeSayaci <= 0)
            {
                bekliyorMu = false; // Süre doldu, yürümeye başla
            }
            return; // Kodun geri kalanını okuma, burada dur
        }

        // 1. Testereyi hedefe doğru yürüt
        transform.position = Vector2.MoveTowards(transform.position, hedefNokta.position, hiz * Time.deltaTime);

        // 2. Hedefe vardık mı?
        if (Vector2.Distance(transform.position, hedefNokta.position) < 0.1f)
        {
            // Hedefe vardık! Bekleme moduna geç
            bekliyorMu = true;
            beklemeSayaci = beklemeSuresi; // Kronometreyi kur

            // Hedefi değiştir
            if (hedefNokta == noktaB)
            {
                hedefNokta = noktaA;
            }
            else
            {
                hedefNokta = noktaB;
            }
        }
    }
}