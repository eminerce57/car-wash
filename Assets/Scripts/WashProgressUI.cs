using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Yıkama ilerleme göstergesi - Dairesel Progress Bar
/// Garajın üstünde görünür
/// </summary>
public class WashProgressUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Image fillImage;          // Dolan daire (Fill)
    public Image backgroundImage;    // Arka plan daire
    public Text timerText;           // Kalan süre yazısı (opsiyonel)
    
    [Header("Renkler")]
    public Color fillColor = new Color(0.2f, 0.8f, 0.2f);      // Yeşil
    public Color backgroundColor = new Color(0.3f, 0.3f, 0.3f); // Gri
    
    [Header("Ayarlar")]
    public bool showTimer = true;    // Süreyi göster
    public bool hideWhenComplete = true;
    
    private Canvas canvas;
    private Camera mainCamera;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        mainCamera = Camera.main;
        
        // World Space canvas ayarla
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
        }
        
        // Renkleri uygula
        if (fillImage != null) fillImage.color = fillColor;
        if (backgroundImage != null) backgroundImage.color = backgroundColor;
        
        // Başlangıçta gizle
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // Kameraya bak (billboard efekti)
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }

    /// <summary>
    /// İlerlemeyi güncelle (0-1 arası)
    /// </summary>
    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        
        if (fillImage != null)
        {
            fillImage.fillAmount = progress;
        }
    }

    /// <summary>
    /// Kalan süreyi göster
    /// </summary>
    public void SetTimer(float remainingTime)
    {
        if (timerText != null && showTimer)
        {
            timerText.text = remainingTime.ToString("F1") + "s";
        }
    }

    /// <summary>
    /// UI'ı göster
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        SetProgress(0);
    }

    /// <summary>
    /// UI'ı gizle
    /// </summary>
    public void Hide()
    {
        if (hideWhenComplete)
        {
            gameObject.SetActive(false);
        }
    }
}
