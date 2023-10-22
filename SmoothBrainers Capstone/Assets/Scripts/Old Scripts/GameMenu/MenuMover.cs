using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMover : MonoBehaviour
{
    OVRGrabbable grabScript;
    MeshRenderer mr;
    Material _material;
    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponentInChildren<MeshRenderer>();
        _material = mr.material;
        grabScript = GetComponent<OVRGrabbable>();
    }

    // Update is called once per frame
    void Update()
    {
        if(grabScript.isGrabbed)
        {
            _material.SetColor("Color", new Color(1, 1, 1, 1));
        }
        else
            _material.SetColor("Color", new Color(1, 1, 1, 0.3f));

    }
}