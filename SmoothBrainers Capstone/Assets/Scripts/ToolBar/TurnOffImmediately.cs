using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffImmediately : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return null;

        //Your Function You Want to Call
        gameObject.SetActive(false);
    }
}
