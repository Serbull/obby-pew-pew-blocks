using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MathfUtils : MonoBehaviour
{
    /// <summary>
    /// Get random index by weight, more value of which gives better chance.
    /// </summary>
    /// <param name="weights">Array of weights</param>
    /// <param name="totalWeight">Total sum of all weights in array</param>
    /// <param name="resultIndex">Result index in array</param>
    /// <returns>Returns true if result found, otherwise false</returns>
    public static int GetRandomIndexByWeight(IReadOnlyList<int> weights)
    {
        if (weights == null || weights.Count == 0)
        {
            return -1;
        }

        int totalWeight = weights.Sum();
        if (totalWeight <= 0)
        {
            return -1;
        }

        int random = Random.Range(1, totalWeight + 1);
        int sum = 0;

        for (int i = 0; i < weights.Count; ++i)
        {
            if (weights[i] <= 0)
            {
                continue;
            }

            sum += weights[i];

            if (sum >= random)
            {
                return i;
            }
        }

        return -1;
    }

    public static List<int> GetUniqueRandomIndicesByWeight(IReadOnlyList<int> weights, int count)
    {
        List<int> resultIndices = new List<int>();
        List<int> availableIndices = Enumerable.Range(0, weights.Count).ToList();

        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0)
            {
                break;
            }

            int totalWeight = availableIndices.Sum(index => weights[index]);
            if (totalWeight <= 0)
            {
                break;
            }

            int random = Random.Range(1, totalWeight + 1);
            int sum = 0;

            foreach (int index in availableIndices)
            {
                sum += weights[index];

                if (sum >= random)
                {
                    resultIndices.Add(index);
                    availableIndices.Remove(index);
                    break;
                }
            }
        }

        return resultIndices;
    }
}
