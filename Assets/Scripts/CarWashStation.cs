using UnityEngine;
using System.Collections;

/// <summary>
/// Araba Yıkama İstasyonu
/// Araç gelince yıkama yapar ve para kazandırır
/// </summary>
public class CarWashStation : MonoBehaviour
{
    [Header("Yıkama Ayarları")]
    public float washDuration = 3f;         // Yıkama süresi (saniye)
    public float moneyPerWash = 50f;        // Yıkama başına kazanç
    
    [Header("Pozisyonlar")]
    public Transform washPoint;              // Aracın yıkanacağı nokta
    public Transform exitPoint;              // Aracın çıkacağı nokta
    public float exitSpeed = 3f;             // Çıkış hızı
    
    [Header("Durum")]
    public bool isOccupied = false;          // İstasyon dolu mu?
    public bool isWashing = false;           // Yıkama yapılıyor mu?
    public float washProgress = 0f;          // Yıkama ilerlemesi (0-1)
    
    [Header("İstatistikler")]
    public int totalCarsWashed = 0;          // Toplam yıkanan araç
    public float totalEarnings = 0f;         // Toplam kazanç
    
    [Header("Görsel (Opsiyonel)")]
    public GameObject washEffectPrefab;      // Su/köpük efekti
    public Transform effectSpawnPoint;       // Efekt spawn noktası
    
    private GameObject currentCar;
    private GameObject currentEffect;

    private void Start()
    {
        // Wash point yoksa kendi pozisyonunu kullan
        if (washPoint == null)
        {
            washPoint = transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Zaten dolu mu?
        if (isOccupied) return;
        
        // Araç mı?
        CarMover car = other.GetComponent<CarMover>();
        if (car == null) return;
        
        // Garaja gelen araç mı?
        if (!car.goingToGarage) return;
        
        // Arabayı al
        StartWash(other.gameObject, car);
    }

    /// <summary>
    /// Yıkama işlemini başlat
    /// </summary>
    private void StartWash(GameObject car, CarMover carMover)
    {
        isOccupied = true;
        currentCar = car;
        
        // Aracı durdur
        carMover.enabled = false;
        
        // Aracı yıkama noktasına taşı
        car.transform.position = washPoint.position;
        
        // Yıkama başlat
        StartCoroutine(WashProcess());
    }

    /// <summary>
    /// Yıkama işlemi
    /// </summary>
    private IEnumerator WashProcess()
    {
        isWashing = true;
        washProgress = 0f;
        
        Debug.Log("Yıkama başladı!");
        
        // Efekt spawn et
        SpawnWashEffect();
        
        // Yıkama süresi boyunca bekle
        float elapsed = 0f;
        while (elapsed < washDuration)
        {
            elapsed += Time.deltaTime;
            washProgress = elapsed / washDuration;
            yield return null;
        }
        
        washProgress = 1f;
        isWashing = false;
        
        // Para kazan
        EarnMoney();
        
        // Efekti kaldır
        StopWashEffect();
        
        Debug.Log("Yıkama tamamlandı! Para kazanıldı: $" + moneyPerWash);
        
        // Aracı çıkışa gönder
        StartCoroutine(ExitCar());
    }

    /// <summary>
    /// Para kazan
    /// </summary>
    private void EarnMoney()
    {
        totalCarsWashed++;
        totalEarnings += moneyPerWash;
        
        // GameManager veya EconomyManager varsa ona da bildir
        // EconomyManager.Instance?.AddMoney(moneyPerWash);
    }

    /// <summary>
    /// Aracı çıkışa gönder
    /// </summary>
    private IEnumerator ExitCar()
    {
        if (currentCar == null)
        {
            isOccupied = false;
            yield break;
        }
        
        // Çıkış noktası
        Vector3 exitPos = exitPoint != null ? exitPoint.position : transform.position + transform.forward * 10f;
        
        // Aracı çıkışa doğru hareket ettir
        while (currentCar != null && Vector3.Distance(currentCar.transform.position, exitPos) > 1f)
        {
            Vector3 direction = (exitPos - currentCar.transform.position).normalized;
            currentCar.transform.position += direction * exitSpeed * Time.deltaTime;
            
            // Aracı çıkış yönüne döndür
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                currentCar.transform.rotation = Quaternion.Slerp(
                    currentCar.transform.rotation, 
                    targetRot, 
                    Time.deltaTime * 5f
                );
            }
            
            yield return null;
        }
        
        // Aracı yok et
        if (currentCar != null)
        {
            Destroy(currentCar);
        }
        
        currentCar = null;
        isOccupied = false;
        
        Debug.Log("Araç çıktı! İstasyon boş.");
    }

    /// <summary>
    /// Yıkama efekti spawn et
    /// </summary>
    private void SpawnWashEffect()
    {
        if (washEffectPrefab == null) return;
        
        Vector3 spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : washPoint.position;
        currentEffect = Instantiate(washEffectPrefab, spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// Yıkama efektini durdur
    /// </summary>
    private void StopWashEffect()
    {
        if (currentEffect != null)
        {
            Destroy(currentEffect);
        }
    }

    // Editörde göster
    private void OnDrawGizmos()
    {
        // Yıkama noktası
        Gizmos.color = Color.blue;
        Vector3 washPos = washPoint != null ? washPoint.position : transform.position;
        Gizmos.DrawWireSphere(washPos, 1f);
        
        // Çıkış noktası
        if (exitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(exitPoint.position, 0.5f);
            Gizmos.DrawLine(washPos, exitPoint.position);
        }
        
        // Trigger alanı
        Gizmos.color = isOccupied ? Color.red : Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(3f, 2f, 3f));
    }
}
