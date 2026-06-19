using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Oyunun son noktasına koyulacak bitiş trigger'ı.
/// Oyuncu bu bölgeye girdiğinde sinematik bir bitiş ekranı gösterir.
/// 
/// KULLANIM:
/// 1. level_tower sahnesinde son noktaya boş bir GameObject koyun
/// 2. BoxCollider2D ekleyin, "Is Trigger" işaretleyin
/// 3. Bu scripti o objeye ekleyin
/// 4. PlayerMovement'taki fadeScreen'i Inspector'dan bağlayın (opsiyonel - kendi fade'ini de oluşturabilir)
/// </summary>
public class GameFinale : MonoBehaviour
{
    [Header("Bitiş Ayarları")]
    [Tooltip("Bitiş ekranı başlamadan önce bekleme süresi")]
    public float baslangicBekleme = 0.5f;

    [Tooltip("Fade hızı")]
    public float fadeHizi = 2f;

    [Header("Ses")]
    [Tooltip("Bitiş müziği/efekti (opsiyonel)")]
    public AudioClip bitisEfekti;

    // Dahili değişkenler
    private bool tetiklendi = false;
    private Canvas finaleCanvas;
    private PlayerMovement player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (tetiklendi) return;

        PlayerMovement pm = collision.GetComponentInParent<PlayerMovement>();
        if (pm != null)
        {
            tetiklendi = true;
            player = pm;
            StartCoroutine(FinaleBaslat());
        }
    }

    private IEnumerator FinaleBaslat()
    {
        // 1. Oyuncuyu dondur
        player.enabled = false;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        // Kısa bekleme
        yield return new WaitForSeconds(baslangicBekleme);

        // 2. Bitiş Canvas'ını oluştur
        FinaleCanvasOlustur();

        // 3. Oyuncunun kendi fade ekranını karart (varsa)
        if (player.fadeScreen != null)
        {
            while (player.fadeScreen.color.a < 1f)
            {
                Color c = player.fadeScreen.color;
                c.a += Time.deltaTime * fadeHizi;
                player.fadeScreen.color = c;
                yield return null;
            }
        }

        // Sahnedeki TÜM ses kaynaklarını bul ve kapat
        AudioSource[] tumSesler = FindObjectsOfType<AudioSource>();
        foreach (AudioSource ses in tumSesler)
        {
            if (ses.isPlaying)
            {
                float baslangicSes = ses.volume;
                float t = 0;
                while (t < 1f)
                {
                    t += Time.deltaTime * 2f;
                    ses.volume = Mathf.Lerp(baslangicSes, 0f, t);
                    yield return null;
                }
                ses.Stop();
                ses.volume = 0f;
            }
        }

        // 4. Bitiş efekti çal
        if (bitisEfekti != null && player.sfxSource != null)
        {
            player.sfxSource.PlayOneShot(bitisEfekti);
        }

        // 5. Finale Canvas'ını göster
        yield return new WaitForSeconds(0.5f);
        finaleCanvas.gameObject.SetActive(true);

        // 6. Animasyonları başlat
        StartCoroutine(FinaleAnimasyonlari());
    }

    private void FinaleCanvasOlustur()
    {
        // ===== ANA CANVAS =====
        GameObject canvasObj = new GameObject("FinaleCanvas");
        finaleCanvas = canvasObj.AddComponent<Canvas>();
        finaleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        finaleCanvas.sortingOrder = 100; // Her şeyin üstünde

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // ===== ARKA PLAN (Koyu gradient) =====
        GameObject arkaPlanObj = new GameObject("ArkaPlan");
        arkaPlanObj.transform.SetParent(canvasObj.transform, false);
        Image arkaPlan = arkaPlanObj.AddComponent<Image>();
        arkaPlan.color = new Color(0.02f, 0.01f, 0.05f, 1f); // Çok koyu mor-siyah
        RectTransform arkaPlanRT = arkaPlanObj.GetComponent<RectTransform>();
        arkaPlanRT.anchorMin = Vector2.zero;
        arkaPlanRT.anchorMax = Vector2.one;
        arkaPlanRT.sizeDelta = Vector2.zero;

        // ===== YILDIZ PARTİKÜLLERİ (Basit UI noktaları) =====
        for (int i = 0; i < 60; i++)
        {
            GameObject yildiz = new GameObject("Yildiz_" + i);
            yildiz.transform.SetParent(canvasObj.transform, false);
            Image yildizImg = yildiz.AddComponent<Image>();
            
            float parlaklik = Random.Range(0.4f, 1f);
            float boyut = Random.Range(2f, 6f);
            
            // Farklı yıldız renkleri
            Color[] yildizRenkleri = {
                new Color(1f, 1f, 1f, parlaklik),          // Beyaz
                new Color(0.7f, 0.8f, 1f, parlaklik),      // Açık mavi
                new Color(1f, 0.9f, 0.5f, parlaklik),      // Sarı
                new Color(0.8f, 0.6f, 1f, parlaklik),      // Mor
            };
            yildizImg.color = yildizRenkleri[Random.Range(0, yildizRenkleri.Length)];

            RectTransform yildizRT = yildiz.GetComponent<RectTransform>();
            yildizRT.anchorMin = new Vector2(0.5f, 0.5f);
            yildizRT.anchorMax = new Vector2(0.5f, 0.5f);
            yildizRT.sizeDelta = new Vector2(boyut, boyut);
            yildizRT.anchoredPosition = new Vector2(
                Random.Range(-960f, 960f),
                Random.Range(-540f, 540f)
            );

            // Yıldız kırpışma animasyonu
            YildizAnimasyonu ya = yildiz.AddComponent<YildizAnimasyonu>();
            ya.hiz = Random.Range(0.5f, 3f);
            ya.minAlpha = Random.Range(0.1f, 0.3f);
            ya.maxAlpha = parlaklik;
        }

        // ===== ANA METİN GRUBU =====
        GameObject metinGrubu = new GameObject("MetinGrubu");
        metinGrubu.transform.SetParent(canvasObj.transform, false);
        RectTransform mgRT = metinGrubu.AddComponent<RectTransform>();
        mgRT.anchorMin = new Vector2(0.5f, 0.5f);
        mgRT.anchorMax = new Vector2(0.5f, 0.5f);
        mgRT.sizeDelta = new Vector2(1200, 600);
        mgRT.anchoredPosition = Vector2.zero;

        // ===== "TEBRİKLER!" BAŞLIĞI =====
        GameObject tebrikObj = new GameObject("TebriklerYazisi");
        tebrikObj.transform.SetParent(metinGrubu.transform, false);
        TextMeshProUGUI tebrikTMP = tebrikObj.AddComponent<TextMeshProUGUI>();
        tebrikTMP.text = "T H A N K S   F O R   P L A Y I N G !";
        tebrikTMP.fontSize = 72;
        tebrikTMP.fontStyle = FontStyles.Bold;
        tebrikTMP.alignment = TextAlignmentOptions.Center;
        tebrikTMP.enableWordWrapping = false;

        // Altın gradient
        tebrikTMP.enableVertexGradient = true;
        tebrikTMP.colorGradient = new VertexGradient(
            new Color(1f, 0.95f, 0.4f, 1f),  // Parlak altın üst
            new Color(1f, 0.95f, 0.4f, 1f),
            new Color(1f, 0.6f, 0.1f, 1f),   // Koyu altın alt
            new Color(1f, 0.6f, 0.1f, 1f)
        );

        RectTransform tebrikRT = tebrikObj.GetComponent<RectTransform>();
        tebrikRT.anchorMin = new Vector2(0.5f, 0.5f);
        tebrikRT.anchorMax = new Vector2(0.5f, 0.5f);
        tebrikRT.sizeDelta = new Vector2(1000, 100);
        tebrikRT.anchoredPosition = new Vector2(0, 120);

        CanvasGroup tebrikCG = tebrikObj.AddComponent<CanvasGroup>();
        tebrikCG.alpha = 0f;

        // ===== MESAJ METNİ =====
        GameObject mesajObj = new GameObject("MesajYazisi");
        mesajObj.transform.SetParent(metinGrubu.transform, false);
        TextMeshProUGUI mesajTMP = mesajObj.AddComponent<TextMeshProUGUI>();
        mesajTMP.text = "You conquered every parkour!\nThis adventure was yours.";
        mesajTMP.fontSize = 36;
        mesajTMP.fontStyle = FontStyles.Normal;
        mesajTMP.alignment = TextAlignmentOptions.Center;
        mesajTMP.color = new Color(0.8f, 0.85f, 0.95f, 1f); // Açık gümüş
        mesajTMP.characterSpacing = 2f;
        mesajTMP.lineSpacing = 15f;

        RectTransform mesajRT = mesajObj.GetComponent<RectTransform>();
        mesajRT.anchorMin = new Vector2(0.5f, 0.5f);
        mesajRT.anchorMax = new Vector2(0.5f, 0.5f);
        mesajRT.sizeDelta = new Vector2(800, 120);
        mesajRT.anchoredPosition = new Vector2(0, 20);

        CanvasGroup mesajCG = mesajObj.AddComponent<CanvasGroup>();
        mesajCG.alpha = 0f;

        // ===== AYIRICI ÇİZGİ =====
        GameObject cizgiObj = new GameObject("AyiriciCizgi");
        cizgiObj.transform.SetParent(metinGrubu.transform, false);
        Image cizgiImg = cizgiObj.AddComponent<Image>();
        cizgiImg.color = new Color(1f, 0.8f, 0.3f, 0.6f);

        RectTransform cizgiRT = cizgiObj.GetComponent<RectTransform>();
        cizgiRT.anchorMin = new Vector2(0.5f, 0.5f);
        cizgiRT.anchorMax = new Vector2(0.5f, 0.5f);
        cizgiRT.sizeDelta = new Vector2(0, 2); // Genişlik animasyonla açılacak
        cizgiRT.anchoredPosition = new Vector2(0, -40);

        CanvasGroup cizgiCG = cizgiObj.AddComponent<CanvasGroup>();
        cizgiCG.alpha = 0f;

        // ===== KREDİ METNİ =====
        GameObject krediObj = new GameObject("KrediYazisi");
        krediObj.transform.SetParent(metinGrubu.transform, false);
        TextMeshProUGUI krediTMP = krediObj.AddComponent<TextMeshProUGUI>();
        krediTMP.text = "Made by samequik";
        krediTMP.fontSize = 28;
        krediTMP.fontStyle = FontStyles.Italic;
        krediTMP.alignment = TextAlignmentOptions.Center;

        // Soft cyan gradient
        krediTMP.enableVertexGradient = true;
        krediTMP.colorGradient = new VertexGradient(
            new Color(0.5f, 1f, 0.9f, 1f),
            new Color(0.5f, 1f, 0.9f, 1f),
            new Color(0.3f, 0.7f, 1f, 1f),
            new Color(0.3f, 0.7f, 1f, 1f)
        );
        krediTMP.characterSpacing = 3f;

        RectTransform krediRT = krediObj.GetComponent<RectTransform>();
        krediRT.anchorMin = new Vector2(0.5f, 0.5f);
        krediRT.anchorMax = new Vector2(0.5f, 0.5f);
        krediRT.sizeDelta = new Vector2(800, 60);
        krediRT.anchoredPosition = new Vector2(0, -80);

        CanvasGroup krediCG = krediObj.AddComponent<CanvasGroup>();
        krediCG.alpha = 0f;

        // ===== ANA MENÜYE DÖN BUTONU =====
        GameObject butonObj = new GameObject("AnaMenuButonu");
        butonObj.transform.SetParent(metinGrubu.transform, false);

        Image butonImg = butonObj.AddComponent<Image>();
        butonImg.color = new Color(0.15f, 0.1f, 0.25f, 0.85f);

        Button buton = butonObj.AddComponent<Button>();
        ColorBlock cb = buton.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.1f, 1.3f, 1f);
        cb.pressedColor = new Color(0.8f, 0.7f, 0.9f, 1f);
        cb.fadeDuration = 0.1f;
        buton.colors = cb;

        buton.onClick.AddListener(AnaMenuyeDon);

        RectTransform butonRT = butonObj.GetComponent<RectTransform>();
        butonRT.anchorMin = new Vector2(0.5f, 0.5f);
        butonRT.anchorMax = new Vector2(0.5f, 0.5f);
        butonRT.sizeDelta = new Vector2(320, 60);
        butonRT.anchoredPosition = new Vector2(0, -170);

        // Buton yazısı
        GameObject butonYaziObj = new GameObject("ButonYazisi");
        butonYaziObj.transform.SetParent(butonObj.transform, false);
        TextMeshProUGUI butonTMP = butonYaziObj.AddComponent<TextMeshProUGUI>();
        butonTMP.text = "MAIN MENU";
        butonTMP.fontSize = 32;
        butonTMP.fontStyle = FontStyles.Bold;
        butonTMP.alignment = TextAlignmentOptions.Center;
        butonTMP.characterSpacing = 5f;

        butonTMP.enableVertexGradient = true;
        butonTMP.colorGradient = new VertexGradient(
            new Color(1f, 0.9f, 0.4f, 1f),
            new Color(1f, 0.9f, 0.4f, 1f),
            new Color(1f, 0.6f, 0.2f, 1f),
            new Color(1f, 0.6f, 0.2f, 1f)
        );

        RectTransform butonYaziRT = butonYaziObj.GetComponent<RectTransform>();
        butonYaziRT.anchorMin = Vector2.zero;
        butonYaziRT.anchorMax = Vector2.one;
        butonYaziRT.sizeDelta = Vector2.zero;

        CanvasGroup butonCG = butonObj.AddComponent<CanvasGroup>();
        butonCG.alpha = 0f;

        // Event System kontrolü
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Başlangıçta gizle
        canvasObj.SetActive(false);
    }

    private IEnumerator FinaleAnimasyonlari()
    {
        Transform metinGrubu = finaleCanvas.transform.Find("MetinGrubu");
        if (metinGrubu == null) yield break;

        // 1. TEBRİKLER yazısı - yukarıdan kayarak gelsin
        Transform tebrik = metinGrubu.Find("TebriklerYazisi");
        if (tebrik != null)
        {
            CanvasGroup cg = tebrik.GetComponent<CanvasGroup>();
            RectTransform rt = tebrik.GetComponent<RectTransform>();
            Vector2 hedefPoz = rt.anchoredPosition;
            rt.anchoredPosition = hedefPoz + new Vector2(0, 80);

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                float ease = EaseOutBack(t);
                cg.alpha = Mathf.Lerp(0, 1, t * 2f);
                rt.anchoredPosition = Vector2.Lerp(hedefPoz + new Vector2(0, 80), hedefPoz, ease);
                yield return null;
            }
            cg.alpha = 1f;
            rt.anchoredPosition = hedefPoz;

            // Tebrikler yazısına hafif sallanma ekle
            StartCoroutine(YaziSallanma(tebrik));
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Mesaj yazısı - fade in
        Transform mesaj = metinGrubu.Find("MesajYazisi");
        if (mesaj != null)
        {
            CanvasGroup cg = mesaj.GetComponent<CanvasGroup>();
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.2f;
                cg.alpha = t;
                yield return null;
            }
            cg.alpha = 1f;
        }

        yield return new WaitForSeconds(0.3f);

        // 3. Ayırıcı çizgi - genişleyerek açılsın
        Transform cizgi = metinGrubu.Find("AyiriciCizgi");
        if (cizgi != null)
        {
            CanvasGroup cg = cizgi.GetComponent<CanvasGroup>();
            RectTransform rt = cizgi.GetComponent<RectTransform>();
            cg.alpha = 1f;

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                float ease = EaseOutCubic(t);
                rt.sizeDelta = new Vector2(Mathf.Lerp(0, 400, ease), 2);
                yield return null;
            }
            rt.sizeDelta = new Vector2(400, 2);
        }

        yield return new WaitForSeconds(0.3f);

        // 4. Kredi yazısı - fade in
        Transform kredi = metinGrubu.Find("KrediYazisi");
        if (kredi != null)
        {
            CanvasGroup cg = kredi.GetComponent<CanvasGroup>();
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1f;
                cg.alpha = t;
                yield return null;
            }
            cg.alpha = 1f;
        }

        yield return new WaitForSeconds(0.8f);

        // 5. Ana Menü butonu - fade in
        Transform buton = metinGrubu.Find("AnaMenuButonu");
        if (buton != null)
        {
            CanvasGroup cg = buton.GetComponent<CanvasGroup>();
            RectTransform rt = buton.GetComponent<RectTransform>();
            Vector2 hedefPoz = rt.anchoredPosition;
            rt.anchoredPosition = hedefPoz + new Vector2(0, -30);

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                float ease = EaseOutCubic(t);
                cg.alpha = Mathf.Lerp(0, 1, t);
                rt.anchoredPosition = Vector2.Lerp(hedefPoz + new Vector2(0, -30), hedefPoz, ease);
                yield return null;
            }
            cg.alpha = 1f;
            rt.anchoredPosition = hedefPoz;

            // Buton pulse animasyonu
            StartCoroutine(ButonPulse(buton));
        }
    }

    private IEnumerator YaziSallanma(Transform yazi)
    {
        RectTransform rt = yazi.GetComponent<RectTransform>();
        float baslangicY = rt.anchoredPosition.y;

        while (true)
        {
            float yOffset = Mathf.Sin(Time.time * 1.5f) * 4f;
            Vector2 pos = rt.anchoredPosition;
            pos.y = baslangicY + yOffset;
            rt.anchoredPosition = pos;
            yield return null;
        }
    }

    private IEnumerator ButonPulse(Transform buton)
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.time * 1.5f, 1f);
            float scale = Mathf.Lerp(0.95f, 1.05f, t);
            buton.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    private void AnaMenuyeDon()
    {
        // Kayıtları temizle (oyun bitti)
        YetenekManager.TumYetenekleriSifirla();
        SceneManager.LoadScene("MainMenu");
    }

    // ======== Easing fonksiyonları ========
    private float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}

/// <summary>
/// Yıldızlara kırpışma animasyonu ekler.
/// </summary>
public class YildizAnimasyonu : MonoBehaviour
{
    public float hiz = 1f;
    public float minAlpha = 0.1f;
    public float maxAlpha = 1f;
    private Image img;
    private float offset;

    void Start()
    {
        img = GetComponent<Image>();
        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (img == null) return;
        float t = (Mathf.Sin(Time.time * hiz + offset) + 1f) / 2f;
        Color c = img.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        img.color = c;
    }
}
