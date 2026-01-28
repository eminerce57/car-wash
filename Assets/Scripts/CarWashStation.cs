using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Araba Yıkama İstasyonu - Basit ve Temiz
/// </summary>
public class CarWashStation : MonoBehaviour
{
    public static CarWashStation Instance { get; private set; }
    
    [Header("Yıkama Ayarları")]
    public float washDuration = 3f;
    public float moneyPerWash = 50f;
    
    [Header("Kuyruk Ayarları")]
    public int maxQueueSize = 3;
    
    [Header("Durum")]
    public bool isWashing = false;
    public float washProgress = 0f;
    
    [Header("İstatistikler")]
    public int totalCarsWashed = 0;
    public float totalEarnings = 0f;
    
    [Header("UI")]
    public WashProgressUI progressUI;  // Progress bar referansı
    
    // Kuyruk
    private Queue<CarMover> carQueue = new Queue<CarMover>();
    private CarMover currentCar;
    
    public int QueueCount => carQueue.Count;
    public bool CanAcceptCar => carQueue.Count < maxQueueSize && !isWashing || carQueue.Count < maxQueueSize;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        CarMover car = other.GetComponent<CarMover>();
        if (car == null) return;
        if (!car.goingToGarage) return;
        
        // Kuyruğa ekle
        if (!carQueue.Contains(car) && carQueue.Count < maxQueueSize)
        {
            car.StopCar();
            carQueue.Enqueue(car);
            Debug.Log($"Araç kuyruğa eklendi! Kuyruk: {carQueue.Count}");
            
            // Yıkama yapmıyorsak başlat
            if (!isWashing)
            {
                StartCoroutine(ProcessQueue());
            }
        }
    }

    private IEnumerator ProcessQueue()
    {
        while (carQueue.Count > 0)
        {
            // Sıradaki aracı al
            currentCar = carQueue.Dequeue();
            
            if (currentCar == null)
            {
                continue;
            }
            
            // Yıkama başlat
            isWashing = true;
            washProgress = 0f;
            Debug.Log("Yıkama başladı!");
            
            // Progress UI göster
            if (progressUI != null) progressUI.Show();
            
            // Yıkama süresi
            float elapsed = 0f;
            while (elapsed < washDuration)
            {
                elapsed += Time.deltaTime;
                washProgress = elapsed / washDuration;
                
                // UI güncelle
                if (progressUI != null)
                {
                    progressUI.SetProgress(washProgress);
                    progressUI.SetTimer(washDuration - elapsed);
                }
                
                yield return null;
            }
            
            // Yıkama tamamlandı
            washProgress = 1f;
            
            // Progress UI gizle
            if (progressUI != null) progressUI.Hide();
            totalCarsWashed++;
            totalEarnings += moneyPerWash;
            
            // Para ekle
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(moneyPerWash);
            }
            
            Debug.Log($"Yıkama tamamlandı! +${moneyPerWash}");
            
            // Aracı yok et
            if (currentCar != null)
            {
                Destroy(currentCar.gameObject);
            }
            
            isWashing = false;
            
            // Biraz bekle (sonraki araç için)
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Kuyruk dolu mu?
    /// </summary>
    public bool IsQueueFull()
    {
        return carQueue.Count >= maxQueueSize;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isWashing ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(3f, 2f, 3f));
    }
}
