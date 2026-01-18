using UnityEngine;

/// <summary>
/// Belirli aralıklarla farklı renklerde araba spawn eder
/// </summary>
public class CarSpawner : MonoBehaviour
{
    [Header("Spawn Ayarları")]
    public GameObject[] carPrefabs;  // Araba prefab'ları (sedan, suv, vb.)
    public float spawnInterval = 2f; // Kaç saniyede bir spawn
    public Transform spawnPoint;     // Spawn noktası (boş bırakırsan spawner pozisyonu kullanılır)
    
    [Header("Hareket Ayarları")]
    public float minSpeed = 3f;
    public float maxSpeed = 7f;
    public Vector3 moveDirection = Vector3.forward;
    public float destroyDistance = 50f;
    
    [Header("Renk Ayarları")]
    public Color[] carColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.white,
        Color.black,
        new Color(1f, 0.5f, 0f),    // Turuncu
        new Color(0.5f, 0f, 0.5f),  // Mor
        new Color(0f, 1f, 1f),      // Cyan
        new Color(1f, 0.75f, 0.8f)  // Pembe
    };
    
    private float timer = 0f;

    private void Start()
    {
        // Spawn noktası belirtilmemişse kendi pozisyonunu kullan
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        // Hemen bir araba spawn et
        SpawnCar();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= spawnInterval)
        {
            SpawnCar();
            timer = 0f;
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
        
        // Arabayı spawn et
        GameObject newCar = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // CarMover ekle ve ayarla
        CarMover mover = newCar.AddComponent<CarMover>();
        mover.speed = Random.Range(minSpeed, maxSpeed);
        mover.moveDirection = moveDirection;
        mover.destroyAfterDistance = destroyDistance;
        
        // Rastgele renk uygula
        ApplyRandomColor(newCar);
        
        Debug.Log($"Araba spawn edildi: {carPrefab.name}");
    }

    private void ApplyRandomColor(GameObject car)
    {
        if (carColors == null || carColors.Length == 0) return;
        
        // Rastgele renk seç
        Color randomColor = carColors[Random.Range(0, carColors.Length)];
        
        // Arabanın tüm renderer'larını bul ve rengi uygula
        Renderer[] renderers = car.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            // Her material için renk ayarla
            foreach (Material mat in renderer.materials)
            {
                // Ana rengi değiştir
                if (mat.HasProperty("_Color"))
                {
                    mat.color = randomColor;
                }
                // URP için BaseColor
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", randomColor);
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

