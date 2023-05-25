using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class LetMeGrabAIs : MonoBehaviour
{
    OVRGrabbable grabScript;
    Tank tankScript;
    NavMeshAgent agentComp;
    GameObject unitMenu;
    // Start is called before the first frame update
    void Start()
    {
        agentComp = gameObject.GetComponent<NavMeshAgent>();
        tankScript = gameObject.GetComponent<Tank>();
        grabScript = gameObject.GetComponent<OVRGrabbable>();
        unitMenu = tankScript.unitDisplay;
    }

    // Update is called once per frame
    public void PickedUp()
    {
        agentComp.enabled = false;
        agentComp.enabled = false;

        unitMenu.SetActive(true);
    }
    public void Dropped()
    {
        agentComp.enabled = true;
        tankScript.enabled = true;
    
        unitMenu.SetActive(false);
    }
}
