using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Ana menüdeki yazıları güzelleştiren script.
/// Canvas objesine ekleyin - tüm child TMP yazılarını otomatik bulur ve stillendirir.
/// Butonlara hover animasyonu, gradient renk, glow efekti ve pulse animasyonu ekler.
/// </summary>
public class MenuTextStyler : MonoBehaviour
{
    [Header("Genel Yazı Ayarları")]
    [Tooltip("Ana menü buton yazı boyutu")]
    public float anaMenuFontSize = 42f;


    [Header("Başlık Ayarları")]
    [Tooltip("Oyun başlığı eklensin mi?")]
    public bool baslikEkle = true;
    
    [Tooltip("Oyun başlığı metni")]
    public string baslikMetni = "2D PARKUR";
    
    [Tooltip("Başlık font boyutu")]
    public float baslikFontSize = 72f;

    [Header("Renk Ayarları")]
    public Color ustRenk = new Color(1f, 0.85f, 0.2f, 1f);      // Altın sarısı
    public Color altRenk = new Color(1f, 0.45f, 0.1f, 1f);       // Turuncu
    public Color baslikUstRenk = new Color(0.4f, 1f, 0.8f, 1f);  // Neon cyan
    public Color baslikAltRenk = new Color(0.2f, 0.6f, 1f, 1f);  // Mavi

    [Header("Glow / Outline Efekti")]
    [Tooltip("Yazılara dış glow eklensin mi?")]
    public bool glowEkle = true;
    public Color glowRenk = new Color(1f, 0.6f, 0f, 0.5f);
    public float glowKalinlik = 0.3f;

    [Header("Hover Animasyonu")]
    [Tooltip("Butonlara hover büyütme efekti")]
    public float hoverBuyutme = 1.15f;
    public float hoverAnimSuresi = 0.15f;

    [Header("Pulse Animasyonu (OYNA butonu)")]
    [Tooltip("OYNA butonuna nabız animasyonu")]
    public bool pulseAnimasyonu = true;
    public float pulseMin = 0.95f;
    public float pulseMax = 1.08f;
    public float pulseHizi = 2f;

    [Header("Buton Arka Plan Stili")]
    [Tooltip("Buton arka planlarını özelleştir")]
    public bool butonArkaPlanStili = true;
    public Color butonNormalRenk = new Color(0.1f, 0.1f, 0.15f, 0.85f);
    public Color butonHoverRenk = new Color(0.2f, 0.15f, 0.3f, 0.95f);

    [Header("Giriş Animasyonu")]
    [Tooltip("Menü açılırken yazılar sırayla gelsin")]
    public bool girisAnimasyonu = true;
    public float girisGecikme = 0.15f;

    // ======== Dahili değişkenler ========
    private List<ButtonAnimator> butonAnimatorleri = new List<ButtonAnimator>();
    private GameObject baslikObjesi;

    private void Start()
    {
        StartCoroutine(StilleriUygula());
    }

    private IEnumerator StilleriUygula()
    {
        // Bir frame bekle ki tüm UI elemanları oluşsun
        yield return null;

        // 1) Başlık ekle
        if (baslikEkle)
        {
            BaslikOlustur();
        }

        // 2) Tüm butonları bul ve stillendir
        Button[] tumButonlar = GetComponentsInChildren<Button>(true);

        int sira = 0;
        foreach (Button buton in tumButonlar)
        {
            StilUygula(buton, sira);
            sira++;
        }

        // 3) Giriş animasyonu
        if (girisAnimasyonu)
        {
            StartCoroutine(GirisAnimasyonuBaslat());
        }
    }

    private void BaslikOlustur()
    {
        // Canvas'ı bul
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) return;

        // AnaMenuPanel'i bul
        Transform anaMenu = transform.Find("AnaMenuPanel");
        if (anaMenu == null) return;

        // Başlık objesi oluştur
        baslikObjesi = new GameObject("OyunBasligi");
        baslikObjesi.transform.SetParent(anaMenu, false);

