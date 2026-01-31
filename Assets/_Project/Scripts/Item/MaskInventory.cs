using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaskInventory : MonoBehaviour
{
    List<Mask> _maskList = new List<Mask>();
    public List<Mask> MaskList => _maskList;
    int _currentMaskIndex;
    public int CurrentMaskIndex => _currentMaskIndex;

    bool _isInventoryOpened;
    public bool IsInventoryOpened => _isInventoryOpened;

    InputAction navigateAction;
    InputAction toggleIventoryAction;


    void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        StartListeningForInput();
    }

    void OnDisable()
    {
        StopListeningForInput();
    }


    void Init()
    {
        navigateAction = InputSystem.actions.FindAction("Navigate");
        toggleIventoryAction = InputSystem.actions.FindAction("ToggleInventory");
    }

    void StartListeningForInput()
    {
        navigateAction.started += SwitchMask;
        toggleIventoryAction.started += ToggleInventory;
    }

    void StopListeningForInput()
    {
        navigateAction.started -= SwitchMask;
        toggleIventoryAction.started -= ToggleInventory;
    }


    public void ToggleInventory(InputAction.CallbackContext ctx)
    {
        _isInventoryOpened = !_isInventoryOpened;
    }

    public void SwitchMask(InputAction.CallbackContext ctx)
    {

        var axis = ctx.ReadValue<Vector2>();
        _currentMaskIndex += (int) axis.x;
        
        if (_currentMaskIndex > _maskList.Count-1)
        {
            _currentMaskIndex = 0;
        }
        else if (_currentMaskIndex < 0)
        {
            _currentMaskIndex = _maskList.Count-1;
        }
    }

    public void AddMask(Mask mask)
    {
        _maskList.Add(mask);
    }

    public void RemoveMask(Mask mask)
    {
        _maskList.Remove(mask);
    }
}
