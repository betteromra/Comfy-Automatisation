using UnityEngine;

/// <summary>
/// Debug helper to verify outline system is working.
/// Attach to a GameObject and it will automatically select/deselect on keypress.
/// </summary>
public class OutlineDebugHelper : MonoBehaviour
{
    private SelectableObjects selectable;

    void Start()
    {
        // Add SelectableObjects component if not present
        selectable = GetComponent<SelectableObjects>();
        if (selectable == null)
        {
            selectable = gameObject.AddComponent<SelectableObjects>();
            Debug.Log($"Added SelectableObjects to {gameObject.name}");
        }

        // Auto-select on start for testing
        if (SelectionManager.Instance != null)
        {
            selectable.ForceSelect();
            Debug.Log($"<color=green>OutlineDebugHelper: {gameObject.name} is now SELECTED for outline rendering</color>");
        }
        else
        {
            Debug.LogWarning($"<color=orange>OutlineDebugHelper: SelectionManager not found in scene!</color>");
        }

        // Check if object has a renderer
        var renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError($"<color=red>OutlineDebugHelper: {gameObject.name} has NO RENDERER! Outline will not work!</color>");
        }
        else
        {
            Debug.Log($"OutlineDebugHelper: {gameObject.name} has renderer: {renderer.GetType().Name}");
        }
    }

    void Update()
    {
        // Toggle selection with Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (SelectionManager.Instance != null)
            {
                if (selectable.IsSelected)
                {
                    selectable.ForceDeselect();
                    Debug.Log($"<color=yellow>OutlineDebugHelper: {gameObject.name} DESELECTED</color>");
                }
                else
                {
                    selectable.ForceSelect();
                    Debug.Log($"<color=yellow>OutlineDebugHelper: {gameObject.name} SELECTED</color>");
                }
            }
        }
    }
}
