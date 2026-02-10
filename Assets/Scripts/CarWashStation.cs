using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Araba Yıkama İstasyonu - Level Sistemi ile
/// </summary>
public class CarWashStation : MonoBehaviour
{
    public static CarWashStation Instance { get; private set; }
    
    [Header("Level Sistemi")]
    public int currentLevel = 1;
    public int maxLevel = 7;
    
    [Header("Temel Değerler (Level 1)")]
    public float baseWashDuration = 10f;     // Başlangıç yıkama süresi (yavaş)
    public float baseMoneyPerWash = 15f;     // Başlangıç kazanç (düşük)
    public float baseUpgradeCost = 75f;      // Başlangıç upgrade maliyeti
    
    [Header("Level Başına Artış")]
    public float durationReductionPerLevel = 1.2f;   // Her level'da süre azalması
    public float moneyIncreasePerLevel = 12f;        // Her level'da kazanç artışı
    public float upgradeCostMultiplier = 2f;         // Her level'da maliyet çarpanı
    
    [Header("Hesaplanan Değerler (Otomatik)")]
    public float washDuration;
    public float moneyPerWash;
    public float nextUpgradeCost;
    
    [Header("Kuyruk Ayarları")]
    public int maxQueueSize = 4;  // Maksimum 4 araç bekleyebilir
    
    [Header("Durum")]
    public bool isWashing = false;
    public float washProgress = 0f;
    
    [Header("İstatistikler")]
    public int totalCarsWashed = 0;
    public float totalEarnings = 0f;
    
    [Header("Reklam Tabelası")]
    public bool hasAdvertising = false;      // Reklam satın alındı mı?
    public float advertisingCost = 1000f;    // Reklam fiyatı
    public float advertisingTurnBonus = 0.3f;        // Dönüş şansı bonusu (+%30)
    public GameObject advertisingSignPrefab;         // Tabela prefab'ı (yolun başına koyulacak)
    public Transform advertisingSignPosition;        // Tabelanın konacağı yer
    private GameObject spawnedSign;                  // Spawn edilen tabela
    
    [Header("UI")]
    public WashProgressUI progressUI;
    public UpgradePanel upgradePanel;  // Upgrade paneli
    
    // Kuyruk
    private Queue<CarMover> carQueue = new Queue<CarMover>();
    private CarMover currentCar;
    
