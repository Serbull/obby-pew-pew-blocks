using UnityEngine;
using Serbull.GameAssets.Pets;

[RequireComponent(typeof(EggStand))]
public class EggStandTrigger : MonoBehaviour
{
    private EggStand _eggStand;
    private EggPopup _eggPopup;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_eggStand == null)
            {
                _eggStand = GetComponent<EggStand>();
            }

            if (_eggPopup == null)
            {
                _eggPopup = PetManager.EggPopup;
            }

            _eggPopup.Show(_eggStand.EggId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_eggPopup != null)
            {
                _eggPopup.gameObject.SetActive(false);
            }
        }
    }
}
