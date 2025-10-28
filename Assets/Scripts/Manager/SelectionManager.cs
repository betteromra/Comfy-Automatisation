using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    [SerializeField] private LayerMask selectableLayers = -1;

    private HashSet<Renderer> selectedRenderers = new HashSet<Renderer>();
    private HashSet<GameObject> selectedObjects = new HashSet<GameObject>();

    public event System.Action<HashSet<Renderer>> OnSelectionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
    }

    private void HandleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayers))
        {
            ToggleSelection(hit.collider.gameObject);
        }
        else
        {
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
}