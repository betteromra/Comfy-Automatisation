using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    Vector2 _mouseMoveInput = Vector2.zero;
    public Vector2 mouseMoveInput { get => _mouseMoveInput; }
    Vector2 _keyboardMoveInput = Vector2.zero;
    public Vector2 keyboardMoveInput { get => _keyboardMoveInput; }
    bool _cameraMouseMove = false;
    public bool cameraMouseMove { get => _cameraMouseMove; }
    float _zoomInput = 0;
    public float zoomInput { get => _zoomInput; }
    public event Action onPressedSelect;
    public event Action onPressedDeselect;
    public event Action onPressedBuild;
    public event Action onPressedCancelBuild;

    #region Input
    void OnMouseMove(InputValue value)
    {
        // the pan need to be inverted so it look like you grab the terrain and move
        if (_cameraMouseMove)
        {
            _mouseMoveInput = value.Get<Vector2>() * -1;
        }
        // we need to make sure that we want to pan
        else _mouseMoveInput = Vector3.zero;
    }
    void OnMouseEnableMovePressed(InputAction.CallbackContext value)
    {
        // if the button is pressed the value will be over 0
        _cameraMouseMove = 0 < value.ReadValue<float>();
    }
    void OnKeyboardMove(InputValue value)
    {
        // we need to make sure that we aren't already moving with the mouse
        if (!_cameraMouseMove)
        {
            _keyboardMoveInput = value.Get<Vector2>();
        }
        else _keyboardMoveInput = Vector3.zero;
    }
    void OnZoom(InputValue value)
    {
        // inverted the zoom since if we scrolldown the y is higher
        _zoomInput = value.Get<float>() * -1;
    }

    void OnSelect(InputValue value)
    {
        if (value.isPressed)
        {
            onPressedSelect?.Invoke();
        }
    }
    void OnDeselect(InputValue value)
    {
        if (value.isPressed)
        {
            onPressedDeselect?.Invoke();
        }
    }
    void OnBuild(InputValue value)
    {
        if (value.isPressed)
        {
            onPressedBuild?.Invoke();
        }
    }
    void OnCancelBuild(InputValue value)
    {
        if (value.isPressed)
        {
            onPressedCancelBuild?.Invoke();
        }
    }

    #endregion

    void OnEnable()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        InputAction enableMoveAction = playerInput.actions["MouseEnableMove"];
        enableMoveAction.started += OnMouseEnableMovePressed;
        enableMoveAction.canceled += OnMouseEnableMovePressed;
        enableMoveAction.Enable();
    }
    void OnDisable()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        InputAction enableMoveAction = playerInput.actions["MouseEnableMove"];
        enableMoveAction.started -= OnMouseEnableMovePressed;
        enableMoveAction.canceled -= OnMouseEnableMovePressed;
        enableMoveAction.Disable();
    }

    // Selection is in the selection manager now
}
