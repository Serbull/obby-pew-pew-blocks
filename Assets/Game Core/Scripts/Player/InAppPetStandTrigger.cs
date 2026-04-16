using Serbull.GameAssets.Pets;
using UnityEngine;

[RequireComponent(typeof(InAppPetStand))]
public class InAppPetStandTrigger : MonoBehaviour
{
    [SerializeField] private string _inappId;

    private InAppPetStand _inappStand;
    private InappPetPopup _inappPopup;
    private YG.PurchaseYG _inappButton;

    public string InappId => _inappId;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_inappStand == null)
            {
                _inappStand = GetComponent<InAppPetStand>();
            }

            if (_inappPopup == null)
            {
                _inappPopup = FindAnyObjectByType<InappPetPopup>(FindObjectsInactive.Include);
            }

            if (_inappButton == null)
            {
                _inappButton = _inappPopup.GetComponentInChildren<YG.PurchaseYG>();
            }

            _inappButton.id = _inappId;
            _inappPopup.Show(_inappStand.PetId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_inappPopup != null)
            {
                _inappPopup.gameObject.SetActive(false);
            }
        }
    }
}
