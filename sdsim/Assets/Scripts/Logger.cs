using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[Serializable]
public class MetaJson
{
    public string[] inputs;
    public string[] types;

    public void Init(string[] _inputs, string[] _types)
    {
        inputs = _inputs;
        types = _types;
    }
}

[Serializable]
public class DonkeyRecord
{
    public string cam_image_array;
    public float user_throttle;
    public float user_angle;
    public string user_mode; 

    public int track_lap;
    public int track_sector;
    public float speed;
    public int loc;
    public float track_x;
    public float track_y;
    public float track_z;
    public float track_cte;
    public bool track_oot;
    public float time;

    public void Init(string image_name, float angle, float throttle, string mode,
        int lap, int sector, float s, int local, float x, float y, float z,
        float cte, bool isOffTrack, float t)
    {
        cam_image_array = image_name;
        user_angle = angle;
        user_throttle = throttle;
        user_mode = mode;
        track_lap = lap;
        track_sector = sector;
        speed = s;
        loc = local;
        track_x = x;
        track_y = y;
        track_z = z;
        track_cte = cte;
        track_oot = isOffTrack;
        time = t;
    }

    public string AsString()
    {
        string json = JsonUtility.ToJson(this, true);

        // Can't name the variable names with a slash, so replace on output
        json = json.Replace("cam_image", "cam/image");
        json = json.Replace("user_angle", "user/angle");
        json = json.Replace("user_throttle", "user/throttle");
        json = json.Replace("user_mode", "user/mode");
        json = json.Replace("track_lap", "track/lap");
        json = json.Replace("track_sector", "track/sector");
        json = json.Replace("track_speed", "track/speed");
        json = json.Replace("track_loc", "track/loc");
        json = json.Replace("track_x", "track/x");
        json = json.Replace("track_y", "track/y");
        json = json.Replace("track_z", "track/z");
        json = json.Replace("track_cte", "track/cte");
        json = json.Replace("track_oot", "track/oot");
        json = json.Replace("track_time", "track/time");

        return json;
    }
}
public class Logger : MonoBehaviour {

	public GameObject carObj;
	public ICar car;
	public CameraSensor camSensor;
    public CameraSensor optionlB_CamSensor;
	public Lidar lidar;

	//what's the current frame index
    public int frameCounter = 0;

    //which lap
    public int lapCounter = 1;

	//is there an upper bound on the number of frames to log
	public int maxFramesToLog = 14000;

	//should we log when we are enabled
	public bool bDoLog = true;

    // sampling how many frames per seconds are saved
    public int limitFPS = 21;

    float timeSinceLastCapture = 0.0f;

	//We can output our logs in the style that matched the output from the udacity simulator
	public bool UdacityStyle = false;

    //Tub style as prefered by Donkey2
    public bool DonkeyStyle2 = true;

    public Text logDisplay;

	string outputFilename = "log_car_controls.txt";
	private StreamWriter writer;

	class ImageSaveJob {
		public string filename;
		public byte[] bytes;
	}
		
	List<ImageSaveJob> imagesToSave;

	Thread thread;

    // Path manager for XTE
    private static PathManager pm;
    private static int currentWaypoint;

    string GetLogPath()
    {
        if(GlobalState.log_path != "default")
            return GlobalState.log_path + "/";

        return Application.dataPath + "/../log/";
    }

    // void setLogPath(string path){
    //     Application.dataPath + "/../log/";
    // }

    void Awake()
	{

        // Initializing PM
        pm = FindObjectOfType<PathManager>();

        DonkeyStyle2 = true;
        UdacityStyle = false;

        car = carObj.GetComponent<ICar>();

		if(bDoLog && car != null)
		{
			if(UdacityStyle)
			{
				outputFilename = "driving_log.csv";
                var file = Directory.CreateDirectory(GetLogPath() +  "IMG/"); // returns a DirectoryInfo object
            }

            string filename = GetLogPath() + outputFilename;

			writer = new StreamWriter(filename);

			Debug.Log("Opening file for log at: " + filename);

			if(UdacityStyle)
			{
				writer.WriteLine("center,steering,throttle,speed (km/h),cte,time");
			}

            if(DonkeyStyle2)
            {
                MetaJson mjson = new MetaJson();
                string[] inputs = {"cam/image_array", "user/angle", "user/throttle", "user/mode", "track/lap", "track/sector",
                                   "track/speed", "track/loc", "track/x", "track/y", "track/z", "track/cte", "track/time"};
                string[] types = {"image_array", "float", "float", "str", "int","int", "float", "int", "float", "float", "float", "float", "float" };

                mjson.Init(inputs, types);
                string json = JsonUtility.ToJson(mjson, true);
				var f = File.CreateText(GetLogPath() + "meta.json");
				f.Write(json);
				f.Close();
            }
		}

        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        GameObject go = CarSpawner.getChildGameObject(canvas.gameObject, "LogCount");
        if (go != null)
            logDisplay = go.GetComponent<Text>();

        imagesToSave = new List<ImageSaveJob>();

		thread = new Thread(SaverThread);
		thread.Start();

    }

