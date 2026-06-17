using System.Collections.Generic;
using DG.Tweening;

public static class Leaderboards
{
    private static readonly Queue<string> _order = new();
    private static readonly Dictionary<string, int> _latestScore = new();

    private static bool _isProcessing;

    public static void Send(string name, int score)
    {
        if (!_latestScore.ContainsKey(name))
            _order.Enqueue(name);

        _latestScore[name] = score;

        ProcessQueue();
    }

    private static void ProcessQueue()
    {
        if (_isProcessing) return;
        if (_order.Count == 0) return;

        _isProcessing = true;

        var id = _order.Dequeue();
        var score = _latestScore[id];

        _latestScore.Remove(id);

        //var vipAccess = VipValues.IsBought() ? "vip" : null;

        YG.YG2.SetLeaderboard(id, score);
        DOVirtual.DelayedCall(2f, FinishProcess);
    }

    private static void FinishProcess()
    {
        _isProcessing = false;
        ProcessQueue();
    }
}
