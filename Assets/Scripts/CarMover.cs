using UnityEngine;

/// <summary>
/// Arabayı yol boyunca hareket ettirir
/// Önündeki aracı algılar ve çarpışmayı önler
/// </summary>
public class CarMover : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 5f;
    public float originalSpeed;
    public Vector3 moveDirection = Vector3.right;
    
    [Header("Yok Olma")]
    public float destroyAfterDistance = 50f;
    
    [Header("Altın Araç")]
    public bool isGolden = false;           // Bu altın araç mı?
    public float goldenBonusMoney = 100f;   // Tıklayınca kazanılacak para
    private bool goldCollected = false;     // Zaten tıklandı mı?
    private GameObject coinIcon;            // Dönen coin simgesi
    public float coinRotationSpeed = 180f;  // Coin dönüş hızı (derece/saniye)
    public float coinBobSpeed = 2f;         // Coin yukarı-aşağı hızı
    public float coinBobAmount = 0.2f;      // Coin yukarı-aşağı mesafesi
    private float coinStartY;               // Coin başlangıç Y pozisyonu
    
    [Header("Çarpışma Algılama")]
    public float detectionDistance = 2f;    // Daha yakın algılama
    public float safeDistance = 1f;         // Daha yakın mesafe (kuyrukta sıkı dur)
    
    [Header("Garaj/Dönüş")]
    public bool hasTurned = false;
    public bool goingToGarage = false;
    public Transform alignTarget;     // İlk hedef: Hizalama noktası
    public Transform garageTarget;    // Son hedef: Garaj
    public float garageStopDistance = 2f;
    public float alignDistance = 0.5f; // Hizalama noktasına bu kadar yaklaşınca geç
    
    private Vector3 startPosition;
    private float distanceTraveled = 0f;
    private bool isBlocked = false;
    private bool isAtGarage = false;
    private bool alignReached = false; // Hizalama noktasına ulaştı mı?
    private BoxCollider myCollider;

    private void Start()
    {
        startPosition = transform.position;
        
        // Collider ekle
        myCollider = GetComponent<BoxCollider>();
        if (myCollider == null)
        {
            myCollider = gameObject.AddComponent<BoxCollider>();
        }
        myCollider.size = new Vector3(1.5f, 1f, 3f);  // Araç boyutu
        myCollider.center = new Vector3(0f, 0.5f, 0f);
        myCollider.isTrigger = true;
        
        // Rigidbody ekle
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        
        gameObject.tag = "Car";
    }

    private void Update()
    {
        if (isAtGarage)
        {
            return; // Yıkamada bekliyor
        }
        
        // Altın araç tıklama kontrolü ve coin animasyonu
        if (isGolden && !goldCollected)
        {
            CheckGoldenCarClick();
            AnimateCoinIcon();
        }
        
        // Garaja mı gidiyor?
        if (goingToGarage && garageTarget != null)
        {
            MoveToGarage();
            return;
        }
        
        // Önünde araç var mı kontrol et
        CheckForCarAhead();
        
        // Eğer önü açıksa hareket et
        if (!isBlocked)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }
        
        // Belirli mesafe sonra yok et
        distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= destroyAfterDistance)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Altın araca tıklama kontrolü
    /// </summary>
    private void CheckGoldenCarClick()
    {
        // Mouse tıklama
        bool clicked = false;
        Vector2 clickPosition = Vector2.zero;
        
        if (UnityEngine.InputSystem.Mouse.current != null && 
            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            clicked = true;
            clickPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }
        // Touch tıklama
        else if (UnityEngine.InputSystem.Touchscreen.current != null &&
                 UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            clicked = true;
            clickPosition = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();
        }
        
        if (clicked)
        {
            Ray ray = Camera.main.ScreenPointToRay(clickPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    CollectGoldenBonus();
                }
            }
        }
    }
    
    /// <summary>
    /// Altın bonus topla
    /// </summary>
    private void CollectGoldenBonus()
    {
        if (goldCollected) return;
        goldCollected = true;
        
        // Para ekle
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(goldenBonusMoney);
        }
        
        // Ses çal
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoinSound();
        }
        
        Debug.Log($"ALTIN ARAÇ YAKALANDI! +${goldenBonusMoney}");
        
        // Efekt: Aracı normal renge çevir veya parıltı efekti
        // Şimdilik aracı yok et
        Destroy(gameObject, 0.3f);
    }
    
    /// <summary>
    /// Altın araç olarak ayarla
    /// </summary>
    public void SetAsGolden(float bonusMoney)
    {
        isGolden = true;
        goldenBonusMoney = bonusMoney;
        
        // Altın renk uygula
        ApplyGoldenColor();
    }
    
    /// <summary>
    /// Araca altın renk uygula
    /// </summary>
    private void ApplyGoldenColor()
    {
        Color goldColor = new Color(1f, 0.84f, 0f); // Altın sarısı
        
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    mat.color = goldColor;
                }
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", goldColor);
                }
                // Parlak yap
                if (mat.HasProperty("_Metallic"))
                {
                    mat.SetFloat("_Metallic", 1f);
                }
                if (mat.HasProperty("_Smoothness"))
                {
                    mat.SetFloat("_Smoothness", 0.9f);
                }
            }
        }
        
        // Coin simgesi oluştur
        CreateCoinIcon();
    }
    
    /// <summary>
    /// Dönen coin simgesi oluştur
    /// </summary>
    private void CreateCoinIcon()
    {
        // Coin objesi oluştur (Cylinder kullanacağız - yassı coin şeklinde)
        coinIcon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coinIcon.name = "CoinIcon";
        coinIcon.transform.SetParent(transform);
        
        // Pozisyon (aracın üstünde)
        coinIcon.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        coinIcon.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f); // Yassı coin
        coinStartY = coinIcon.transform.localPosition.y;
        
        // Collider'ı kaldır (tıklama aracın kendisine olsun)
        Collider coinCollider = coinIcon.GetComponent<Collider>();
        if (coinCollider != null)
        {
            Destroy(coinCollider);
        }
        
        // URP için yeni materyal oluştur
        Renderer coinRenderer = coinIcon.GetComponent<Renderer>();
        if (coinRenderer != null)
        {
            // Yeni unlit materyal oluştur (shader'a bağımlı olmayan)
            Material coinMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Shader bulunamazsa fallback
            if (coinMat.shader == null || coinMat.shader.name == "Hidden/InternalErrorShader")
            {
                coinMat = new Material(Shader.Find("Standard"));
            }
            
            Color goldColor = new Color(1f, 0.84f, 0f); // Altın sarısı
            
            // Tüm olası renk property'lerini ayarla
            coinMat.SetColor("_BaseColor", goldColor);
            coinMat.SetColor("_Color", goldColor);
            coinMat.color = goldColor;
            
            // Metalik ve parlak yap
            if (coinMat.HasProperty("_Metallic"))
                coinMat.SetFloat("_Metallic", 1f);
            if (coinMat.HasProperty("_Smoothness"))
                coinMat.SetFloat("_Smoothness", 0.8f);
            
            coinRenderer.material = coinMat;
        }
    }
    
    /// <summary>
    /// Coin simgesini animasyonla (döndür + yukarı-aşağı)
    /// </summary>
    private void AnimateCoinIcon()
    {
        if (coinIcon == null) return;
        
        // Y ekseninde döndür (360 derece)
        coinIcon.transform.Rotate(Vector3.up, coinRotationSpeed * Time.deltaTime, Space.World);
        
        // Yukarı-aşağı hareket (bob effect)
        float newY = coinStartY + Mathf.Sin(Time.time * coinBobSpeed) * coinBobAmount;
        Vector3 localPos = coinIcon.transform.localPosition;
        localPos.y = newY;
        coinIcon.transform.localPosition = localPos;
    }
    
    private void MoveToGarage()
    {
        // Önünde araç var mı kontrol et
        CheckForCarAhead();
        
        // Önü kapalıysa bekle
        if (isBlocked) return;
        
        Transform currentTarget;
        float targetDistance;
        
        // Önce align noktasına git, sonra garaja
        if (!alignReached && alignTarget != null)
        {
            currentTarget = alignTarget;
            targetDistance = alignDistance;
        }
        else
        {
            currentTarget = garageTarget;
            targetDistance = garageStopDistance;
        }
        
        if (currentTarget == null) return;
        
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        
        // Hedefe ulaştık mı?
        if (distance <= targetDistance)
        {
            if (!alignReached && alignTarget != null)
            {
                alignReached = true; // Align noktasına ulaştık, garaja geç
                return;
            }
            else
            {
                isAtGarage = true; // Garaja ulaştık
                return;
            }
        }
        
        // Hedefe doğru git
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 500f * Time.deltaTime);
        }
        
        float garageSpeed = Mathf.Max(originalSpeed, 2f);
        transform.position += transform.forward * garageSpeed * Time.deltaTime;
    }
    
    public void TurnToDirection(Vector3 newDirection, Transform align, Transform garage, float stopDistance)
    {
        hasTurned = true;
        
        if (garage != null)
        {
            goingToGarage = true;
            alignTarget = align;      // Önce buraya git (opsiyonel)
            garageTarget = garage;    // Sonra buraya git
            garageStopDistance = stopDistance;
            alignReached = (align == null); // Align yoksa direkt garaja git
        }
    }
    
    /// <summary>
    /// Aracı durdur (yıkama için)
    /// </summary>
    public void StopCar()
    {
        isAtGarage = true;
    }

    private void CheckForCarAhead()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        Vector3 checkDirection = goingToGarage ? transform.forward : moveDirection;
        
        // Tüm collider'ları bul (sadece ilkini değil)
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, checkDirection, detectionDistance);
        
        foreach (RaycastHit hit in hits)
        {
            // Kendimizi atla
            if (hit.collider.gameObject == gameObject) continue;
            
            CarMover otherCar = hit.collider.GetComponent<CarMover>();
            if (otherCar != null)
            {
                // ÖNEMLİ: Ana yoldaki araçlar, garaja giden araçları yoksaysın
                // Bu sayede kuyruk yola taşsa bile ana yol trafiği durmaz
                if (!goingToGarage && otherCar.goingToGarage)
                {
                    continue; // Bu aracı yoksay, diğerlerine bak
                }
                
                float distance = hit.distance;
                
                if (distance < safeDistance)
                {
                    isBlocked = true;
                    return;
                }
                else
                {
                    // Önündeki araçtan yavaş git
                    speed = Mathf.Min(speed, otherCar.speed * 0.8f);
                }
                return;
            }
        }
        
        isBlocked = false;
        speed = originalSpeed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isBlocked ? Color.red : Color.green;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 checkDirection = goingToGarage ? transform.forward : moveDirection;
        Gizmos.DrawRay(rayOrigin, checkDirection * detectionDistance);
    }
}
