using UnityEngine;

public class WaterDeath : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. ЕСЛИ В ВОДУ УПАЛ ИГРОК
        if (other.CompareTag("Player"))
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
            {
                GameController controller = FindFirstObjectByType<GameController>();
                if (controller != null)
                {
                    // Мгновенно передаем сигнал проигрыша в контроллер!
                    controller.PlayerDeath();
                }
            }
            return;
        }

        // 2. ЕСЛИ В ВОДУ УПАЛ БОТ (проверяем тег или имя префаба)
        if (other.CompareTag("Bot") || other.name.Contains("Bot"))
        {
            GameController controller = FindFirstObjectByType<GameController>();
            if (controller != null)
            {
                // ИСПРАВЛЕНИЕ: Никаких задержек и корутин, сносим бота сразу!
                Debug.Log($"[Вода] {other.gameObject.name} упал в воду! Мгновенное уничтожение.");
                controller.BotDeath(other.gameObject);
            }
        }
    }
}