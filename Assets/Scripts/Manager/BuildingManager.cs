using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] RessourceNode[] _ressourcesNode;
    public RessourceNode[] ressourcesNode { get => _ressourcesNode; set => _ressourcesNode = value; }
    [SerializeField] StorageBuilding _barn;
    public StorageBuilding barn { get => _barn; set => _barn = value; }
    [SerializeField] Transform _npcSpawnPoint;
    [SerializeField] StorageBuilding _merchantCaptain;
    [SerializeField] Material _buildPreviewMaterial;
    Material _ghostBuildMaterial;
    List<Building> _buildings = new List<Building>();
    public List<Building> buildings { get => _buildings; }
    List<Building> _buildingsCreated = new List<Building>();
    public List<Building> buildingsCreated { get => _buildingsCreated; }
    BuildingSO _buildingSOToolBarSelected;
    public BuildingSO buildingSOToolBarSelected
    {
        get => _buildingSOToolBarSelected;
        set
        {
            if (GameManager.instance.userInterfaceManager.isBuildingUIOpen) return;
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
    [SerializeField] float _ghostMoveSmoothing;
    [SerializeField] float _ghostRotationSmoothing;
    Vector3 _targetGhostPosition = Vector3.zero;
    Quaternion _targetGhostRotation = Quaternion.identity;
    Dictionary<BuildingSO, Building> _ghosts = new Dictionary<BuildingSO, Building>();
    Building _ghost;
    Coroutine _showingGhost;
    Player _player;
    public event Action onBuildingCreated;
    public event Action onBuildingDeleted;

    void Awake()
    {
        _player = GameManager.instance.player;
        foreach (RessourceNode ressourceNode in _ressourcesNode)
        {
            _buildings.Add(ressourceNode);
        }
        _buildings.Add(_barn);
        _buildings.Add(_merchantCaptain);
        _ghostBuildMaterial = new Material(_buildPreviewMaterial);
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

            MeshRenderer[] meshRenderers = _ghost.meshRenderer.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                MeshRenderer meshRenderer = meshRenderers[i];
                Material[] materials = new Material[meshRenderer.materials.Length];

                for (int j = 0; j < meshRenderer.materials.Length; j++)
                {
                    materials[j] = _ghostBuildMaterial;
                }

                meshRenderer.materials = materials;
            }
        }

        if (_showingGhost == null)
        {
            _barn.inventory.onContentChange += CancelIfCanNotBuild;
            _showingGhost = StartCoroutine(ShowGhostBuilding());
        }
    }

    IEnumerator ShowGhostBuilding()
    {
        while (true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _placingBuildingLayers))
            {
                MoveBuilding(hit.point);

                // rotation

                RotateBuilding(_player.rotateBuildInput);

                _ghostBuildMaterial.SetFloat("_IsValidPlacement", CheckPlacementCollision() ? 1f : 0f);
            }
            yield return null;
        }
    }
    void MoveBuilding(Vector3 position)
    {
        _targetGhostPosition = position;

        _targetGhostPosition.x = Mathf.Round(_targetGhostPosition.x);
        _targetGhostPosition.z = Mathf.Round(_targetGhostPosition.z);

        Vector3 smoothPosition = _targetGhostPosition;
        smoothPosition.x = Mathf.Lerp(_ghost.transform.position.x, _targetGhostPosition.x, Time.deltaTime * _ghostMoveSmoothing);
        smoothPosition.z = Mathf.Lerp(_ghost.transform.position.z, _targetGhostPosition.z, Time.deltaTime * _ghostMoveSmoothing);

        _ghost.transform.position = smoothPosition;
    }
    void RotateBuilding(float rotationMotion)
    {
        if (Quaternion.Angle(_ghost.transform.rotation, _targetGhostRotation) > .1f) rotationMotion = 0;
        _targetGhostRotation *= Quaternion.Euler(0, rotationMotion * 90, 0);

        Quaternion smoothRotation = Quaternion.RotateTowards(_ghost.transform.rotation, _targetGhostRotation, Time.deltaTime * _ghostRotationSmoothing);

        _ghost.transform.rotation = smoothRotation;
    }

    private bool CheckPlacementCollision()
    {
        Collider[] collisions = Physics.OverlapBox(_targetGhostPosition, _ghost.boxCollider.size * .5f, _targetGhostRotation, _blockingBuildingLayers);

        return collisions.Length == 0;
    }

    void CancelIfCanNotBuild()
    {
        if (!CanBuild(_buildingSOToolBarSelected)) CancelBuild();
    }

    public bool CanBuild(BuildingSO buildingSO)
    {
        return _barn.inventory.ContainsAmount(buildingSO.ressourceSO.recipe.ingredientsInput);
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
        _player.enableRotateBuild = false;
    }

    void CreateBuilding()
    {
        if (!isBuilding) return;

        if (CheckPlacementCollision())
        {
            Building building = Instantiate(_buildingSOToolBarSelected.prefab, _targetGhostPosition, _targetGhostRotation, _buildingsParent).GetComponent<Building>();
            _buildings.Add(building);
            _buildingsCreated.Add(building);
            onBuildingCreated?.Invoke();
            CancelBuild();
        }
    }

    void SpawnNpcIfPossible()
    {
        int npcCreated = 0;
        foreach (NpcSO npc in GameManager.instance.nonPlayableCharacter.npcsSO)
        {
            int npcRemoved = _barn.inventory.Remove(new RessourceAndAmount(npc.ressourceSO, int.MaxValue), false);
            npcCreated += npcRemoved;
            for (int i = 0; i < npcRemoved; i++)
            {
                GameManager.instance.nonPlayableCharacter.InstantiateNewNPC(npc, _npcSpawnPoint.position);
            }
        }
        if (npcCreated != 0) _barn.inventory.ConetentChanged();
    }

    void OnEnable()
    {
        _player.onPressedBuild += CreateBuilding;
        _player.onPressedCancelBuild += CancelBuild;
        _barn.inventory.onContentChange += SpawnNpcIfPossible;
    }

    void OnDisable()
    {
        _player.onPressedBuild -= CreateBuilding;
        _player.onPressedCancelBuild -= CancelBuild;
        _barn.inventory.onContentChange -= CancelIfCanNotBuild;
        _barn.inventory.onContentChange -= SpawnNpcIfPossible;
    }
}
