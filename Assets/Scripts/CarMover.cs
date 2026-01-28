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
    public float detectionDistance = 3f;
    public float safeDistance = 1.5f;
    
    [Header("Garaj/Dönüş")]
    public bool hasTurned = false;
    public bool goingToGarage = false;
    public Transform garageTarget;
    public float garageStopDistance = 2f;
    
    private Vector3 startPosition;
    private float distanceTraveled = 0f;
    private bool isBlocked = false;
    private bool isAtGarage = false;
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
        if (garageTarget == null) return;
        
        // Önünde araç var mı kontrol et
        CheckForCarAhead();
        
        float distance = Vector3.Distance(transform.position, garageTarget.position);
        
        if (distance <= garageStopDistance)
        {
            isAtGarage = true;
            return;
        }
        
        // Önü kapalıysa bekle
        if (isBlocked) return;
        
        // Garaja doğru git
        Vector3 direction = (garageTarget.position - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 500f * Time.deltaTime);
        }
        
        float garageSpeed = Mathf.Max(originalSpeed, 2f);
        transform.position += transform.forward * garageSpeed * Time.deltaTime;
    }
    
    public void TurnToDirection(Vector3 newDirection, Transform garage, float stopDistance)
    {
        hasTurned = true;
        
        if (garage != null)
        {
            goingToGarage = true;
            garageTarget = garage;
            garageStopDistance = stopDistance;
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
