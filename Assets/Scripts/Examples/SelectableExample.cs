using UnityEngine;

/// <summary>
/// Example script showing how to programmatically configure Selectable for parent prefabs
/// </summary>
public class SelectableExample : MonoBehaviour
{
    void Start()
    {
        DemonstrateUsage();
    }

    void DemonstrateUsage()
    {
        // Example 1: Simple object - default behavior
        // Just add the Selectable component and it will use all renderers
        
        // Example 2: Parent prefab - use all child renderers
        // Set isParent to true, leave specificRenderers empty
        
        // Example 3: Parent prefab - use only specific child renderers
        // Set isParent to true, populate specificRenderers array
    }

    // Method to configure a selectable programmatically
    public void ConfigureSelectableAtRuntime(GameObject targetObject)
    {
        Selectable selectable = targetObject.GetComponent<Selectable>();
        if (selectable == null)
        {
            selectable = targetObject.AddComponent<Selectable>();
        }

        // Enable parent mode
        selectable.SetIsParent(true);

        // Get all renderers in children
        Renderer[] allRenderers = targetObject.GetComponentsInChildren<Renderer>();

        // Example: Only select renderers with "Selectable" in their name
        System.Collections.Generic.List<Renderer> selectedRenderers = new System.Collections.Generic.List<Renderer>();
        
        foreach (var renderer in allRenderers)
        {
            if (renderer.gameObject.name.Contains("Selectable"))
            {
                selectedRenderers.Add(renderer);
            }
        }

        // Set the specific renderers
        selectable.SetSpecificRenderers(selectedRenderers.ToArray());

        Debug.Log($"Configured {targetObject.name} with {selectedRenderers.Count} specific renderers for outlining");
    }

    // Method to add a single renderer to an existing selectable
    public void AddRendererToSelectable(Selectable selectable, Renderer renderer)
    {
        if (selectable != null && renderer != null)
        {
            selectable.AddRenderer(renderer);
            Debug.Log($"Added renderer {renderer.gameObject.name} to {selectable.gameObject.name}");
        }
    }

    // Method to remove a renderer from a selectable
    public void RemoveRendererFromSelectable(Selectable selectable, Renderer renderer)
    {
        if (selectable != null && renderer != null)
        {
            selectable.RemoveRenderer(renderer);
            Debug.Log($"Removed renderer {renderer.gameObject.name} from {selectable.gameObject.name}");
        }
    }
}
