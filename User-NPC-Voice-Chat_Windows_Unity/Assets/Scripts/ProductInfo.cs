using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class ProductInfo : MonoBehaviour
{
    [Header("Product Details")]
    public string nombre = "Manzanas";
    public float precio = 2.50f;
    public string pasillo = "Frutas - Entrada principal";
    public string categoria = "Frutas";
    
    [Header("Product Display")]
    public bool enOferta = false;
    public float precioOriginal = 0f;
    public string descripcion = "Manzanas rojas frescas";
    
    void Start()
    {
        // Add collider if none exists (for raycast detection)
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
        
        // Add XR Interactable for VR controller interaction
        if (GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>() == null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            
            // Set up interaction events
            interactable.selectEntered.AddListener(OnVRSelect);
            interactable.hoverEntered.AddListener(OnVRHover);
        }
        
        // Set original price if on sale
        if (enOferta && precioOriginal <= 0f)
        {
            precioOriginal = precio * 1.2f; // Assume 20% discount
        }
    }
    
    // Called when VR controller selects this product
    public void OnVRSelect(SelectEnterEventArgs args)
    {
        Debug.Log($"VR Controller selected: {nombre}");
        OnProductSelected();
    }
    
    // Called when VR controller hovers over this product
    public void OnVRHover(HoverEnterEventArgs args)
    {
        Debug.Log($"VR Controller hovering: {nombre} - ${precio}");
    }
    
    // Method to get product info as string
    public string GetProductInfo()
    {
        if (enOferta)
        {
            return $"{nombre} - ${precio} (antes ${precioOriginal:F2}) - {pasillo}";
        }
        else
        {
            return $"{nombre} - ${precio} - {pasillo}";
        }
    }
    
    // Method for voice controller to call
    public void OnProductSelected()
    {
        Debug.Log($"Cliente seleccionó: {GetProductInfo()}");
        
        // Find voice controller and trigger response
        SpanishGroceryVoiceControllerTTS voiceController = FindObjectOfType<SpanishGroceryVoiceControllerTTS>();
        if (voiceController != null)
        {
            // You can add custom responses based on product type
            if (enOferta)
            {
                Debug.Log($"Asistente: ¡Excelente elección! {nombre} está en oferta hoy.");
            }
            else
            {
                Debug.Log($"Asistente: {nombre} cuesta ${precio}. ¿Necesita algo más?");
            }
        }
    }
}
