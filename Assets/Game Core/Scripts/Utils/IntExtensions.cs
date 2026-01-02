using System;

public static class IntExtensions
{
    private readonly static string[] keys = new[] { "K", "M", "B", "T", "Q" };

    public static string ToShortValue(this int count)
    {
        if (count < 1000)
        {
            return count.ToString();
        }

        var log = (int)Math.Log10(count);
        var del = Math.Pow(10, log - 2);
        var ork = Math.Pow(10, 2 - log % 3);
        var key = keys[Math.Clamp(log / 3 - 1, 0, keys.Length - 1)];
        return Math.Floor(count / del) / ork + key;
    }

    public static string ToShortValue(this float count)
    {
        if (count < 1000)
        {
            return count.ToString("0.##");
        }

        var log = (int)Math.Log10(count);
        var del = Math.Pow(10, log - 2);
        var ork = Math.Pow(10, 2 - log % 3);
        var key = keys[Math.Clamp(log / 3 - 1, 0, keys.Length - 1)];
        return Math.Floor(count / del) / ork + key;
    }
}