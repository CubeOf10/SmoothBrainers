using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNewUnit : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject unitToSpawn;
    public GameObject currentUnit;
    GameManager gameManager;
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    void OnTriggerExit(Collider other)
    {
        if(other.tag == unitToSpawn.tag && other != currentUnit)
        {
            GameObject newUnit = Instantiate(unitToSpawn, spawnPoint);
            currentUnit = newUnit;
            
            other.transform.parent = null;

            gameManager.GetComponent<GameManager>().units.Add(newUnit);
        }
    }
}
