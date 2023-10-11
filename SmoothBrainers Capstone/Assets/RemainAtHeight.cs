using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemainAtHeight : MonoBehaviour
{
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
        transform.position = new Vector3(transform.position.x, terrain.transform.position.y + offset, transform.position.z);
        transform.rotation = startRot;
    }
}
