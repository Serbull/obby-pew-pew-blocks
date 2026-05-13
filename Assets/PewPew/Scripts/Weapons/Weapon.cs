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

        Ray camRay = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit camHit;
        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out camHit, range))
            targetPoint = camHit.point;
        else
            targetPoint = camRay.origin + camRay.direction * range;

        Vector3 shootDir = (targetPoint - muzzlePoint.position).normalized;

        // 3. Создаем пулю
        GameObject bullet = Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            Quaternion.LookRotation(shootDir)
        );

        bullet.transform.localPosition = bullet.transform.localPosition;

        if (bullet.TryGetComponent<BulletTracer>(out var tracer))
        {
            tracer.Init(shootDir);
        }
    }
}