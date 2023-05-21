using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class retainOffset : MonoBehaviour
{
    public GameObject grabPoint;
    public GameObject KeyItem;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = grabPoint.transform.position - KeyItem.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        KeyItem.transform.position = grabPoint.transform.position - offset;
    }
}
