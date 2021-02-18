using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    // Tries to find the car in the scene with the specified name
    public static ICar tryGetCar(string name)
    {
        GameObject carObj = GameObject.FindGameObjectWithTag(name);

        if (carObj != null)
        {
            ICar car = carObj.GetComponent<ICar>();
            return car;
        }

        return null;
    }

    // Computes the angle at which the car looks at the next waypoint
    public static float angle(Transform transform, PathNode waypoint)
	{
		Vector3 heading = waypoint.pos - transform.position;
		heading.y = 0;
		return Quaternion.Angle(transform.rotation, Quaternion.LookRotation(heading));
	}

    // Tells if the car is going forward
    public static bool carIsGoingForward(Car car)
    {
        if (car != null)
        {
            return Quaternion.Angle(car.transform.rotation, Quaternion.LookRotation(car.GetVelocity())) < 90;
        }
        return false;
    }

    // Returns the mean of an array of values
    public static float getMean(List<float> array)
    {
        float mean = 0;

        foreach (float f in array)
        {
            mean += f;
        }

        return mean / array.Count;
    }

    // Returns the variance of an array of values
    public static float getVar(List<float> array, float mean)
    {
        // Returns variance
        float variance = 0;

        foreach (float f in array)
        {
            variance += (f - mean) * (f - mean);
        }

        return variance / (array.Count);
    }
}
