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
    public static float prevTime;
    public static int offTrackCounter;
    public static float xte;

    // Parameters
    public float offTrackXTE; // 1.4
    private bool isOffTrack = false;
    private float startTime;

    // Labels to update
    private Text lapLabel;
    private Text timeLabel;
    private Text prevTimeLabel;
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
        startTime = Time.time;

        // Initializing stats
        lap = 1;
        time = 0;
        prevTime = 0;
        offTrackCounter = 0;
        xte = 0;

        // Initializing PM
        pm = FindObjectOfType<PathManager>();

        // Initializing Car
        GameObject carObj = GameObject.FindGameObjectWithTag("DonkeyCar");

        if (carObj != null)
        {
            car = (Car)carObj.GetComponent<ICar>();
        }

        // Getting labels to update
        GameObject statsPanel = GameObject.Find("StatsPanel");
        if (statsPanel != null)
        {
            Text[] labels = statsPanel.GetComponentsInChildren<Text>();

            if (labels.Length >= 5)
            {
                lapLabel = labels[0];
                timeLabel = labels[1];
                outOfTrackLabel = labels[2];
                xteLabel = labels[3];
                prevTimeLabel = labels[4];
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
        prevTimeLabel.text = prevTimeLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + prevTime;
        outOfTrackLabel.text = outOfTrackLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + offTrackCounter;
        xteLabel.text = xteLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + xte;
    }

    private void getUpdatedStats()
    {
        if(car == null)
        {
            GameObject carObj = GameObject.FindGameObjectWithTag("DonkeyCar");

            if (carObj != null)
            {
                car = (Car)carObj.GetComponent<ICar>();
            }

            if(car == null) { return; }
        }

        // Updating lap
        if (car != null && lap < car.nrExitedTriggers + 1)
        {
            lap = car.nrExitedTriggers + 1;

            float finishTime = Time.time;
            prevTime = finishTime- startTime;
            startTime = finishTime;
        }

        // Updating time
        time = Time.time - startTime;

        // Updating XTE
        // pm.path.GetCrossTrackErr(car.GetTransform().position + (car.GetTransform().forward * car.GetVelocity().magnitude), ref xte); // Alternative
        if (pm != null && car != null)
        {
            pm.path.GetCrossTrackErr(car.GetTransform().position, ref xte);

            // Updating Out-of-track counter
            if (Math.Abs(xte) > Math.Abs(offTrackXTE))
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
}
