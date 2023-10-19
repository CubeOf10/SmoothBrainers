using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowPath : MonoBehaviour
{
    public bool flyingType;
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
            
                if(flyingType)
                {
                    Vector2 previousPos = new Vector2(dragScript.getMarkerHolder().GetChild(moveTargetIndex-1).position.x,
                                                      dragScript.getMarkerHolder().GetChild(moveTargetIndex-1).position.z);
                    Vector2 nextPos = new Vector2(dragScript.getMarkerHolder().GetChild(moveTargetIndex).position.x,
                                                      dragScript.getMarkerHolder().GetChild(moveTargetIndex).position.z);
                    Vector2 unitPos = new Vector2(transform.position.x, transform.position.z);


                    navMesh.baseOffset = Mathf.Lerp(dragScript.getMarkerHolder().GetChild(moveTargetIndex).GetComponent<WaypointHeight>().height, 
                                                    dragScript.getMarkerHolder().GetChild(moveTargetIndex-1).GetComponent<WaypointHeight>().height, 
                                                    
                                                    Vector2.Distance(unitPos, nextPos) / Vector2.Distance(previousPos, nextPos));
                                                    
                                                    ;               

                Debug.Log(Vector2.Distance(new Vector2(transform.position.x, transform.position.z), 
                                            new Vector2(moveTarget.position.x, moveTarget.position.z)) + " / "  +
                          Vector3.Distance(dragScript.getMarkerHolder().GetChild(moveTargetIndex-1).transform.position, 
                            dragScript.getMarkerHolder().GetChild(moveTargetIndex).transform.position) + " = " + 
                            Vector2.Distance(unitPos, nextPos) / Vector2.Distance(previousPos, nextPos));

                }
            }

            else if(moveTargetIndex < dragScript.getMarkerHolder().childCount)
            {
                moveTarget.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 1);
                moveTargetIndex++;
            }
            
            if(moveTargetIndex == dragScript.getMarkerHolder().childCount)
            {
                following = false;
            }            


        }
    }
}
