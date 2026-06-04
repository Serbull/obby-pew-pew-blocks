using UnityEngine;
using System.Collections;

public class WaterDeath : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Задержка перед уничтожением бота после падения")]
    public float deathDelay = 2f; 

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
            StartCoroutine(BotDeathTimer(other.gameObject));
        }
    }

    // Для ботов оставляем небольшую задержку, чтобы они красиво тонули
    IEnumerator BotDeathTimer(GameObject bot)
    {
        Debug.Log($"[Вода] {bot.name} упал в воду! Уничтожение бота через {deathDelay} сек...");
        yield return new WaitForSeconds(deathDelay);

        GameController controller = FindFirstObjectByType<GameController>();
        if (controller != null)
        {
            controller.BotDeath(bot);
        }
    }
}