using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HoldWithinButton : MonoBehaviour
{
    Material outerButton;
    public float maxTimer = 1.0f;
    public float currentTimer = 0.0f;
    public bool withinButton = false;
    public bool pressable = true;
    public ButtonEffect buttonEffect;
    public GameObject visualEffect;
    Vector3 initialSmallSize;

    // Start is called before the first frame update
    void Start()
    {
        initialSmallSize = visualEffect.transform.localScale;
        outerButton = gameObject.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if(withinButton)// && pressable)
        {
            currentTimer += Time.deltaTime;  
        }
        if(!withinButton)
        {
            currentTimer -= Time.deltaTime;
        }
        
        Color newColor = outerButton.color;
        newColor.a = Mathf.Lerp(0.75f, 0.0f, currentTimer / maxTimer);
        outerButton.color = newColor;

        visualEffect.transform.localScale = Vector3.Lerp(initialSmallSize, new Vector3(1, 1, 1), currentTimer / maxTimer);
        
        if(currentTimer < 0)
        {
            currentTimer = 0;
        }

        if(currentTimer >= maxTimer)
        {
            currentTimer = maxTimer;
            if(pressable)
            {
                pressable = false;
                buttonEffect.ButtonPressed();
            }
        }
        if(pressable == false && currentTimer == 0)
        {
            pressable = true;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "PlayerHands")
        {
            withinButton = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "PlayerHands")
        {
            withinButton = false;
        }
    }

    

}
