using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Each entity menu needs to be ON upon starting to initialise values in scripts. 
// This will immediately turn them off once done. It's a bandaid fix but it works. 
public class TurnOffImmediately : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return null; //Wait one frame
        gameObject.SetActive(false); 
    }
}
