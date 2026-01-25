using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Kamera kontrolü - Sürükleme ve Zoom
/// Yeni Input System ile çalışır
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float dragSpeed = 0.5f;          // Sürükleme hızı
    public bool invertDrag = true;          // Sürükleme yönünü tersle
    
    [Header("Zoom Ayarları")]
    public float zoomSpeed = 2f;            // Zoom hızı
    public float minZoom = 5f;              // Minimum yakınlık (en yakın)
    public float maxZoom = 30f;             // Maximum uzaklık (en uzak)
    public float minHeight = 3f;            // Minimum yükseklik (ground'a girmemesi için)
    
    [Header("Sınırlar (Opsiyonel)")]
    public bool useBounds = true;           // Sınır kullan
    public Vector2 minBounds = new Vector2(-50f, -50f);  // Sol-alt köşe
    public Vector2 maxBounds = new Vector2(50f, 50f);    // Sağ-üst köşe
    
    [Header("Yumuşaklık")]
    public float smoothSpeed = 10f;         // Hareket yumuşaklığı
    
    private Vector3 targetPosition;
    private float targetZoom;
    private Camera cam;
    
    // Input
    private Vector2 lastPointerPosition;
    private bool isDragging = false;
    private Mouse mouse;
    private Touchscreen touchscreen;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        targetPosition = transform.position;
        targetZoom = transform.position.y;
        
        // Input cihazları
        mouse = Mouse.current;
        touchscreen = Touchscreen.current;
    }

    private void Update()
    {
        HandleInput();
        ApplyMovement();
    }

    /// <summary>
    /// Mouse ve Touch kontrolü
    /// </summary>
    private void HandleInput()
    {
        // Mouse kontrolü
        if (mouse != null)
        {
            // Sol tık - sürükleme
            if (mouse.leftButton.wasPressedThisFrame)
            {
                lastPointerPosition = mouse.position.ReadValue();
                isDragging = true;
            }
            
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
            }
            
            if (isDragging && mouse.leftButton.isPressed)
            {
                Vector2 currentPos = mouse.position.ReadValue();
                Vector2 delta = lastPointerPosition - currentPos;
                lastPointerPosition = currentPos;
                
                ApplyDrag(delta);
            }
            
            // Scroll - zoom
            float scroll = mouse.scroll.ReadValue().y;
            if (scroll != 0)
            {
                targetZoom -= scroll * zoomSpeed * 0.1f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }
        
        // Touch kontrolü
        if (touchscreen != null && touchscreen.touches.Count > 0)
        {
            var touches = touchscreen.touches;
            
            // Tek parmak - sürükleme
            if (touches.Count == 1)
            {
                var touch = touches[0];
                
                if (touch.press.wasPressedThisFrame)
                {
                    lastPointerPosition = touch.position.ReadValue();
                    isDragging = true;
                }
                
                if (touch.press.wasReleasedThisFrame)
                {
                    isDragging = false;
                }
                
                if (isDragging && touch.press.isPressed)
                {
                    Vector2 currentPos = touch.position.ReadValue();
                    Vector2 delta = lastPointerPosition - currentPos;
                    lastPointerPosition = currentPos;
                    
                    ApplyDrag(delta);
                }
            }
            
            // İki parmak - zoom (pinch)
            if (touches.Count >= 2)
            {
                isDragging = false;
                
                var touch0 = touches[0];
                var touch1 = touches[1];
                
                Vector2 pos0 = touch0.position.ReadValue();
                Vector2 pos1 = touch1.position.ReadValue();
                Vector2 delta0 = touch0.delta.ReadValue();
                Vector2 delta1 = touch1.delta.ReadValue();
                
                Vector2 prevPos0 = pos0 - delta0;
                Vector2 prevPos1 = pos1 - delta1;
                
                float prevMag = (prevPos0 - prevPos1).magnitude;
                float currentMag = (pos0 - pos1).magnitude;
                float diff = prevMag - currentMag;
                
                targetZoom += diff * zoomSpeed * 0.01f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }
    }

    /// <summary>
    /// Sürükleme uygula
    /// </summary>
    private void ApplyDrag(Vector2 delta)
    {
        float multiplier = invertDrag ? 1f : -1f;
        Vector3 move = new Vector3(delta.x, 0, delta.y) * dragSpeed * multiplier * Time.deltaTime;
        targetPosition += move;
    }

    /// <summary>
    /// Hareketi yumuşak şekilde uygula
    /// </summary>
    private void ApplyMovement()
    {
        // Sınırları uygula
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
        }
        
        // Zoom'u Y pozisyonuna uygula (yukarıdan bakış için)
        targetPosition.y = targetZoom;
        
        // Minimum yükseklik kontrolü - Ground'a girmesin!
        targetPosition.y = Mathf.Max(targetPosition.y, minHeight);
        
        // Yumuşak hareket
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    // Editörde sınırları göster
    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            (minBounds.x + maxBounds.x) / 2f,
            transform.position.y,
            (minBounds.y + maxBounds.y) / 2f
        );
        Vector3 size = new Vector3(
            maxBounds.x - minBounds.x,
            1f,
            maxBounds.y - minBounds.y
        );
        Gizmos.DrawWireCube(center, size);
    }
}
