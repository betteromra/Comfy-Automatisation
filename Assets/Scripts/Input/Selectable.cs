using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float outlineWidth = 0.1f;
    [SerializeField] private Color hoverTint = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private float hoverIntensity = 0.3f;

    [Header("Selection Settings")]
    [SerializeField] private bool useOutline = true;

    [Header("Parent/Prefab Settings")]
    [SerializeField] private bool isParent = false;
    [Tooltip("If empty and isParent is true, all child renderers will be used. Otherwise, only specified renderers.")]
    [SerializeField] private Renderer[] specificRenderers;

    private bool isSelected = false;
    private bool isHovered = false;
    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private SelectionManager selectionManager;
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    public bool IsSelected
    {
        get => isSelected;
        private set => isSelected = value;
    }

    private void Awake()
    {
        // Determine which renderers to use based on settings
        if (isParent && specificRenderers != null && specificRenderers.Length > 0)
        {
            // Use only the specified renderers
            renderers = specificRenderers;
        }
        else if (isParent)
        {
            // Use all child renderers
            renderers = GetComponentsInChildren<Renderer>();
        }
        else
        {
            // Default: use renderers on this object and its children
            renderers = GetComponentsInChildren<Renderer>();
        }

        propertyBlock = new MaterialPropertyBlock();
        selectionManager = GameManager.instance.selectionManager;

        // Store original colors for each renderer
        foreach (var renderer in renderers)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                if (renderer.sharedMaterial.HasProperty("_BaseColor"))
                {
                    originalColors[renderer] = renderer.sharedMaterial.GetColor("_BaseColor");
                }
                else if (renderer.sharedMaterial.HasProperty("_Color"))
                {
                    originalColors[renderer] = renderer.sharedMaterial.GetColor("_Color");
                }
            }
        }
    }

    private void OnEnable()
    {
        selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        // Clear hover effect when disabled
        if (isHovered)
        {
            isHovered = false;
            ApplyHoverEffect(false);
        }


        selectionManager.OnSelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(System.Collections.Generic.HashSet<Renderer> selectedRenderers)
    {
        // Check if any of our renderers are in the selected set
        bool wasSelected = isSelected;
        isSelected = false;

        foreach (Renderer renderer in renderers)
        {
            if (selectedRenderers.Contains(renderer))
            {
                isSelected = true;
                break;
            }
        }

        // Update visual feedback if selection state changed
        if (wasSelected != isSelected)
        {
            UpdateVisualFeedback();
        }
    }

    // Public method for Player to control hover state
    public void SetHovered(bool hovered)
    {
        if (isHovered != hovered)
        {
            isHovered = hovered;
            Debug.Log($"Hover state changed for {gameObject.name}: {isHovered}");
            UpdateVisualFeedback();
        }
    }

    private void UpdateVisualFeedback()
    {
        // Apply hover effect - subtle color tint
        if (isHovered && !isSelected)
        {
            ApplyHoverEffect(true);
        }
        else
        {
            ApplyHoverEffect(false);
        }

        // Apply outline (managed by OutlineFeature based on selection state)
        // The outline rendering is handled by the render feature, not here
    }

    private void ApplyHoverEffect(bool enable)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                renderer.GetPropertyBlock(propertyBlock);

                if (enable)
                {
                    // Get the original color
                    Color originalColor = Color.white;
                    if (originalColors.ContainsKey(renderer))
                    {
                        originalColor = originalColors[renderer];
                    }

                    // Create hovered color
                    Color hoveredColor = Color.Lerp(originalColor, hoverTint, hoverIntensity);

                    // Apply to the appropriate property
                    if (renderer.sharedMaterial.HasProperty("_BaseColor"))
                    {
                        propertyBlock.SetColor("_BaseColor", hoveredColor);
                        //Debug.Log($"Applied hover to {renderer.gameObject.name} - Original: {originalColor}, Hovered: {hoveredColor}");
                    }
                    else if (renderer.sharedMaterial.HasProperty("_Color"))
                    {
                        propertyBlock.SetColor("_Color", hoveredColor);
                        //Debug.Log($"Applied hover to {renderer.gameObject.name} - Original: {originalColor}, Hovered: {hoveredColor}");
                    }

                    renderer.SetPropertyBlock(propertyBlock);
                }
                else
                {
                    // Clear hover effect - restore original colors
                    if (originalColors.ContainsKey(renderer))
                    {
                        Color originalColor = originalColors[renderer];

                        if (renderer.sharedMaterial.HasProperty("_BaseColor"))
                        {
                            propertyBlock.SetColor("_BaseColor", originalColor);
                        }
                        else if (renderer.sharedMaterial.HasProperty("_Color"))
                        {
                            propertyBlock.SetColor("_Color", originalColor);
                        }
                    }

                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }
    }

    public void ForceSelect()
    {
        selectionManager.SelectObject(this);
    }

    public void ForceDeselect()
    {
        selectionManager.DeselectObject(this);
    }

    private void OnDestroy()
    {
        // Clear hover effect before destroying
        if (isHovered)
        {
            ApplyHoverEffect(false);
        }

        // Clean up if this object was selected
        if (isSelected)
        {
            selectionManager.DeselectObject(this);
        }
    }

    // Editor helper to visualize selection bounds
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    // Public methods for managing renderers
    public void SetSpecificRenderers(Renderer[] newRenderers)
    {
        specificRenderers = newRenderers;
        RefreshRenderers();
    }

    public void AddRenderer(Renderer renderer)
    {
        if (specificRenderers == null)
        {
            specificRenderers = new Renderer[] { renderer };
        }
        else
        {
            System.Array.Resize(ref specificRenderers, specificRenderers.Length + 1);
            specificRenderers[specificRenderers.Length - 1] = renderer;
        }
        RefreshRenderers();
    }

    public void RemoveRenderer(Renderer renderer)
    {
        if (specificRenderers == null) return;

        var list = new System.Collections.Generic.List<Renderer>(specificRenderers);
        list.Remove(renderer);
        specificRenderers = list.ToArray();
        RefreshRenderers();
    }

    public void SetIsParent(bool parent)
    {
        isParent = parent;
        RefreshRenderers();
    }

    public void RefreshRenderers()
    {
        // Re-gather renderers based on current settings
        if (isParent && specificRenderers != null && specificRenderers.Length > 0)
        {
            renderers = specificRenderers;
        }
        else if (isParent)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }
        else
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        // Update visual feedback
        UpdateVisualFeedback();
    }

    public Renderer[] GetRenderers()
    {
        return renderers;
    }

    public Renderer[] GetSpecificRenderers()
    {
        return specificRenderers;
    }

    public bool GetIsParent()
    {
        return isParent;
    }
}
