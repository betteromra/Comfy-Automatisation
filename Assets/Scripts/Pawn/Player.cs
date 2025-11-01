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
    bool _showRawRecipeInput = false;
    public bool showRawRecipeInput { get => _showRawRecipeInput; }
    bool _rotateBuild = false;
    public bool rotateBuild { get => _rotateBuild; }
    public event Action onPressedSelect;
    public event Action onPressedDeselect;
    public event Action onPressedBuild;
    public event Action onPressedCancelBuild;
    public event Action onShowRawRecipe;
    public event Action onDeleteBuild;
    public event Action onRotateBuild;
    public event Action onNpcAction;

    #region Input
    void OnMouseMoveCamera(InputValue value)
    {
        // the pan need to be inverted so it look like you grab the terrain and move
        if (_cameraMouseMove)
        {
            _mouseMoveInput = value.Get<Vector2>() * -1;
        }
        // we need to make sure that we want to pan
        else _mouseMoveInput = Vector3.zero;
    }
    void OnMouseEnableMoveCameraPressed(InputAction.CallbackContext value)
    {
        // if the button is pressed the value will be over 0
        _cameraMouseMove = 0 < value.ReadValue<float>();
    }
    void OnKeyboardMoveCamera(InputValue value)
    {
        // we need to make sure that we aren't already moving with the mouse
        if (!_cameraMouseMove)
        {
            _keyboardMoveInput = value.Get<Vector2>();
        }
        else _keyboardMoveInput = Vector3.zero;
    }
    void OnZoomCamera(InputValue value)
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
    void OnDeleteBuild(InputValue value)
    {
        if (value.isPressed)
        {
            onDeleteBuild?.Invoke();
        }
    }
    void OnShowRawRecipePressed(InputAction.CallbackContext value)
    {
        _showRawRecipeInput = 0 < value.ReadValue<float>();
        onShowRawRecipe?.Invoke();
    }
    void OnRotateBuildingPressed(InputAction.CallbackContext value)
    {
        _showRawRecipeInput = 0 < value.ReadValue<float>();
        onRotateBuild?.Invoke();
    }
    void OnNpcAction(InputValue value)
    {
        onNpcAction?.Invoke();
    }

    #endregion

    void OnEnable()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();

        playerInput.actions["MouseEnableMoveCamera"].started += OnMouseEnableMoveCameraPressed;
        playerInput.actions["MouseEnableMoveCamera"].canceled += OnMouseEnableMoveCameraPressed;
        playerInput.actions["MouseEnableMoveCamera"].Enable();

        playerInput.actions["ShowRawRecipe"].started += OnShowRawRecipePressed;
        playerInput.actions["ShowRawRecipe"].canceled += OnShowRawRecipePressed;
        playerInput.actions["ShowRawRecipe"].Enable();

        playerInput.actions["RotateBuilding"].started += OnRotateBuildingPressed;
        playerInput.actions["RotateBuilding"].canceled += OnRotateBuildingPressed;
        playerInput.actions["RotateBuilding"].Enable();
    }
    void OnDisable()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();

        playerInput.actions["MouseEnableMoveCamera"].started -= OnMouseEnableMoveCameraPressed;
        playerInput.actions["MouseEnableMoveCamera"].canceled -= OnMouseEnableMoveCameraPressed;
        playerInput.actions["MouseEnableMoveCamera"].Disable();

        playerInput.actions["ShowRawRecipe"].started -= OnShowRawRecipePressed;
        playerInput.actions["ShowRawRecipe"].canceled -= OnShowRawRecipePressed;
        playerInput.actions["ShowRawRecipe"].Disable();
        
        playerInput.actions["RotateBuilding"].started -= OnRotateBuildingPressed;
        playerInput.actions["RotateBuilding"].canceled -= OnRotateBuildingPressed;
        playerInput.actions["RotateBuilding"].Disable();
    }

    // Selection is in the selection manager now
}
