using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private LayerMask selectableLayers = -1;
    private HashSet<Selectable> _selectedObjects = new HashSet<Selectable>();
    public HashSet<Selectable> selectedObjects { get => _selectedObjects; }
    private HashSet<Renderer> selectedRenderers = new HashSet<Renderer>();
    private Selectable currentHoveredObject = null;
    public event Action<HashSet<Renderer>> OnSelectionChanged;
    BuildingManager buildingManager;
    UserInterfaceManager userInterfaceManager;

    #region Selection Logic

    private void HandleSelection()
    {
        // if we are building don t select
        // if we are in a building UI don t select
        if (buildingManager.isBuilding || userInterfaceManager.isBuildingUIOpen) return;

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
    void Awake()
    {
        buildingManager = GameManager.instance.buildingManager;
        userInterfaceManager = GameManager.instance.userInterfaceManager;
    }

    void Update()
    {
        // Handle hover detection
        HandleHover();
    }

    void OnEnable()
    {
        GameManager.instance.player.onPressedSelect += HandleSelection;
        GameManager.instance.player.onPressedDeselect += ClearSelection;
    }

    void OnDisable()
    {
        GameManager.instance.player.onPressedSelect -= HandleSelection;
        GameManager.instance.player.onPressedDeselect -= ClearSelection;
    }

    #endregion
    #region Hover
    private void HandleHover()
    {
        // if we are building don t hover
        // if we are in a building UI don t hover
        if (buildingManager.isBuilding || userInterfaceManager.isBuildingUIOpen) return;

        // Don't do hover detection if camera is being moved
        if (GameManager.instance.player.enableCameraMouseMove) return;

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
    #endregion
}