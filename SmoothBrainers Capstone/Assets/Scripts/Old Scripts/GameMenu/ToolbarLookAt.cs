using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolbarLookAt : MonoBehaviour
{
    public Transform vrCamera;
    public Vector3 offset; 
    public bool followRotation = true;  
    // Start is called before the first frame update
    void Start()
    {
        if (vrCamera == null)
        {
            vrCamera = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = vrCamera.position + offset;

        if (followRotation)
        {
            transform.rotation = vrCamera.rotation;
        }

        Debug.Log("Camera position: " + vrCamera.position);
        Debug.Log("Canvas position: " + transform.position);

    }
}
