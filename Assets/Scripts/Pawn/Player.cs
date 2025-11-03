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
    bool _enableCameraMouseMove = false;
    public bool enableCameraMouseMove { get => _enableCameraMouseMove; }
    float _zoomInput = 0;
    public float zoomInput { get => _zoomInput; }
    bool _showRawRecipeInput = false;
    public bool showRawRecipeInput { get => _showRawRecipeInput; }
    float _rotateBuildInput = 0;
    public float rotateBuildInput { get => _rotateBuildInput; }
    bool _enableRotateBuild = false;
    public bool enableRotateBuild { get => _enableRotateBuild; }
    public event Action onPressedSelect;
    public event Action onPressedDeselect;
    public event Action onPressedBuild;
    public event Action onPressedCancelBuild;
    public event Action onShowRawRecipe;
    public event Action onDeleteBuild;
    public event Action onEnableRotateBuild;
    public event Action onNpcAction;

    #region Input
    void OnMouseMoveCamera(InputValue value)
    {
        // the pan need to be inverted so it look like you grab the terrain and move
        if (_enableCameraMouseMove)
        {
            _mouseMoveInput = value.Get<Vector2>() * -1;
        }
        // we need to make sure that we want to pan
        else _mouseMoveInput = Vector3.zero;
    }
    void OnMouseEnableMoveCameraPressed(InputAction.CallbackContext value)
    {
        // if the button is pressed the value will be over 0
        _enableCameraMouseMove = 0 < value.ReadValue<float>();
    }
    void OnKeyboardMoveCamera(InputValue value)
    {
        // we need to make sure that we aren't already moving with the mouse
        if (!_enableCameraMouseMove)
        {
            _keyboardMoveInput = value.Get<Vector2>();
        }
        else _keyboardMoveInput = Vector3.zero;
    }
    void OnZoomCamera(InputValue value)
    {
        if (_enableRotateBuild) return;
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
    void OnRotateBuilding(InputValue value)
    {
        if (!GameManager.instance.buildingManager.isBuilding) return;
        if (!_enableRotateBuild) return;

        _rotateBuildInput = value.Get<float>();
    }
    void OnEnableRotateBuildingPressed(InputAction.CallbackContext value)
    {
        if (!GameManager.instance.buildingManager.isBuilding) return;

        _enableRotateBuild = 0 < value.ReadValue<float>();

        if (_enableRotateBuild) _zoomInput = 0;
        else _rotateBuildInput = 0;
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

        playerInput.actions["EnableRotateBuilding"].started += OnEnableRotateBuildingPressed;
        playerInput.actions["EnableRotateBuilding"].canceled += OnEnableRotateBuildingPressed;
        playerInput.actions["EnableRotateBuilding"].Enable();
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

        playerInput.actions["EnableRotateBuilding"].started -= OnEnableRotateBuildingPressed;
        playerInput.actions["EnableRotateBuilding"].canceled -= OnEnableRotateBuildingPressed;
        playerInput.actions["EnableRotateBuilding"].Disable();
    }

    // Selection is in the selection manager now
}
