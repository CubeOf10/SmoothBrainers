using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores items for spawning
public class ListOfEntities : MonoBehaviour
{
    [SerializeField]
    Transform spawnTransform;
    GameManager gameManager;
    
    //Store entities here - trees, units, etc.
    public List<GameObject> itemList = new List<GameObject>();
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        spawnTransform = GameObject.Find("SpawnPoint").transform;   
    }

    //Run through items in list - prefab name must match string submitted
    public void SpawnItem(string itemName)
    {
        foreach(GameObject item in itemList)
        {
            if(item.name == itemName)
            {
                GameObject newItem = Instantiate(item, spawnTransform);

                //Place units within the gameManager's list to play animation
                if(item.tag == "Manual" || item.tag == "Automatic") 
                    gameManager.units.Add(newItem);
            }
        }
    }
    //Will remove items with matching Tags from a transform
    public void RemoveAllItems(string tagName)
    {
        foreach(Transform item in spawnTransform)
        {
            if(item.tag == tagName)
            {
                //Remove entities from gameManager list before destroying to avoid 'Not Referenced' errors
                if(gameManager.units.Contains(item.gameObject))
                    gameManager.units.Remove(item.gameObject);

                if(gameManager.targets.Contains(item.gameObject))
                    gameManager.targets.Remove(item.gameObject);

                Destroy(item.gameObject);
            }
        }
    }
}