    // Update is called once per frame
    void Update () 
	{
		if(!bDoLog)
			return;

        timeSinceLastCapture += Time.deltaTime;

        if (timeSinceLastCapture < 1.0f / limitFPS)
            return;

        timeSinceLastCapture -= (1.0f / limitFPS);

        string activity = car.GetActivity();

		if(writer != null)
		{
			if(UdacityStyle)
            { 
                string image_filename = GetUdacityStyleImageFilename();
				float steering = car.GetSteering() / car.GetMaxSteering();
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}",
                    image_filename, // center
                    steering.ToString(), // steering
                    car.GetThrottle().ToString(), // throttle
                    //car.GetFootBrake(), // brake, not working, prob not implemented yet
                    Math.Round(car.GetVelocity().magnitude * 3.6, 2), // speed (* 3.6 => m/s to km/h)
                    StatsDisplayer.xte, // cte
                    StatsDisplayer.lapTime)); // simulation time so far
            }
            else if(DonkeyStyle2)
            {
                DonkeyRecord mjson = new DonkeyRecord();
                float steering = car.GetSteering() / car.GetMaxSteering();
                float throttle = car.GetThrottle();
                int loc = LocationMarker.GetNearestLocMarker(carObj.transform.position);

                // training code like steering clamped between -1, 1
                steering = Mathf.Clamp(steering, -1.0f, 1.0f);

                float xte = StatsDisplayer.xte;
                bool isOffTrack = false;

                // Out-of-track check
                if (Math.Abs(xte) > Math.Abs(2.0))
                {
                   isOffTrack = true;
                }
                else
                {
                    isOffTrack = false;
                }

                if (SceneManager.GetActiveScene().name == "road_generator")
                {
                    mjson.Init(
                        string.Format("{0}_cam-image_array_.jpg", frameCounter),
                        steering,
                        throttle,
                        "user",
                        0,
                        StatsDisplayerRoadGenerator.getCurrentWaypoint(),
                        car.GetVelocity().magnitude,
                        loc,
                        car.GetTransform().position.x,
                        car.GetTransform().position.y,
                        car.GetTransform().position.z,
                        StatsDisplayerRoadGenerator.getXTE(),
                        isOffTrack,
                        Time.timeSinceLevelLoad);
                }
                else
                {
                    mjson.Init(
                        string.Format("{0}_cam-image_array_.jpg", frameCounter),
                        steering,
                        throttle,
                        "user",
                        StatsDisplayer.getLap(),
                        StatsDisplayer.getCurrentWaypoint(),
                        car.GetVelocity().magnitude,
                        loc,
                        car.GetTransform().position.x,
                        car.GetTransform().position.y,
                        car.GetTransform().position.z,
                        StatsDisplayer.xte,
                        isOffTrack,
                        Time.timeSinceLevelLoad);
                }

                string json = mjson.AsString();
                string filename = string.Format("record_{0}.json", frameCounter);
                var f = File.CreateText(GetLogPath() + filename);
                f.Write(json);
                f.Close();

            }
            else
			{
				writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", frameCounter.ToString(), activity, car.GetSteering().ToString(), car.GetThrottle().ToString(), car.GetVelocity().magnitude, car.GetTransform().position.x, car.GetTransform().position.z));
			}
		}

		if(lidar != null)
		{
			LidarPointArray pa = lidar.GetOutput();

			if(pa != null)
			{
				string json = JsonUtility.ToJson(pa, true);
				var filename = string.Format("lidar_{0}_{1}.txt", frameCounter.ToString(), activity);
				var f = File.CreateText(GetLogPath() + filename);
				f.Write(json);
				f.Close();
			}
		}

        if (optionlB_CamSensor != null)
        {
            SaveCamSensor(camSensor, activity, "_a");
            SaveCamSensor(optionlB_CamSensor, activity, "_b");
        }
        else
        {
            SaveCamSensor(camSensor, activity, "");
        }

        if (maxFramesToLog != -1 && frameCounter >= maxFramesToLog)
        {
            Shutdown();
            gameObject.SetActive(false);
        }

        frameCounter = frameCounter + 1;

        if (logDisplay != null)
            logDisplay.text = "Log: " + frameCounter;
	}

	string GetUdacityStyleImageFilename()
	{
		return GetLogPath() + string.Format("IMG/center_{0,8:D8}.jpg", frameCounter);
    }

    string GetDonkey2StyleImageFilename()
    {
        return GetLogPath() + string.Format("{0}_cam-image_array_.jpg", frameCounter);
    }

    //Save the camera sensor to an image. Use the suffix to distinguish between cameras.
    void SaveCamSensor(CameraSensor cs, string prefix, string suffix)
    {
        if (cs != null)
        {
            Texture2D image = cs.GetImage();

            ImageSaveJob ij = new ImageSaveJob();

			if(UdacityStyle)
			{
				ij.filename = GetUdacityStyleImageFilename();

				ij.bytes = image.EncodeToJPG();
			}
            else if (DonkeyStyle2)
            {
                ij.filename = GetDonkey2StyleImageFilename();

                ij.bytes = image.EncodeToJPG();
            }

            lock (this)
            {
                imagesToSave.Add(ij);
            }
        }
    }

    public void SaverThread()
	{
		while(true)
		{
			int count = 0;

			lock(this)
			{
				count = imagesToSave.Count; 
			}

			if(count > 0)
			{
				ImageSaveJob ij = imagesToSave[0];

                //Debug.Log("saving: " + ij.filename);

                File.WriteAllBytes(ij.filename, ij.bytes);

				lock(this)
				{
					imagesToSave.RemoveAt(0);
				}
			}
		}
	}

	public void Shutdown()
	{
		if(writer != null)
		{
			writer.Close();
			writer = null;
		}

		if(thread != null)
		{
			thread.Abort();
			thread = null;
		}

		bDoLog = false;
	}

	void OnDestroy()
	{
		Shutdown();
	}
}
