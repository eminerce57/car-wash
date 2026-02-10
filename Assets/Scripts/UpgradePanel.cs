using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Upgrade Panel - CarWashStation'a tıklayınca açılır
/// Level, kazanç, hız ve upgrade bilgilerini gösterir
/// </summary>
public class UpgradePanel : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelObject;          // Ana panel objesi
    
    [Header("Bilgi Yazıları")]
    public TextMeshProUGUI levelText;       // "Level 3"
    public TextMeshProUGUI speedText;       // "Hız: 2.5s"
    public TextMeshProUGUI earningText;     // "Kazanç: $75"
    public TextMeshProUGUI statsText;       // "Toplam: 45 araç, $2250"
    
    [Header("Upgrade Butonu")]
    public Button upgradeButton;
    public TextMeshProUGUI upgradeCostText; // "$180"
    public TextMeshProUGUI upgradeInfoText; // "Hız +0.3s, Kazanç +$15"
    
    [Header("Reklam Butonu")]
    public Button advertisingButton;  // Sadece buton yeterli
    
    [Header("Kapat Butonu")]
    public Button closeButton;
    
    [Header("Referans")]
    public CarWashStation targetStation;    // Hangi istasyonu gösteriyor
    
    [Header("Renkler")]
    public Color canAffordColor = Color.green;
    public Color cantAffordColor = Color.gray;
    public Color maxLevelColor = Color.yellow;
    public Color purchasedColor = Color.cyan;  // Satın alınmış

    private void Start()
    {
        // Başlangıçta gizle
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
        
        // Buton eventleri
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
            Debug.Log("Upgrade butonu bağlandı");
        }
        
        if (advertisingButton != null)
        {
            advertisingButton.onClick.AddListener(OnAdvertisingClicked);
            Debug.Log("Reklam butonu bağlandı");
        }
        else
        {
            Debug.LogWarning("Advertising Button referansı BOŞ! Inspector'dan bağla.");
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
    }

    private void Update()
    {
        // Panel açıksa sürekli güncelle
        if (panelObject != null && panelObject.activeSelf && targetStation != null)
        {
            UpdateUI();
        }
    }

    /// <summary>
    /// Paneli göster
    /// </summary>
    public void Show(CarWashStation station)
    {
        targetStation = station;
        
        if (panelObject != null)
        {
            panelObject.SetActive(true);
        }
        
        UpdateUI();
    }

    /// <summary>
    /// Paneli gizle
    /// </summary>
    public void Hide()
    {
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
    }

    /// <summary>
    /// UI güncelle
    /// </summary>
    public void UpdateUI()
    {
        if (targetStation == null) return;
        
        // Level
        if (levelText != null)
        {
            levelText.text = $"Level {targetStation.currentLevel}";
            
            if (targetStation.IsMaxLevel())
            {
                levelText.text += " (MAX)";
            }
        }
        
        // Hız (yıkama süresi)
        if (speedText != null)
        {
            speedText.text = $"Yıkama Süresi: {targetStation.washDuration:F1}s";
        }
        
        // Kazanç
        if (earningText != null)
        {
            earningText.text = $"Kazanç: ${targetStation.moneyPerWash:F0}";
        }
        
        // İstatistikler
        if (statsText != null)
        {
            statsText.text = $"Toplam: {targetStation.totalCarsWashed} araç, ${targetStation.totalEarnings:F0}";
        }
        
        // Upgrade butonu
        if (upgradeButton != null)
        {
            if (targetStation.IsMaxLevel())
            {
                // Max level
                upgradeButton.interactable = false;
                
                if (upgradeCostText != null)
                    upgradeCostText.text = "MAX";
                    
                if (upgradeInfoText != null)
                    upgradeInfoText.text = "Maksimum seviye!";
                    
                var buttonImage = upgradeButton.GetComponent<Image>();
                if (buttonImage != null)
                    buttonImage.color = maxLevelColor;
            }
            else
            {
                // Normal upgrade
                upgradeButton.interactable = targetStation.CanUpgrade();
                
                if (upgradeCostText != null)
                    upgradeCostText.text = $"${targetStation.nextUpgradeCost:F0}";
                    
                if (upgradeInfoText != null)
                {
                    float newDuration = Mathf.Max(1f, targetStation.washDuration - targetStation.durationReductionPerLevel);
                    float newMoney = targetStation.moneyPerWash + targetStation.moneyIncreasePerLevel;
                    upgradeInfoText.text = $"Hız: {newDuration:F1}s | Kazanç: ${newMoney:F0}";
                }
                
                var buttonImage = upgradeButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = targetStation.CanUpgrade() ? canAffordColor : cantAffordColor;
                }
            }
        }
        
        // Reklam butonu
        if (advertisingButton != null)
        {
            var adButtonImage = advertisingButton.GetComponent<Image>();
            
            if (targetStation.hasAdvertising)
            {
                // Zaten satın alınmış
                advertisingButton.interactable = false;
                if (adButtonImage != null)
                    adButtonImage.color = purchasedColor;
            }
            else
            {
                // Satın alınabilir
                bool canBuy = targetStation.CanBuyAdvertising();
                advertisingButton.interactable = canBuy;
                
                if (adButtonImage != null)
                    adButtonImage.color = canBuy ? canAffordColor : cantAffordColor;
            }
        }
    }

    /// <summary>
    /// Upgrade butonuna basıldı
    /// </summary>
    private void OnUpgradeClicked()
    {
        if (targetStation != null)
        {
            if (targetStation.TryUpgrade())
            {
                UpdateUI();
            }
        }
    }
    
    /// <summary>
    /// Reklam butonuna basıldı
    /// </summary>
    private void OnAdvertisingClicked()
    {
        Debug.Log("Reklam butonuna tıklandı!");
        
        if (targetStation != null)
        {
            Debug.Log($"TargetStation var. HasAdvertising: {targetStation.hasAdvertising}, CanBuy: {targetStation.CanBuyAdvertising()}");
            
            if (targetStation.TryBuyAdvertising())
            {
                Debug.Log("Reklam satın alındı!");
                UpdateUI();
            }
        }
        else
        {
            Debug.Log("TargetStation NULL!");
        }
    }
}