        // RectTransform ayarla
        RectTransform rt = baslikObjesi.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 260f); // Butonların üstünde
        rt.sizeDelta = new Vector2(600, 100);

        // TextMeshPro ekle
        TextMeshProUGUI tmp = baslikObjesi.AddComponent<TextMeshProUGUI>();
        tmp.text = baslikMetni;
        tmp.fontSize = baslikFontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;

        // Gradient renk
        tmp.enableVertexGradient = true;
        tmp.colorGradient = new VertexGradient(
            baslikUstRenk,                                            // Sol üst
            baslikUstRenk,                                            // Sağ üst
            baslikAltRenk,                                            // Sol alt
            baslikAltRenk                                             // Sağ alt
        );

        // Outline / Glow efekti (Material üzerinden)
        if (glowEkle && tmp.fontSharedMaterial != null)
        {
            Material mat = new Material(tmp.fontSharedMaterial);
            mat.EnableKeyword("UNDERLAY_ON");
            mat.SetFloat("_UnderlayOffsetX", 0f);
            mat.SetFloat("_UnderlayOffsetY", -1f);
            mat.SetFloat("_UnderlayDilate", 0.5f);
            mat.SetFloat("_UnderlaySoftness", 0.3f);
            mat.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.6f));
            tmp.fontMaterial = mat;
        }

        // Başlığa yavaş sallanma (bounce) animasyonu
        StartCoroutine(BaslikAnimasyonu(baslikObjesi.transform));
    }

    private void StilUygula(Button buton, int sira)
    {
        // TMP yazısını bul
        TextMeshProUGUI tmp = buton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp == null) return;

        // Buton adını kontrol et
        string butonAdi = buton.gameObject.name.ToLower();
        bool anaMenuButonu = (butonAdi == "oyna" || butonAdi.Contains("k\u0131\u015f") || butonAdi.Contains("cik"));
        bool oynaButonu = butonAdi == "oyna";

        // ---- Font Boyutu ----
        tmp.fontSize = anaMenuFontSize;
        
        // ---- Font Stili ----
        tmp.fontStyle = FontStyles.Bold;

        // ---- Gradient Renk ----
        tmp.enableVertexGradient = true;

        if (oynaButonu)
        {
            // OYNA butonu özel parlak renk
            tmp.colorGradient = new VertexGradient(
                new Color(1f, 1f, 0.4f, 1f),   // Parlak sarı üst
                new Color(1f, 1f, 0.4f, 1f),
                new Color(1f, 0.7f, 0f, 1f),   // Koyu altın alt
                new Color(1f, 0.7f, 0f, 1f)
            );
        }
        else if (butonAdi.Contains("ye\u015fil"))
        {
            tmp.colorGradient = new VertexGradient(
                new Color(0.4f, 1f, 0.5f, 1f),
                new Color(0.4f, 1f, 0.5f, 1f),
                new Color(0.1f, 0.7f, 0.3f, 1f),
                new Color(0.1f, 0.7f, 0.3f, 1f)
            );
        }
        else if (butonAdi == "mor")
        {
            tmp.colorGradient = new VertexGradient(
                new Color(0.8f, 0.5f, 1f, 1f),
                new Color(0.8f, 0.5f, 1f, 1f),
                new Color(0.5f, 0.2f, 0.8f, 1f),
                new Color(0.5f, 0.2f, 0.8f, 1f)
            );
        }
        else if (butonAdi.Contains("k\u0131rm\u0131z\u0131"))
        {
            tmp.colorGradient = new VertexGradient(
                new Color(1f, 0.4f, 0.3f, 1f),
                new Color(1f, 0.4f, 0.3f, 1f),
                new Color(0.8f, 0.1f, 0.1f, 1f),
                new Color(0.8f, 0.1f, 0.1f, 1f)
            );
        }
        else
        {
            // Diğer butonlar için genel gradient
            tmp.colorGradient = new VertexGradient(
                ustRenk, ustRenk,
                altRenk, altRenk
            );
        }

        // ---- Character Spacing (harf aralığı) ----
        tmp.characterSpacing = 5f;

        // ---- Buton arka plan rengi ----
        if (butonArkaPlanStili)
        {
            Image butonImage = buton.GetComponent<Image>();
            if (butonImage != null)
            {
                butonImage.color = butonNormalRenk;

                // Buton geçiş renklerini güncelle
                ColorBlock cb = buton.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(1.2f, 1.1f, 1.3f, 1f);
                cb.pressedColor = new Color(0.8f, 0.7f, 0.9f, 1f);
                cb.selectedColor = new Color(1.1f, 1.05f, 1.15f, 1f);
                cb.fadeDuration = 0.1f;
                buton.colors = cb;
            }
        }

        // ---- Buton boyutunu büyüt (yazıya uygun) ----
        RectTransform butonRT = buton.GetComponent<RectTransform>();
        if (butonRT != null)
        {
            Vector2 mevcut = butonRT.sizeDelta;
            if (anaMenuButonu)
            {
                butonRT.sizeDelta = new Vector2(Mathf.Max(mevcut.x, 320), Mathf.Max(mevcut.y, 65));
            }
            else
            {
                butonRT.sizeDelta = new Vector2(Mathf.Max(mevcut.x, 260), Mathf.Max(mevcut.y, 55));
            }
        }

        // ---- Hover animasyonu ekle ----
        ButtonAnimator animator = buton.gameObject.AddComponent<ButtonAnimator>();
        animator.hedefBuyukluk = hoverBuyutme;
        animator.animSuresi = hoverAnimSuresi;
        animator.normalBuyukluk = 1f;
        butonAnimatorleri.Add(animator);

        // ---- OYNA butonuna pulse animasyonu ----
        if (oynaButonu && pulseAnimasyonu)
        {
            StartCoroutine(PulseAnimasyonu(buton.transform));
        }

        // ---- Giriş animasyonu için başlangıç ayarı ----
        if (girisAnimasyonu)
        {
            CanvasGroup cg = buton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            buton.transform.localScale = Vector3.one * 0.5f;
        }
    }

    // ======== ANİMASYONLAR ========

    private IEnumerator GirisAnimasyonuBaslat()
    {
        Button[] tumButonlar = GetComponentsInChildren<Button>(true);
        
        // Başlık animasyonu
        if (baslikObjesi != null)
        {
            CanvasGroup baslikCG = baslikObjesi.AddComponent<CanvasGroup>();
            baslikCG.alpha = 0f;
            baslikObjesi.transform.localScale = Vector3.one * 0.3f;

            yield return new WaitForSeconds(0.2f);

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 2.5f;
                float ease = EaseOutBack(t);
                baslikCG.alpha = Mathf.Lerp(0, 1, t * 2f);
                baslikObjesi.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1f, ease);
                yield return null;
            }
            baslikObjesi.transform.localScale = Vector3.one;
            baslikCG.alpha = 1f;
        }

        // Butonlar sırayla gelsin
        foreach (Button buton in tumButonlar)
        {
            CanvasGroup cg = buton.GetComponent<CanvasGroup>();
            if (cg == null) continue;

            StartCoroutine(ButonGirisAnimasyonu(buton.transform, cg));
            yield return new WaitForSeconds(girisGecikme);
        }
    }

    private IEnumerator ButonGirisAnimasyonu(Transform butonTransform, CanvasGroup cg)
    {
        float t = 0;
        Vector3 baslangicPoz = butonTransform.localPosition + new Vector3(-60f, 0, 0);
        Vector3 hedefPoz = butonTransform.localPosition;

        butonTransform.localPosition = baslangicPoz;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            float ease = EaseOutBack(t);
            
            cg.alpha = Mathf.Lerp(0, 1, t * 1.5f);
            butonTransform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, ease);
            butonTransform.localPosition = Vector3.Lerp(baslangicPoz, hedefPoz, ease);
            
            yield return null;
        }

        cg.alpha = 1f;
        butonTransform.localScale = Vector3.one;
        butonTransform.localPosition = hedefPoz;
    }

    private IEnumerator PulseAnimasyonu(Transform butonTransform)
    {
        // Giriş animasyonunun bitmesini bekle
        yield return new WaitForSeconds(1.5f);

        while (true)
        {
            float t = Mathf.PingPong(Time.time * pulseHizi, 1f);
            float scale = Mathf.Lerp(pulseMin, pulseMax, t);
            
            // Hover kontrolü - hover'daysa pulse yapma
            ButtonAnimator anim = butonTransform.GetComponent<ButtonAnimator>();
            if (anim != null && !anim.hoverAktif)
            {
                butonTransform.localScale = Vector3.one * scale;
            }

            yield return null;
        }
    }

    private IEnumerator BaslikAnimasyonu(Transform baslik)
    {
        while (true)
        {
            float t = Time.time;
            float yOffset = Mathf.Sin(t * 1.2f) * 5f;
            float rotation = Mathf.Sin(t * 0.8f) * 1.5f;

            RectTransform rt = baslik.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 pos = rt.anchoredPosition;
                pos.y = 260f + yOffset;
                rt.anchoredPosition = pos;
            }

            baslik.localRotation = Quaternion.Euler(0, 0, rotation);
            yield return null;
        }
    }

    // ======== YARDIMCI FONKSİYONLAR ========

    private float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}

/// <summary>
/// Butonlara hover (üzerine gelince) büyüme/küçülme animasyonu ekler.
/// </summary>
public class ButtonAnimator : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public float hedefBuyukluk = 1.15f;
    public float normalBuyukluk = 1f;
    public float animSuresi = 0.15f;
    [HideInInspector] public bool hoverAktif = false;

    private Coroutine aktifAnimasyon;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        hoverAktif = true;
        if (aktifAnimasyon != null) StopCoroutine(aktifAnimasyon);
        aktifAnimasyon = StartCoroutine(BuyutAnimasyonu(hedefBuyukluk));
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        hoverAktif = false;
        if (aktifAnimasyon != null) StopCoroutine(aktifAnimasyon);
        aktifAnimasyon = StartCoroutine(BuyutAnimasyonu(normalBuyukluk));
    }

    private IEnumerator BuyutAnimasyonu(float hedef)
    {
        Vector3 baslangic = transform.localScale;
        Vector3 son = Vector3.one * hedef;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / animSuresi;
            transform.localScale = Vector3.Lerp(baslangic, son, t);
            yield return null;
        }
        transform.localScale = son;
    }
}
