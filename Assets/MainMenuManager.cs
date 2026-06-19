using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Ana menü yöneticisi - tüm UI kodla oluşturulur, Inspector'dan bağlamana gerek yok.
/// 
/// ÖZELLİKLER:
///  - Oyna  (kayıt varsa onay diyaloğu sorar)
///  - Devam Et (kayıt varsa görünür)
///  - Ayarlar (Müzik + SFX slider'ları, kayıtlı)
///  - Çıkış
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // ---- Inspector'dan bağlanabilir ----
    [Header("Müzik")]
    [Tooltip("Ana menü müziğini buraya sürükle")]
    public AudioClip muzikKlip;

    [Range(0f, 1f)]
    public float muzikSesi = 0.8f;

    [Header("Geçiş Hızı")]
    public float fadeSpeed = 3f;

    // ---- Dahili ----
    private AudioSource muzikSource;
    private const string KEY_MUZIK_SES = "Ayar_MuzikSes";
    private const string KEY_SFX_SES   = "Ayar_SfxSes";
    private const string ILK_BOLUM     = "level_green";

    // ---- Dahili referanslar ----
    private Canvas  anaCanvas;
    private Image   fadeScreen;
    private CanvasGroup anaPanel;
    private CanvasGroup ayarlarPanel;
    private CanvasGroup onayPanel;
    private Button  devamEtButonu;
    private Slider  muzikSlider;
    private Slider  sfxSlider;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Start()
    {
        // Müziği kur ve çal
        if (muzikKlip != null)
        {
            muzikSource = gameObject.AddComponent<AudioSource>();
            muzikSource.clip   = muzikKlip;
            muzikSource.loop   = true;
            muzikSource.volume = PlayerPrefs.GetFloat(KEY_MUZIK_SES, muzikSesi);
            muzikSource.Play();
        }

        // UI'yi oluştur
        CanvasOlustur();

        // Başlangıçta siyah → aydınlan
        StartCoroutine(BaslangicFadeIn());
    }

    // ============================================================
    // BUTON AKSIYONLARI
    // ============================================================

    /// <summary> OYNA butonuna basılınca — kayıt varsa onay sor </summary>
    public void OynaBasildi()
    {
        if (YetenekManager.KayitVarMi)
            OnayPaneliniGoster();
        else
            YeniOyunBaslat();
    }

    private void YeniOyunBaslat()
    {
        YetenekManager.TumYetenekleriSifirla();
        StartCoroutine(SahneYukleCinematik(ILK_BOLUM));
    }

    /// <summary> DEVAM ET butonuna basılınca </summary>
    public void DevamEt()
    {
        string sonSahne = YetenekManager.SonKayitliSahne;
        StartCoroutine(SahneYukleCinematik(sonSahne));
    }

    /// <summary> AYARLAR butonuna basılınca </summary>
    public void AyarlariAc()
    {
        PanelGec(anaPanel, ayarlarPanel);
    }

    public void AyarlariKapat()
    {
        PanelGec(ayarlarPanel, anaPanel);
    }

    /// <summary> ÇIKIŞ butonuna basılınca </summary>
    public void OyundanCik()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ---- Onay diyaloğu ----

    private void OnayPaneliniGoster()
    {
        anaPanel.interactable   = false;
        anaPanel.blocksRaycasts = false;
        onayPanel.gameObject.SetActive(true);
        StartCoroutine(FadeCanvasGroup(onayPanel, 0f, 1f, 0.2f));
    }

    public void OnayEvet()   // "EVET, SİL" butonuna basılınca
    {
        StartCoroutine(OnayPanelKapat(() => YeniOyunBaslat()));
    }

    public void OnayHayir()  // "İPTAL" butonuna basılınca
    {
        StartCoroutine(OnayPanelKapat(() => {
            anaPanel.interactable   = true;
            anaPanel.blocksRaycasts = true;
        }));
    }

    private IEnumerator OnayPanelKapat(System.Action sonra)
    {
        yield return StartCoroutine(FadeCanvasGroup(onayPanel, 1f, 0f, 0.2f));
        onayPanel.gameObject.SetActive(false);
        sonra?.Invoke();
    }

    // ---- Ses ayarları ----

    private void MuzikSesiDegisti(float v)
    {
        PlayerPrefs.SetFloat(KEY_MUZIK_SES, v);
        PlayerPrefs.Save();
        if (muzikSource != null) muzikSource.volume = v;
    }

    private void SfxSesiDegisti(float v)
    {
        PlayerPrefs.SetFloat(KEY_SFX_SES, v);
        PlayerPrefs.Save();
        // Ana menüde SFX kaynağı yok, sadece kaydediyoruz
    }

    // ============================================================
    // SİNEMATİK GEÇİŞ
    // ============================================================

    private IEnumerator SahneYukleCinematik(string sahneAdi)
    {
        float originalVol = muzikSource != null ? muzikSource.volume : 0f;

        if (fadeScreen != null)
        {
            while (fadeScreen.color.a < 1f)
            {
                Color c = fadeScreen.color;
                c.a += Time.deltaTime * fadeSpeed;
                fadeScreen.color = c;

                if (muzikSource != null)
                    muzikSource.volume = Mathf.Lerp(originalVol, 0f, c.a);

                yield return null;
            }
        }

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sahneAdi);
    }

    private IEnumerator BaslangicFadeIn()
    {
        if (fadeScreen == null) yield break;

        Color c = fadeScreen.color;
        c.a = 1f;
        fadeScreen.color = c;

        yield return new WaitForSeconds(0.2f);

        while (fadeScreen.color.a > 0f)
        {
            c    = fadeScreen.color;
            c.a -= Time.deltaTime * fadeSpeed;
            fadeScreen.color = c;
            yield return null;
        }
    }

    // ============================================================
    // UI OLUŞTURMA
    // ============================================================

    private void CanvasOlustur()
    {
        // ===== ANA CANVAS =====
        GameObject co = new GameObject("MainMenuCanvas");
        co.transform.SetParent(transform);
        anaCanvas = co.AddComponent<Canvas>();
        anaCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        anaCanvas.sortingOrder = 0;

        CanvasScaler cs = co.AddComponent<CanvasScaler>();
        cs.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        co.AddComponent<GraphicRaycaster>();

        // ===== FADE EKRANI =====
        fadeScreen = UIImage(co.transform, "FadeScreen",
            Vector2.zero, new Vector2(1, 1), Vector2.zero, new Color(0, 0, 0, 1f));
        RectTransform frt = fadeScreen.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.sizeDelta = Vector2.zero;

        // ===== ANA PANEL =====
        GameObject anaPanelObj = AnaPanelOlustur(co.transform);
        anaPanel = anaPanelObj.AddComponent<CanvasGroup>();

        // ===== AYARLAR PANELİ =====
        GameObject ayarlarObj = AyarlarPanelOlustur(co.transform);
        ayarlarPanel = ayarlarObj.AddComponent<CanvasGroup>();
        ayarlarPanel.alpha          = 0f;
        ayarlarPanel.interactable   = false;
        ayarlarPanel.blocksRaycasts = false;

        // ===== ONAY DİYALOĞU =====
        GameObject onayObj = OnayPanelOlustur(co.transform);
        onayPanel = onayObj.AddComponent<CanvasGroup>();
        onayPanel.alpha = 0f;
        onayObj.SetActive(false);

        // Event System
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ------ ANA PANEL ------

    private GameObject AnaPanelOlustur(Transform parent)
    {
        GameObject p = new GameObject("AnaPanel");
        p.transform.SetParent(parent, false);
        RectTransform prt = p.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.sizeDelta = Vector2.zero;

        // Arka plan resmi (renkli katman)
        Image bg = UIImage(p.transform, "BgOverlay",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(1920, 1080), new Color(0.04f, 0.02f, 0.08f, 0.55f));
        bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Başlık
        BaslikYaziOlustur(p.transform);

        // Buton grubu
        float startY  = -20f;
        float aralik  = 80f;
        bool  kayitVar = YetenekManager.KayitVarMi;

        // Devam Et (sadece kayıt varsa)
        if (kayitVar)
        {
            Button db = MenuButonu(p.transform, "CONTINUE",
                new Vector2(0, startY),
                new Color(0.3f, 1f, 0.5f, 1f), new Color(0.1f, 0.7f, 0.3f, 1f));
            db.onClick.AddListener(DevamEt);
            devamEtButonu = db;
            startY -= aralik;
        }

        // Oyna
        Button ob = MenuButonu(p.transform, "NEW GAME",
            new Vector2(0, startY),
            new Color(1f, 0.95f, 0.35f, 1f), new Color(1f, 0.6f, 0.1f, 1f));
        ob.onClick.AddListener(OynaBasildi);
        startY -= aralik;

        // Ayarlar
        Button ab = MenuButonu(p.transform, "SETTINGS",
            new Vector2(0, startY),
            new Color(0.5f, 0.8f, 1f, 1f), new Color(0.3f, 0.5f, 1f, 1f));
        ab.onClick.AddListener(AyarlariAc);
        startY -= aralik;

        // Çıkış
        Button qb = MenuButonu(p.transform, "QUIT",
            new Vector2(0, startY),
            new Color(1f, 0.4f, 0.3f, 1f), new Color(0.8f, 0.15f, 0.1f, 1f));
        qb.onClick.AddListener(OyundanCik);

        return p;
    }

    private void BaslikYaziOlustur(Transform parent)
    {
        // Gölge
        GameObject golge = new GameObject("BaslikGolge");
        golge.transform.SetParent(parent, false);
        TextMeshProUGUI gTMP = golge.AddComponent<TextMeshProUGUI>();
        gTMP.text      = "PARKURBAGA";
        gTMP.fontSize  = 96;
        gTMP.fontStyle = FontStyles.Bold;
        gTMP.alignment = TextAlignmentOptions.Center;
        gTMP.color     = new Color(0f, 0f, 0f, 0.4f);
        RectTransform grt = golge.GetComponent<RectTransform>();
        grt.anchorMin        = new Vector2(0.5f, 0.5f);
        grt.anchorMax        = new Vector2(0.5f, 0.5f);
        grt.sizeDelta        = new Vector2(900, 130);
        grt.anchoredPosition = new Vector2(4, 244);

        // Ana başlık
        GameObject baslik = new GameObject("Baslik");
        baslik.transform.SetParent(parent, false);
        TextMeshProUGUI bTMP = baslik.AddComponent<TextMeshProUGUI>();
        bTMP.text                  = "PARKURBAGA";
        bTMP.fontSize              = 96;
        bTMP.fontStyle             = FontStyles.Bold;
        bTMP.alignment             = TextAlignmentOptions.Center;
        bTMP.enableVertexGradient  = true;
        bTMP.colorGradient         = new VertexGradient(
            new Color(0.4f, 1f, 0.35f, 1f),
            new Color(0.4f, 1f, 0.35f, 1f),
            new Color(0.1f, 0.7f, 0.15f, 1f),
            new Color(0.1f, 0.7f, 0.15f, 1f));
        RectTransform brt = baslik.GetComponent<RectTransform>();
        brt.anchorMin        = new Vector2(0.5f, 0.5f);
        brt.anchorMax        = new Vector2(0.5f, 0.5f);
        brt.sizeDelta        = new Vector2(900, 130);
        brt.anchoredPosition = new Vector2(0, 248);

        // Başlık altına ince çizgi
        Image cizgi = UIImage(parent, "BaslikCizgi",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 3), new Color(0.4f, 1f, 0.85f, 0.4f));
        cizgi.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 192);
        cizgi.color = new Color(0.3f, 0.9f, 0.2f, 0.4f);

        // Başlık "samequik" alt yazısı
        GameObject alt = new GameObject("AltYazi");
        alt.transform.SetParent(parent, false);
        TextMeshProUGUI aTMP = alt.AddComponent<TextMeshProUGUI>();
        aTMP.text           = "by samequik";
        aTMP.fontSize       = 22;
        aTMP.fontStyle      = FontStyles.Italic;
        aTMP.alignment      = TextAlignmentOptions.Center;
        aTMP.color          = new Color(0.55f, 0.55f, 0.7f, 0.8f);
        aTMP.characterSpacing = 3f;
        RectTransform art = alt.GetComponent<RectTransform>();
        art.anchorMin        = new Vector2(0.5f, 0.5f);
        art.anchorMax        = new Vector2(0.5f, 0.5f);
        art.sizeDelta        = new Vector2(400, 35);
        art.anchoredPosition = new Vector2(0, 170);

        // Başlık sallanma animasyonu
        StartCoroutine(BaslikSallanma(brt, 248f));
    }

    private IEnumerator BaslikSallanma(RectTransform rt, float baseY)
    {
        while (rt != null)
        {
            float y = baseY + Mathf.Sin(Time.time * 1.3f) * 5f;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            yield return null;
        }
    }

    // ------ AYARLAR PANELİ ------

    private GameObject AyarlarPanelOlustur(Transform parent)
    {
        GameObject p = new GameObject("AyarlarPanel");
        p.transform.SetParent(parent, false);
        RectTransform prt = p.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.sizeDelta = Vector2.zero;

        // Koyu kart
        GameObject kart = new GameObject("Kart");
        kart.transform.SetParent(p.transform, false);
        Image kartImg = kart.AddComponent<Image>();
        kartImg.color = new Color(0.07f, 0.05f, 0.13f, 0.97f);
        RectTransform krt = kart.GetComponent<RectTransform>();
        krt.anchorMin        = new Vector2(0.5f, 0.5f);
        krt.anchorMax        = new Vector2(0.5f, 0.5f);
        krt.sizeDelta        = new Vector2(500, 440);
        krt.anchoredPosition = Vector2.zero;

        // Kenarlık
        Image kenB = UIImage(kart.transform, "Kenarlik",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Color(0.5f, 0.8f, 1f, 0.3f));
        RectTransform kenRT = kenB.GetComponent<RectTransform>();
        kenRT.anchorMin  = Vector2.zero;
        kenRT.anchorMax  = Vector2.one;
        kenRT.sizeDelta  = new Vector2(4, 4);

        Transform kT = kart.transform;

        // Başlık
        PanelYazi(kT, "SETTINGS", 46, new Vector2(0, 170),
            new Color(0.5f, 0.8f, 1f, 1f), new Color(0.3f, 0.5f, 1f, 1f), FontStyles.Bold, 6f);

        // Çizgi
        UIImage(kT, "Cizgi", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 2), new Color(0.5f, 0.8f, 1f, 0.25f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 135);

        // --- MÜZİK ---
        PanelYazi(kT, "MUSIC", 28, new Vector2(0, 90),
            new Color(0.9f, 0.85f, 1f, 1f), new Color(0.7f, 0.6f, 0.9f, 1f), FontStyles.Normal, 3f);

        muzikSlider = SliderOlustur(kT, "MuzikSlider", new Vector2(0, 48),
            new Color(0.4f, 0.8f, 1f, 1f), PlayerPrefs.GetFloat(KEY_MUZIK_SES, 1f));

        TextMeshProUGUI muzikPct = PanelYazi(kT, Mathf.RoundToInt(muzikSlider.value * 100) + "%", 20,
            new Vector2(210, 48), new Color(0.7f, 0.7f, 0.7f, 1f), new Color(0.5f, 0.5f, 0.5f, 1f),
            FontStyles.Normal, 0f);
        muzikSlider.onValueChanged.AddListener(v => {
            muzikPct.text = Mathf.RoundToInt(v * 100) + "%";
            MuzikSesiDegisti(v);
        });

        // --- SFX ---
        PanelYazi(kT, "SFX", 28, new Vector2(0, -10),
            new Color(0.9f, 0.85f, 1f, 1f), new Color(0.7f, 0.6f, 0.9f, 1f), FontStyles.Normal, 3f);

        sfxSlider = SliderOlustur(kT, "SfxSlider", new Vector2(0, -52),
            new Color(1f, 0.7f, 0.3f, 1f), PlayerPrefs.GetFloat(KEY_SFX_SES, 1f));

        TextMeshProUGUI sfxPct = PanelYazi(kT, Mathf.RoundToInt(sfxSlider.value * 100) + "%", 20,
            new Vector2(210, -52), new Color(0.7f, 0.7f, 0.7f, 1f), new Color(0.5f, 0.5f, 0.5f, 1f),
            FontStyles.Normal, 0f);
        sfxSlider.onValueChanged.AddListener(v => {
            sfxPct.text = Mathf.RoundToInt(v * 100) + "%";
            SfxSesiDegisti(v);
        });

        // Ayırıcı
        UIImage(kT, "Cizgi2", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 2), new Color(1f, 0.8f, 0.3f, 0.2f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -110);

        // Geri butonu
        Button bb = MenuButonuKucuk(kT, "BACK", new Vector2(0, -158),
            new Color(1f, 0.9f, 0.3f, 1f), new Color(1f, 0.6f, 0.1f, 1f));
        bb.onClick.AddListener(AyarlariKapat);

        return p;
    }

    // ------ ONAY DİYALOĞU ------

    private GameObject OnayPanelOlustur(Transform parent)
    {
        // Karartma
        GameObject overlay = new GameObject("OnayOverlay");
        overlay.transform.SetParent(parent, false);
        Image ov = overlay.AddComponent<Image>();
        ov.color = new Color(0, 0, 0, 0.5f);
        RectTransform ovRT = overlay.GetComponent<RectTransform>();
        ovRT.anchorMin = Vector2.zero;
        ovRT.anchorMax = Vector2.one;
        ovRT.sizeDelta = Vector2.zero;

        // Kart
        GameObject kart = new GameObject("OnayKart");
        kart.transform.SetParent(overlay.transform, false);
        Image kartImg = kart.AddComponent<Image>();
        kartImg.color = new Color(0.07f, 0.04f, 0.14f, 0.98f);
        RectTransform krt = kart.GetComponent<RectTransform>();
        krt.anchorMin        = new Vector2(0.5f, 0.5f);
        krt.anchorMax        = new Vector2(0.5f, 0.5f);
        krt.sizeDelta        = new Vector2(500, 290);
        krt.anchoredPosition = Vector2.zero;

        // Kenarlık (kırmızımsı)
        Image kenB = UIImage(kart.transform, "Kenarlik",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Color(1f, 0.35f, 0.25f, 0.5f));
        kenB.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        kenB.GetComponent<RectTransform>().anchorMax = Vector2.one;
        kenB.GetComponent<RectTransform>().sizeDelta = new Vector2(4, 4);

        Transform kT = kart.transform;

        // Uyarı başlığı
        PanelYazi(kT, "! NEW GAME", 38, new Vector2(0, 95),
            new Color(1f, 0.6f, 0.2f, 1f), new Color(1f, 0.3f, 0.1f, 1f), FontStyles.Bold, 4f);

        // Açıklama
        PanelYazi(kT, "Your save data will be deleted.\nAre you sure?", 26,
            new Vector2(0, 20),
            new Color(0.85f, 0.82f, 0.9f, 1f), new Color(0.65f, 0.62f, 0.75f, 1f),
            FontStyles.Normal, 1f);

        // Evet butonu
        Button evet = MenuButonuKucuk(kT, "YES, DELETE", new Vector2(-90, -80),
            new Color(1f, 0.4f, 0.3f, 1f), new Color(0.8f, 0.15f, 0.1f, 1f));
        evet.onClick.AddListener(OnayEvet);

        // Hayır butonu
        Button hayir = MenuButonuKucuk(kT, "CANCEL", new Vector2(90, -80),
            new Color(0.4f, 0.85f, 0.5f, 1f), new Color(0.15f, 0.65f, 0.25f, 1f));
        hayir.onClick.AddListener(OnayHayir);

        return overlay;
    }

    // ============================================================
    // YARDIMCI FONKSİYONLAR
    // ============================================================

    private void PanelGec(CanvasGroup kapanan, CanvasGroup acilan)
    {
        StartCoroutine(PanelGecCoroutine(kapanan, acilan));
    }

    private IEnumerator PanelGecCoroutine(CanvasGroup kapanan, CanvasGroup acilan)
    {
        kapanan.interactable   = false;
        kapanan.blocksRaycasts = false;
        yield return StartCoroutine(FadeCanvasGroup(kapanan, kapanan.alpha, 0f, 0.18f));

        acilan.alpha           = 0f;
        acilan.interactable    = true;
        acilan.blocksRaycasts  = true;
        yield return StartCoroutine(FadeCanvasGroup(acilan, 0f, 1f, 0.18f));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float sure)
    {
        float t = 0;
        while (t < 1f)
        {
            t      += Time.unscaledDeltaTime / sure;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        cg.alpha = to;
    }

    // ---- UI Primitifleri ----

    private Image UIImage(Transform parent, string isim,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Color renk)
    {
        GameObject obj = new GameObject(isim);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = renk;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.sizeDelta  = sizeDelta;
        rt.anchoredPosition = Vector2.zero;
        return img;
    }

    private TextMeshProUGUI PanelYazi(Transform parent, string metin, float fontSize,
        Vector2 pozisyon, Color ustRenk, Color altRenk, FontStyles stil, float spacing)
    {
        GameObject obj = new GameObject("Yazi_" + metin.Substring(0, Mathf.Min(10, metin.Length)));
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text                 = metin;
        tmp.fontSize             = fontSize;
        tmp.fontStyle            = stil;
        tmp.alignment            = TextAlignmentOptions.Center;
        tmp.enableWordWrapping   = true;
        tmp.characterSpacing     = spacing;
        tmp.enableVertexGradient = true;
        tmp.colorGradient        = new VertexGradient(ustRenk, ustRenk, altRenk, altRenk);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(440, 80);
        rt.anchoredPosition = pozisyon;
        return tmp;
    }

    private Button MenuButonu(Transform parent, string metin, Vector2 pozisyon,
        Color ustRenk, Color altRenk)
    {
        return ButonOlustur(parent, metin, pozisyon, ustRenk, altRenk, new Vector2(340, 62), 30);
    }

    private Button MenuButonuKucuk(Transform parent, string metin, Vector2 pozisyon,
        Color ustRenk, Color altRenk)
    {
        return ButonOlustur(parent, metin, pozisyon, ustRenk, altRenk, new Vector2(200, 52), 26);
    }

    private Button ButonOlustur(Transform parent, string metin, Vector2 pozisyon,
        Color ustRenk, Color altRenk, Vector2 boyut, float fontSize)
    {
        GameObject obj = new GameObject("Btn_" + metin);
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.1f, 0.08f, 0.18f, 0.92f);

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1.25f, 1.15f, 1.35f, 1f);
        cb.pressedColor     = new Color(0.7f, 0.6f, 0.8f, 1f);
        cb.selectedColor    = Color.white;
        cb.fadeDuration     = 0.08f;
        btn.colors = cb;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = boyut;
        rt.anchoredPosition = pozisyon;

        obj.AddComponent<MenuButtonHover>();

        // Yazı
        GameObject yaziObj = new GameObject("Label");
        yaziObj.transform.SetParent(obj.transform, false);
        TextMeshProUGUI tmp = yaziObj.AddComponent<TextMeshProUGUI>();
        tmp.text                 = metin;
        tmp.fontSize             = fontSize;
        tmp.fontStyle            = FontStyles.Bold;
        tmp.alignment            = TextAlignmentOptions.Center;
        tmp.enableWordWrapping   = false;
        tmp.characterSpacing     = 4f;
        tmp.enableVertexGradient = true;
        tmp.colorGradient        = new VertexGradient(ustRenk, ustRenk, altRenk, altRenk);
        RectTransform trt = yaziObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        return btn;
    }

    private Slider SliderOlustur(Transform parent, string isim, Vector2 pozisyon,
        Color dolguRenk, float baslangic)
    {
        GameObject sObj = new GameObject(isim);
        sObj.transform.SetParent(parent, false);
        RectTransform sRT = sObj.AddComponent<RectTransform>();
        sRT.anchorMin        = new Vector2(0.5f, 0.5f);
        sRT.anchorMax        = new Vector2(0.5f, 0.5f);
        sRT.sizeDelta        = new Vector2(320, 28);
        sRT.anchoredPosition = pozisyon;

        Slider slider  = sObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = baslangic;

        // Track
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(sObj.transform, false);
        Image bgI = bg.AddComponent<Image>();
        bgI.color = new Color(0.18f, 0.15f, 0.26f, 1f);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.3f);
        bgRT.anchorMax = new Vector2(1, 0.7f);
        bgRT.sizeDelta = Vector2.zero;

        // Fill Area
        GameObject fa = new GameObject("FillArea");
        fa.transform.SetParent(sObj.transform, false);
        RectTransform faRT = fa.AddComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0, 0.3f);
        faRT.anchorMax = new Vector2(1, 0.7f);
        faRT.sizeDelta = new Vector2(-20, 0);

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fa.transform, false);
        Image fillI = fill.AddComponent<Image>();
        fillI.color = dolguRenk;
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.sizeDelta = Vector2.zero;

        // Handle Area
        GameObject ha = new GameObject("HandleArea");
        ha.transform.SetParent(sObj.transform, false);
        RectTransform haRT = ha.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.sizeDelta = new Vector2(-20, 0);

        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(ha.transform, false);
        Image handleI = handle.AddComponent<Image>();
        handleI.color = Color.white;
        RectTransform handleRT = handle.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(22, 22);

        slider.fillRect     = fillRT;
        slider.handleRect   = handleRT;
        slider.targetGraphic = handleI;

        ColorBlock hcb   = slider.colors;
        hcb.highlightedColor = dolguRenk;
        hcb.pressedColor     = new Color(dolguRenk.r * 0.75f, dolguRenk.g * 0.75f, dolguRenk.b * 0.75f);
        hcb.fadeDuration     = 0.08f;
        slider.colors = hcb;

        return slider;
    }
}

/// <summary>
/// Ana menü butonlarına hover scale efekti (unscaled).
/// </summary>
public class MenuButtonHover : MonoBehaviour,
    UnityEngine.EventSystems.IPointerEnterHandler,
    UnityEngine.EventSystems.IPointerExitHandler
{
    private bool hovering;
    private float t;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData e) => hovering = true;
    public void OnPointerExit (UnityEngine.EventSystems.PointerEventData e) => hovering = false;

    void Update()
    {
        float hedef = hovering ? 1f : 0f;
        t = Mathf.MoveTowards(t, hedef, Time.unscaledDeltaTime * 9f);
        transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.07f, t);
    }
}
