using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BasketBehavior : MonoBehaviour
{
    public GameObject quickbeltRef;
    private XRGrabInteractable interactable;
     private Rigidbody rb;
    public bool isBasket;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.transform.position = quickbeltRef.transform.position;
        this.transform.SetParent(quickbeltRef.transform);
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        if (isBasket)
        {
            this.transform.eulerAngles = new Vector3(0, -90, 0);
        }
        else
        {
            this.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        interactable = GetComponent<XRGrabInteractable>();
        if (interactable == null)
        {
            Debug.Log("isnull");
        }
        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnRelease);
    }


    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("Grabbed");
        transform.SetParent(null); // detach from backpack
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        this.transform.position = quickbeltRef.transform.position;
        this.transform.SetParent(quickbeltRef.transform);
        rb.isKinematic = true;
        rb.useGravity = false;
        if (isBasket)
        {
            this.transform.eulerAngles = new Vector3(0, -90, 0);
        }
        else
        {
            this.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        
    }
}
