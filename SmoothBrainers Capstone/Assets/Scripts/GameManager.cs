using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject Player;
    public GameObject target;
    public List<GameObject> targets;
    public List<GameObject> units;
    Transform spawnpoint;
    // Start is called before the first frame update
    void Start()
    {
        spawnpoint = GameObject.Find("SpawnPoint").transform;
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    public void MakeNewTarget()
    {
        GameObject newTarget = Instantiate(target, spawnpoint);
        targets.Add(newTarget);
    }
    public void RemoveAllTargets()
    {
        targets = new List<GameObject>();
        
        foreach(GameObject child in spawnpoint)
            if(child.tag == "Target")
                Destroy(child);
        
    }
}
