using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Level sonundaki meyve objesi. Oyuncu dokunduğunda yetenek açar,
/// ekranda bilgi mesajı gösterir ve kendini yok eder.
/// 
/// KULLANIM:
/// 1. Boş bir GameObject oluştur, sprite ekle (meyve resmi)
/// 2. Collider2D ekle (isTrigger = true)
/// 3. Bu scripti ekle
/// 4. Inspector'dan hangi yeteneği açacağını seç
/// 5. Portalın yakınına yerleştir
/// 
/// NOT: UI mesajı otomatik oluşturulur, elle Canvas/Text eklemeye gerek yok!
/// </summary>
public class MeyveItem : MonoBehaviour
{
    [Header("Yetenek Ayarı")]
    [Tooltip("Bu meyve hangi yeteneği açacak?")]
    public YetenekTipi acilacakYetenek;

    [Header("Mesaj Ayarları")]
    [Tooltip("Mesajın ekranda kalma süresi")]
    public float mesajSuresi = 3f;

    [Header("Efektler")]
    [Tooltip("Toplama ses efekti")]
    public AudioClip toplamaSound;

    [Tooltip("Meyvenin sallanma hızı (idle animasyon)")]
    public float sallanmaHizi = 2f;
    
    [Tooltip("Meyvenin sallanma mesafesi")]
    public float sallanmaMesafesi = 0.15f;

    private Vector3 baslangicPoz;
    private bool toplandimi = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        baslangicPoz = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Eğer bu yetenek zaten açıksa meyveyi gizle (tekrar toplanmasın)
        if (YetenekManager.YetenekAcikMi(acilacakYetenek))
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (toplandimi) return;

        // Meyveyi yukarı-aşağı salla (idle animasyon)
        float yOffset = Mathf.Sin(Time.time * sallanmaHizi) * sallanmaMesafesi;
        transform.position = baslangicPoz + new Vector3(0f, yOffset, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (toplandimi) return;

        // Oyuncuyu bul
        PlayerMovement player = collision.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        toplandimi = true;

        // Yeteneği aç
        YetenekManager.YetenekAc(acilacakYetenek);

        // Ses efekti çal
        if (toplamaSound != null)
        {
            AudioSource sfx = player.sfxSource;
            if (sfx != null)
            {
                sfx.PlayOneShot(toplamaSound);
            }
        }

        // UI mesajını otomatik oluştur ve göster
        StartCoroutine(OtomatikMesajGoster());

        // Toplama animasyonu
        StartCoroutine(ToplamaAnimasyonu());
    }

    /// <summary>
    /// Runtime'da Canvas + Text oluşturur, mesajı gösterir, sonra temizler.
    /// Elle UI oluşturmaya gerek yok!
    /// </summary>
    private IEnumerator OtomatikMesajGoster()
    {
        string yetenekAdi = YetenekManager.YetenekAdiAl(acilacakYetenek);

        // --- Canvas oluştur ---
        GameObject canvasObj = new GameObject("YetenekMesajCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Her şeyin üstünde görünsün
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- Arka plan paneli (yarı saydam siyah şerit) ---
        GameObject panelObj = new GameObject("ArkaPlan");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.15f, 0.75f);
        panelRect.anchorMax = new Vector2(0.85f, 0.88f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // --- Mesaj yazısı ---
        GameObject textObj = new GameObject("MesajText");
        textObj.transform.SetParent(panelObj.transform, false);
        Text mesajText = textObj.AddComponent<Text>();
        mesajText.text = "★" + yetenekAdi + " UNLOCKED★";
        mesajText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mesajText.fontSize = 42;
        mesajText.fontStyle = FontStyle.Bold;
        mesajText.alignment = TextAnchor.MiddleCenter;

        // Her meyveye özel renk
        Color yaziRengi;
        Color outlineRengi;
        switch (acilacakYetenek)
        {
            case YetenekTipi.CiftZiplama: // Kiraz — kırmızı
                yaziRengi = new Color(0.87f, 0.1f, 0.18f, 0f);
                outlineRengi = new Color(0.5f, 0f, 0.05f, 0.9f);
                break;
            case YetenekTipi.Dash: // Karpuz — açık pembe
                yaziRengi = new Color(1f, 0.5f, 0.7f, 0f);
                outlineRengi = new Color(0.7f, 0.15f, 0.35f, 0.9f);
                break;
            case YetenekTipi.DuvarZiplama: // Muz — sarı
                yaziRengi = new Color(1f, 0.85f, 0.1f, 0f);
                outlineRengi = new Color(0.6f, 0.45f, 0f, 0.9f);
                break;
            default:
                yaziRengi = new Color(1f, 1f, 1f, 0f);
                outlineRengi = new Color(0.3f, 0.3f, 0.3f, 0.9f);
                break;
        }
        mesajText.color = yaziRengi;
        
        // Gölge efekti
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);

        // Outline efekti
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = outlineRengi;
        outline.effectDistance = new Vector2(1f, -1f);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // --- Animasyon: Fade In (yukarıdan kayarak gel) ---
        float timer = 0f;
        float fadeInSure = 0.5f;
        Vector2 baslangicAnchorMin = new Vector2(0.15f, 0.85f);
        Vector2 baslangicAnchorMax = new Vector2(0.85f, 0.98f);
        Vector2 hedefAnchorMin = new Vector2(0.15f, 0.75f);
        Vector2 hedefAnchorMax = new Vector2(0.85f, 0.88f);

        panelRect.anchorMin = baslangicAnchorMin;
        panelRect.anchorMax = baslangicAnchorMax;
        panelImage.color = new Color(0f, 0f, 0f, 0f);

        while (timer < fadeInSure)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / fadeInSure);

            // Yazı belirginleş
            Color tc = mesajText.color;
            tc.a = t;
            mesajText.color = tc;

            // Panel belirginleş
            panelImage.color = new Color(0f, 0f, 0f, 0.6f * t);

            // Yukarıdan aşağı kay
            panelRect.anchorMin = Vector2.Lerp(baslangicAnchorMin, hedefAnchorMin, t);
            panelRect.anchorMax = Vector2.Lerp(baslangicAnchorMax, hedefAnchorMax, t);

            yield return null;
        }

        // --- Ekranda bekle ---
        yield return new WaitForSeconds(mesajSuresi);

        // --- Animasyon: Fade Out ---
        timer = 0f;
        float fadeOutSure = 0.5f;
        while (timer < fadeOutSure)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutSure;

            Color tc = mesajText.color;
            tc.a = Mathf.Lerp(1f, 0f, t);
            mesajText.color = tc;

            panelImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.6f, 0f, t));

            yield return null;
        }

        // Temizle
        Destroy(canvasObj);
    }

    private IEnumerator ToplamaAnimasyonu()
    {
        // Meyveyi küçülterek yok et
        Vector3 orijinalScale = transform.localScale;
        float timer = 0f;
        float sure = 0.4f;

        while (timer < sure)
        {
            timer += Time.deltaTime;
            float t = timer / sure;

            // Küçül
            transform.localScale = Vector3.Lerp(orijinalScale, Vector3.zero, t);

            // Yukarı uç
            transform.position += new Vector3(0f, Time.deltaTime * 3f, 0f);

            // Sprite soluklaştır
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = c;
            }

            yield return null;
        }

        // Sprite'ı kapat
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        
        // Mesaj bittikten sonra tamamen yok et
        yield return new WaitForSeconds(mesajSuresi + 1f);
        Destroy(gameObject);
    }
}
