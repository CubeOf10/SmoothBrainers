using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class LetMeGrabAIs : MonoBehaviour
{
    OVRGrabbable grabScript;
    Tank tankScript;
    NavMeshAgent agentComp;
    // Start is called before the first frame update
    void Start()
    {
        agentComp = gameObject.GetComponent<NavMeshAgent>();
        tankScript = gameObject.GetComponent<Tank>();
        grabScript = gameObject.GetComponent<OVRGrabbable>();
    }

    // Update is called once per frame
    public void PickedUp()
    {
        agentComp.enabled = false;
        agentComp.enabled = false;
    }
    public void Dropped()
    {
        agentComp.enabled = true;
        tankScript.enabled = true;
    }
}
