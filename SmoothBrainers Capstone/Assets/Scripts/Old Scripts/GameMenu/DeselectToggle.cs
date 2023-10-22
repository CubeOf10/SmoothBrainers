using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class DeselectToggle : MonoBehaviour
{
    Toggle toggle;
    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.group.allowSwitchOff = true; 
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnToggleValueChanged(bool isOn)
    {
        if (!isOn)
            return;

        if (toggle.group.AnyTogglesOn() && !toggle.group.allowSwitchOff)
        {
            if (toggle.group.ActiveToggles().FirstOrDefault() == toggle)
            {
                toggle.isOn = false;
            }
        }
    }
}
