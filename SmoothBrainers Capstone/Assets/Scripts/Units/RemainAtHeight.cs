using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Keeps entities within a set y position relative to the desk
// An attempt was made to align the object's rotation to be upright though this fell through
// Instead objects remain at the rotation they're spawned at
// (not a major issue - objects are all round, except for barricades)
public class RemainAtHeight : MonoBehaviour
{
    public bool grounded = true;
    public bool rotationAllowed;
    public float offset;
    Quaternion startRot;
    GameObject terrain;
    void Start()
    {
        startRot = transform.rotation; //The rotation the object will be reset to upon being dropped
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
