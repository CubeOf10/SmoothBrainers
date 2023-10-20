using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class GenericWayPointTraversal : MonoBehaviour
{
    public float speed = 0.3f;
    Transform moveTarget;
    public int moveTargetIndex;
    DragMe dragScript;
    public bool following;
    Vector3 direction;
    
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
            if(dragScript.getMarkerHolder().childCount == 0)
                return;
            
            moveTarget = dragScript.getMarkerHolder().GetChild(moveTargetIndex);
            direction = moveTarget.position - transform.position;

            targetPosition = moveTarget.position;
            targetPosition.y = transform.position.y;

            transform.position = Vector3.MoveTowards(transform.position, moveTarget.position, speed * Time.deltaTime);
            
            Vector3 faceProperly = new Vector3(moveTarget.position.x, transform.position.y, moveTarget.position.z);
            transform.LookAt(faceProperly);
            if (Vector3.Distance(transform.position, moveTarget.position) > 0.03f)
            {   
                moveTarget.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 1);
               
            }
            else if(moveTargetIndex < dragScript.getMarkerHolder().childCount)
            {
                moveTarget.transform.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 1);
                moveTargetIndex++;
            }
            if(moveTargetIndex == dragScript.getMarkerHolder().childCount)
                    following = false;
            
        }
        //transform.LookAt(targetPosition);        
    }
}
