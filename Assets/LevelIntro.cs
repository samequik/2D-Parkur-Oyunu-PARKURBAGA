using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Her sahnenin başında siyah ekranda level ismini gösteren sinematik giriş.
/// 
/// KULLANIM:
/// 1. Her level sahnesinde boş bir GameObject oluşturun (ör: "LevelIntro")
/// 2. Bu scripti o objeye ekleyin
/// 3. Oyun başladığında otomatik olarak level ismi gösterilir ve sonra gameplay başlar
/// 
/// NOT: Sahne ismine göre otomatik başlık belirler, Inspector'dan da override edilebilir.
/// </summary>
public class LevelIntro : MonoBehaviour
{
    [Header("Başlık Ayarları")]
    [Tooltip("Boş bırakılırsa sahne adına göre otomatik belirlenir")]
    public string levelBasligi = "";

    [Header("Zamanlama")]
    [Tooltip("Başlık gösterilme süresi (saniye)")]
    public float gosterimSuresi = 2.5f;

    [Tooltip("Fade out hızı")]
    public float fadeHizi = 2f;

    [Header("Oyuncu Referansı")]
    [Tooltip("Boş bırakılırsa otomatik bulunur")]
    public PlayerMovement player;

    // Dahili
    private Canvas introCanvas;
    private CanvasGroup canvasGroup;
    private float kaydedilenGravity;

    void Awake()
    {
        // Level başlığını belirle
        if (string.IsNullOrEmpty(levelBasligi))
        {
            levelBasligi = SahneBasligiBelirle();
        }

        // Intro UI'yi hemen oluştur (siyah ekran görünsün)
        IntroCanvasOlustur();
    }

