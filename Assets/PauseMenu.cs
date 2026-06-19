using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// ESC tuşuyla açılıp kapanan Pause Menü sistemi.
/// 
/// KULLANIM:
/// 1. Her level sahnesinde boş bir GameObject oluşturun (ör: "PauseMenu")
/// 2. Bu scripti o objeye ekleyin
/// 3. Tüm UI otomatik oluşturulur, Inspector'dan bir şey bağlamaya gerek yok
/// 
/// ÖZELLİKLER:
/// - Devam Et (Resume)
/// - Ayarlar (Müzik sesi + Efekt sesi ayrı slider'lar)
/// - Restart (son checkpoint'e ışınla)
/// - Ana Menüye Dön
/// - Oyundan Çık
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // PlayerPrefs anahtarları (ses ayarlarını kaydetmek için)
    private const string KEY_MUZIK_SES = "Ayar_MuzikSes";
    private const string KEY_SFX_SES = "Ayar_SfxSes";

    // Dahili değişkenler
    private bool oyunDuraklatildi = false;
    private Canvas pauseCanvas;
    private GameObject anaPanel;
    private GameObject ayarlarPanel;
    private PlayerMovement player;

    // Ses referansları
    private Slider muzikSlider;
    private Slider sfxSlider;

    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
        PauseCanvasOlustur();

        // Kayıtlı ses ayarlarını uygula
        SesAyarlariniYukle();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (oyunDuraklatildi)
                DevamEt();
            else
                OyunuDuraklat();
        }
    }

    // ============ DURAKLAT / DEVAM ============

    private void OyunuDuraklat()
    {
        oyunDuraklatildi = true;
        Time.timeScale = 0f;
        pauseCanvas.gameObject.SetActive(true);
        anaPanel.SetActive(true);
        ayarlarPanel.SetActive(false);

        // Slider'ları güncelle
        if (muzikSlider != null)
            muzikSlider.value = PlayerPrefs.GetFloat(KEY_MUZIK_SES, 1f);
        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat(KEY_SFX_SES, 1f);
    }

    public void DevamEt()
    {
        oyunDuraklatildi = false;
        Time.timeScale = 1f;
        pauseCanvas.gameObject.SetActive(false);
    }

    public void Restart()
    {
        // Önce oyunu devam ettir (TimeScale)
        Time.timeScale = 1f;
        oyunDuraklatildi = false;
        pauseCanvas.gameObject.SetActive(false);

        // Karakteri öldürüp checkpoint'e ışınla
        if (player != null)
        {
            player.RestartAtCheckpoint();
        }
    }

    public void AnaMenuyeDon()
    {
        Time.timeScale = 1f;
        oyunDuraklatildi = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void OyundanCik()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ============ AYARLAR PANELİ ============

    public void AyarlariAc()
    {
        anaPanel.SetActive(false);
        ayarlarPanel.SetActive(true);
    }

    public void AyarlariKapat()
    {
        ayarlarPanel.SetActive(false);
        anaPanel.SetActive(true);
    }

    private void MuzikSesiDegisti(float deger)
    {
        PlayerPrefs.SetFloat(KEY_MUZIK_SES, deger);
        PlayerPrefs.Save();
        MuzikSesiniUygula(deger);
    }

    private void SfxSesiDegisti(float deger)
    {
        PlayerPrefs.SetFloat(KEY_SFX_SES, deger);
        PlayerPrefs.Save();
        SfxSesiniUygula(deger);
    }

    private void MuzikSesiniUygula(float deger)
    {
        // Sahnedeki tüm AudioSource'ları bul, SFX olanları hariç tut
        AudioSource[] tumSesler = FindObjectsOfType<AudioSource>();
        foreach (AudioSource ses in tumSesler)
        {
            if (player != null && ses == player.sfxSource) continue;
            ses.volume = deger;
        }
    }

    private void SfxSesiniUygula(float deger)
    {
        if (player != null && player.sfxSource != null)
        {
            player.sfxSource.volume = deger;
        }
    }

    private void SesAyarlariniYukle()
    {
        float muzikSes = PlayerPrefs.GetFloat(KEY_MUZIK_SES, 1f);
        float sfxSes = PlayerPrefs.GetFloat(KEY_SFX_SES, 1f);

        MuzikSesiniUygula(muzikSes);
        SfxSesiniUygula(sfxSes);
    }

    // ============ UI OLUŞTURMA ============

    private void PauseCanvasOlustur()
    {
        // ===== CANVAS =====
        GameObject canvasObj = new GameObject("PauseCanvas");
        canvasObj.transform.SetParent(this.transform);
        pauseCanvas = canvasObj.AddComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 90;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // ===== KARARTMA ARKA PLAN =====
        GameObject arkaPlanObj = new GameObject("Karartma");
        arkaPlanObj.transform.SetParent(canvasObj.transform, false);
        Image arkaPlan = arkaPlanObj.AddComponent<Image>();
        arkaPlan.color = new Color(0f, 0f, 0f, 0.7f);

        RectTransform arkaPlanRT = arkaPlanObj.GetComponent<RectTransform>();
        arkaPlanRT.anchorMin = Vector2.zero;
        arkaPlanRT.anchorMax = Vector2.one;
        arkaPlanRT.sizeDelta = Vector2.zero;

        // ===== ANA PANEL =====
        anaPanel = PanelOlustur(canvasObj.transform, "AnaPanel");
        AnaPanelIcergiOlustur(anaPanel.transform);

        // ===== AYARLAR PANELİ =====
        ayarlarPanel = PanelOlustur(canvasObj.transform, "AyarlarPanel");
        AyarlarPanelIcerigiOlustur(ayarlarPanel.transform);
        ayarlarPanel.SetActive(false);

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

    private GameObject PanelOlustur(Transform parent, string isim)
    {
        GameObject panelObj = new GameObject(isim);
        panelObj.transform.SetParent(parent, false);

        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.06f, 0.14f, 0.95f);

        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(420, 480);
        panelRT.anchoredPosition = Vector2.zero;

        // Panel kenarlık efekti
        GameObject kenarlikObj = new GameObject("Kenarlik");
        kenarlikObj.transform.SetParent(panelObj.transform, false);
        Image kenarlikImg = kenarlikObj.AddComponent<Image>();
        kenarlikImg.color = new Color(1f, 0.8f, 0.3f, 0.3f);

        RectTransform kenarlikRT = kenarlikObj.GetComponent<RectTransform>();
        kenarlikRT.anchorMin = Vector2.zero;
        kenarlikRT.anchorMax = Vector2.one;
        kenarlikRT.sizeDelta = new Vector2(4, 4);
        kenarlikRT.anchoredPosition = Vector2.zero;

        // Kenarlığı arka plana al, panelin child'ı olarak en üst sıraya
        // İç dolgu objesi (kenarlik üstüne yazı gelmesin)
        GameObject icObj = new GameObject("IcDolgu");
        icObj.transform.SetParent(panelObj.transform, false);
        Image icImg = icObj.AddComponent<Image>();
        icImg.color = new Color(0.08f, 0.06f, 0.14f, 0.95f);

        RectTransform icRT = icObj.GetComponent<RectTransform>();
        icRT.anchorMin = Vector2.zero;
        icRT.anchorMax = Vector2.one;
        icRT.sizeDelta = new Vector2(-4, -4);
        icRT.anchoredPosition = Vector2.zero;

        return panelObj;
    }

    // ============ ANA PANEL İÇERİĞİ ============

    private void AnaPanelIcergiOlustur(Transform parent)
    {
        // Başlık: "PAUSED"
        YaziOlustur(parent, "PausedBaslik", "PAUSED", 48,
            new Vector2(0, 180),
            new Color(1f, 0.9f, 0.3f, 1f),
            new Color(1f, 0.6f, 0.1f, 1f),
            FontStyles.Bold, 8f);

        // Ayırıcı çizgi
        CizgiOlustur(parent, new Vector2(0, 145), 280);

        // Butonlar
        float butonY = 90f;
        float butonAralik = 70f;

        ButonOlustur(parent, "RESUME", new Vector2(0, butonY),
            new Color(0.3f, 1f, 0.5f, 1f), new Color(0.1f, 0.7f, 0.3f, 1f),
            () => DevamEt());

        ButonOlustur(parent, "SETTINGS", new Vector2(0, butonY - butonAralik),
            new Color(0.5f, 0.8f, 1f, 1f), new Color(0.3f, 0.5f, 1f, 1f),
            () => AyarlariAc());

        ButonOlustur(parent, "RESTART", new Vector2(0, butonY - butonAralik * 2),
            new Color(1f, 0.8f, 0.3f, 1f), new Color(1f, 0.5f, 0.1f, 1f),
            () => Restart());

        ButonOlustur(parent, "MAIN MENU", new Vector2(0, butonY - butonAralik * 3),
            new Color(0.8f, 0.6f, 1f, 1f), new Color(0.5f, 0.3f, 0.8f, 1f),
            () => AnaMenuyeDon());

        ButonOlustur(parent, "QUIT", new Vector2(0, butonY - butonAralik * 4),
            new Color(1f, 0.4f, 0.3f, 1f), new Color(0.8f, 0.2f, 0.1f, 1f),
            () => OyundanCik());
    }

    // ============ AYARLAR PANELİ İÇERİĞİ ============

    private void AyarlarPanelIcerigiOlustur(Transform parent)
    {
        // Başlık: "SETTINGS"
        YaziOlustur(parent, "AyarlarBaslik", "SETTINGS", 44,
            new Vector2(0, 180),
            new Color(0.5f, 0.8f, 1f, 1f),
            new Color(0.3f, 0.5f, 1f, 1f),
            FontStyles.Bold, 6f);

        // Ayırıcı çizgi
        CizgiOlustur(parent, new Vector2(0, 145), 280);

        // --- MÜZİK SESİ ---
        YaziOlustur(parent, "MuzikLabel", "MUSIC", 28,
            new Vector2(0, 100),
            new Color(0.9f, 0.85f, 1f, 1f),
            new Color(0.7f, 0.6f, 0.9f, 1f),
            FontStyles.Normal, 3f);

        muzikSlider = SliderOlustur(parent, "MuzikSlider",
            new Vector2(0, 55),
            new Color(0.4f, 0.8f, 1f, 1f),
            PlayerPrefs.GetFloat(KEY_MUZIK_SES, 1f));
        muzikSlider.onValueChanged.AddListener(MuzikSesiDegisti);

        // Müzik yüzde göstergesi
        TextMeshProUGUI muzikYuzde = YaziOlustur(parent, "MuzikYuzde",
            Mathf.RoundToInt(muzikSlider.value * 100) + "%", 22,
            new Vector2(160, 55),
            new Color(0.7f, 0.7f, 0.7f, 1f),
            new Color(0.5f, 0.5f, 0.5f, 1f),
            FontStyles.Normal, 0f);
        muzikSlider.onValueChanged.AddListener((v) => {
            muzikYuzde.text = Mathf.RoundToInt(v * 100) + "%";
        });

        // --- SFX SESİ ---
        YaziOlustur(parent, "SfxLabel", "SFX", 28,
            new Vector2(0, -5),
            new Color(0.9f, 0.85f, 1f, 1f),
            new Color(0.7f, 0.6f, 0.9f, 1f),
            FontStyles.Normal, 3f);

        sfxSlider = SliderOlustur(parent, "SfxSlider",
            new Vector2(0, -50),
            new Color(1f, 0.7f, 0.3f, 1f),
            PlayerPrefs.GetFloat(KEY_SFX_SES, 1f));
        sfxSlider.onValueChanged.AddListener(SfxSesiDegisti);

        // SFX yüzde göstergesi
        TextMeshProUGUI sfxYuzde = YaziOlustur(parent, "SfxYuzde",
            Mathf.RoundToInt(sfxSlider.value * 100) + "%", 22,
            new Vector2(160, -50),
            new Color(0.7f, 0.7f, 0.7f, 1f),
            new Color(0.5f, 0.5f, 0.5f, 1f),
            FontStyles.Normal, 0f);
        sfxSlider.onValueChanged.AddListener((v) => {
            sfxYuzde.text = Mathf.RoundToInt(v * 100) + "%";
        });

        // Ayırıcı çizgi
        CizgiOlustur(parent, new Vector2(0, -110), 280);

        // BACK butonu
        ButonOlustur(parent, "BACK", new Vector2(0, -160),
            new Color(1f, 0.9f, 0.3f, 1f), new Color(1f, 0.6f, 0.1f, 1f),
            () => AyarlariKapat());
    }

    // ============ UI YARDIMCI FONKSİYONLAR ============

    private TextMeshProUGUI YaziOlustur(Transform parent, string isim, string metin,
        float fontSize, Vector2 pozisyon, Color ustRenk, Color altRenk,
        FontStyles stil, float characterSpacing)
    {
        GameObject yaziObj = new GameObject(isim);
        yaziObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = yaziObj.AddComponent<TextMeshProUGUI>();
        tmp.text = metin;
        tmp.fontSize = fontSize;
        tmp.fontStyle = stil;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.characterSpacing = characterSpacing;

        tmp.enableVertexGradient = true;
        tmp.colorGradient = new VertexGradient(
            ustRenk, ustRenk,
            altRenk, altRenk
        );

        RectTransform rt = yaziObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(380, 50);
        rt.anchoredPosition = pozisyon;

        return tmp;
    }

    private void CizgiOlustur(Transform parent, Vector2 pozisyon, float genislik)
    {
        GameObject cizgiObj = new GameObject("Cizgi");
        cizgiObj.transform.SetParent(parent, false);
        Image cizgiImg = cizgiObj.AddComponent<Image>();
        cizgiImg.color = new Color(1f, 0.8f, 0.3f, 0.25f);

        RectTransform cizgiRT = cizgiObj.GetComponent<RectTransform>();
        cizgiRT.anchorMin = new Vector2(0.5f, 0.5f);
        cizgiRT.anchorMax = new Vector2(0.5f, 0.5f);
        cizgiRT.sizeDelta = new Vector2(genislik, 2);
        cizgiRT.anchoredPosition = pozisyon;
    }

    private void ButonOlustur(Transform parent, string metin, Vector2 pozisyon,
        Color ustRenk, Color altRenk, UnityEngine.Events.UnityAction tiklamasi)
    {
        GameObject butonObj = new GameObject("Buton_" + metin);
        butonObj.transform.SetParent(parent, false);

        // Buton arka planı
        Image butonImg = butonObj.AddComponent<Image>();
        butonImg.color = new Color(0.12f, 0.1f, 0.2f, 0.9f);

        Button buton = butonObj.AddComponent<Button>();
        ColorBlock cb = buton.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.3f, 1.2f, 1.4f, 1f);
        cb.pressedColor = new Color(0.7f, 0.6f, 0.8f, 1f);
        cb.selectedColor = Color.white;
        cb.fadeDuration = 0.1f;
        buton.colors = cb;

        buton.onClick.AddListener(tiklamasi);

        RectTransform butonRT = butonObj.GetComponent<RectTransform>();
        butonRT.anchorMin = new Vector2(0.5f, 0.5f);
        butonRT.anchorMax = new Vector2(0.5f, 0.5f);
        butonRT.sizeDelta = new Vector2(300, 52);
        butonRT.anchoredPosition = pozisyon;

        // Buton hover animasyonu
        PauseButtonHover hover = butonObj.AddComponent<PauseButtonHover>();

        // Buton yazısı
        GameObject yaziObj = new GameObject("Yazi");
        yaziObj.transform.SetParent(butonObj.transform, false);
        TextMeshProUGUI tmp = yaziObj.AddComponent<TextMeshProUGUI>();
        tmp.text = metin;
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.characterSpacing = 4f;

        tmp.enableVertexGradient = true;
        tmp.colorGradient = new VertexGradient(
            ustRenk, ustRenk,
            altRenk, altRenk
        );

        RectTransform yaziRT = yaziObj.GetComponent<RectTransform>();
        yaziRT.anchorMin = Vector2.zero;
        yaziRT.anchorMax = Vector2.one;
        yaziRT.sizeDelta = Vector2.zero;
    }

    private Slider SliderOlustur(Transform parent, string isim, Vector2 pozisyon,
        Color dolguRenk, float baslangicDeger)
    {
        // Ana slider objesi
        GameObject sliderObj = new GameObject(isim);
        sliderObj.transform.SetParent(parent, false);

        RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRT.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRT.sizeDelta = new Vector2(260, 30);
        sliderRT.anchoredPosition = pozisyon;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = baslangicDeger;

        // Arka plan (track)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.18f, 0.28f, 1f);

        RectTransform bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.25f);
        bgRT.anchorMax = new Vector2(1, 0.75f);
        bgRT.sizeDelta = Vector2.zero;
        bgRT.anchoredPosition = Vector2.zero;

        // Dolgu alanı
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);

        RectTransform fillAreaRT = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1, 0.75f);
        fillAreaRT.sizeDelta = new Vector2(-20, 0);
        fillAreaRT.anchoredPosition = Vector2.zero;

        // Dolgu
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = dolguRenk;

        RectTransform fillRT = fillObj.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one; 
        fillRT.sizeDelta = Vector2.zero;

        // Handle alanı
        GameObject handleAreaObj = new GameObject("Handle Slide Area");
        handleAreaObj.transform.SetParent(sliderObj.transform, false);

        RectTransform handleAreaRT = handleAreaObj.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.sizeDelta = new Vector2(-20, 0);
        handleAreaRT.anchoredPosition = Vector2.zero;

        // Handle (tutma noktası)
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleAreaObj.transform, false);
        Image handleImg = handleObj.AddComponent<Image>();
        handleImg.color = Color.white;

        RectTransform handleRT = handleObj.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(22, 22);

        // Slider referanslarını bağla
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;

        // Handle renk geçişi
        ColorBlock hcb = slider.colors;
        hcb.normalColor = Color.white;
        hcb.highlightedColor = dolguRenk;
        hcb.pressedColor = new Color(dolguRenk.r * 0.8f, dolguRenk.g * 0.8f, dolguRenk.b * 0.8f, 1f);
        hcb.fadeDuration = 0.1f;
        slider.colors = hcb;

        return slider;
    }
}

/// <summary>
/// Pause menü butonlarına hover efekti ekler.
/// TimeScale = 0 olduğu için unscaledTime kullanır.
/// </summary>
public class PauseButtonHover : MonoBehaviour,
    UnityEngine.EventSystems.IPointerEnterHandler,
    UnityEngine.EventSystems.IPointerExitHandler
{
    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = Vector3.one * 1.08f;
    private bool hovering = false;
    private float animT = 0f;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        hovering = false;
    }

    void Update()
    {
        // unscaledDeltaTime kullan çünkü TimeScale = 0
        float hedef = hovering ? 1f : 0f;
        animT = Mathf.MoveTowards(animT, hedef, Time.unscaledDeltaTime * 8f);
        transform.localScale = Vector3.Lerp(normalScale, hoverScale, animT);
    }
}
