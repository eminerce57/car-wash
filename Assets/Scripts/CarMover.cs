using UnityEngine;

/// <summary>
/// Arabayı yol boyunca hareket ettirir
/// Önündeki aracı algılar ve çarpışmayı önler
/// Yan yola sapabilir ve garaja gidebilir
/// </summary>
public class CarMover : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 5f;
    public float originalSpeed;
    public Vector3 moveDirection = Vector3.right; // X yönünde hareket
    
    [Header("Yok Olma")]
    public float destroyAfterDistance = 50f;
    
    [Header("Çarpışma Algılama")]
    public float detectionDistance = 1.5f; // Önündeki aracı algılama mesafesi
    public float safeDistance = 1f;        // Güvenli takip mesafesi
    public LayerMask carLayer;             // Araç layer'ı (opsiyonel)
    
    [Header("Garaj/Dönüş")]
    public bool hasTurned = false;         // Dönüş yaptı mı?
    public bool goingToGarage = false;     // Garaja mı gidiyor?
    public Transform garageTarget;          // Garaj hedefi
    public float garageStopDistance = 2f;   // Garaja bu kadar yaklaşınca dur
    
    [Header("Dönüş Ayarları")]
    public float turnSpeed = 8f;            // Dönüş hızı (ne kadar yüksek o kadar hızlı döner)
    public bool isTurning = false;          // Şu an dönüyor mu?
    private Vector3 targetDirection;        // Hedef yön
    
    private Vector3 startPosition;
    private float distanceTraveled = 0f;
    private bool isBlocked = false;
    private bool isAtGarage = false;        // Garaja vardı mı?
    private BoxCollider myCollider;

    private void Start()
    {
        startPosition = transform.position;
        
        // Collider ekle (yoksa)
        myCollider = GetComponent<BoxCollider>();
        if (myCollider == null)
        {
            myCollider = gameObject.AddComponent<BoxCollider>();
            myCollider.size = new Vector3(2f, 1f, 4f); // Araç boyutu
            myCollider.center = new Vector3(0f, 0.5f, 0f);
        }
        myCollider.isTrigger = true;
        
        // Rigidbody ekle (yoksa)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // Tag ayarla
        gameObject.tag = "Car";
    }

    private void Update()
    {
        // Garaja vardıysa bekle (şimdilik yok ol)
        if (isAtGarage)
        {
            // Şimdilik 2 saniye bekleyip yok ol
            Destroy(gameObject, 2f);
            return;
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
        
        // Ne kadar yol aldık?
        distanceTraveled = Vector3.Distance(startPosition, transform.position);
        
        // Belirli mesafe sonra yok et
        if (distanceTraveled >= destroyAfterDistance)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Yumuşak dönüş - yavaş yavaş yön değiştirir
    /// </summary>
    private void SmoothTurn()
    {
        // Mevcut yönü hedef yöne doğru yavaşça değiştir
        moveDirection = Vector3.Lerp(moveDirection, targetDirection, Time.deltaTime * turnSpeed);
        
        // Aracın rotasyonunu da yavaşça değiştir
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
        
        // Yeterince yaklaştıysa dönüşü bitir
        if (Vector3.Angle(moveDirection, targetDirection) < 1f)
        {
            moveDirection = targetDirection;
            isTurning = false;
        }
    }
    
    /// <summary>
    /// Garaja doğru hareket et - HIZ DEĞİŞMEZ!
    /// </summary>
    private void MoveToGarage()
    {
        if (garageTarget == null) return;
        
        // Garaja olan mesafe
        float distance = Vector3.Distance(transform.position, garageTarget.position);
        
        if (distance <= garageStopDistance)
        {
            // Garaja vardık!
            isAtGarage = true;
            Debug.Log("Araç garaja vardı!");
            return;
        }
        
        // Garaja doğru yön hesapla
        Vector3 direction = (garageTarget.position - transform.position).normalized;
        direction.y = 0;
        
        // Aracı hızlıca garaja doğru döndür
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 500f * Time.deltaTime);
        }
        
        // İLERİ GİT - transform.forward yönünde
        // Garaja giderken normal yoldaki hızla aynı git
        float garageSpeed = Mathf.Max(originalSpeed, 2f); // Minimum 2 hız
        transform.position += transform.forward * garageSpeed * Time.deltaTime;
    }
    
    /// <summary>
    /// Garaja git - hız aynı kalır!
    /// </summary>
    public void TurnToDirection(Vector3 newDirection, Transform garage, float stopDistance)
    {
        hasTurned = true;
        
        // Garaj hedefi varsa direkt garaja git
        if (garage != null)
        {
            goingToGarage = true;
            garageTarget = garage;
            garageStopDistance = stopDistance;
        }
    }

    private void CheckForCarAhead()
    {
        // Raycast ile önündeki aracı algıla
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        
        if (Physics.Raycast(rayOrigin, moveDirection, out hit, detectionDistance))
        {
            // Önünde bir araç var mı?
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                CarMover otherCar = hit.collider.GetComponent<CarMover>();
                if (otherCar != null)
                {
                    // Önündeki araçla mesafeyi kontrol et
                    float distance = hit.distance;
                    
                    if (distance < safeDistance)
                    {
                        // Çok yakın - dur
                        isBlocked = true;
                    }
                    else
                    {
                        // Yavaşla ama dur değil
                        isBlocked = false;
                        speed = Mathf.Min(speed, otherCar.speed * 0.9f);
                    }
                    return;
                }
            }
        }
        
        // Önü açık - normal hızda git
        isBlocked = false;
        speed = originalSpeed;
    }

    // Debug için önündeki raycast'i göster
    private void OnDrawGizmos()
    {
        Gizmos.color = isBlocked ? Color.red : Color.green;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawRay(rayOrigin, moveDirection * detectionDistance);
    }
}

