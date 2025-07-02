using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BackpackItem : MonoBehaviour
{
    private XRGrabInteractable interactable;
    private Rigidbody rb;

    public  BackpackVolume backpackRef;

    private bool isHeld = false;

    public string itemName;

    public bool IsHeld()
    {
        return isHeld;
    }

    void Start()
    {
        interactable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        if (interactable == null)
        {
            Debug.Log("isnull");
        }
        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        Debug.Log("Grabbed");
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.SetParent(null); // detach from backpack
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        Debug.Log("Released");

        if (backpackRef != null)
        {
            backpackRef.CallLockCoroutine(this.gameObject, this);
        }
    }

}
