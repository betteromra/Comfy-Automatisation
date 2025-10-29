using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SelectableObjects : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color hoverColor = Color.green;
    [SerializeField] private float outlineWidth = 0.1f;

    [Header("Selection Settings")]
    [SerializeField] private bool useOutline = true;
    [SerializeField] private bool useColorTint = false;

    private bool isSelected = false;
    private bool isHovered = false;
    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private Color[] originalColors;
    private Player playerController;

    public bool IsSelected
    {
        get => isSelected;
        private set => isSelected = value;
    }

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        playerController = FindAnyObjectByType<Player>();

        if (useColorTint)
        {
            // Store original colors
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].GetPropertyBlock(propertyBlock);
                    originalColors[i] = propertyBlock.GetColor("_Color");
                }
            }
        }
    }

    private void OnEnable()
    {
        if (playerController == null)
            playerController = FindAnyObjectByType<Player>();
            
        if (playerController != null)
        {
            playerController.OnSelectionChanged += OnSelectionChanged;
        }
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.OnSelectionChanged -= OnSelectionChanged;
        }
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

    private void OnMouseEnter()
    {
        if (!isSelected)
        {
            isHovered = true;
            UpdateVisualFeedback();
        }
    }

    private void OnMouseExit()
    {
        isHovered = false;
        UpdateVisualFeedback();
    }

    private void UpdateVisualFeedback()
    {
        Color targetColor = normalColor;

        if (isSelected)
        {
            targetColor = selectedColor;
        }
        else if (isHovered)
        {
            targetColor = hoverColor;
        }

        if (useColorTint)
        {
            ApplyColorTint(targetColor);
        }

        if (useOutline)
        {
            ApplyOutline(isSelected || isHovered, targetColor);
        }
    }

    private void ApplyColorTint(Color color)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", color);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }

    private void ApplyOutline(bool enable, Color color)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.GetPropertyBlock(propertyBlock);

                if (enable)
                {
                    propertyBlock.SetFloat("_OutlineWidth", outlineWidth);
                    propertyBlock.SetColor("_OutlineColor", color);
                }
                else
                {
                    propertyBlock.SetFloat("_OutlineWidth", 0f);
                }

                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }

    public void ForceSelect()
    {
        if (playerController == null)
            playerController = FindAnyObjectByType<Player>();
            
        if (playerController != null)
        {
            playerController.SelectObject(gameObject);
        }
    }

    public void ForceDeselect()
    {
        if (playerController == null)
            playerController = FindAnyObjectByType<Player>();
            
        if (playerController != null)
        {
            playerController.DeselectObject(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Clean up if this object was selected
        if (isSelected && playerController != null)
        {
            playerController.DeselectObject(gameObject);
        }
    }

    // Editor helper to visualize selection bounds
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = selectedColor;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
