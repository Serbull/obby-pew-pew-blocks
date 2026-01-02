using System;
using Serbull.GameAssets;

public static class GameValues
{
    public static event Action OnCoinsChanged;
    public static event Action OnCupsChanged;
    public static event Action OnEnergyChanged;
    public static event Action OnLuckySpinChanged;
    public static event Action OnRevive;

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

    //
    //  public static void AddCups(int value)
    //  { 
    //      SaveManager.Data.cup += value;
    //      OnCupsChanged?.Invoke();
    //  }
    //
    //  public static void SubstractCups(int value)
    //  {
    //      SaveManager.Data.cup -= value;
    //      OnCupsChanged?.Invoke();
    //  }

    //  public static void AddEnergy(float value)
    //  {
    //      SaveManager.Data.energy += value;
    //      OnEnergyChanged?.Invoke();
    //  }
    //
    //  public static void SubstractEnergy(float value)
    //  {
    //      SaveManager.Data.energy -= value;
    //      OnEnergyChanged?.Invoke();
    //  }

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

    // public static void AddRevive(int value)
    // {
    //     SaveManager.Data.revive += value;
    //     OnRevive?.Invoke();
    // }
}
