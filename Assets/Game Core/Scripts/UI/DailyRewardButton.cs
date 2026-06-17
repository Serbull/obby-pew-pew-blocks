using UnityEngine;

public class DailyRewardButton : MonoBehaviour
{
    [SerializeField] private GameObject _root;

    private void OnEnable()
    {
        if (_root == null) return;

        _root.SetActive(true);
    }
}
