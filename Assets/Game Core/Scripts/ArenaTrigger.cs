using UnityEngine;

public class ArenaTrigger : MonoBehaviour
{
    private ModeManager modeManager;

    void Start()
    {
        // Ищем главный менеджер режимов на сцене
        modeManager = FindFirstObjectByType<ModeManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Если на плиту наступил объект с тегом Player
        if (other.CompareTag("Player") && modeManager != null)
        {
            // Запускаем режим дуэли
            modeManager.EnterBotMode();
        }
    }
}