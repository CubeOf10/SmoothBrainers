using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristActivater : MonoBehaviour
{
    GameObject rightMiddleFinger;
    GameObject rightIndexFinger;
    public float activationRate = 2f;
    float currentDelay;
    bool ready;
    bool index;
    bool middle;
    [Header("Palm Menu")]
    public GameObject palmMenu;
    void Start()
    {
        rightIndexFinger = GameObject.Find("r_index_finger_pad_marker");
        rightMiddleFinger = GameObject.Find("r_middle_finger_pad_marker");
    }
    void Update()
    {
        currentDelay -= Time.deltaTime;
        if(currentDelay < 0){
            ready = true;
            currentDelay = 0;
        }

    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == rightIndexFinger.name)
            index = true;
        
        if(other.gameObject.name == rightMiddleFinger.name)
            middle = true;
        
        if(index && middle && ready)
        {
            currentDelay = activationRate;
            ready = false;
            ActivateMenu();
        }
    }

    void ActivateMenu()
    {
        if(palmMenu.activeInHierarchy)
            palmMenu.SetActive(false);
        else palmMenu.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.name == rightIndexFinger.name)
            index = false;
        
        if(other.gameObject.name == rightMiddleFinger.name)
            middle = false;
    }
}
