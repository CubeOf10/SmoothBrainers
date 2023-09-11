using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDelete : MonoBehaviour
{
    public float deleteTimer = 2f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(transform.gameObject, deleteTimer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
