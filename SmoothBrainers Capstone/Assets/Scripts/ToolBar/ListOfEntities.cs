using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListOfEntities : MonoBehaviour
{
    Transform spawnTransform;
    public List<GameObject> itemList = new List<GameObject>();
    void Start()
    {
        spawnTransform = GameObject.Find("SpawnPoint").transform;   
    }

    public void SpawnItem(string itemName)
    {
        foreach(GameObject item in itemList)
        {
            if(item.name == itemName)
            {
                GameObject newItem = Instantiate(item, spawnTransform);
            }
        }
    }
    public void RemoveAllItems(string tagName)
    {
        foreach(Transform item in spawnTransform)
        {
            Debug.Log("Found:" + item.name + " " + item.tag);
            if(item.tag == tagName)
            {
                Debug.Log("Matched");
                Destroy(item.gameObject);
            }
        }
    }
}
