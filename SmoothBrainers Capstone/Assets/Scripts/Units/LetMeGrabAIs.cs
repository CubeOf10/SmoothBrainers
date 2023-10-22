using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//NavMeshAgents dragged over a NavMeshSurface will automatically snap to it, even out of the 
// users hand. This script turns off the agent until the unit is dropped, at which point it 
// properlysnaps to a nearby surface

//When the unit details were a thing, this would show and hide them too. 
public class LetMeGrabAIs : MonoBehaviour
{
    NavMeshAgent agentComp;
  
    void Start()
    {
        agentComp = gameObject.GetComponent<NavMeshAgent>();
    }

    //Turn off the NavMeshAgent when grabbed 
    public void PickedUp()
    {
        agentComp.enabled = false;
    }

    //Then turn it on again
    public void Dropped()
    {
        agentComp.enabled = true;
    }
}
