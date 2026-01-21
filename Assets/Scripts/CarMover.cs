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
    public Vector3 moveDirection = Vector3.right; // X yönünde hareket
    
    [Header("Yok Olma")]
    public float destroyAfterDistance = 50f;
    
    [Header("Çarpışma Algılama")]
    public float detectionDistance = 1.5f; // Önündeki aracı algılama mesafesi
    public float safeDistance = 1f;        // Güvenli takip mesafesi
    public LayerMask carLayer;             // Araç layer'ı (opsiyonel)
    
    private Vector3 startPosition;
    private float distanceTraveled = 0f;
    private bool isBlocked = false;
    private BoxCollider myCollider;

    private void Start()
    {
        startPosition = transform.position;
        originalSpeed = speed;
        
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

