using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGate : MonoBehaviour
{
    [Header("Geçiş Ayarları")]
    public Transform nextSpawnPoint; // Işınlanacağımız yer
    public bool isFinalLevelOfBiome = false; // Bu biyomun son bölümü mü?
    public string nextBiomeName; // Eğer son bölümse, yüklenecek yeni sahne adı

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Karakterimizin Parent (Ana) objesinde veya değen alt objesinde PlayerMovement var mı diye bakıyoruz.
        // Bu sayede 'FeetCollider' (Ayak) gibi alt objeler bayrağa değse bile kod sorunsuz çalışır!
        PlayerMovement player = collision.GetComponentInParent<PlayerMovement>();

        if (player != null) // Eğer çarpan kişi oyuncuysa (üzerinde PlayerMovement varsa)
        {
            Debug.Log("-> DOKUNAN KİŞİ OYUNCU (" + collision.gameObject.name + ")! Işınlanma tetikleniyor...");
            
            if (isFinalLevelOfBiome)
            {
                Debug.Log("-> Bu biyomun son bölümü. Yeni sahne yükleniyor: " + nextBiomeName);
                // Geçilen sahneyi kaydet (Devam Et butonu için)
                YetenekManager.SonSahneyiKaydet(nextBiomeName);
                SceneManager.LoadScene(nextBiomeName);
            }
            else
            {
                Debug.Log("-> Biyom devam ediyor, diğer bayrağa ışınlanacak.");
                // Mevcut sahneyi kaydet
                YetenekManager.SonSahneyiKaydet(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                StartCoroutine(TeleportPlayer(player));
            }
        }
    }

    private System.Collections.IEnumerator TeleportPlayer(PlayerMovement player)
    {
        // 1. Karakteri dondur ve fiziksel hızını sıfırla ki ışınlandıktan sonra kaymasın
        player.enabled = false; 
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // HATA KONTROLÜ: Eğer Unity Inspector'dan gidilecek yeri atamadıysan hata ver ve durdur.
        if (nextSpawnPoint == null)
        {
            Debug.LogError("IŞINLANMA HATASI: LevelGate objesinde 'Next Spawn Point' boş! Lütfen Unity'den gideceği noktayı sürükleyin.");
            player.enabled = true; // Karakterin donup kalmaması için geri canlandır
            yield break;
        }

        // 2. Kararma efektini başlat (DieAndRespawnRoutine mantığı)
        if (player.fadeScreen != null)
        {
            while (player.fadeScreen.color.a < 1f)
            {
                Color c = player.fadeScreen.color;
                c.a += Time.deltaTime * player.fadeSpeed;
                player.fadeScreen.color = c;
                yield return null; 
            }
        }
        else
        {
            // Eğer Fade Screen atanmamışsa kısa bir süre bekle (göz yanılması için)
            yield return new WaitForSeconds(0.2f);
        }

        // 3. Işınla ve Respawn noktasını GÜNCELLE
        // DİKKAT: Yeni noktanın (nextSpawnPoint) yanlışlıkla Z pozisyonu farklıysa 
        // haritanın görünmez olmasına yol açar (Kamera duvarın/haritanın içine veya arkasına geçer).
        // Bu yüzden karakterin Z değerini KORUYORUZ!
        Vector3 newPos = nextSpawnPoint.position;
        newPos.z = player.transform.position.z; 
        player.transform.position = newPos;

        player.respawnPoint = nextSpawnPoint; 
        
        // Geçiş arası siyah ekranda bekleme süresi
        yield return new WaitForSeconds(player.waitInBlackTime);

        // 4. Ekranı geri aydınlat
        if (player.fadeScreen != null)
        {
            while (player.fadeScreen.color.a > 0f)
            {
                Color c = player.fadeScreen.color;
                c.a -= Time.deltaTime * player.fadeSpeed;
                player.fadeScreen.color = c;
                yield return null; 
            }
        }

        // 5. Karakteri serbest bırak
        player.enabled = true;
        
        Debug.Log("Yeni bölüme ışınlandı ve kayıt noktası güncellendi!");
    }
}