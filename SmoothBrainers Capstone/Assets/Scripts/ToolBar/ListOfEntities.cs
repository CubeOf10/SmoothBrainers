using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListOfEntities : MonoBehaviour
{
    [SerializeField]
    Transform spawnTransform;
    GameManager gameManager;
    public List<GameObject> itemList = new List<GameObject>();
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        spawnTransform = GameObject.Find("SpawnPoint").transform;   
    }

    public void SpawnItem(string itemName)
    {
        foreach(GameObject item in itemList)
        {
            if(item.name == itemName)
            {
                GameObject newItem = Instantiate(item, spawnTransform);
                if(item.tag == "Manual" || item.tag == "Automatic")
                    gameManager.units.Add(newItem);
            }
        }
    }
    public void RemoveAllItems(string tagName)
    {
        foreach(Transform item in spawnTransform)
        {
            if(item.tag == tagName)
            {
                if(gameManager.units.Contains(item.gameObject))
                    gameManager.units.Remove(item.gameObject);

                if(gameManager.targets.Contains(item.gameObject))
                    gameManager.targets.Remove(item.gameObject);

                Destroy(item.gameObject);
            }
        }
    }
}
