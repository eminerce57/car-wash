using UnityEngine;

/// <summary>
/// Araçların yan yola sapmasını sağlayan trigger
/// Bu objeye gelen araçların bir kısmı yön değiştirir
/// </summary>
public class TurnTrigger : MonoBehaviour
{
    [Header("Dönüş Ayarları")]
    [Range(0f, 1f)]
    public float turnChance = 0.5f;  // %50 ihtimalle dönecek
    public Vector3 newDirection = Vector3.forward;  // Yeni hareket yönü (Z yönü = ileri)
    
    [Header("Garaj Noktası")]
    public Transform alignPoint;   // Dönüş hizalama noktası (yolun ortası)
    public Transform garagePoint;  // Garajın önü (opsiyonel)
    public float stopDistance = 2f; // Garaja bu kadar yaklaşınca dur
    
    [Header("Çakışma Önleme")]
    private float lastTurnTime = -999f;
    public float turnCooldown = 0.5f;  // İki araç arası minimum süre
    
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
        
        // Kuyruk dolu mu kontrol et
        if (CarWashStation.Instance != null && CarWashStation.Instance.IsQueueFull())
        {
            Debug.Log("Kuyruk dolu! Araç düz devam ediyor.");
            return; // Dönme, düz devam et
        }
        
        // Çakışma önleme - son dönüşten beri yeterli süre geçti mi?
        if (Time.time - lastTurnTime < turnCooldown)
        {
            Debug.Log("Başka araç döndü, bu araç bekleyecek.");
            return; // Düz devam et
        }
        
        // Rastgele karar ver
        float random = Random.Range(0f, 1f);
        
        if (random <= turnChance)
        {
            lastTurnTime = Time.time; // Cooldown başlat
            
            // Dön! Align point varsa onu da gönder
            car.TurnToDirection(newDirection, alignPoint, garagePoint, stopDistance);
            Debug.Log($"Araç yan yola saptı!");
        }
        else
        {
            Debug.Log($"Araç düz devam etti.");
        }
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
