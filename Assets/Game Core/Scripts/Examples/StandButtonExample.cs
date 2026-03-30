using Serbull.GameAssets.Interact;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Cysharp.Threading.Tasks.Triggers;
using Serbull.GameAssets;

public class StandButtonExample : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractTrigger _interactTrigger;

    [SerializeField] private MeshRenderer _buttonRenderer;
    [SerializeField] private Material _disableBtnMaterial;
    [SerializeField] private Material _activeBtnMaterial;

    private bool _isActive;

    bool IInteractable.CanInteract => !_isActive;

    InteractData IInteractable.GetInteractData()
    {
        return new InteractData()
        {
            TargetObject = transform,
            TargetOffset = transform.up * 1.5f,
            Callback = Interact,
            Text = Services.Localization.GetText("use"),
        };
    }

    private void Start()
    {
        _interactTrigger.SetInteractable(this);
        SetActive(false);
    }

    private async void Interact()
    {
        SetActive(true);

        await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: destroyCancellationToken);

        SetActive(false);

        _interactTrigger.UpdateTrigger();
    }

    private void SetActive(bool value)
    {
        _isActive = value;
        _buttonRenderer.material = _isActive ? _disableBtnMaterial : _activeBtnMaterial;
    }
}
