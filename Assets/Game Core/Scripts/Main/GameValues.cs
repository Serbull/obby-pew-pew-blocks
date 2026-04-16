using System;
using Serbull.GameAssets;

public static class GameValues
{
    public static event Action OnCoinsChanged;
    public static event Action OnLuckySpinChanged;

    public static void AddCoins(long value)
    {
        SaveManager.Data.coins += value;
        OnCoinsChanged?.Invoke();
    }

    public static void SubstractCoins(long value)
    {
        SaveManager.Data.coins -= value;
        OnCoinsChanged?.Invoke();
    }

    public static void AddLuckySpin(int value)
    {
        SaveManager.Data.luckySpin += value;
        OnLuckySpinChanged?.Invoke();
    }

    public static void SubstractLuckySpin(int value)
    {
        SaveManager.Data.luckySpin -= value;
        OnLuckySpinChanged?.Invoke();
    }
}
