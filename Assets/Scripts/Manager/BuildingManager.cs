using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] RessourceNode[] _ressourcesNode;
    public RessourceNode[] ressourcesNode { get => _ressourcesNode; set => _ressourcesNode = value; }
    [SerializeField] StorageBuilding _barn;
    public StorageBuilding barn { get => _barn; set => _barn = value; }
    List<Building> _buildings = new List<Building>();
    public List<Building> buildings { get => _buildings; }
    BuildingSO _buildingSOToolBarSelected;
    public BuildingSO buildingSOToolBarSelected
    {
        get => _buildingSOToolBarSelected;
        set
        {
            if (_buildingSOToolBarSelected == value) return;
            _isBuilding = true;
            _buildingSOToolBarSelected = value;
            OnBuildingSelected();
        }
    }

    bool _isBuilding = false;
    public bool isBuilding { get => _isBuilding; }

    [SerializeField] LayerMask _blockingBuildingLayers;
    [SerializeField] LayerMask _placingBuildingLayers;
    [SerializeField] Transform _buildingsParent;
    Dictionary<BuildingSO, Building> _ghosts = new Dictionary<BuildingSO, Building>();
    Building _ghost;
    Coroutine _showingGhost;
    public event Action onBuildingCreated;

    void Awake()
    {
        foreach (RessourceNode ressourceNode in _ressourcesNode)
        {
            _buildings.Add(ressourceNode);
        }
        _buildings.Add(_barn);
    }

    private void OnBuildingSelected()
    {
        if (_ghosts.ContainsKey(_buildingSOToolBarSelected))
        {
            _ghost = _ghosts[_buildingSOToolBarSelected];
            _ghost.gameObject.SetActive(true);
        }
        else
        {
            _ghost = Instantiate(_buildingSOToolBarSelected.prefab, Vector3.zero, Quaternion.identity, _buildingsParent).GetComponent<Building>();
            _ghosts.Add(_buildingSOToolBarSelected, _ghost);
            _ghost.enabled = false;
        }

        if (_showingGhost == null)
        {
            _barn.inventory.onContentChange += CancelIfCanNotBuild;
            _showingGhost = StartCoroutine(ShowGhostBuilding());
        }
    }

    System.Collections.IEnumerator ShowGhostBuilding()
    {
        while (true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _placingBuildingLayers))
            {
                Vector3 hitPosition = hit.point;

                hitPosition.x = Mathf.Round(hitPosition.x);
                hitPosition.z = Mathf.Round(hitPosition.z);

                _ghost.transform.position = hitPosition;
                if (CheckPlacementCollision())
                {

                }
                else
                {

                }
            }
            yield return null;
        }
    }

    private bool CheckPlacementCollision()
    {
        Collider[] collisions = Physics.OverlapBox(transform.position, _ghost.boxCollider.size, Quaternion.identity, _blockingBuildingLayers);

        return collisions.Length == 0;
    }

    void CancelIfCanNotBuild()
    {
        if (!CanBuild(_buildingSOToolBarSelected)) CancelBuild();
    }

    public bool CanBuild(BuildingSO buildingSO)
    {
        return _barn.inventory.ContainsAmount(buildingSO.recipe.ingredientsInput);
    }

    void CancelBuild()
    {
        if (!isBuilding) return;
        _isBuilding = false;
        StopCoroutine(_showingGhost);
        _showingGhost = null;
        _ghost.gameObject.SetActive(false);
        _ghost = null;
        _buildingSOToolBarSelected = null;
        _barn.inventory.onContentChange -= CancelIfCanNotBuild;
    }

    void CreateBuilding()
    {
        if (!isBuilding) return;

        if (CheckPlacementCollision())
        {
            _buildings.Add(Instantiate(_buildingSOToolBarSelected.prefab, _ghost.transform.position, _ghost.transform.rotation, _buildingsParent).GetComponent<Building>());
            onBuildingCreated?.Invoke();
            CancelBuild();
        }
    }

    void OnEnable()
    {
        GameManager.instance.player.onPressedBuild += CreateBuilding;
        GameManager.instance.player.onPressedCancelBuild += CancelBuild;
    }

    void OnDisable()
    {
        GameManager.instance.player.onPressedBuild -= CreateBuilding;
        GameManager.instance.player.onPressedCancelBuild -= CancelBuild;
        _barn.inventory.onContentChange -= CancelIfCanNotBuild;
    }
}
