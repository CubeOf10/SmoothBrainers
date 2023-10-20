using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
            moveTarget = dragScript.getMarkerHolder().GetChild(moveTargetIndex);
            if(Vector2.Distance(new Vector2(transform.position.x, transform.position.z), 
                                new Vector2(moveTarget.position.x, moveTarget.position.z)) > 0.03f)
            {
                navMesh.destination = moveTarget.transform.position;
                moveTarget.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 1);

            }
            

            else if(moveTargetIndex < dragScript.getMarkerHolder().childCount)
            {
                moveTarget.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 1);
                //moveTarget.transform.gameObject.GetComponent<MeshRenderer>().enabled = false;
                moveTargetIndex++;
            }
            
            if(moveTargetIndex == dragScript.getMarkerHolder().childCount)
            {
                following = false;
                foreach(GameObject marker in dragScript.getMarkerHolder())
                {
                    marker.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 1);
                }
            }            


        }
    }
}
