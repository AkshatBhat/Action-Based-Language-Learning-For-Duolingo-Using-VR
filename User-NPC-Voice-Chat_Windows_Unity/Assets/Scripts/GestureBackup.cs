using UnityEngine;

public class GestureBackup : MonoBehaviour
{
    [Header("References")]
    public SpanishGroceryVoiceControllerTTS voiceController;
    
    [Header("Raycast Settings")]
    public float maxRaycastDistance = 10f;
    public LayerMask productLayer = -1; // All layers by default
    
    [Header("Visual Feedback")]
    public GameObject highlightPrefab; // Optional: highlight selected products
    private GameObject currentHighlight;
    
    void Start()
    {
        // Auto-find voice controller if not assigned
        if (voiceController == null)
        {
            voiceController = FindObjectOfType<SpanishGroceryVoiceControllerTTS>();
        }
        
        if (voiceController == null)
        {
            Debug.LogWarning("GestureBackup: No SpanishGroceryVoiceControllerTTS found in scene!");
        }
    }
    
    void Update()
    {
        // Mouse click for testing (replace with VR controller later)
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
        
        // Optional: Highlight products on hover
        if (Input.GetMouseButton(1)) // Right mouse button for hover
        {
            HandleMouseHover();
        }
        
        // Clear highlight when not hovering
        if (Input.GetMouseButtonUp(1))
        {
            ClearHighlight();
        }
    }
    
    private void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, productLayer))
        {
            ProductInfo product = hit.collider.GetComponent<ProductInfo>();
            
            if (product != null)
            {
                // Call the product's selection method
                product.OnProductSelected();
                
                // Alternative: Direct response from gesture backup
                Debug.Log($"Asistente: Esto es {product.nombre}, cuesta ${product.precio}");
                
                // Play animation if voice controller is available
                if (voiceController != null)
                {
                    // Access the PlayAnimation method (you may need to make it public)
                    Debug.Log("Asistente: ¿Le interesa este producto?");
                }
                
                // Add visual feedback
                CreateHighlight(hit.point);
            }
            else
            {
                Debug.Log("Asistente: Lo siento, no reconozco ese producto. ¿Puede decirme qué busca?");
            }
        }
        else
        {
            Debug.Log("Asistente: Señale un producto específico para obtener información.");
        }
    }
    
    private void HandleMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, productLayer))
        {
            ProductInfo product = hit.collider.GetComponent<ProductInfo>();
            
            if (product != null)
            {
                // Show quick info on hover
                Debug.Log($"Hovering: {product.nombre} - ${product.precio}");
                CreateHighlight(hit.point);
            }
        }
    }
    
    private void CreateHighlight(Vector3 position)
    {
        if (highlightPrefab != null)
        {
            // Remove previous highlight
            ClearHighlight();
            
            // Create new highlight
            currentHighlight = Instantiate(highlightPrefab, position, Quaternion.identity);
            
            // Auto-destroy after 2 seconds
            Destroy(currentHighlight, 2f);
        }
    }
    
    private void ClearHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
            currentHighlight = null;
        }
    }
    
    // VR Controller support (call this from VR input)
    public void HandleVRControllerInput(Transform controllerTransform)
    {
        Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, productLayer))
        {
            ProductInfo product = hit.collider.GetComponent<ProductInfo>();
            
            if (product != null)
            {
                product.OnProductSelected();
                CreateHighlight(hit.point);
            }
        }
    }
}

// Helper script to easily set up products
[System.Serializable]
public class QuickProductSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    public ProductData[] products;
    
    [System.Serializable]
    public class ProductData
    {
        public GameObject productObject;
        public string nombre;
        public float precio;
        public string categoria;
        public bool enOferta;
    }
    
    [ContextMenu("Setup All Products")]
    public void SetupProducts()
    {
        foreach (var productData in products)
        {
            if (productData.productObject != null)
            {
                ProductInfo productInfo = productData.productObject.GetComponent<ProductInfo>();
                if (productInfo == null)
                {
                    productInfo = productData.productObject.AddComponent<ProductInfo>();
                }
                
                productInfo.nombre = productData.nombre;
                productInfo.precio = productData.precio;
                productInfo.categoria = productData.categoria;
                productInfo.enOferta = productData.enOferta;
            }
        }
        
        Debug.Log($"Configured {products.Length} products!");
    }
}