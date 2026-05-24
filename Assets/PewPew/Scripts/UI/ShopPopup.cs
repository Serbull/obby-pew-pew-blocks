using UnityEngine;
using UnityEngine.UI;

public class ShopPopup : MonoBehaviour
{
	[SerializeField] private Button closeButton;

	private void Start()
	{
		closeButton.onClick.AddListener(Close);
	}

	private void Close()
	{
		gameObject.SetActive(false);
	}
}
