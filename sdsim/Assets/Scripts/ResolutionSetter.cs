using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSetter : MonoBehaviour
{
    //Attach this script to a Dropdown GameObject
    Dropdown m_Dropdown;
    //This is the string that stores the current selection m_Text of the Dropdown
    string m_Message;
    //This Text outputs the current selection to the screen
    public Text m_Text;
    //This is the index value of the Dropdown
    int m_DropdownValue;
    void Start()
    {
        //Fetch the Dropdown GameObject
        m_Dropdown = GetComponent<Dropdown>();
        //Add listener for when the value of the Dropdown changes, to take action
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });
        setCameraSensorRes(m_Dropdown.options[m_DropdownValue].text);
    }

    void DropdownValueChanged(Dropdown change)
    {
        setCameraSensorRes(m_Dropdown.options[change.value].text);
    }

    void setCameraSensorRes(string resolution)
    {
        CameraSensor.width = System.Convert.ToInt32(resolution.Split('x')[0]);
        CameraSensor.height = System.Convert.ToInt32(resolution.Split('x')[1]);
    }
}