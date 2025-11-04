using System.Collections.Generic;
using UnityEngine;

public class NonPlayableCharacterManager : MonoBehaviour
{
    [Tooltip("It's weird but NPC needs to be clickable layer, else the ray travels through NPC and hits ground")]
    [SerializeField] private LayerMask clickableLayers = -1;
    [SerializeField] private NpcSO[] _npcsSO;
    [SerializeField] private Color _inputSelectedColor;
    [SerializeField] private Color _outputSelectedColor;
    public NpcSO[] npcsSO { get => _npcsSO; }
    [SerializeField] Transform _npcsParent;
    private List<Npc> _npcs = new();
    private List<Npc> _currentSelectedNPCs = new();
    private List<NodeLink> _linkedNodeList = new();

    private GameObject _lastSelected;

    private Color _lastColour;

    void Start()
    {
        for (int i = 0; i < 15; i++) //TEMP
        {
            InstantiateNewNPC(_npcsSO[0], new(-60 + 2 * i, 6, 1.5f));
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
        Npc newNPC = Instantiate(npcSO.prefab, position, Quaternion.identity, _npcsParent).GetComponent<Npc>();

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
        _lastSelected = null;
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
                ResetLastSelected();
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
                    _lastSelected = clicked;

                    SpriteRenderer sr = clicked.GetComponentInChildren<SpriteRenderer>();
                    _lastColour = sr.color;
                    if (_lastSelected.TryGetComponent<OutputNode>(out _))
                        sr.color = _outputSelectedColor;
                    else
                        sr.color = _inputSelectedColor;

                    return;
                }

                //Stops binding output node to outputnode/input to input
                if (_lastSelected.TryGetComponent<OutputNode>(out _) == clicked.TryGetComponent<OutputNode>(out _))
                {
                    ResetLastSelected();
                    return; //TODO alert user to error
                }

                NodeLink nodeLink = new(_lastSelected, clicked);
                //This check is/should be order independent.
                if (!_linkedNodeList.Exists(l => l == nodeLink))
                {
                    bool isOutputNode = _lastSelected.TryGetComponent<OutputNode>(out _);

                    //This is needed so that we in NonPlayableCharacterManager store the node link in only one direction
                    GameObject nodeA = isOutputNode ? _lastSelected : clicked;
                    GameObject nodeB = isOutputNode ? clicked : _lastSelected;

                    _linkedNodeList.Add(new NodeLink(nodeA, nodeB));
                }

                foreach (Npc npc in _currentSelectedNPCs)
                {
                    //NodeA and NodeB should already be correctly assigned here
                    NodeLink existing = _linkedNodeList.Find(l => l.GetHashCode() == nodeLink.GetHashCode());
                    OutputNode outputNode = existing.NodeA.GetComponent<OutputNode>();
                    InputNode inputNode = existing.NodeB.GetComponent<InputNode>();

                    bool isAllowedToLinkOutput = outputNode.Link(inputNode);
                    bool isAllowedToLinkInput = inputNode.Link(outputNode);

                    if (isAllowedToLinkInput && isAllowedToLinkOutput)
                        npc.LinkNode(nodeLink);
                    else
                    {
                        outputNode.Unlink(inputNode);
                        inputNode.Unlink(outputNode);
                    }
                }

                ResetLastSelected();
            }
        }
    }

    private void ResetLastSelected()
    {
        if (_lastSelected != null)
        {
            var renderer = _lastSelected.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
                renderer.color = _lastColour;
        }
        _lastSelected = null;
    }
}
