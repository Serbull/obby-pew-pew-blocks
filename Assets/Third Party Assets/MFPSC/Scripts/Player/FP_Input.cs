using UnityEngine;

public class FP_Input : MonoBehaviour 
{
    [HideInInspector] public bool UseMobileInput;

    public Inputs mobileInputs;

    private void Start()
    {
        UseMobileInput = !YG.YG2.envir.isDesktop;
        gameObject.SetActive(UseMobileInput);
    }

    public Vector3 MoveInput()
    {
        return mobileInputs.moveJoystick.MoveInput();
    }

    public bool MovePressed()
    {
        return mobileInputs.moveJoystick.IsPressed();
    }

    public Vector2 LookInput()
    {
        return mobileInputs.lookPad != null ? mobileInputs.lookPad.LookInput() : Vector2.zero;
    }

    public Vector2 ShotInput()
    {
        return mobileInputs.shotButton != null ? mobileInputs.shotButton.MoveInput() : Vector2.zero;
    }

    public bool Shoot()
    {
        return mobileInputs.shotButton != null && mobileInputs.shotButton.IsPressed();
    }

    public bool Reload()
    {
        return mobileInputs.reloadButton != null && mobileInputs.reloadButton.OnRelease();
    }

    public bool Run()
    {
        return mobileInputs.runButton != null && mobileInputs.runButton.IsPressed();
    }

    public bool Jump()
    {
        return mobileInputs.jumpButton != null && mobileInputs.jumpButton.IsPressed();
    }

    public bool Crouch()
    {
        return mobileInputs.crouchButton != null && mobileInputs.crouchButton.Toggle();
    }

    public bool ZoomIn()
    {
        return mobileInputs.zoomInButton != null && mobileInputs.zoomInButton.IsPressed();
    }

    public bool ZoomOut()
    {
        return mobileInputs.zoomOutButton != null && mobileInputs.zoomOutButton.IsPressed();
    }
}

[System.Serializable]
public class Inputs
{
    public FP_Joystick moveJoystick;
    public FP_Lookpad lookPad;
    public FP_Button runButton, jumpButton, crouchButton, shotButton, reloadButton, zoomInButton, zoomOutButton;
}