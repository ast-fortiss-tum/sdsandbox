using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPathStorer : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] wayPoints = GameObject.FindGameObjectsWithTag("pathNode");

        string text = "";
        foreach (GameObject waypoint in wayPoints)
        {
            text += waypoint.transform.position.x + "," + waypoint.transform.position.y + "," + waypoint.transform.position.z;
            text += '\n';
        }


        System.IO.File.WriteAllText("Assets/Resources/LatestWaypoints.txt", text);

        Debug.Log("Point-Path found and written to LatestWaypoints.txt");
    }
}
