using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform muzzlePoint;
    public Camera playerCamera;
    public GameObject bulletPrefab;

    public float fireRate = 0.15f;
    public float range = 200f;

    private float nextFireTime;

    public void Shoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        // 🔥 РЕЙ ИЗ КУРСОРА
        Ray camRay = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit camHit;

        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out camHit, range))
            targetPoint = camHit.point;
        else
            targetPoint = camRay.origin + camRay.direction * range;

        // 🔥 НАПРАВЛЕНИЕ ОТ МУЗЛА К ТОЧКЕ ПРИЦЕЛА
        Vector3 shootDir = (targetPoint - muzzlePoint.position).normalized;
        Debug.DrawRay(muzzlePoint.position, shootDir * 10f, Color.red, 2f);

        // 🔥 ПУЛЯ ЛЕТИТ В ПРИЦЕЛ
        GameObject bullet = Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            Quaternion.LookRotation(shootDir)
        );

        bullet.GetComponent<BulletTracer>().Init(shootDir);
    }
}