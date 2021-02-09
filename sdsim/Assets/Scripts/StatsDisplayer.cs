﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class StatsDisplayer : MonoBehaviour
{
    // Flag to see if car started
    public static bool carStarted;

    // Stats
    public static int lap = 1;
    public static float time;
    public static float prevTime;
    public static int offTrackCounter;
    public static float xte;
    public static float maxXte;
    public static float steerVar;
    public static List<float> steers;

    // Parameters
    public float offTrackXTE; // 1.4
    private bool isOffTrack = false;
    public static float startTime;

    // Labels to update
    private Text lapLabel;
    private Text timeLabel;
    private Text prevTimeLabel;
    private Text outOfTrackLabel;
    private Text xteLabel;
    private Text xteMaxLabel;
    private Text steerVarLabel;

    // Path manager for XTE
    private PathManager pm;

    // Car
    private static double epsilon = 0.01;
    private Vector3 startingCarPosition;
    public GameObject carObj;
    private Car car;

    // Start is called before the first frame update
    void Start()
    {
        // Initializing stats
        time = 0;
        prevTime = 0;
        offTrackCounter = 0;
        xte = 0;
        maxXte = 0;
        steerVar = 0;
        steers = new List<float>();

        // Initializing PM
        pm = FindObjectOfType<PathManager>();

        // Initializing Car
        tryGetCar();

        // Getting labels to update
        GameObject statsPanel = GameObject.Find("StatsPanel");
        if (statsPanel != null)
        {
            Text[] labels = statsPanel.GetComponentsInChildren<Text>();

            if (labels.Length >= 7)
            {
                lapLabel = labels[0];
                timeLabel = labels[1];
                outOfTrackLabel = labels[2];
                xteLabel = labels[3];
                prevTimeLabel = labels[4];
                xteMaxLabel = labels[5];
                steerVarLabel = labels[6];
            }
        }

        carStarted = checkCarStarted();
    }

    // Update is called once per frame
    void Update()
    {
        if (car == null)
        {
            tryGetCar();
        }

        if (!carStarted)
        {
            carStarted = checkCarStarted();
        }

        if (carStarted)
        {
            getUpdatedStats();
            displayStats();
        }
    }

    private void tryGetCar()
    {
        GameObject carObj = GameObject.FindGameObjectWithTag("DonkeyCar");

        if (carObj != null)
        {
            car = (Car)carObj.GetComponent<ICar>();
            startingCarPosition = car.startPos;
        }
    }

    private bool checkCarStarted()
    {
        if (carStarted)
        {
            return true;
        }

        bool areClose(double a, double b)
        {
            if (Math.Abs(a - b) < epsilon) {
                return true;
            };

            return false;
        }

        if(car == null)
        {
            return false;
        }

        if(
            areClose(car.transform.position.x, startingCarPosition.x) &&
            areClose(car.transform.position.z, startingCarPosition.z)
            )
        {
            return false;
        }

        startTime = Time.time;
        return true;
    }


    private void getUpdatedStats()
    {
        if(car == null)
        {
            tryGetCar();

            if(car == null) { return; }
        }

        // Updating time
        time = Time.time - startTime;

        // Updating steering infos
        steers.Add(car.GetSteering());

        if (pm != null)
        {
            // Updating XTE
            if (!pm.path.GetCrossTrackErr(car.GetTransform().position, ref xte))
            {
                // Lap finished, looping
                pm.path.ResetActiveSpan();

                // Counting lap
                lap += 1;

                // Updating prev. lap time and current starting time
                float finishTime = Time.time;
                prevTime = finishTime - startTime;
                startTime = finishTime;

                updateSteerVar();
            };

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

        // Updating max XTE
        if(Math.Abs(xte) > maxXte)
        {
            maxXte = Math.Abs(xte);
        }
    }

    private void displayStats()
    {
        lapLabel.text = lapLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + lap;
        timeLabel.text = timeLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + time;
        prevTimeLabel.text = prevTimeLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + prevTime;
        outOfTrackLabel.text = outOfTrackLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + offTrackCounter;
        xteLabel.text = xteLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + xte;
        xteMaxLabel.text = xteMaxLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + maxXte;
        steerVarLabel.text = steerVarLabel.text.Split(new string[] { ": "}, StringSplitOptions.None)[0] + ": " + steerVar;
    }

    private static float getMean(List<float> array)
    {
        float mean = 0;

        foreach (float f in array)
        {
            mean += f;
        }

        return mean / array.Count;
    }

    private static float getVar(List<float> array, float mean)
    {
        // Returns variance
        float variance = 0;

        foreach(float f in array)
        {
            variance += (f - mean) * (f - mean);
        }

        return variance / (array.Count);
    }

    public static void updateSteerVar()
    {
        // Updating steering variance for this lap
        steerVar = getVar(steers, getMean(steers));
        steers.Clear();
    }
}
