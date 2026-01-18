using UnityEngine;

/// <summary>
/// Arabayı yol boyunca hareket ettirir
/// </summary>
public class CarMover : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 5f;
    public Vector3 moveDirection = Vector3.forward;
    
    [Header("Yok Olma")]
    public float destroyAfterDistance = 50f;
    
    private Vector3 startPosition;
    private float distanceTraveled = 0f;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Arabayı hareket ettir
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        
        // Ne kadar yol aldık?
        distanceTraveled = Vector3.Distance(startPosition, transform.position);
        
        // Belirli mesafe sonra yok et
        if (distanceTraveled >= destroyAfterDistance)
        {
            Destroy(gameObject);
        }
    }
}

