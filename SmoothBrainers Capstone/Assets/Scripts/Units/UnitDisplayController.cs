using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UnitDisplayController : MonoBehaviour
{
    
    [SerializeField] private TMP_InputField unitName;
    [SerializeField] private TextMeshProUGUI ammo;
    [SerializeField] private TextMeshProUGUI acceleration;
    [SerializeField] private TextMeshProUGUI firePower;
    [SerializeField] private Slider health;
    [SerializeField] private TMP_InputField currentAction;

    // Start is called before the first frame update
    void Start()
    {
        unitName.text = gameObject.transform.parent.name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setAmmo(float currentAmmo, float maximumAmmo)
    {
        ammo.text = currentAmmo.ToString() + " / " + maximumAmmo.ToString();
    }
    public void setAcceleration(float currentAcceleration)
    {
        acceleration.text = currentAcceleration.ToString();
    }
    public void setFirepower(float availableFirePower)
    {
        firePower.text = availableFirePower.ToString();
    }
    public void setHealth(float currentHealthPercent)
    {
        health.value = currentHealthPercent;
    }
    public void setAction(string newAction)
    {
        currentAction.text = newAction;
    }
}
