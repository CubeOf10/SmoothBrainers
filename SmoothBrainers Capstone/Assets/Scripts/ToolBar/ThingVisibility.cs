using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThingVisibility : ButtonEffect
{
    public GameObject thingToHide;
    // Start is called before the first frame update
    public override void ButtonPressed()
    {
        if(thingToHide.activeInHierarchy)
        {
            thingToHide.SetActive(false);
        }
        else
        {
            thingToHide.SetActive(true);
        }
    }
}
