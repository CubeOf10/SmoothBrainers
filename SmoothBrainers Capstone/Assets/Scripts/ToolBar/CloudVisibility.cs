using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudVisibility : ButtonEffect
{
    public GameObject clouds;
    // Start is called before the first frame update
    public override void ButtonPressed()
    {
        if(clouds.activeInHierarchy)
        {
            clouds.SetActive(false);
        }
        else
        {
            clouds.SetActive(true);
        }
    }
}
