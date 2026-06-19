using UnityEngine;

/// <summary>
/// Oyuncunun açtığı yetenekleri yönetir ve PlayerPrefs ile kalıcı olarak kaydeder.
/// Statik sınıf — her yerden erişilebilir, sahneler arası çalışır.
/// 
/// KULLANIM:
///   YetenekManager.YetenekAc(YetenekTipi.CiftZiplama);
///   if (YetenekManager.YetenekAcikMi(YetenekTipi.Dash)) { ... }
/// </summary>
public enum YetenekTipi
{
    CiftZiplama,    // Level Green sonunda açılır
    Dash,           // Level Purple sonunda açılır
    DuvarZiplama    // Level Red sonunda açılır
}

public static class YetenekManager
{
    // PlayerPrefs anahtarları
    private const string KEY_CIFT_ZIPLAMA = "Yetenek_CiftZiplama";
    private const string KEY_DASH = "Yetenek_Dash";
    private const string KEY_DUVAR_ZIPLAMA = "Yetenek_DuvarZiplama";
    private const string KEY_KAYIT_VAR = "Kayit_Var"; // Devam Et butonu için

    // ============ YETENEK SORGULAMA ============

    public static bool CiftZiplamaAcikMi
    {
        get { return PlayerPrefs.GetInt(KEY_CIFT_ZIPLAMA, 0) == 1; }
    }

    public static bool DashAcikMi
    {
        get { return PlayerPrefs.GetInt(KEY_DASH, 0) == 1; }
    }

    public static bool DuvarZiplamaAcikMi
    {
        get { return PlayerPrefs.GetInt(KEY_DUVAR_ZIPLAMA, 0) == 1; }
    }

    /// <summary>
    /// Belirli bir yeteneğin açık olup olmadığını kontrol eder.
    /// </summary>
    public static bool YetenekAcikMi(YetenekTipi yetenek)
    {
        switch (yetenek)
        {
            case YetenekTipi.CiftZiplama: return CiftZiplamaAcikMi;
            case YetenekTipi.Dash: return DashAcikMi;
            case YetenekTipi.DuvarZiplama: return DuvarZiplamaAcikMi;
            default: return false;
        }
    }

    // ============ YETENEK AÇMA ============

    /// <summary>
    /// Bir yeteneği kalıcı olarak açar ve kaydeder.
    /// </summary>
    public static void YetenekAc(YetenekTipi yetenek)
    {
        switch (yetenek)
        {
            case YetenekTipi.CiftZiplama:
                PlayerPrefs.SetInt(KEY_CIFT_ZIPLAMA, 1);
                break;
            case YetenekTipi.Dash:
                PlayerPrefs.SetInt(KEY_DASH, 1);
                break;
            case YetenekTipi.DuvarZiplama:
                PlayerPrefs.SetInt(KEY_DUVAR_ZIPLAMA, 1);
                break;
        }

        // Kayıt var işaretle (Devam Et butonu için)
        PlayerPrefs.SetInt(KEY_KAYIT_VAR, 1);
        PlayerPrefs.Save();

        Debug.Log("[YetenekManager] Yetenek açıldı: " + yetenek);
    }

    // ============ KAYIT SİSTEMİ ============

    /// <summary>
    /// Oyuncunun kayıtlı bir oyunu var mı? (Devam Et butonu göstermek için)
    /// </summary>
    public static bool KayitVarMi
    {
        get { return PlayerPrefs.GetInt(KEY_KAYIT_VAR, 0) == 1; }
    }

    /// <summary>
    /// Son ulaşılan sahneyi kaydeder (level geçişlerinde çağrılır).
    /// </summary>
    public static void SonSahneyiKaydet(string sahneAdi)
    {
        PlayerPrefs.SetString("Son_Sahne", sahneAdi);
        PlayerPrefs.SetInt(KEY_KAYIT_VAR, 1);
        PlayerPrefs.Save();
        Debug.Log("[YetenekManager] Son sahne kaydedildi: " + sahneAdi);
    }

    /// <summary>
    /// Son kaydedilen sahne adını döndürür.
    /// </summary>
    public static string SonKayitliSahne
    {
        get { return PlayerPrefs.GetString("Son_Sahne", "level_green"); }
    }

    /// <summary>
    /// Tüm yetenekleri ve kayıtları sıfırlar (Yeni Oyun için).
    /// </summary>
    public static void TumYetenekleriSifirla()
    {
        PlayerPrefs.DeleteKey(KEY_CIFT_ZIPLAMA);
        PlayerPrefs.DeleteKey(KEY_DASH);
        PlayerPrefs.DeleteKey(KEY_DUVAR_ZIPLAMA);
        PlayerPrefs.DeleteKey(KEY_KAYIT_VAR);
        PlayerPrefs.DeleteKey("Son_Sahne");
        PlayerPrefs.Save();

        Debug.Log("[YetenekManager] Tüm yetenekler ve kayıtlar sıfırlandı!");
    }

    /// <summary>
    /// Yetenek adını Türkçe olarak döndürür (UI mesajları için).
    /// </summary>
    public static string YetenekAdiAl(YetenekTipi yetenek)
    {
        switch (yetenek)
        {
            case YetenekTipi.CiftZiplama: return "DOUBLE JUMP";
            case YetenekTipi.Dash: return "DASH";
            case YetenekTipi.DuvarZiplama: return "WALL JUMP";
            default: return "";
        }
    }
}
