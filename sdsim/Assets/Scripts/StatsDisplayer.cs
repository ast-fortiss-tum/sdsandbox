using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class StatsDisplayer : MonoBehaviour
{

    // Stats
    public static int lap;
    public static float time;
    public static int offTrackCounter;
    public static float xte;

    // Parameters
    public float offTrackXTE; // 1.4
    private bool isOffTrack = false;

    // Labels to update
    private Text lapLabel;
    private Text timeLabel;
    private Text outOfTrackLabel;
    private Text xteLabel;

    
    // Path manager for XTE
    private PathManager pm;

    // Car
    public GameObject carObj;
    private Car car;

    // Start is called before the first frame update
    void Start()
    {
        // Initializing stats
        lap = 1;
        time = 0;
        offTrackCounter = 0;
        xte = 0;

        // Initializing PM
        pm = GameObject.FindObjectOfType<PathManager>();

        // Initializing Car
        car = (Car)(GameObject.Find("Donkey(Clone)").GetComponent<ICar>());

        // Getting labels to update
        GameObject statsPanel = GameObject.Find("StatsPanel");
        if (statsPanel != null)
        {
            Text[] labels = statsPanel.GetComponentsInChildren<Text>();

            if (labels.Length >= 4)
            {
                lapLabel = labels[0];
                timeLabel = labels[1];
                outOfTrackLabel = labels[2];
                xteLabel = labels[3];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        getUpdatedStats();
        displayStats();
    }

    private void displayStats()
    {
        lapLabel.text = lapLabel.text.Split(new string[] { ": "}, StringSplitOptions.None)[0] + ": " +lap;
        timeLabel.text = timeLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + time;
        outOfTrackLabel.text = outOfTrackLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + offTrackCounter;
        xteLabel.text = xteLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + xte;
    }

    private void getUpdatedStats()
    {
        // Updating lap
        lap = car.nrExitedTriggers + 1;

        // Updating time
        time = Time.time;

        // Updating XTE
        // pm.path.GetCrossTrackErr(car.GetTransform().position + (car.GetTransform().forward * car.GetVelocity().magnitude), ref xte); // Alternative
        pm.path.GetCrossTrackErr(car.GetTransform().position, ref xte);

        // Updating Out-of-track counter
        if(Math.Abs(xte) > Math.Abs(offTrackXTE))
        {
            if (!isOffTrack)
            {
                offTrackCounter += 1;
                isOffTrack = true;
            }
        }
        else
        {
            isOffTrack = false;
        }
    }
}
