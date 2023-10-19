using System.Collections;
using System.Collections.Generic;
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
            unit.GetComponent<FollowPath>().moveTargetIndex = 0;
            if(unit.GetComponent<DragMe>().getMarkerHolder().childCount > 0)
            {
                //
                unit.transform.position = unit.GetComponent<DragMe>().getMarkerHolder().GetChild(0).transform.position;
                
                if(unit.GetComponent<FollowPath>().flyingType)
                unit.GetComponent<NavMeshAgent>().baseOffset = 
                    unit.GetComponent<DragMe>().getMarkerHolder().GetChild(0).GetComponent<WaypointHeight>().height;
                
                unit.GetComponent<FollowPath>().following = true;
            }
        }
    }
}
