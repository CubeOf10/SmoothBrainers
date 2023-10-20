using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class PlayPaths : ButtonEffect
{
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    public override void ButtonPressed()
    {        
        foreach(GameObject unit in gameManager.GetComponent<GameManager>().units)
        {
            if(unit.tag == "Manual")
            {
                if(unit.GetComponent<DragMe>().getMarkerHolder().childCount > 0)
                {
                    unit.transform.GetComponent<NavMeshAgent>().enabled = false;
                    unit.transform.position = unit.GetComponent<DragMe>().getMarkerHolder().GetChild(0).transform.position;
                    unit.transform.GetComponent<NavMeshAgent>().enabled = true;

                    unit.GetComponent<FollowPath>().moveTargetIndex = 0;
                    unit.GetComponent<FollowPath>().following = true;                
                }
            }
        }
    }
    public void PlayAI()
    {
        foreach(GameObject AI_unit in gameManager.units)
        {
            
            if(AI_unit.tag == "Automatic")
            {
                Debug.Log(AI_unit.name);
                float maxDistance = 1000;
                GameObject chosenTarget = null;
                foreach(GameObject target in gameManager.targets)
                {
                    if(Vector3.Distance(AI_unit.transform.position, target.transform.position) < maxDistance)
                    {
                        maxDistance = Vector3.Distance(AI_unit.transform.position, target.transform.position);
                        chosenTarget = target;
                    }
                }
                if(chosenTarget != null)
                    AI_unit.GetComponent<NavMeshAgent>().destination = chosenTarget.transform.position; 

                else
                    AI_unit.GetComponent<NavMeshAgent>().destination = transform.position; 
                    
            }
        }
    }
}
