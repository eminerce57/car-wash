using UnityEngine;
using TMPro;

/// <summary>
/// Para Yöneticisi - Singleton
/// Oyundaki tüm para işlemlerini yönetir
/// </summary>
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }
    
    [Header("Para")]
    public float currentMoney = 0f;
    
    [Header("UI")]
    public TextMeshProUGUI moneyText;  // TextMeshPro para yazısı
    
    [Header("Animasyon")]
    public float countSpeed = 100f;  // Para sayma hızı
    
    private float displayedMoney = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        displayedMoney = currentMoney;
        UpdateUI();
    }

    private void Update()
    {
        // Para animasyonu (yavaş yavaş artar)
        if (displayedMoney != currentMoney)
        {
            displayedMoney = Mathf.MoveTowards(displayedMoney, currentMoney, countSpeed * Time.deltaTime);
            UpdateUI();
        }
    }

    /// <summary>
    /// Para ekle
    /// </summary>
    public void AddMoney(float amount)
    {
        currentMoney += amount;
        Debug.Log($"+${amount} | Toplam: ${currentMoney}");
    }

    /// <summary>
    /// Para harca
    /// </summary>
    public bool SpendMoney(float amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            Debug.Log($"-${amount} | Kalan: ${currentMoney}");
            return true;
        }
        
        Debug.Log($"Yetersiz bakiye! Gereken: ${amount}, Mevcut: ${currentMoney}");
        return false;
    }

    /// <summary>
    /// Yeterli para var mı?
    /// </summary>
    public bool CanAfford(float amount)
    {
        return currentMoney >= amount;
    }

    /// <summary>
    /// UI güncelle
    /// </summary>
    private void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + Mathf.FloorToInt(displayedMoney).ToString();
        }
    }

    /// <summary>
    /// Parayı formatla (1.5K, 2.3M gibi)
    /// </summary>
    public static string FormatMoney(float amount)
    {
        if (amount < 1000)
            return "$" + Mathf.FloorToInt(amount);
        if (amount < 1000000)
            return "$" + (amount / 1000f).ToString("F1") + "K";
        if (amount < 1000000000)
            return "$" + (amount / 1000000f).ToString("F2") + "M";
        
        return "$" + (amount / 1000000000f).ToString("F2") + "B";
    }
}
