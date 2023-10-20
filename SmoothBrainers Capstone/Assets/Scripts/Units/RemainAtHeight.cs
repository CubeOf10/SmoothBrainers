using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemainAtHeight : MonoBehaviour
{
    public bool grounded = true;
    public bool rotationAllowed;
    public float offset;
    Quaternion startRot;
    GameObject terrain;
    // Start is called before the first frame update
    void Start()
    {
        startRot = transform.rotation;
        terrain = GameObject.Find("Terrain");
    }

    public void Released()
    {
        if(grounded)
        
            transform.position = new Vector3(transform.position.x, terrain.transform.position.y + offset, transform.position.z);

        if(!rotationAllowed){     
            transform.rotation = startRot;
            return;
        }
        
    }
}
