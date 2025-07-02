using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

public class BackpackVolume : MonoBehaviour
{
    public HashSet<GameObject> overlappingItems = new HashSet<GameObject>();
    public List<GameObject> debugOverlappingItems = new List<GameObject>();

    public IndicatorManager IMRef;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("GroceryObject")) return;
        overlappingItems.Add(other.gameObject);
        debugOverlappingItems.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("GroceryObject")) return;
        overlappingItems.Remove(other.gameObject);
        debugOverlappingItems.Remove(other.gameObject);
    }

    private bool IsInside(GameObject obj)
    {
        return overlappingItems.Contains(obj);
    }

    public void CallLockCoroutine(GameObject obj, BackpackItem bp)
    {
        if (IsInside(obj) && bp != null && !bp.IsHeld())
        {
            StartCoroutine(AttemptLock(obj));
        }
        else
        {
            StartCoroutine(AttemptUnlock(obj));
        }
    }

    private IEnumerator AttemptLock(GameObject obj)
    {
        Debug.Log("Attempting to lock object");
        yield return new WaitForSeconds(0.1f);
        if (!obj.CompareTag("GroceryObject")) yield break;

        var rb = obj.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        obj.transform.SetParent(transform);
        IMRef.Test();
        IMRef.ReceiveUpdate(debugOverlappingItems);
        Debug.Log(obj.GetComponent<BackpackItem>().itemName);
        Debug.Log(string.Join(", ", debugOverlappingItems));
    }

    private IEnumerator AttemptUnlock(GameObject obj)
    {
        Debug.Log("Attempting to unlock object");
        yield return new WaitForSeconds(0.1f);
        if (!obj.CompareTag("GroceryObject")) yield break;

        var rb = obj.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        obj.transform.SetParent(null);
        IMRef.ReceiveUpdate(debugOverlappingItems);
    }
}
