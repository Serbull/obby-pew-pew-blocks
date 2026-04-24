using UnityEngine;

public class CursorController : MonoBehaviour
{
    public RectTransform crosshair;
    public Animator playerAnimator;

    void Update()
    {
        if (playerAnimator == null || crosshair == null) return;

        bool isAiming = playerAnimator.GetBool("IsAiming");

        crosshair.gameObject.SetActive(isAiming);

        if (isAiming)
        {
            crosshair.position = Input.mousePosition;
        }
        else
        {
            crosshair.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        }

        ApplyCursor(isAiming);
    }

    void ApplyCursor(bool isAiming)
    {
        Cursor.visible = !isAiming;
        Cursor.lockState = CursorLockMode.None;
    }
}