using UnityEngine;
using YG;

[RequireComponent(typeof(LeaderboardYG))]
public class LB_Updater : MonoBehaviour
{
    private LeaderboardYG _lb;

    private void Start()
    {
        _lb = GetComponent<LeaderboardYG>();
        InvokeRepeating(nameof(UpdateLB), 50f, 50f);
    }

    private void UpdateLB()
    {
        _lb.UpdateLB();
    }
}
