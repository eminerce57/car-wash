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
