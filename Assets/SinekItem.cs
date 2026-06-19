using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// level_tower'ın sonuna koyulan özel sinek objesi.
/// Meyve gibi çalışır — oyuncu yanına gidip dokunduğunda yutma animasyonu
/// oynar ve ardından GameFinale bitiş ekranını başlatır.
/// 
/// KULLANIM:
/// 1. level_tower sahnesinde boş bir GameObject oluştur
/// 2. Sprite Renderer ekle (sineğin sprite'ını ata)
/// 3. CircleCollider2D veya PolygonCollider2D ekle → Is Trigger: TRUE
/// 4. Bu scripti ekle
/// 5. Inspector'dan Toplama Sound ve Bitis Efekti ata (opsiyonel)
/// </summary>
public class SinekItem : MonoBehaviour
{
    [Header("Efektler")]
    [Tooltip("Sinekle temas edildiğinde çalacak ses")]
    public AudioClip toplamaSound;

    [Tooltip("Final ekranında çalacak müzik")]
    public AudioClip finaleMuzik;

    [Tooltip("Sinekten sonra bitiş ekranı başlamadan önceki bekleme")]
    public float bitisGecikme = 1.2f;

    [Header("İdle Animasyon")]
    public float sallanmaHizi = 2.5f;
    public float sallanmaMesafesi = 0.12f;

    // Dahili
    private Vector3 baslangicPoz;
    private bool yendi = false;
    private SpriteRenderer sr;

