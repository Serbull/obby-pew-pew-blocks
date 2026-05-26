using UnityEngine;
using System.Collections;

public class WaterDeath : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform spawnPoint;  // Наша пустая точка SpawnPoint со сцены
    public float deathDelay = 3f; // Задержка в 3 секунды

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в триггер упал именно игрок
        if (other.CompareTag("Player"))
        {
            StartCoroutine(RespawnTimer(other.gameObject));
        }
    }

    IEnumerator RespawnTimer(GameObject player)
    {
        Debug.Log("Игрок упал в воду! Возрождение через " + deathDelay + " секунды...");
        
        // Ждем 3 секунды, пока игрок чутка проваливается
        yield return new WaitForSeconds(deathDelay);

        if (spawnPoint != null)
        {
            // На время перемещения отключаем CharacterController игрока, если он есть
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Телепортируем на спавн
            player.transform.position = spawnPoint.position;

            // Включаем обратно
            if (cc != null) cc.enabled = true;
            
            Debug.Log("Игрок успешно возрожден!");
        }
        else
        {
            Debug.LogError("Ошибка: Забыл перетащить SpawnPoint в инспектор воды!");
        }
    }
}
