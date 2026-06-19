using UnityEngine;
using System.Collections;

/// <summary>
/// Ateş Tuzağı - Sadece animasyon ve zamanlama.
/// Collider'ları SEN Unity editöründe ekle ve ayarla!
/// 
/// KURULUM:
/// 1. GameObject oluştur
/// 2. SpriteRenderer ekle (Off sprite'ını ata)
/// 3. BoxCollider2D ekle → platform için (isTrigger KAPALI, oyuncu basar)
/// 4. Child obje oluştur "AtesAlani" → tag = "Trap"
///    → BoxCollider2D ekle (isTrigger AÇIK, ateş alanı)
/// 5. Bu scripti ana objeye ekle
/// 6. Sprite'ları Inspector'dan ata
/// </summary>
public class AtesTuzagi : MonoBehaviour
{
    [Header("===== SPRITE'LAR =====")]
    [Tooltip("Kapalı hali (Off.png)")]
    public Sprite kapaliSprite;

    [Tooltip("Ateş animasyonu frame'leri (On_0, On_1, On_2)")]
    public Sprite[] atesFrameleri;

    [Tooltip("Hit animasyonu frame'leri (Hit_0, Hit_1, Hit_2, Hit_3)")]
    public Sprite[] hitFrameleri;

    [Header("===== ZAMANLAMA =====")]
    [Tooltip("Ateşin açık kalma süresi")]
    public float atesAcikSuresi = 2f;

    [Tooltip("Ateşin kapalı kalma süresi")]
    public float atesKapaliSuresi = 2.5f;

    [Tooltip("Ateş açılmadan önce titreme süresi")]
    public float uyariSuresi = 0.5f;

    [Tooltip("Başlangıç gecikmesi")]
    public float baslangicGecikmesi = 0f;

    [Header("===== ANİMASYON =====")]
    public float animasyonHizi = 8f;
    public float hitAnimasyonHizi = 12f;

    [Header("===== ATEŞ ALANI =====")]
    [Tooltip("Ateş collider'ını içeren child obje (tag=Trap, isTrigger=true)")]
    public GameObject atesAlaniObjesi;

    // ======== Dahili ========
    private SpriteRenderer spriteRenderer;
    private bool atesAcik = false;
    private int mevcutFrame = 0;
    private float animasyonSayaci = 0f;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Başlangıçta kapalı
        if (kapaliSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = kapaliSprite;

        // Ateş alanını başta kapat
        if (atesAlaniObjesi != null)
            atesAlaniObjesi.SetActive(false);

        StartCoroutine(AtesDongusu());
    }

    // ======== ANA DÖNGÜ ========

    private IEnumerator AtesDongusu()
    {
        if (baslangicGecikmesi > 0)
            yield return new WaitForSeconds(baslangicGecikmesi);

        while (true)
        {
            // KAPALI
            AtesiKapat();
            yield return new WaitForSeconds(atesKapaliSuresi);

            // UYARI (titreme)
            if (uyariSuresi > 0)
                yield return StartCoroutine(UyariAnimasyonu());

            // AÇIK
            AtesiAc();
            yield return new WaitForSeconds(atesAcikSuresi);
        }
    }

    private void AtesiAc()
    {
        atesAcik = true;
        mevcutFrame = 0;
        animasyonSayaci = 0f;

        // Ateş alanını aç (oyuncuyu öldürür)
        if (atesAlaniObjesi != null)
            atesAlaniObjesi.SetActive(true);

        // Ateş sprite'ını göster
        if (atesFrameleri != null && atesFrameleri.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = atesFrameleri[0];
    }

    private void AtesiKapat()
    {
        atesAcik = false;

        // Ateş alanını kapat (güvenli)
        if (atesAlaniObjesi != null)
            atesAlaniObjesi.SetActive(false);

        // Kapalı sprite
        if (kapaliSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = kapaliSprite;
    }

    private IEnumerator UyariAnimasyonu()
    {
        float gecenSure = 0f;
        Vector3 orijinalPoz = transform.localPosition;

        while (gecenSure < uyariSuresi)
        {
            float yogunluk = Mathf.Lerp(0.02f, 0.06f, gecenSure / uyariSuresi);
            float hiz = Mathf.Lerp(20f, 40f, gecenSure / uyariSuresi);
            float offsetX = Mathf.Sin(gecenSure * hiz) * yogunluk;
            transform.localPosition = orijinalPoz + new Vector3(offsetX, 0f, 0f);
            gecenSure += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = orijinalPoz;
    }

    // ======== ANİMASYON ========

    private void Update()
    {
        if (!atesAcik) return;
        if (atesFrameleri == null || atesFrameleri.Length <= 1) return;

        animasyonSayaci += Time.deltaTime;
        float frameAraligi = 1f / animasyonHizi;

        if (animasyonSayaci >= frameAraligi)
        {
            animasyonSayaci -= frameAraligi;
            mevcutFrame = (mevcutFrame + 1) % atesFrameleri.Length;
            spriteRenderer.sprite = atesFrameleri[mevcutFrame];
        }
    }
}
