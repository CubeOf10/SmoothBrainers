using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controls a list of targets for AI's to focus on
// Also holds a list of all entities 
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

    //New target for AI's to focus
    public void MakeNewTarget()
    {
        GameObject newTarget = Instantiate(target, spawnpoint);
        targets.Add(newTarget);
    }

    //Clears all targets
    public void RemoveAllTargets()
    {
        targets = new List<GameObject>();
        
        foreach(Transform child in spawnpoint.transform)
            if(child.gameObject.tag == "Target")
                Destroy(child.gameObject);
        
    }
}
