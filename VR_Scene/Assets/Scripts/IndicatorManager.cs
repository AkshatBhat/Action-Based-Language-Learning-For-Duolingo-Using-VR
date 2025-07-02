using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    public List<GameObject> indicatorRefs;

    public Dictionary<string, bool> itemDict;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemDict = new Dictionary<string, bool>() {
            { "manzana roja", false},
        {"manzana verde", false},
        {"botella de agua", false},
        {"pringles verde", false},
        {"sardinas rojas", false},
        {"hogaza de pan", false},
        {"bol de ensalada", false}
        };
    }
    public void Test()
    {
        Debug.Log("TESTFUCK");
    }

    public void ReceiveUpdate(List<GameObject> list)
    {
        Debug.Log("Receive update called");
        // Turn off all indicators and reset dict
        foreach (var key in new List<string>(itemDict.Keys))
        {
            itemDict[key] = false;

            foreach (GameObject indicator in indicatorRefs)
            {
                var a = indicator.GetComponent<IndicatorBehavior>();
                if (a != null && a.listName == key)
                {
                    a.turnOff();
                }
            }
        }
        Debug.Log("Here1");
        // Now activate only the matching ones
        foreach (GameObject obj in list)
        {
            var backpackItem = obj.GetComponent<BackpackItem>();
            if (backpackItem == null) continue;
            Debug.Log("Here 2");
            string itemName = backpackItem.itemName;

            if (itemDict.ContainsKey(itemName))
            {
                itemDict[itemName] = true;

                foreach (GameObject indicator in indicatorRefs)
                {
                    var a = indicator.GetComponent<IndicatorBehavior>();
                    if (a != null && a.listName == itemName)
                    {
                        a.turnOn();
                    }
                }
            }
        }
    }
}
