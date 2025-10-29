using System.Collections.Generic;
using UnityEngine;

public class BuildingPreviewSystem : MonoBehaviour
{
    [Header("Preview Settings")]
    [SerializeField] private Material previewMaterial;
    [SerializeField] private float previewHeight = 2f;
    [SerializeField] private Vector3 previewOffset = new Vector3(0, 0, 2);
    [SerializeField] private bool followMouse = true;
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private Camera playerCamera;

    [Header("Preview Colors")]
    [SerializeField] private Color validColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.5f);

    private GameObject currentPreview;
    private Selectable selectedSource;
    private Player playerController;
    private Material previewMaterialInstance;

    void Start()
    {
        playerController = GetComponent<Player>();
        if (playerController != null)
        {
            playerController.OnSelectionChanged += OnSelectionChanged;
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        // Create material instance if preview material exists
        if (previewMaterial != null)
        {
            previewMaterialInstance = new Material(previewMaterial);
        }
    }

    void Update()
    {
        if (currentPreview != null)
        {
            UpdatePreviewPosition();
        }
    }

    private void OnSelectionChanged(HashSet<Renderer> selectedRenderers)
    {
        // Clear existing preview
        ClearPreview();

        // Get the selected objects
        var selectedObjects = playerController.GetSelectedObjects();
        
        if (selectedObjects.Count == 1)
        {
            // Get the first (and only) selected object
            foreach (var selectedObject in selectedObjects)
            {
                selectedSource = selectedObject;
                CreatePreview(selectedObject.gameObject);
                break;
            }
        }
        else
        {
            selectedSource = null;
        }
    }

    private void CreatePreview(GameObject sourceObject)
    {
        if (sourceObject == null) return;

        // Create a preview copy
        currentPreview = new GameObject($"Preview_{sourceObject.name}");
        currentPreview.transform.position = sourceObject.transform.position + previewOffset;
        currentPreview.transform.rotation = sourceObject.transform.rotation;
        currentPreview.transform.localScale = sourceObject.transform.localScale;

        // Copy mesh renderers
        CopyMeshRenderersRecursive(sourceObject.transform, currentPreview.transform);

        // Disable colliders on preview
        foreach (var collider in currentPreview.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        // Apply preview material to all renderers
        ApplyPreviewMaterial(currentPreview);
    }

    private void CopyMeshRenderersRecursive(Transform source, Transform target)
    {
        // Copy MeshFilter and MeshRenderer from source to target
        MeshFilter sourceMeshFilter = source.GetComponent<MeshFilter>();
        MeshRenderer sourceMeshRenderer = source.GetComponent<MeshRenderer>();

        if (sourceMeshFilter != null && sourceMeshRenderer != null)
        {
            MeshFilter targetMeshFilter = target.gameObject.AddComponent<MeshFilter>();
            MeshRenderer targetMeshRenderer = target.gameObject.AddComponent<MeshRenderer>();

            targetMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;
        }

        // Recursively copy children
        for (int i = 0; i < source.childCount; i++)
        {
            Transform sourceChild = source.GetChild(i);
            
            GameObject targetChild = new GameObject(sourceChild.name);
            targetChild.transform.SetParent(target);
            targetChild.transform.localPosition = sourceChild.localPosition;
            targetChild.transform.localRotation = sourceChild.localRotation;
            targetChild.transform.localScale = sourceChild.localScale;

            CopyMeshRenderersRecursive(sourceChild, targetChild.transform);
        }
    }

    private void ApplyPreviewMaterial(GameObject previewObject)
    {
        MeshRenderer[] renderers = previewObject.GetComponentsInChildren<MeshRenderer>();
        
        foreach (var renderer in renderers)
        {
            Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
            
            if (previewMaterialInstance != null)
            {
                // Use the preview material
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = previewMaterialInstance;
                }
            }
            else
            {
                // Fallback: create transparent versions of original materials
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = new Material(renderer.sharedMaterials[i]);
                    newMaterials[i].SetFloat("_Surface", 1); // Transparent
                    newMaterials[i].SetFloat("_Blend", 0); // Alpha blend
                    newMaterials[i].SetColor("_BaseColor", validColor);
                    newMaterials[i].renderQueue = 3000; // Transparent queue
                }
            }
            
            renderer.materials = newMaterials;
        }

        // Set valid color initially
        SetPreviewValid(true);
    }

    private void UpdatePreviewPosition()
    {
        if (!followMouse || playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            currentPreview.transform.position = hit.point + Vector3.up * previewHeight;
            SetPreviewValid(true);
        }
        else
        {
            // If no ground hit, place at fixed distance from camera
            currentPreview.transform.position = ray.GetPoint(10f);
            SetPreviewValid(false);
        }
    }

    private void SetPreviewValid(bool isValid)
    {
        if (previewMaterialInstance == null) return;

        // Update material properties if using the building preview shader
        if (previewMaterialInstance.HasProperty("_IsValidPlacement"))
        {
            previewMaterialInstance.SetFloat("_IsValidPlacement", isValid ? 1f : 0f);
        }

        // Update color for other materials
        Color targetColor = isValid ? validColor : invalidColor;
        if (previewMaterialInstance.HasProperty("_Color"))
        {
            previewMaterialInstance.SetColor("_Color", targetColor);
        }
        if (previewMaterialInstance.HasProperty("_BaseColor"))
        {
            previewMaterialInstance.SetColor("_BaseColor", targetColor);
        }
    }

    private void ClearPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnSelectionChanged -= OnSelectionChanged;
        }

        ClearPreview();

        if (previewMaterialInstance != null)
        {
            Destroy(previewMaterialInstance);
        }
    }

    // Public methods for external control
    public void SetFollowMouse(bool follow)
    {
        followMouse = follow;
    }

    public void SetPreviewHeight(float height)
    {
        previewHeight = height;
    }

    public GameObject GetCurrentPreview()
    {
        return currentPreview;
    }
}
