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
                Instantiate(item, spawnTransform);
                
            }
        }
    }
}
