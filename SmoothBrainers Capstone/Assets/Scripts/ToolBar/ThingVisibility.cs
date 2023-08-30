using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThingVisibility : ButtonEffect
{
    public GameObject thingToHide;
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
