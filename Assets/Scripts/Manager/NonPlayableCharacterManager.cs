using System.Collections.Generic;
using UnityEngine;

public class NonPlayableCharacterManager : MonoBehaviour
{
    [Tooltip("It's weird but NPC needs to be clickable layer, else the ray travels through NPC and hits ground")]
    [SerializeField] private LayerMask clickableLayers = -1;
    [SerializeField] private NpcSO _basicNpcSO;

    private List<Npc> _npcs = new();
    private List<Npc> _currentSelectedNPCs = new();
    private List<NodeLink> _linkedNodeList = new();

    private GameObject _lastSelected;

    void Start()
    {
        for (int i = 0; i < 15; i++) //TEMP
        {
            InstantiateNewNPC(_basicNpcSO, new(-60 + 2 * i, 6, 1.5f));
        }
    }

    void OnEnable()
    {
        GameManager.instance.player.onPressedSelect += HandleClick;
        GameManager.instance.player.onPressedDeselect += HandleDeselect;
    }

    void OnDisable()
    {
        GameManager.instance.player.onPressedSelect -= HandleClick;
        GameManager.instance.player.onPressedDeselect -= HandleDeselect;
    }

    public void InstantiateNewNPC(NpcSO npcSO, Vector3 position)
    {
        Npc newNPC = Instantiate(npcSO.prefab, position, Quaternion.identity).GetComponent<Npc>();

        _npcs.Add(newNPC);

        newNPC.OnSelfSelected += HandleNPCSelected;
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
        Debug.LogWarning("NPC deselected");
    }

    private void HandleClick()
    {
        if (_currentSelectedNPCs.Count <= 0)
            return;

        Camera playerCamera = GameManager.instance.cameraManager.mainCamera;
        if (playerCamera == null)
            playerCamera = Camera.main;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayers))
        {
            //Needed so that it doesn't instaselect the ground behind the NPC
            if (hit.collider.GetComponentInParent<Npc>() != null)
                return;

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _lastSelected = null;
                foreach (Npc npc in _currentSelectedNPCs)
                {
                    npc.Link(hit.point);
                }
            }
            else
            {
                GameObject clicked = hit.collider.gameObject;

                if (_lastSelected == null)
                {
                    _lastSelected = hit.collider.gameObject;
                    return;
                }

                NodeLink nodeLink = new(_lastSelected, clicked);
                bool exists = _linkedNodeList.Exists(l => l == nodeLink);
                if (!exists)
                {
                    _linkedNodeList.Add(new NodeLink(_lastSelected, clicked));
                }

                foreach (Npc npc in _currentSelectedNPCs)
                {
                    npc.LinkNode(nodeLink);
                }

                _lastSelected = null;
            }
        }
    }
}
