using System;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayableCharacterManager : MonoBehaviour
{
    [Tooltip("It's weird but NPC needs to be clickable layer, else the ray travels through NPC and hits ground")]
    [SerializeField] private LayerMask clickableLayers = -1;
    [SerializeField] private Npc[] _startNpcs;
    [SerializeField] private NpcSO[] _npcsSO;
    public NpcSO[] npcsSO { get => _npcsSO; }
    [SerializeField] Transform _npcsParent;
    private List<Npc> _npcs = new();
    public List<Npc> npcs { get => _npcs; }
    private List<Npc> _currentSelectedNPCs = new();
    public event Action onNpcCreated;
    public event Action onNpcDeleted;

    Player _player;

    void Awake()
    {
        _player = GameManager.instance.player;
    }
    void Start()
    {
        foreach (Npc startNpc in _startNpcs)
        {
            _npcs.Add(startNpc);

            startNpc.OnSelfSelected += HandleNPCSelected;
            onNpcCreated?.Invoke();
        }
    }

    void OnEnable()
    {
        _player.onPressedSelect += HandleClick;
        _player.onPressedDeselect += HandleDeselect;
    }

    void OnDisable()
    {
        _player.onPressedSelect -= HandleClick;
        _player.onPressedDeselect -= HandleDeselect;
        foreach (Npc npc in _npcs)
        {
            npc.OnSelfSelected -= HandleNPCSelected;
        }
    }

    public void InstantiateNewNPC(NpcSO npcSO, Vector3 position)
    {
        Npc newNPC = Instantiate(npcSO.prefab, position, Quaternion.identity, _npcsParent).GetComponent<Npc>();

        _npcs.Add(newNPC);

        newNPC.OnSelfSelected += HandleNPCSelected;
        onNpcCreated?.Invoke();
    }

    private void HandleNPCSelected(Npc npc, bool isSelected)
    {
        if (isSelected)
        {
            if (!_currentSelectedNPCs.Contains(npc))
                _currentSelectedNPCs.Add(npc);
        }
        else
            _currentSelectedNPCs.Remove(npc);
    }

    private void HandleDeselect()
    {
        _currentSelectedNPCs.Clear();
    }

    private void HandleClick()
    {
        if (_currentSelectedNPCs.Count <= 0)
            return;

        Camera playerCamera = GameManager.instance.cameraManager.mainCamera;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayers))
        {
            //Needed so that it doesn't instaselect the ground behind the NPC
            if (hit.collider.GetComponentInParent<Npc>() != null)
                return;

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Buildings"))
                return;

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                foreach (Npc npc in _currentSelectedNPCs)
                {
                    npc.GoToPositionWithoutNode(hit.point);
                }

                return;
            }

            BuildingNode clickedBuildingNode = hit.collider.gameObject.GetComponent<BuildingNode>();
            BuildingNode lastSelectedNode = null;

            // we check if the selection is the same lastNode, If not then we remove the link when chosing a new selected
            bool firstLastSelectedNode = true;
            foreach (Npc npc in _currentSelectedNPCs)
            {
                if (npc.buildingNodesList.Count == 0)
                {
                    lastSelectedNode = null;
                    break;
                }

                if (firstLastSelectedNode)
                {
                    lastSelectedNode = npc.buildingNodesList[^1];
                }
                if (lastSelectedNode != npc.buildingNodesList[^1])
                {
                    lastSelectedNode = null;
                    break;
                }
            }

            if (lastSelectedNode == null)
            {
                foreach (Npc npc in _currentSelectedNPCs)
                {
                    npc.ResetAllPath();
                    npc.AddBuildingNode(clickedBuildingNode);
                }

                return;
            }

            foreach (Npc npc in _currentSelectedNPCs)
            {
                if (npc.buildingNodesList.Contains(clickedBuildingNode)) npc.RemoveBuildingNode(clickedBuildingNode);
                else npc.AddBuildingNode(clickedBuildingNode);
            }
        }
    }
}
