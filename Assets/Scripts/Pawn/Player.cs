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
    private HashSet<GameObject> selectedObjects = new HashSet<GameObject>();

    public event System.Action<HashSet<Renderer>> OnSelectionChanged;

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
            Selectable selectable = hit.collider.GetComponent<Selectable>();
            if (selectable != null)
            {
                if (isMultiSelect)
                {
                    ToggleSelection(hit.collider.gameObject);
                }
                else
                {
                    // Single selection mode - clear others first
                    if (!IsSelected(hit.collider.gameObject))
                    {
                        ClearSelection();
                        SelectObject(hit.collider.gameObject);
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

    public void ToggleSelection(GameObject obj)
    {
        if (selectedObjects.Contains(obj))
        {
            DeselectObject(obj);
        }
        else
        {
            SelectObject(obj);
        }

        OnSelectionChanged?.Invoke(selectedRenderers);
    }

    public void SelectObject(GameObject obj)
    {
        if (selectedObjects.Contains(obj)) return;

        selectedObjects.Add(obj);

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            selectedRenderers.Add(renderer);
        }

        OnSelectionChanged?.Invoke(selectedRenderers);
    }

    public void DeselectObject(GameObject obj)
    {
        if (!selectedObjects.Contains(obj)) return;

        selectedObjects.Remove(obj);

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
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
    public HashSet<GameObject> GetSelectedObjects() => selectedObjects;

    public bool IsSelected(GameObject obj) => selectedObjects.Contains(obj);

    #endregion

    #region Unity

    void Update()
    {
        // Clear selection with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();
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
