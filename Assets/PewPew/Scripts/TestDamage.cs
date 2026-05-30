using UnityEngine;

public class TestDamage : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            print("KEY USE");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                IDamageable dmg = hit.collider.GetComponent<IDamageable>();
                if (dmg != null)
                {
                    // Передаем урон, направление луча и ТОЧНУЮ точку клика (hit.point)
                    dmg.TakeDamage(1, ray.direction, hit.point);
                }
            }
        }
    }
}