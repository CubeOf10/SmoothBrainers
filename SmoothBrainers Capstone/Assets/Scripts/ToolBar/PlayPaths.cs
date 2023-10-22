using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

//Used to play the routes that entities have planned
public class PlayPaths : MonoBehaviour
{
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    //Play the dragged units animations
    public void ButtonPressed()
    {        
        foreach(GameObject unit in gameManager.GetComponent<GameManager>().units)
        {
            if(unit.tag == "Manual") //Only dragged units are tagged as manual
            {
                if(unit.GetComponent<DragMe>().getMarkerHolder().childCount > 0) //Only play if the unit has a place to go to
                {
                    if(unit.transform.GetComponent<NavMeshAgent>()!=null) //Only ground units have nav-mesh agents
                    {

                        //Teleport unit to beginning of path
                        unit.transform.GetComponent<NavMeshAgent>().enabled = false; 
                        unit.transform.position = unit.GetComponent<DragMe>().getMarkerHolder().GetChild(0).transform.position;
                        unit.transform.GetComponent<NavMeshAgent>().enabled = true;

                        //Begin following the path
                        unit.GetComponent<FollowPath>().moveTargetIndex = 0;
                        unit.GetComponent<FollowPath>().following = true;                
                    }

                    else //For the flying units
                    {
                        //Reset position and begin follow
                        unit.transform.position = unit.GetComponent<DragMe>().getMarkerHolder().GetChild(0).transform.position;
                        unit.GetComponent<GenericWayPointTraversal>().moveTargetIndex = 0;
                        unit.GetComponent<GenericWayPointTraversal>().following = true; 
                    }
                }
            }
        }
    }

    //Play the fully AI units - Find nearest target to move towards
    public void PlayAI()
    {
        foreach(GameObject AI_unit in gameManager.units)
        {
            if(AI_unit.tag == "Automatic") //Only AI's are tagged as Automatic
            {

                //Finding nearest target to move to
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

                //If there's nothing, remain stationary
                if(chosenTarget != null)
                    AI_unit.GetComponent<NavMeshAgent>().destination = chosenTarget.transform.position; 

                else
                    AI_unit.GetComponent<NavMeshAgent>().destination = transform.position; 
                    
            }
        }
    }
}
