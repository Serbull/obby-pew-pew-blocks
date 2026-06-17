using UnityEngine;

public class GameplayDevider : MonoBehaviour
{
    private void OnEnable()
    {
        YG.YG2.GameplayStop();
    }

    private void OnDisable()
    {
        YG.YG2.GameplayStart();
    }
}
