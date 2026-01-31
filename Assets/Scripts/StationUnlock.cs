using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// İstasyon Kilidi - Para ile açılabilir
/// </summary>
public class StationUnlock : MonoBehaviour
{
    [Header("Ayarlar")]
    public float unlockPrice = 500f;     // Açma fiyatı
    public bool isUnlocked = false;      // Açık mı?
    
    [Header("Referanslar")]
    public GameObject lockedVisual;       // Kilitli görünüm (kilit ikonu vs)
    public GameObject unlockedStation;    // Açık istasyon (CarWashStation objesi)
    public CarWashStation carWashStation; // Script referansı
    
    [Header("UI")]
    public GameObject unlockButton;       // Satın al butonu
    public TextMeshProUGUI priceText;     // Fiyat yazısı (TextMeshPro)
    
    [Header("TurnTrigger")]
    public TurnTrigger turnTrigger;       // Bu istasyona yönlendiren trigger

    private void Start()
    {
        UpdateVisuals();
        UpdatePriceText();
    }

    private void Update()
    {
        // Butonu aktif/pasif yap (para yeterliliğine göre)
        if (unlockButton != null && !isUnlocked)
        {
            bool canAfford = MoneyManager.Instance != null && 
                            MoneyManager.Instance.CanAfford(unlockPrice);
            
            // Buton rengini değiştir
            var buttonImage = unlockButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = canAfford ? Color.green : Color.gray;
            }
        }
    }

    /// <summary>
    /// Satın al butonuna basıldığında
    /// </summary>
    public void OnUnlockButtonClicked()
    {
        if (isUnlocked) return;
        
        if (MoneyManager.Instance != null && MoneyManager.Instance.SpendMoney(unlockPrice))
        {
            Unlock();
        }
        else
        {
            Debug.Log("Yetersiz bakiye!");
        }
    }

    /// <summary>
    /// İstasyonu aç
    /// </summary>
    public void Unlock()
    {
        isUnlocked = true;
        
        // Unlock sesi çal
        if (SoundManager.Instance != null) SoundManager.Instance.PlayUnlockSound();
        
        // TurnTrigger'ı aktif et
        if (turnTrigger != null)
        {
            turnTrigger.gameObject.SetActive(true);
        }
        
        UpdateVisuals();
        Debug.Log("Yeni istasyon açıldı!");
    }

    /// <summary>
    /// Görselleri güncelle
    /// </summary>
    private void UpdateVisuals()
    {
        // Kilitli görünüm
        if (lockedVisual != null)
        {
            lockedVisual.SetActive(!isUnlocked);
        }
        
        // Açık istasyon
        if (unlockedStation != null)
        {
            unlockedStation.SetActive(isUnlocked);
        }
        
        // CarWashStation scripti
        if (carWashStation != null)
        {
            carWashStation.enabled = isUnlocked;
        }
        
        // Satın al butonu
        if (unlockButton != null)
        {
            unlockButton.SetActive(!isUnlocked);
        }
    }

    /// <summary>
    /// Fiyat yazısını güncelle
    /// </summary>
    private void UpdatePriceText()
    {
        if (priceText != null)
        {
            priceText.text = "$" + unlockPrice;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isUnlocked ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 2f, 2f));
    }
}
