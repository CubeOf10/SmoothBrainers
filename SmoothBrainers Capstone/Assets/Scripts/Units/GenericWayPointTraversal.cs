using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

//Issues involving Unity's navigation surfaces resulted in the 
// flying units receiving a less than perfect final controller
// They will move directly to their next point, in 3D space, regardless of obstacles. 
// (Don't drag units through mountains) 
public class GenericWayPointTraversal : MonoBehaviour
{
    public float speed = 0.3f;
    Transform moveTarget;
    public int moveTargetIndex;
    DragMe dragScript;
    public bool following;
    
    private Vector3 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        dragScript = GetComponent<DragMe>();
    }

    // Update is called once per frame
    void Update()
    {
        if(following)
        {
            // Stop if no markers
            if(dragScript.getMarkerHolder().childCount == 0)
                return;
            
            moveTarget = dragScript.getMarkerHolder().GetChild(moveTargetIndex);

            //Look at the next target
            targetPosition = moveTarget.position;
            targetPosition.y = transform.position.y;

            //Keep the correct y axis rotation (upright)
            Vector3 faceProperly = new Vector3(moveTarget.position.x, transform.position.y, moveTarget.position.z);
            transform.LookAt(faceProperly);
            
            //Move to target
            transform.position = Vector3.MoveTowards(transform.position, moveTarget.position, speed * Time.deltaTime);

            //Set target to red
            if (Vector3.Distance(transform.position, moveTarget.position) > 0.03f)
                moveTarget.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 1);
               
            //Change previous target to black, find next target
            else if(moveTargetIndex < dragScript.getMarkerHolder().childCount)
            {
                moveTarget.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 1);
                moveTargetIndex++;
            }

            //End of markers
            if(moveTargetIndex == dragScript.getMarkerHolder().childCount)
                    following = false;
            
        }
    }
}
