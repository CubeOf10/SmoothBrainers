using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Will make the manual units follow the path set out by the DragMe script
public class FollowPath : MonoBehaviour
{
    Transform moveTarget;
    public int moveTargetIndex;
    DragMe dragScript;
    public bool following;
    NavMeshAgent navMesh;

    void Start()
    {
        dragScript = GetComponent<DragMe>();
        navMesh = GetComponent<NavMeshAgent>();

    }
    void Update()
    {
        if(following) 
        {
            moveTarget = dragScript.getMarkerHolder().GetChild(moveTargetIndex); //Object to move to

            //If not there yet         
            if(Vector2.Distance(new Vector2(transform.position.x, transform.position.z), 
                                new Vector2(moveTarget.position.x, moveTarget.position.z)) > 0.03f)
            {
                navMesh.destination = moveTarget.transform.position; //Move to target
                transform.LookAt(moveTarget.transform.position); //Face target
                moveTarget.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 1); //Paint target red

            }

            //Else, reached target
            else if(moveTargetIndex < dragScript.getMarkerHolder().childCount)
            {
                //Set target to black again
                moveTarget.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 1);
                
                //Find new target
                moveTargetIndex++;
            }
            
            //If reached end of markers
            if(moveTargetIndex == dragScript.getMarkerHolder().childCount)
                following = false;
        }
    }
}
