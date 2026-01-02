using UnityEngine;
using UnityEngine.AI;

public static class Closer
{
    public static T GetClosest<T>(Component[] massive, Vector3 point) where T : Component
    {
        return GetClosest<T>(massive, point, float.MaxValue);
    }

    public static T GetClosest<T>(Component[] massive, Vector3 point, float maxDistance) where T : Component
    {
        var result = GetClosest(massive, point, maxDistance);
        return result.GetComponent<T>() ?? default;
    }

    public static T GetClosest<T>(T[] massive, Vector3 point) where T : Component
    {
        return GetClosest(massive, point, float.MaxValue);
    }

    public static T GetClosest<T>(T[] massive, Vector3 point, float maxDistance) where T : Component
    {
        T closest = null;
        float minDistance = float.MaxValue;
        for (int i = 0; i < massive.Length; i++)
        {
            float dist = Vector3.Distance(massive[i].transform.position, point);
            if (dist < minDistance && dist < maxDistance)
            {
                closest = massive[i];
                minDistance = dist;
            }
        }

        return closest ?? default;
    }

    public static Vector3 GetClosest(Vector3[] massive, Vector3 point)
    {
        return GetClosest(massive, point, out int id);
    }

    public static Vector3 GetClosest(Vector3[] massive, Vector3 point, out int id)
    {
        id = 0;
        float minDistance = float.MaxValue;
        for (int i = 0; i < massive.Length; i++)
        {
            float dist = Vector3.Distance(massive[i], point);
            if (dist < minDistance)
            {
                id = i;
                minDistance = dist;
            }
        }

        return massive[id];
    }

    public static T[] GetClosest<T>(Component[] massive, Vector3 point, int count) where T : Component
    {
        var components = GetClosest(massive, point, count);
        var result = new T[components.Length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = components[i].GetComponent<T>();
        }

        return result;
    }

    public static T[] GetClosest<T>(T[] massive, Vector3 point, int count) where T : Component
    {
        count = Mathf.Min(massive.Length, count);
        var result = new T[count];
        var minDistances = new float[count];
        for (int i = 0; i < count; i++)
        {
            minDistances[i] = float.MaxValue;
        }
        int maxDistanceId = 0;

        for (int i = 0; i < massive.Length; i++)
        {
            float dist = Vector3.Distance(massive[i].transform.position, point);

            if (dist < minDistances[maxDistanceId])
            {
                result[maxDistanceId] = massive[i];
                minDistances[maxDistanceId] = dist;

                float targetValue = Mathf.Max(minDistances);
                for (int j = 0; j < count; j++)
                {
                    if (minDistances[j] == targetValue)
                    {
                        maxDistanceId = j;
                        break;
                    }
                }
            }
        }

        return result;
    }
     
    /*
    public static T[] SortByDistance<T>(T[] massive, Vector3 point) where T : Component
    {
        var list = new List<T>(massive);
        var result = new T[massive.Length];

        for (int i = 0; i < result.Length; i++)
        {
            var closest = GetClosest(list.ToArray(), point);
            list.Remove(closest);
            result[i] = closest;
        }

        return result;
    }
    */
}