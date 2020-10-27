using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSetter : MonoBehaviour
{
    // DropDown from the Menu canvas
    Dropdown dropdown;

    // Current selection message from the dropdown
    string message;

    // Text of the current selection
    public Text text;

    // Index of the current selection
    int dropDownValue;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<Dropdown>();

        dropdown.onValueChanged.AddListener(
            delegate
            {
                DropdownValueChanged(dropdown);
            });
    }

    void DropdownValueChanged(Dropdown change)
    {
        setCameraSensorRes(dropdown.options[change.value].text);
    }

    void setCameraSensorRes(string resolution)
    {
        CameraSensor.width = System.Convert.ToInt32(resolution.Split('x')[0]);
        CameraSensor.height = System.Convert.ToInt32(resolution.Split('x')[1]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
