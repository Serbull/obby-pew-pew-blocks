using UnityEngine;
using System.Collections;

public class WaterDeath : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float deathDelay = 3f; // Задержка в 3 секунды

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в триггер упал именно игрок
        if (other.CompareTag("Player"))
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
            {
                StartCoroutine(PlayerRespawnTimer());
            }
            return;
        }

        if (other.CompareTag("Bot"))
        {
            StartCoroutine(BotDeathTimer(other.gameObject));
        }
    }

    IEnumerator PlayerRespawnTimer()
    {
        Debug.Log("Игрок упал в воду! Конец игры через " + deathDelay + " сек...");
        yield return new WaitForSeconds(deathDelay);

        GameController controller = FindFirstObjectByType<GameController>();
        if (controller != null) controller.StopGame();
    }

    IEnumerator BotDeathTimer(GameObject bot)
    {
        Debug.Log($"{bot.name} упал в воду! Исчезнет через " + deathDelay + " сек...");
        yield return new WaitForSeconds(deathDelay);

        GameController controller = FindFirstObjectByType<GameController>();
        if (controller != null)
        {
            controller.BotDeath(bot);
        }
    }
}
