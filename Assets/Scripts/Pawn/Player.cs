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
    float _zoomInput = 0;
    public float zoomInput { get => _zoomInput; }
    public event Action onMoveCamera;
    public event Action onZoomCamera;

    [Header("Selection Settings")]
    [SerializeField] private LayerMask selectableLayers = -1;
    private HashSet<Renderer> selectedRenderers = new HashSet<Renderer>();
    private HashSet<Selectable> selectedObjects = new HashSet<Selectable>();
    private Selectable currentHoveredObject = null;

    public event Action<HashSet<Renderer>> OnSelectionChanged;

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

    void OnSelect(InputValue value)
    {
        if (value.isPressed)
        {
            HandleSelection();
        }
    }

    #endregion

    #region Selection Logic

    private void HandleSelection()
    {
        Camera playerCamera = GameManager.instance.cameraManager.mainCamera;
        if (playerCamera == null)
            playerCamera = Camera.main;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        bool isMultiSelect = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                            Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayers))
        {
            // Check if the hit object has a SelectableObjects component
            Selectable selectable = hit.collider.GetComponentInParent<Selectable>();

            if (selectable != null)
            {
                if (isMultiSelect)
                {
                    ToggleSelection(selectable);
                }
                else
                {
                    // Single selection mode - clear others first
                    if (!IsSelected(selectable))
                    {
                        ClearSelection();
                        SelectObject(selectable);
                    }
                }
            }
        }
        else if (!isMultiSelect)
        {
            // Only clear if not holding multi-select key
            ClearSelection();
        }
    }

    public void ToggleSelection(Selectable selected)
    {
        if (selectedObjects.Contains(selected))
        {
            DeselectObject(selected);
        }
        else
        {
            SelectObject(selected);
        }

        OnSelectionChanged?.Invoke(selectedRenderers);
    }

    public void SelectObject(Selectable selected)
    {
        if (selectedObjects.Contains(selected)) return;

        selectedObjects.Add(selected);

        Renderer[] renderers = selected.GetComponentsInChildren<Renderer>();
        Debug.Log(renderers.Length);
        foreach (Renderer renderer in renderers)
        {
            selectedRenderers.Add(renderer);
        }

        OnSelectionChanged?.Invoke(selectedRenderers);
    }

    public void DeselectObject(Selectable selected)
    {
        if (!selectedObjects.Contains(selected)) return;

        selectedObjects.Remove(selected);

        Renderer[] renderers = selected.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            selectedRenderers.Remove(renderer);
        }

        OnSelectionChanged?.Invoke(selectedRenderers);
    }

    public void ClearSelection()
    {
        selectedObjects.Clear();
        selectedRenderers.Clear();
        OnSelectionChanged?.Invoke(selectedRenderers);
    }

    public HashSet<Renderer> GetSelectedRenderers() => selectedRenderers;
    public HashSet<Selectable> GetSelectedObjects() => selectedObjects;

    public bool IsSelected(Selectable slecected) => selectedObjects.Contains(slecected);

    #endregion

    #region Unity

    void Update()
    {
        // Handle hover detection
        HandleHover();

        // Clear selection with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();
        }
    }

    private void HandleHover()
    {
        // Don't do hover detection if camera is being moved
        if (_cameraMouseMove) return;

        Camera playerCamera = GameManager.instance.cameraManager.mainCamera;
        if (playerCamera == null)
            playerCamera = Camera.main;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayers))
        {
            Selectable hoveredSelectable = hit.collider.GetComponentInParent<Selectable>();

            if (hoveredSelectable != null)
            {
                // New object being hovered
                if (currentHoveredObject != hoveredSelectable)
                {
                    // Clear previous hover
                    if (currentHoveredObject != null)
                    {
                        currentHoveredObject.SetHovered(false);
                    }

                    // Set new hover
                    currentHoveredObject = hoveredSelectable;
                    currentHoveredObject.SetHovered(true);
                }
            }
            else
            {
                // Hit something but it's not selectable, clear hover
                ClearHover();
            }
        }
        else
        {
            // Didn't hit anything, clear hover
            ClearHover();
        }
    }

    private void ClearHover()
    {
        if (currentHoveredObject != null)
        {
            currentHoveredObject.SetHovered(false);
            currentHoveredObject = null;
        }
    }

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
