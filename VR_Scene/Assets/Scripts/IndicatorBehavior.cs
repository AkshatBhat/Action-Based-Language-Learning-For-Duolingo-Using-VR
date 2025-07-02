using System;
using UnityEngine;

public class IndicatorBehavior : MonoBehaviour
{
    private MeshRenderer myMeshRenderer;
    public Material unactivatedMat;
    public Material activatedMat;

    public string listName = "";
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMeshRenderer = this.transform.gameObject.GetComponent<MeshRenderer>();
        myMeshRenderer.material = unactivatedMat;
    }

    public void turnOn()
    {
        myMeshRenderer.material = activatedMat;
    }

    public void turnOff()
    {
        myMeshRenderer.material = unactivatedMat;
    }
}
