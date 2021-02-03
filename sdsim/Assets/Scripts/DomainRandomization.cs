using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomainRandomization : MonoBehaviour
{
    // Whether to use randomization or not
    public static bool useRandomization;

    // Things that can be randomized (walls, lights, track)
    public Light[] lights;
    public GameObject[] walls;
    public GameObject track;

    // Randomization time interval
    public float randomizationSeconds;
    private float latestRandomizationTime;

    // Attributes that can be modified for lights
    private Color[] colors = { Color.red, Color.blue, Color.yellow, Color.green, Color.white};
    //private GameObject[] lightRanges = { };

    // Attributes that can be modified for walls
    private GameObject[] wallTextures = { };

    // Attributes that can be modified for track
    private Texture2D[] trackTextures = { };

    T pickRandom <T>(T[] array)
    {
        int index = (int)(Random.Range(0, array.Length));
        return array[index];
    }


    void randomizeLights()
    {
        foreach (Light l in lights)
        {
            l.color = pickRandom(colors);
            l.transform.position = new Vector3(
                Random.Range(-32.4f, 4.4f),
                Random.Range(10.9f, 19.9f),
                Random.Range(-6, 6)
            );
        }
    }

    void randomizeWalls()
    {
        foreach(GameObject wall in walls)
        {
            Renderer renderer = wall.GetComponent<Renderer>();

            renderer.material.color = pickRandom(colors);
            // renderer.material.SetTexture()
        }
    }

    void randomizeTrack()
    {

    }


    void randomizeDomain()
    {
        randomizeLights();
        randomizeWalls();
        randomizeTrack();
    }



    // Start is called before the first frame update
    void Start()
    {
        latestRandomizationTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (useRandomization)
        {
            if (Time.time - latestRandomizationTime >= randomizationSeconds)
            {
                randomizeDomain();
                latestRandomizationTime = Time.time;
            }
        }
    }
}
