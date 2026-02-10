using UnityEngine;

/// <summary>
/// Belirli aralıklarla farklı renklerde araba spawn eder
/// </summary>
public class CarSpawner : MonoBehaviour
{
    [Header("Spawn Ayarları")]
    public GameObject[] carPrefabs;  // Araba prefab'ları (sedan, suv, vb.)
    public float minSpawnInterval = 0.8f; // Minimum spawn aralığı
    public float maxSpawnInterval = 2f;   // Maximum spawn aralığı
    public Transform spawnPoint;     // Spawn noktası (boş bırakırsan spawner pozisyonu kullanılır)
    
    [Header("Hareket Ayarları")]
    public float minSpeed = 0.3f;   // Yavaş araçlar
    public float maxSpeed = 0.8f;   // Hızlı araçlar
    public Vector3 moveDirection = Vector3.right; // X yönünde hareket
    public float destroyDistance = 100f; // Yol bitene kadar
    
    [Header("Boyut Ayarları")]
    public float carScale = 0.5f; // Araç boyutu (1 = normal, 0.5 = yarı boyut)
    
    [Header("Renk Ayarları")]
    public Color[] carColors = new Color[]
    {
        new Color(0.1f, 0.1f, 0.1f),     // Siyah
        new Color(0.95f, 0.95f, 0.95f),  // Beyaz
        new Color(0.6f, 0.6f, 0.65f),    // Gümüş Gri
        new Color(0.25f, 0.25f, 0.28f),  // Koyu Gri (Antrasit)
        new Color(0.7f, 0.05f, 0.1f),    // Kırmızı
        new Color(0.1f, 0.2f, 0.4f),     // Lacivert
        new Color(0.4f, 0.1f, 0.1f),     // Bordo
        new Color(0.85f, 0.85f, 0.8f),   // Bej / Krem
    };
    
    [Header("Altın Araç Ayarları")]
    [Range(0f, 1f)]
    public float goldenCarChance = 0.05f;    // %5 şans
    public float goldenMinBonus = 100f;       // Minimum bonus
    public float goldenMaxBonus = 300f;       // Maximum bonus
    
    private float timer = 0f;
    private float currentSpawnInterval;

    private void Start()
    {
        // Spawn noktası belirtilmemişse kendi pozisyonunu kullan
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        // İlk spawn aralığını belirle
        currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        
        // Hemen bir araba spawn et
        SpawnCar();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= currentSpawnInterval)
        {
            SpawnCar();
            timer = 0f;
            // Yeni rastgele spawn aralığı (doğal trafik için)
            currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    private void SpawnCar()
    {
        // Prefab kontrolü
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogWarning("CarSpawner: Araba prefab'ı eklenmemiş!");
            return;
        }
        
        // Rastgele bir araba seç
        int carIndex = Random.Range(0, carPrefabs.Length);
        GameObject carPrefab = carPrefabs[carIndex];
        
        if (carPrefab == null) return;
        
        // Hareket yönüne göre rotasyon hesapla
        Quaternion rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        
        // Arabayı spawn et (hareket yönüne bakacak şekilde)
        GameObject newCar = Instantiate(carPrefab, spawnPoint.position, rotation);
        
        // Boyutu ayarla
        newCar.transform.localScale = Vector3.one * carScale;
        
        // CarMover ekle ve ayarla
        CarMover mover = newCar.AddComponent<CarMover>();
        float carSpeed = Random.Range(minSpeed, maxSpeed);
        mover.speed = carSpeed;
        mover.originalSpeed = carSpeed;
        mover.moveDirection = moveDirection;
        mover.destroyAfterDistance = destroyDistance;
        
        // Altın araç mı kontrol et
        bool isGolden = Random.Range(0f, 1f) < goldenCarChance;
        
        if (isGolden)
        {
            // Altın araç!
            float bonusMoney = Random.Range(goldenMinBonus, goldenMaxBonus);
            mover.SetAsGolden(bonusMoney);
            Debug.Log($"✨ ALTIN ARAÇ spawn edildi! Bonus: ${bonusMoney:F0}");
        }
        else
        {
            // Normal araç - rastgele renk uygula
            ApplyRandomColor(newCar);
            Debug.Log($"Araba spawn edildi: {carPrefab.name}");
        }
    }

    private void ApplyRandomColor(GameObject car)
    {
        if (carColors == null || carColors.Length == 0) return;
        
        // Rastgele renk seç
        Color randomColor = carColors[Random.Range(0, carColors.Length)];
        
        // Sabit renkler
        Color tireColor = new Color(0.15f, 0.15f, 0.15f);  // Koyu gri (lastik)
        Color glassColor = new Color(0.1f, 0.1f, 0.12f);   // Koyu cam rengi
        Color metalColor = new Color(0.7f, 0.7f, 0.7f);    // Metal/krom
        
        // Arabanın tüm renderer'larını bul
        Renderer[] renderers = car.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            string objName = renderer.gameObject.name.ToLower();
            
            foreach (Material mat in renderer.materials)
            {
                string matName = mat.name.ToLower();
                
                // Hangi rengi uygulayacağımıza karar ver
                Color colorToApply;
                
                // Lastik kontrolü (wheel içeren her şey)
                if (objName.Contains("wheel") || matName.Contains("wheel") ||
                    objName.Contains("tire") || matName.Contains("tire"))
                {
                    colorToApply = tireColor;
                }
                // Body = kaporta rengi
                else if (objName.Contains("body") || matName.Contains("body"))
                {
                    colorToApply = randomColor;
                }
                // Diğer her şey koyu gri (detaylar, aksesuarlar)
                else
                {
                    colorToApply = metalColor;
                }
                
                // Rengi uygula
                if (mat.HasProperty("_Color"))
                {
                    mat.color = colorToApply;
                }
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", colorToApply);
                }
            }
        }
    }

    // Editörde spawn noktasını göster
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Gizmos.DrawWireSphere(pos, 1f);
        
        // Hareket yönünü göster
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(pos, moveDirection * 5f);
    }
}