    void Start()
    {
        baslangicPoz = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (yendi) return;

        // Yukarı-aşağı sallanma
        float yOffset = Mathf.Sin(Time.time * sallanmaHizi) * sallanmaMesafesi;
        transform.position = baslangicPoz + new Vector3(0f, yOffset, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (yendi) return;

        PlayerMovement player = collision.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        yendi = true;

        // Ses çal
        if (toplamaSound != null && player.sfxSource != null)
            player.sfxSource.PlayOneShot(toplamaSound);

        // Animasyonları başlat
        StartCoroutine(YutumaAnimasyonu());
        StartCoroutine(BitisBaslat(player));
    }

    // ============================================================
    // ANİMASYONLAR
    // ============================================================

    private IEnumerator YutumaAnimasyonu()
    {
        // 1. Hızla dönerek büyü
        Vector3 baslangicScale = transform.localScale;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = Vector3.Lerp(baslangicScale, baslangicScale * 1.6f, ease);
            transform.Rotate(0f, 0f, 600f * Time.deltaTime);
            yield return null;
        }

        // 2. Küçülerek yok ol
        t = 0;
        Vector3 buyukScale = transform.localScale;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.localScale = Vector3.Lerp(buyukScale, Vector3.zero, t);
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }
            yield return null;
        }

        if (sr != null) sr.enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    private IEnumerator BitisBaslat(PlayerMovement player)
    {
        // Sineğin yenme animasyonu bitsin
        yield return new WaitForSeconds(0.5f);

        // Oyuncuyu dondur
        player.enabled = false;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        // Ufak bekleme
        yield return new WaitForSeconds(bitisGecikme);

        // Bitiş sekansını başlat
        StartCoroutine(FinaleSecans(player));
    }

    // ============================================================
    // BİTİŞ EKRANI (GameFinale.cs ile aynı sistem)
    // ============================================================

    private IEnumerator FinaleSecans(PlayerMovement player)
    {
        float fadeHizi = 2f;

        // Bitiş canvas'ını oluştur
        Canvas finaleCanvas = FinaleCanvasOlustur();

        // Ekranı karart (player'ın fade ekranı varsa kullan)
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

        // Tüm sesleri kapat
        AudioSource[] tumSesler = FindObjectsOfType<AudioSource>();
        foreach (AudioSource ses in tumSesler)
        {
            if (!ses.isPlaying) continue;
            float basVol = ses.volume;
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 2.5f;
                ses.volume = Mathf.Lerp(basVol, 0f, t);
                yield return null;
            }
            ses.Stop();
            ses.volume = 0f;
        }

        // Finale canvas'ını göster
        yield return new WaitForSeconds(0.4f);
        finaleCanvas.gameObject.SetActive(true);

        // Final müziğini çal
        if (finaleMuzik != null)
        {
            GameObject muzikObj = new GameObject("FinaleMuzik");
            AudioSource muzikSrc = muzikObj.AddComponent<AudioSource>();
            muzikSrc.clip = finaleMuzik;
            muzikSrc.volume = PlayerPrefs.GetFloat("Ayar_MuzikSes", 1f);
            muzikSrc.loop = true;
            muzikSrc.Play();
        }

        // Animasyonları başlat
        StartCoroutine(FinaleAnimasyonlari(finaleCanvas));
    }

    // ---- Canvas Oluşturma ----

    private Canvas FinaleCanvasOlustur()
    {
        GameObject canvasObj = new GameObject("FinaleCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Koyu arka plan
        GameObject bgObj = new GameObject("BG");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.02f, 0.01f, 0.05f, 1f);
        RectTransform bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        // Yıldızlar
        for (int i = 0; i < 60; i++)
        {
            GameObject yildiz = new GameObject("Y_" + i);
            yildiz.transform.SetParent(canvasObj.transform, false);
            Image yImg = yildiz.AddComponent<Image>();
            float p = Random.Range(0.4f, 1f);
            float b = Random.Range(2f, 6f);
            Color[] renkler = {
                new Color(1f, 1f, 1f, p),
                new Color(0.7f, 0.9f, 1f, p),
                new Color(1f, 0.9f, 0.5f, p),
                new Color(0.8f, 0.6f, 1f, p),
            };
            yImg.color = renkler[Random.Range(0, renkler.Length)];
            RectTransform yRT = yildiz.GetComponent<RectTransform>();
            yRT.anchorMin = new Vector2(0.5f, 0.5f);
            yRT.anchorMax = new Vector2(0.5f, 0.5f);
            yRT.sizeDelta = new Vector2(b, b);
            yRT.anchoredPosition = new Vector2(Random.Range(-960f, 960f), Random.Range(-540f, 540f));
            YildizKirpis kirpis = yildiz.AddComponent<YildizKirpis>();
            kirpis.hiz = Random.Range(0.5f, 3f);
            kirpis.minA = Random.Range(0.1f, 0.3f);
            kirpis.maxA = p;
        }

        // Metin grubu
        GameObject mg = new GameObject("MetinGrubu");
        mg.transform.SetParent(canvasObj.transform, false);
        RectTransform mgRT = mg.AddComponent<RectTransform>();
        mgRT.anchorMin        = new Vector2(0.5f, 0.5f);
        mgRT.anchorMax        = new Vector2(0.5f, 0.5f);
        mgRT.sizeDelta        = new Vector2(1200, 600);
        mgRT.anchoredPosition = Vector2.zero;

        // "THANKS FOR PLAYING !"
        TmpYazi(mg.transform, "BaslikYazi", "THANKS FOR PLAYING!", 68,
            new Vector2(0, 120),
            new Color(1f, 0.95f, 0.4f, 1f), new Color(1f, 0.6f, 0.1f, 1f),
            FontStyles.Bold, 8f, wordWrap: false);

        // "You conquered every parkour!"
        TmpYazi(mg.transform, "MesajYazi", "You conquered every parkour!\nThis adventure was yours.", 34,
            new Vector2(0, 18),
            new Color(0.8f, 0.85f, 0.95f, 1f), new Color(0.6f, 0.65f, 0.78f, 1f),
            FontStyles.Normal, 2f, wordWrap: true);

        // Altın çizgi
        GameObject cizgiObj = new GameObject("Cizgi");
        cizgiObj.transform.SetParent(mg.transform, false);
        Image cizgiImg = cizgiObj.AddComponent<Image>();
        cizgiImg.color = new Color(1f, 0.8f, 0.3f, 0.6f);
        RectTransform cizgiRT = cizgiObj.GetComponent<RectTransform>();
        cizgiRT.anchorMin        = new Vector2(0.5f, 0.5f);
        cizgiRT.anchorMax        = new Vector2(0.5f, 0.5f);
        cizgiRT.sizeDelta        = new Vector2(0, 2);
        cizgiRT.anchoredPosition = new Vector2(0, -42);
        CanvasGroup cizgiCG = cizgiObj.AddComponent<CanvasGroup>();
        cizgiCG.alpha = 0f;

        // "Made by samequik"
        TmpYazi(mg.transform, "KrediYazi", "Made by samequik", 28,
            new Vector2(0, -82),
            new Color(0.5f, 1f, 0.9f, 1f), new Color(0.3f, 0.7f, 1f, 1f),
            FontStyles.Italic, 3f, wordWrap: false);

        // MAIN MENU butonu
        GameObject butonObj = new GameObject("MainMenuBtn");
        butonObj.transform.SetParent(mg.transform, false);
        Image butonImg = butonObj.AddComponent<Image>();
        butonImg.color = new Color(0.15f, 0.1f, 0.25f, 0.85f);
        Button buton = butonObj.AddComponent<Button>();
        ColorBlock cb = buton.colors;
        cb.highlightedColor = new Color(1.2f, 1.1f, 1.3f, 1f);
        cb.pressedColor     = new Color(0.7f, 0.6f, 0.8f, 1f);
        cb.fadeDuration     = 0.1f;
        buton.colors = cb;
        buton.onClick.AddListener(() => {
            YetenekManager.TumYetenekleriSifirla();
            SceneManager.LoadScene("MainMenu");
        });
        RectTransform butonRT = butonObj.GetComponent<RectTransform>();
        butonRT.anchorMin        = new Vector2(0.5f, 0.5f);
        butonRT.anchorMax        = new Vector2(0.5f, 0.5f);
        butonRT.sizeDelta        = new Vector2(320, 60);
        butonRT.anchoredPosition = new Vector2(0, -170);

        GameObject btnYazi = new GameObject("BtnLabel");
        btnYazi.transform.SetParent(butonObj.transform, false);
        TextMeshProUGUI btnTMP = btnYazi.AddComponent<TextMeshProUGUI>();
        btnTMP.text                 = "MAIN MENU";
        btnTMP.fontSize             = 32;
        btnTMP.fontStyle            = FontStyles.Bold;
        btnTMP.alignment            = TextAlignmentOptions.Center;
        btnTMP.characterSpacing     = 5f;
        btnTMP.enableVertexGradient = true;
        btnTMP.colorGradient        = new VertexGradient(
            new Color(1f, 0.9f, 0.4f, 1f), new Color(1f, 0.9f, 0.4f, 1f),
            new Color(1f, 0.6f, 0.2f, 1f), new Color(1f, 0.6f, 0.2f, 1f));
        RectTransform btnRT = btnYazi.GetComponent<RectTransform>();
        btnRT.anchorMin = Vector2.zero;
        btnRT.anchorMax = Vector2.one;
        btnRT.sizeDelta = Vector2.zero;

        CanvasGroup butonCG = butonObj.AddComponent<CanvasGroup>();
        butonCG.alpha = 0f;

        // EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        canvasObj.SetActive(false);
        return canvas;
    }

    private IEnumerator FinaleAnimasyonlari(Canvas canvas)
    {
        Transform mg = canvas.transform.Find("MetinGrubu");
        if (mg == null) yield break;

        // 1. Başlık
        yield return StartCoroutine(FadeVeKaydir(mg.Find("BaslikYazi"), new Vector2(0, 80)));

        yield return new WaitForSeconds(0.4f);

        // 2. Mesaj
        yield return StartCoroutine(FadeIn(mg.Find("MesajYazi"), 1.1f));

        yield return new WaitForSeconds(0.3f);

        // 3. Çizgi genişle
        Transform cizgi = mg.Find("Cizgi");
        if (cizgi != null)
        {
            CanvasGroup cg = cizgi.GetComponent<CanvasGroup>();
            RectTransform rt = cizgi.GetComponent<RectTransform>();
            cg.alpha = 1f;
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                rt.sizeDelta = new Vector2(Mathf.Lerp(0, 400, EaseOut(t)), 2);
                yield return null;
            }
            rt.sizeDelta = new Vector2(400, 2);
        }

        yield return new WaitForSeconds(0.3f);

        // 4. Kredi
        yield return StartCoroutine(FadeIn(mg.Find("KrediYazi"), 0.9f));

        yield return new WaitForSeconds(0.8f);

        // 5. Main Menu butonu
        yield return StartCoroutine(FadeVeKaydir(mg.Find("MainMenuBtn"), new Vector2(0, -30)));

        // Buton pulse
        if (mg.Find("MainMenuBtn") != null)
            StartCoroutine(ButonPulse(mg.Find("MainMenuBtn")));
    }

    private IEnumerator FadeVeKaydir(Transform hedef, Vector2 offsetten)
    {
        if (hedef == null) yield break;
        CanvasGroup cg = hedef.GetComponent<CanvasGroup>();
        RectTransform rt = hedef.GetComponent<RectTransform>();
        Vector2 hedefPoz = rt.anchoredPosition;
        rt.anchoredPosition = hedefPoz + offsetten;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.8f;
            float ease = EaseOutBack(t);
            if (cg != null) cg.alpha = Mathf.Lerp(0, 1, t * 2f);
            rt.anchoredPosition = Vector2.Lerp(hedefPoz + offsetten, hedefPoz, ease);
            yield return null;
        }
        if (cg != null) cg.alpha = 1f;
        rt.anchoredPosition = hedefPoz;

        // Başlığa sallanma ekle
        StartCoroutine(YaziSallanma(rt, hedefPoz.y));
    }

    private IEnumerator FadeIn(Transform hedef, float hiz)
    {
        if (hedef == null) yield break;
        CanvasGroup cg = hedef.GetComponent<CanvasGroup>();
        if (cg == null) yield break;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * hiz;
            cg.alpha = t;
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator YaziSallanma(RectTransform rt, float baseY)
    {
        while (rt != null)
        {
            float y = baseY + Mathf.Sin(Time.time * 1.5f) * 4f;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            yield return null;
        }
    }

    private IEnumerator ButonPulse(Transform t)
    {
        while (t != null)
        {
            float s = Mathf.Lerp(0.95f, 1.05f, Mathf.PingPong(Time.time * 1.5f, 1f));
            t.localScale = Vector3.one * s;
            yield return null;
        }
    }

    // ============================================================
    // YARDIMCILAR
    // ============================================================

    private Image UIImage(Transform parent, string isim, Color renk)
    {
        GameObject obj = new GameObject(isim);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = renk;
        return img;
    }

    private GameObject TmpYazi(Transform parent, string isim, string metin, float size,
        Vector2 poz, Color ust, Color alt, FontStyles stil, float spacing, bool wordWrap = false)
    {
        GameObject obj = new GameObject(isim);
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text                 = metin;
        tmp.fontSize             = size;
        tmp.fontStyle            = stil;
        tmp.alignment            = TextAlignmentOptions.Center;
        tmp.enableWordWrapping   = wordWrap;
        tmp.overflowMode         = TextOverflowModes.Overflow;
        tmp.characterSpacing     = spacing;
        tmp.enableVertexGradient = true;
        tmp.colorGradient        = new VertexGradient(ust, ust, alt, alt);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(1800, 130);
        rt.anchoredPosition = poz;
        obj.AddComponent<CanvasGroup>().alpha = 0f;
        return obj;
    }

    private float EaseOut(float t)  => 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
    private float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
    }
}

/// <summary>
/// Yıldız noktalarına kırpışma animasyonu — SinekItem için bağımsız sınıf.
/// </summary>
public class YildizKirpis : MonoBehaviour
{
    public float hiz = 1f;
    public float minA = 0.1f;
    public float maxA = 1f;
    private Image img;
    private float offset;

    void Start()
    {
        img    = GetComponent<Image>();
        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (img == null) return;
        float t = (Mathf.Sin(Time.time * hiz + offset) + 1f) / 2f;
        Color c = img.color;
        c.a     = Mathf.Lerp(minA, maxA, t);
        img.color = c;
    }
}

/// <summary>
/// C# extension — tek satırda lambda ile RectTransform ayarlamak için.
/// </summary>
public static class RectTransformExt
{
    public static T Let<T>(this T self, System.Action<T> block)
    {
        block(self);
        return self;
    }
}
