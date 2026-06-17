using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform muzzlePoint;
    public Camera playerCamera;
    public GameObject bulletPrefab;

    public float fireRate = 1f;
    public float range = 200f;

    private float nextFireTime;
    private WeaponController weaponController;

    void Start()
    {
        playerCamera = Camera.main;
        weaponController = GetComponent<WeaponController>();
    }

    public void Shoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        // Защита: если камеры нет (например, у бота), чтобы код не падал с ошибкой
        if (playerCamera == null)
        {
            Debug.LogError("Camera is null");
            return;
        }

        // На ПК целимся курсором мыши, на телефоне — точкой касания экрана.
        // Input.mousePosition на мобильных не следует за пальцем, поэтому берём позицию тача.
        Vector3 aimScreenPos = Input.mousePosition;
        if (Input.touchCount > 0)
        {
            aimScreenPos = Input.GetTouch(Input.touchCount - 1).position;
        }

        Ray camRay = playerCamera.ScreenPointToRay(aimScreenPos);
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
            // Берём параметры из экипированного скина (у бота скин == null, тогда остаются дефолтные значения пули)
            WeaponSkin skin = weaponController != null ? weaponController.EquippedSkin : null;
            if (skin != null)
            {
                tracer.Init(shootDir, skin.bulletSpeed, skin.forceMultiplier);
            }
            else
            {
                tracer.Init(shootDir);
            }
        }
    }
}