    void Start()
    {
        // Oyuncuyu bul
        if (player == null)
            player = FindObjectOfType<PlayerMovement>();

        // Gravity'yi PlayerMovement.Start() çalışmadan ÖNCE Rigidbody'den kaydet
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                kaydedilenGravity = rb.gravityScale;
            }
        }

        // Bir frame bekleyip oyuncuyu dondur (PlayerMovement.Start() çalışsın diye)
        StartCoroutine(OyuncuyuDondurVeBaslat());
    }

    private IEnumerator OyuncuyuDondurVeBaslat()
    {
        // Bir frame bekle — PlayerMovement.Start() kesinlikle çalışmış olsun
        yield return null;

        if (player != null)
        {
            player.enabled = false;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Gravity'yi tekrar kaydet (artık PlayerMovement.Start() çalıştı)
                kaydedilenGravity = player.originalGravity;
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
        }

        // Intro sekansını başlat
        StartCoroutine(IntroSekans());
    }

    private string SahneBasligiBelirle()
    {
        string sahneAdi = SceneManager.GetActiveScene().name.ToLower();

        if (sahneAdi.Contains("green"))
            return "FOREST";
        else if (sahneAdi.Contains("purple"))
            return "SPACE";
        else if (sahneAdi.Contains("red"))
            return "HELL";
        else if (sahneAdi.Contains("tower"))
            return "TOWER OF HELL";
        else
            return sahneAdi.ToUpper();
    }

    private void IntroCanvasOlustur()
    {
        // ===== CANVAS =====
        GameObject canvasObj = new GameObject("IntroCanvas");
        introCanvas = canvasObj.AddComponent<Canvas>();
        introCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        introCanvas.sortingOrder = 99;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // CanvasGroup (tüm canvas'ı fade etmek için)
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        // ===== SİYAH ARKA PLAN =====
        GameObject arkaPlanObj = new GameObject("ArkaPlan");
        arkaPlanObj.transform.SetParent(canvasObj.transform, false);
        Image arkaPlan = arkaPlanObj.AddComponent<Image>();
        arkaPlan.color = new Color(0f, 0f, 0f, 1f);

        RectTransform arkaPlanRT = arkaPlanObj.GetComponent<RectTransform>();
        arkaPlanRT.anchorMin = Vector2.zero;
        arkaPlanRT.anchorMax = Vector2.one;
        arkaPlanRT.sizeDelta = Vector2.zero;

        // ===== LEVEL BAŞLIĞI =====
        GameObject baslikObj = new GameObject("LevelBasligi");
        baslikObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI baslikTMP = baslikObj.AddComponent<TextMeshProUGUI>();
        baslikTMP.text = levelBasligi;
        baslikTMP.fontSize = 86;
        baslikTMP.fontStyle = FontStyles.Bold;
        baslikTMP.alignment = TextAlignmentOptions.Center;
        baslikTMP.enableWordWrapping = false;
        baslikTMP.characterSpacing = 12f;

        // Sahneye göre renk belirle
        Color ustRenk, altRenk;
        SahneRenkBelirle(out ustRenk, out altRenk);

        baslikTMP.enableVertexGradient = true;
        baslikTMP.colorGradient = new VertexGradient(
            ustRenk, ustRenk,
            altRenk, altRenk
        );

        RectTransform baslikRT = baslikObj.GetComponent<RectTransform>();
        baslikRT.anchorMin = new Vector2(0.5f, 0.5f);
        baslikRT.anchorMax = new Vector2(0.5f, 0.5f);
        baslikRT.sizeDelta = new Vector2(1200, 120);
        baslikRT.anchoredPosition = new Vector2(0, 20);

        CanvasGroup baslikCG = baslikObj.AddComponent<CanvasGroup>();
        baslikCG.alpha = 0f;

        // ===== ALT ÇİZGİ (dekoratif) =====
        GameObject cizgiObj = new GameObject("AltCizgi");
        cizgiObj.transform.SetParent(canvasObj.transform, false);
        Image cizgiImg = cizgiObj.AddComponent<Image>();
        cizgiImg.color = new Color(ustRenk.r, ustRenk.g, ustRenk.b, 0.5f);

        RectTransform cizgiRT = cizgiObj.GetComponent<RectTransform>();
        cizgiRT.anchorMin = new Vector2(0.5f, 0.5f);
        cizgiRT.anchorMax = new Vector2(0.5f, 0.5f);
        cizgiRT.sizeDelta = new Vector2(0, 3);
        cizgiRT.anchoredPosition = new Vector2(0, -40);

        CanvasGroup cizgiCG = cizgiObj.AddComponent<CanvasGroup>();
        cizgiCG.alpha = 0f;
    }

    private void SahneRenkBelirle(out Color ust, out Color alt)
    {
        string sahneAdi = SceneManager.GetActiveScene().name.ToLower();

        if (sahneAdi.Contains("green"))
        {
            // Orman yeşili
            ust = new Color(0.3f, 1f, 0.5f, 1f);
            alt = new Color(0.1f, 0.6f, 0.2f, 1f);
        }
        else if (sahneAdi.Contains("purple"))
        {
            // Uzay moru
            ust = new Color(0.7f, 0.5f, 1f, 1f);
            alt = new Color(0.4f, 0.2f, 0.8f, 1f);
        }
        else if (sahneAdi.Contains("red"))
        {
            // Cehennem kırmızısı
            ust = new Color(1f, 0.4f, 0.2f, 1f);
            alt = new Color(0.8f, 0.1f, 0.05f, 1f);
        }
        else if (sahneAdi.Contains("tower"))
        {
            // Tower - ateşli turuncu/kırmızı
            ust = new Color(1f, 0.7f, 0.2f, 1f);
            alt = new Color(1f, 0.3f, 0.1f, 1f);
        }
        else
        {
            // Varsayılan
            ust = new Color(1f, 1f, 1f, 1f);
            alt = new Color(0.7f, 0.7f, 0.7f, 1f);
        }
    }

    private IEnumerator IntroSekans()
    {
        Transform baslik = introCanvas.transform.Find("LevelBasligi");
        Transform cizgi = introCanvas.transform.Find("AltCizgi");

        // Kısa başlangıç beklemesi
        yield return new WaitForSeconds(0.3f);

        // 1. Başlığı fade in + hafif yukarı kayma
        if (baslik != null)
        {
            CanvasGroup cg = baslik.GetComponent<CanvasGroup>();
            RectTransform rt = baslik.GetComponent<RectTransform>();
            Vector2 hedefPoz = rt.anchoredPosition;
            rt.anchoredPosition = hedefPoz + new Vector2(0, -30);

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                float ease = EaseOutCubic(t);
                cg.alpha = Mathf.Lerp(0, 1, t * 1.5f);
                rt.anchoredPosition = Vector2.Lerp(hedefPoz + new Vector2(0, -30), hedefPoz, ease);
                yield return null;
            }
            cg.alpha = 1f;
            rt.anchoredPosition = hedefPoz;
        }

        // 2. Alt çizgi genişleyerek açılsın
        if (cizgi != null)
        {
            CanvasGroup cg = cizgi.GetComponent<CanvasGroup>();
            RectTransform rt = cizgi.GetComponent<RectTransform>();
            cg.alpha = 1f;

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 3f;
                float ease = EaseOutCubic(t);
                rt.sizeDelta = new Vector2(Mathf.Lerp(0, 350, ease), 3);
                yield return null;
            }
            rt.sizeDelta = new Vector2(350, 3);
        }

        // 3. Gösterim süresi boyunca bekle
        yield return new WaitForSeconds(gosterimSuresi);

        // 4. Tüm intro canvas'ını fade out
        float fadeT = 0;
        while (fadeT < 1f)
        {
            fadeT += Time.deltaTime * fadeHizi;
            canvasGroup.alpha = Mathf.Lerp(1, 0, fadeT);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // 5. Oyuncuyu serbest bırak
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = kaydedilenGravity;
            }
            player.enabled = true;
        }

        // 6. Canvas'ı temizle
        Destroy(introCanvas.gameObject);
        Destroy(this.gameObject);
    }

    // ======== Easing ========
    private float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
