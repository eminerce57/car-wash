using UnityEngine;

/// <summary>
/// Araçların yan yola sapmasını sağlayan trigger
/// Bu objeye gelen araçların bir kısmı yön değiştirir
/// </summary>
public class TurnTrigger : MonoBehaviour
{
    [Header("Dönüş Ayarları")]
    [Range(0f, 1f)]
    public float turnChance = 0.2f;  // %20 ihtimalle dönecek (reklam ile artar)
    public Vector3 newDirection = Vector3.forward;  // Yeni hareket yönü (Z yönü = ileri)
    
    [Header("Garaj Noktası")]
    public Transform alignPoint;   // Dönüş hizalama noktası (yolun ortası)
    public Transform garagePoint;  // Garajın önü (opsiyonel)
    public float stopDistance = 2f; // Garaja bu kadar yaklaşınca dur
    
    [Header("Çakışma Önleme")]
    private float lastTurnTime = -999f;
    public float turnCooldown = 2f;  // İki araç arası minimum süre
    
    // Garaja giden araç sayısını takip et
    private static int carsGoingToGarage = 0;
    
    [Header("Görsel")]
    public Color gizmoColor = Color.yellow;

    private void Start()
    {
        // Collider kontrolü - çok küçük nokta
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = 0.3f; // Küçük nokta
            sphere.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
        
        // Rigidbody (trigger için gerekli)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Araç mı kontrol et
        CarMover car = other.GetComponent<CarMover>();
        if (car == null) return;
        
        // Zaten dönmüş mü?
        if (car.hasTurned) return;
        
        // Zaten garaja gidiyor mu?
        if (car.goingToGarage) return;
        
        // Toplam araç sayısı kontrolü (giden + bekleyen + yıkanan)
        int maxAllowed = 4; // Maksimum 4 araç
        if (CarWashStation.Instance != null)
        {
            maxAllowed = CarWashStation.Instance.maxQueueSize;
        }
        
        int totalCars = carsGoingToGarage;
        if (CarWashStation.Instance != null)
        {
            totalCars += CarWashStation.Instance.GetTotalWaitingCars();
        }
        
        if (totalCars >= maxAllowed)
        {
            Debug.Log($"Maksimum araç sayısına ulaşıldı ({totalCars}/{maxAllowed})! Araç düz devam ediyor.");
            car.hasTurned = true;
            return;
        }
        
        // Çakışma önleme - son dönüşten beri yeterli süre geçti mi?
        if (Time.time - lastTurnTime < turnCooldown)
        {
            Debug.Log("Başka araç döndü, bu araç bekleyecek.");
            car.hasTurned = true;
            return;
        }
        
        // Rastgele karar ver
        float random = Random.Range(0f, 1f);
        
        if (random <= turnChance)
        {
            lastTurnTime = Time.time;
            carsGoingToGarage++; // Sayacı artır
            
            // Dön!
            car.TurnToDirection(newDirection, alignPoint, garagePoint, stopDistance);
            Debug.Log($"Araç yan yola saptı! (Toplam: {carsGoingToGarage + (CarWashStation.Instance != null ? CarWashStation.Instance.GetTotalWaitingCars() : 0)})");
        }
        else
        {
            car.hasTurned = true;
            Debug.Log($"Araç düz devam etti.");
        }
    }
    
    /// <summary>
    /// Araç garaja ulaştığında sayacı azalt (CarWashStation'dan çağrılır)
    /// </summary>
    public static void OnCarReachedGarage()
    {
        carsGoingToGarage = Mathf.Max(0, carsGoingToGarage - 1);
    }

    // Editörde göster
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.3f); // Küçük nokta
        
        // Dönüş yönünü göster
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, newDirection * 3f);
        
        // Align point göster (mavi)
        if (alignPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(alignPoint.position, 0.4f);
            Gizmos.DrawLine(transform.position, alignPoint.position);
        }
        
        // Garaj noktasını göster (kırmızı)
        if (garagePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(garagePoint.position, 0.5f);
            if (alignPoint != null)
                Gizmos.DrawLine(alignPoint.position, garagePoint.position);
            else
                Gizmos.DrawLine(transform.position, garagePoint.position);
        }
    }
}
