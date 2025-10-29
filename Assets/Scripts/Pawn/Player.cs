using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    Vector2 _mouseMoveInput = Vector2.zero;
    public Vector2 mouseMoveInput { get => _mouseMoveInput; }
    Vector2 _keyboardMoveInput = Vector2.zero;
    public Vector2 keyboardMoveInput { get => _keyboardMoveInput; }
    bool _cameraMouseMove = false;
    float _zoomInput = 0;
    public float zoomInput { get => _zoomInput; }
    public event Action onMoveCamera;
    public event Action onZoomCamera;

    #region Input
    void OnMouseMove(InputValue value)
    {
        // the pan need to be inverted so it look like you grab the terrain and move
        if (_cameraMouseMove)
        {
            onMoveCamera.Invoke();
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
            onMoveCamera.Invoke();
            _keyboardMoveInput = value.Get<Vector2>();
        }
        else _keyboardMoveInput = Vector3.zero;
    }
    void OnZoom(InputValue value)
    {
        // inverted the zoom since if we scrolldown the y is higher
        _zoomInput = value.Get<float>() * -1;
        onZoomCamera.Invoke();
    }

    #endregion

    #region Unity
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
    #endregion
}
