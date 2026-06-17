using UnityEngine;
using System.Collections;

public class PlaytimeSender : MonoBehaviour
{
    private static readonly WaitForSecondsRealtime _timer = new (10f);
    private static int _savedPlaytime = -1;

    private IEnumerator Start()
    {
        if (_savedPlaytime == -1)
        {
            _savedPlaytime = SaveManager.Data.playtime;
        }

        while (true)
        {
            yield return _timer;

            SaveManager.Data.playtime = _savedPlaytime + (int)(Time.fixedTime * 1000);
            Leaderboards.Send("playtime", SaveManager.Data.playtime);
        }
    }
}
