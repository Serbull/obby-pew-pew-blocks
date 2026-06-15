using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform muzzlePoint;
    public Camera playerCamera;
    public GameObject bulletPrefab;

    public float fireRate = 1f;
    public float range = 200f;

    private float nextFireTime;

    public void Shoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        // Защита: если камеры нет (например, у бота), чтобы код не падал с ошибкой
        if (playerCamera == null) return;

        Ray camRay = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit camHit;
        Vector3 targetPoint;

        // ИСПРАВЛЕНИЕ: Создаем маску игнорирования слоев персонажа и UI
        int ignoreMask = LayerMask.GetMask("Character", "UI");

        // Добавляем ~ignoreMask в Raycast, чтобы луч пролетал сквозь игрока и текст
        if (Physics.Raycast(camRay, out camHit, range, ~ignoreMask))
        {
            targetPoint = camHit.point;
        }
        else
        {
            targetPoint = camRay.origin + camRay.direction * range;
        }

        SpawnBullet(targetPoint);
    }

    public void ShootBot(Vector3 targetPoint)
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        SpawnBullet(targetPoint);
    }

    private void SpawnBullet(Vector3 targetPoint)
    {
        // Здесь используется твой muzzlePoint, теперь всё скомпилируется без ошибок
        Vector3 shootDir = (targetPoint - muzzlePoint.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            Quaternion.LookRotation(shootDir)
        );

        if (bullet.TryGetComponent<BulletTracer>(out var tracer))
        {
            tracer.Init(shootDir);
        }
    }
}