using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class LetMeGrabAIs : MonoBehaviour
{
    public int menuFadeDelay;
    public float menuFadeSpeed;
    OVRGrabbable grabScript;
    Tank tankScript;
    NavMeshAgent agentComp;
    Canvas unitMenu;
    // Start is called before the first frame update
    void Start()
    {
        agentComp = gameObject.GetComponent<NavMeshAgent>();
        tankScript = gameObject.GetComponent<Tank>();
        grabScript = gameObject.GetComponent<OVRGrabbable>();
        unitMenu = tankScript.unitDisplayCanvas;
    }

    // Update is called once per frame
    public void PickedUp()
    {
        agentComp.enabled = false;
        agentComp.enabled = false;

        unitMenu.GetComponent<CanvasGroup>().alpha = 100;
    }
    public void Dropped()
    {
        agentComp.enabled = true;
        tankScript.enabled = true;
    
        StartCoroutine(FadeUnitMenu());
    }

    private IEnumerator FadeUnitMenu()
    {
        yield return new WaitForSeconds(menuFadeDelay);
        StartCoroutine(ReduceAlpha());
    }

    private IEnumerator ReduceAlpha()
    {
        unitMenu.GetComponent<CanvasGroup>().alpha -= menuFadeSpeed * Time.deltaTime;

        if(unitMenu.GetComponent<CanvasGroup>().alpha <= 0)
        {
            yield break;
        }

        yield return new WaitForEndOfFrame();
        StartCoroutine(ReduceAlpha());
    }
}
