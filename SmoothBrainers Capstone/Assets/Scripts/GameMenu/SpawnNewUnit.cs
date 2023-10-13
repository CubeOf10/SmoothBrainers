using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNewUnit : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject unitToSpawn;
    public GameObject currentUnit;
    GameObject Terrain;
    GameManager gameManager;
    void Start()
    {
        Terrain = GameObject.Find("Terrain");
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    void OnTriggerExit(Collider other)
    {
        Debug.Log(other);
        if(other.tag == unitToSpawn.tag && other.gameObject == currentUnit)
        {
            GameObject newUnit = Instantiate(unitToSpawn, spawnPoint);
            currentUnit = newUnit;
            if(other.tag == "Environment")
            {
                other.transform.parent = Terrain.transform;
                return;
            }
            other.transform.parent = null;

            gameManager.GetComponent<GameManager>().units.Add(newUnit);
        }
    }
}
