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
                    dmg.TakeDamage(1);
            }
        }
    }
}