    public int QueueCount => carQueue.Count;
    public bool CanAcceptCar => carQueue.Count < maxQueueSize && !isWashing || carQueue.Count < maxQueueSize;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        CalculateStats();
    }
    
    private void Update()
    {
        // Tıklama kontrolü
        Update_CheckClick();
    }
    
    /// <summary>
    /// Level'a göre değerleri hesapla
    /// </summary>
    public void CalculateStats()
    {
        // Yıkama süresi (her level'da azalır, minimum 1 saniye)
        washDuration = Mathf.Max(1f, baseWashDuration - (durationReductionPerLevel * (currentLevel - 1)));
        
        // Kazanç (her level'da artar)
        moneyPerWash = baseMoneyPerWash + (moneyIncreasePerLevel * (currentLevel - 1));
        
        // Sonraki upgrade maliyeti
        nextUpgradeCost = baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, currentLevel - 1);
    }
    
    /// <summary>
    /// Upgrade yap
    /// </summary>
    public bool TryUpgrade()
    {
        if (currentLevel >= maxLevel)
        {
            Debug.Log("Maksimum level'a ulaşıldı!");
            return false;
        }
        
        if (MoneyManager.Instance == null) return false;
        
        if (MoneyManager.Instance.SpendMoney(nextUpgradeCost))
        {
            currentLevel++;
            CalculateStats();
            
            // Ses çal
            if (SoundManager.Instance != null) SoundManager.Instance.PlayUpgradeSound();
            
            Debug.Log($"Upgrade başarılı! Yeni Level: {currentLevel}");
            return true;
        }
        
        Debug.Log("Yetersiz bakiye!");
        return false;
    }
    
    /// <summary>
    /// Upgrade yapılabilir mi?
    /// </summary>
    public bool CanUpgrade()
    {
        if (currentLevel >= maxLevel) return false;
        if (MoneyManager.Instance == null) return false;
        return MoneyManager.Instance.CanAfford(nextUpgradeCost);
    }
    
    /// <summary>
    /// Max level'a ulaşıldı mı?
    /// </summary>
    public bool IsMaxLevel()
    {
        return currentLevel >= maxLevel;
    }
    
    /// <summary>
    /// Reklam satın al
    /// </summary>
    public bool TryBuyAdvertising()
    {
        if (hasAdvertising)
        {
            Debug.Log("Reklam zaten satın alınmış!");
            return false;
        }
        
        if (MoneyManager.Instance == null) return false;
        
        if (MoneyManager.Instance.SpendMoney(advertisingCost))
        {
            hasAdvertising = true;
            
            // TurnTrigger'ın dönüş şansını artır
            TurnTrigger trigger = FindObjectOfType<TurnTrigger>();
            if (trigger != null)
            {
                trigger.turnChance += advertisingTurnBonus;
                trigger.turnChance = Mathf.Clamp01(trigger.turnChance); // Max %100
            }
            
            // Fiziksel tabelayı spawn et
            SpawnAdvertisingSign();
            
            // Ses çal
            if (SoundManager.Instance != null) SoundManager.Instance.PlayUnlockSound();
            
            Debug.Log($"Reklam tabelası satın alındı! Dönüş şansı +%{advertisingTurnBonus * 100}");
            return true;
        }
        
        Debug.Log("Yetersiz bakiye!");
        return false;
    }
    
    /// <summary>
    /// Reklam satın alınabilir mi?
    /// </summary>
    public bool CanBuyAdvertising()
    {
        if (hasAdvertising)
        {
            Debug.Log("CanBuyAdvertising: Zaten satın alınmış");
            return false;
        }
        if (MoneyManager.Instance == null)
        {
            Debug.LogWarning("CanBuyAdvertising: MoneyManager.Instance NULL!");
            return false;
        }
        
        bool canAfford = MoneyManager.Instance.CanAfford(advertisingCost);
        Debug.Log($"CanBuyAdvertising: Para={MoneyManager.Instance.currentMoney}, Fiyat={advertisingCost}, CanAfford={canAfford}");
        return canAfford;
    }
    
    /// <summary>
    /// Gerçek kazancı hesapla
    /// </summary>
    public float GetActualMoneyPerWash()
    {
        return moneyPerWash; // Reklam kazancı etkilemiyor, sadece müşteri artışı
    }
    
    /// <summary>
    /// Fiziksel reklam tabelasını spawn et
    /// </summary>
    private void SpawnAdvertisingSign()
    {
        if (advertisingSignPrefab == null)
        {
            Debug.Log("Reklam tabelası prefab'ı atanmamış!");
            return;
        }
        
        // Pozisyon belirle
        Vector3 spawnPos;
        Quaternion spawnRot;
        
        if (advertisingSignPosition != null)
        {
            spawnPos = advertisingSignPosition.position;
            spawnRot = advertisingSignPosition.rotation;
        }
        else
        {
            // Varsayılan: İstasyonun önünde
            spawnPos = transform.position + transform.forward * 10f;
            spawnRot = transform.rotation;
        }
        
        // Tabelayı spawn et
        spawnedSign = Instantiate(advertisingSignPrefab, spawnPos, spawnRot);
        spawnedSign.name = "AdvertisingSign";
        
        Debug.Log("Reklam tabelası yerleştirildi!");
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
            
            // Garaja giden araç sayısını azalt (artık kuyrukta)
            TurnTrigger.OnCarReachedGarage();
            
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
            
            // Yıkama sesi başlat
            if (SoundManager.Instance != null) SoundManager.Instance.StartWashingSound();
            
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
            
            // Yıkama sesini durdur
            if (SoundManager.Instance != null) SoundManager.Instance.StopWashingSound();
            
            // Progress UI gizle
            if (progressUI != null) progressUI.Hide();
            totalCarsWashed++;
            float actualMoney = GetActualMoneyPerWash(); // Reklam bonusu dahil
            totalEarnings += actualMoney;
            
            // Para ekle + ses çal
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(actualMoney);
            }
            if (SoundManager.Instance != null) SoundManager.Instance.PlayCoinSound();
            
            Debug.Log($"Yıkama tamamlandı! +${actualMoney:F0}" + (hasAdvertising ? " (Reklam bonusu!)" : ""));
            
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
    /// Kuyruk dolu mu? (yıkanan araç dahil)
    /// </summary>
    public bool IsQueueFull()
    {
        int totalCars = carQueue.Count;
        if (isWashing) totalCars++; // Yıkanan araç da sayılsın
        return totalCars >= maxQueueSize;
    }
    
    /// <summary>
    /// Toplam bekleyen araç sayısı
    /// </summary>
    public int GetTotalWaitingCars()
    {
        int total = carQueue.Count;
        if (isWashing) total++;
        return total;
    }

    /// <summary>
    /// Tıklama kontrolü
    /// </summary>
    private void Update_CheckClick()
    {
        // Sol tık kontrolü
        if (UnityEngine.InputSystem.Mouse.current != null && 
            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckClickOnStation();
        }
        // Touch kontrolü (mobil)
        else if (UnityEngine.InputSystem.Touchscreen.current != null &&
                 UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckClickOnStation();
        }
    }
    
    private void CheckClickOnStation()
    {
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Pointer.current.position.ReadValue());
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Bu objeye mi tıklandı?
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                if (upgradePanel != null)
                {
                    upgradePanel.Show(this);
                    Debug.Log("Upgrade Panel açıldı!");
                }
                else
                {
                    Debug.Log("Upgrade Panel referansı boş!");
                }
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = isWashing ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(3f, 2f, 3f));
    }
}
