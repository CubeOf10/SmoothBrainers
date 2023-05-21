using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDeleter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        if(!other.GetComponent<OVRGrabbable>().isGrabbed)
        {
            Destroy(other.gameObject);
        }
    }
}
