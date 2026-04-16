using UnityEngine;
using Serbull.GameAssets;

public class GameManager : MonoBehaviour, IResourceGiver, ICurrency
{
    long ICurrency.Amount => SaveManager.Data.coins;

    void ICurrency.Add(long amount)
    {
        GameValues.AddCoins(amount);
    }

    void ICurrency.Spend(long amount)
    {
        GameValues.SubstractCoins(amount);
    }

    public void AddResource(string resource, int count)
    {
        Debug.LogError($"Add resource {resource} x {count}");
    }
}
