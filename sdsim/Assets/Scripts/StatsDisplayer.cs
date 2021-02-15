using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class StatsDisplayer : MonoBehaviour
{
    // Flag to see if car started
    public static bool carStarted;

    // Log informations / histories per Lap
    public static bool writeLog = true;
    public static List<float> timesHistory;
    public static List<float> offTrackHistory;
    public static List<float> xteAvgHistory;
    public static List<float> xteVarHistory;
    public static List<float> maxXteHistory;
    public static List<float> steersAvgHistory;
    public static List<float> steersVarsHistory;
    public static List<float> speedAvgHistory;
    public static List<float> speedVarHistory;
    public static List<int> crashesHistory;

    // Displayable Stats
    public static int lap = 1;
    public static int lapCrashes = 0;
    public static float lapTime;
    public static float prevLapTime;
    public static int offTrackCounter;
    public static float xte;
    public static float maxXte;
    public static float lapSteerVar;

    // Lap frames
    public static List<float> lapXtes;
    public static List<float> lapSteers;
    public static List<float> lapSpeeds;

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
        lapTime = 0;
        prevLapTime = 0;
        offTrackCounter = 0;
        xte = 0;
        maxXte = 0;
        lapSteerVar = 0;

        lapXtes = new List<float>();
        lapSteers = new List<float>();
        lapSpeeds = new List<float>();

        // Initializing histories
        if (writeLog)
        {
            timesHistory = new List<float>();
            offTrackHistory = new List<float>();
            xteAvgHistory = new List<float>();
            xteVarHistory = new List<float>();
            maxXteHistory = new List<float>();
            steersAvgHistory = new List<float>();
            steersVarsHistory = new List<float>();
            speedAvgHistory = new List<float>();
            speedVarHistory = new List<float>();
            crashesHistory = new List<int>();
        }

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

    private void OnDestroy()
    {
        if (writeLog)
        {
            DateTime now = DateTime.Now;
            string hour = "" + now.Hour;
            string minute = "" + now.Minute;

            if(now.Hour < 10)
            {
                hour = "0" + hour;
            }

            if(now.Minute < 10)
            {
                minute = "0" + minute;
            }

            string filename = "Online Testing - " + now.Day + "-"+ now.Month + "-" + now.Year + "-" + hour + minute;

            // MacOS
            string filepath = Application.dataPath + "/" + filename + ".csv";

            string text = "Lap time; Off-track; Max XTE; XTE avg; XTE var; Steer avg; Steer var; Speed avg; Speed var; Crashes;\n";
            for(int i = 0; i<timesHistory.Count; i++)
            {
                float t = timesHistory[i];
                float o = offTrackHistory[i];
                float m = maxXteHistory[i];
                float xa = xteAvgHistory[i];
                float xv = xteVarHistory[i];
                float sta = steersAvgHistory[i];
                float stv = steersVarsHistory[i];
                float spa = speedAvgHistory[i];
                float spv = speedVarHistory[i];
                float c = crashesHistory[i];

                text += t + ";" + o + ";" + m + ";" + xa + ";" + xv + ";" + sta + ";" + stv + ";" + spa + ";" + spv + ";" + c +";\n";
            }

            System.IO.File.WriteAllText(filepath, text);
            Debug.Log("Log file written to " + filepath);
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
        lapTime = Time.time - startTime;

        // Updating frame infos
        lapSteers.Add(Math.Abs(car.GetSteering()));
        lapSpeeds.Add(Math.Abs(car.GetVelocity().magnitude));

        if(car.GetLastCollision() != null)
        {
            lapCrashes += 1;
            car.ClearLastCollision();
        }

        if (pm != null)
        {
            // Updating XTE
            if (!pm.path.GetCrossTrackErr(car.GetTransform().position, ref xte))
            {
                // Lap finished, looping
                pm.path.ResetActiveSpan();

                endOfLapUpdates();
            };

            // Updating xte infos
            lapXtes.Add(Math.Abs(xte));

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

    public static void endOfLapUpdates()
    {
        // Counting lap
        lap += 1;

        // Updating prev. lap time and current starting time
        float finishTime = Time.time;
        prevLapTime = finishTime - startTime;
        startTime = finishTime;


        // Updating log infos
        if (writeLog)
        {
            timesHistory.Add(prevLapTime);
            offTrackHistory.Add(offTrackCounter);

            // XTE
            float lapxteavg = getMean(lapXtes);
            xteAvgHistory.Add(lapxteavg);
            xteVarHistory.Add(getVar(lapXtes, lapxteavg));
            maxXteHistory.Add(maxXte);

            // Steer
            float lapsteeravg = getMean(lapSteers);
            steersAvgHistory.Add(lapsteeravg);
            steersVarsHistory.Add(getVar(lapSteers, lapsteeravg));

            // Speed
            float lapspeedavg = getMean(lapSpeeds);
            speedAvgHistory.Add(lapspeedavg);
            speedVarHistory.Add(getVar(lapSpeeds, lapspeedavg));

            // Crashes
            crashesHistory.Add(lapCrashes);
        }

        // Clear lap frames
        lapXtes.Clear();
        lapSteers.Clear();
        lapSpeeds.Clear();

        // Resetting max XTE and off-track counter for this lap
        maxXte = 0;
        offTrackCounter = 0;
        lapCrashes = 0;
    }

    private void displayStats()
    {
        lapLabel.text = lapLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + lap;
        timeLabel.text = timeLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + lapTime;
        prevTimeLabel.text = prevTimeLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + prevLapTime;
        outOfTrackLabel.text = outOfTrackLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + offTrackCounter;
        xteLabel.text = xteLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + xte;
        xteMaxLabel.text = xteMaxLabel.text.Split(new string[] { ": " }, StringSplitOptions.None)[0] + ": " + maxXte;
        steerVarLabel.text = steerVarLabel.text.Split(new string[] { ": "}, StringSplitOptions.None)[0] + ": " + lapSteerVar;
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
}
