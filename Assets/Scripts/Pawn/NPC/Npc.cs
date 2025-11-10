using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(NavMeshAgent))]
public class Npc : Pawn
{
    [SerializeField] private NpcSO _nonPlayableCharacterSO;
    public NpcSO nonPlayableCharacterSO { get => _nonPlayableCharacterSO; }
    [SerializeField] private NpcPathRenderer _npcPathRenderer;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _cardboardSpriteRenderer;
    [SerializeField] private Transform _itemSpriteParent;
    [SerializeField] private SpriteRenderer _itemSpriteRenderer;
    [SerializeField] private SpriteRenderer _cardboardItemSpriteRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private NPCSound _npcSound;

    public event Action<Npc, bool> OnSelfSelected;

    private List<BuildingNode> _buildingNodesList = new();
    public List<BuildingNode> buildingNodesList { get => _buildingNodesList; }
    int _buildingNodesIndex = 0;
    private Vector3 _sideItemPosition;

    private Inventory _inventory;
    private BehaviorGraphAgent _behaviorAgent;
    private Selectable _selectable;
    private NavMeshAgent _agent;

    bool _isSelected = false;
    private bool? _wasMovingRight = false;
    private bool? _movingRight = false;
    private bool? _wasMovingUp = false;
    private bool? _movingUp = false;

    void Awake()
    {
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();
        _selectable = GetComponent<Selectable>();
        _inventory = GetComponent<Inventory>();

        _inventory.maxSameRessourceSpace = _nonPlayableCharacterSO.maxSameRessourceSpace;
        _inventory.maxDifferentRessourceAmount = _nonPlayableCharacterSO.maxSameRessourceSpace;

        _behaviorAgent.BlackboardReference.SetVariableValue("NPCSpeed", _nonPlayableCharacterSO.Speed);
        _behaviorAgent.BlackboardReference.SetVariableValue("PositionToGo", transform.position);

        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        _sideItemPosition = _itemSpriteParent.localPosition;
    }

    void OnEnable()
    {
        _selectable.onSelfSelected += HandleSelection;
    }

    void OnDisable()
    {
        _selectable.onSelfSelected -= HandleSelection;
    }

    private void Update()
    {
        if (_isSelected)
        {
            _behaviorAgent.BlackboardReference.GetVariableValue("Target", out BuildingNode buildingNode);
            if (buildingNode == null)
            {
                _behaviorAgent.BlackboardReference.GetVariableValue("PositionToGo", out Vector3 positionToGo);
                _npcPathRenderer.DrawPathNoBuildingNode(transform.position, positionToGo);
            }
            else if (_buildingNodesList.Count == 1)
            {
                _npcPathRenderer.DrawPathNoBuildingNode(transform.position, buildingNode.transform.position);
            }
        }
        _animator.SetFloat("SpeedMagnitude", _agent.velocity.magnitude);

        if (_agent.velocity.magnitude < 0.1f)
        {
            Idle();
            return;
        }

        _npcSound.PlayRandomWalk();

        Vector3 direction = _agent.velocity.normalized;

        direction = Quaternion.Euler(0, 45f, 0) * direction;

        _wasMovingRight = _movingRight;
        _movingRight = direction.x > 0;

        _wasMovingUp = _movingUp;
        _movingUp = direction.z > 0;

        if (_wasMovingRight != _movingRight) TurnSprite();
        if (_wasMovingUp != _movingUp) SwapFrontBackSprite();

        // Send to animator
        _animator.SetFloat("XInput", direction.x);
        _animator.SetFloat("YInput", direction.z);
    }

    private void Idle()
    {
        _movingUp = null;
        _movingRight = null;
        _itemSpriteParent.localPosition = new Vector3(0, _sideItemPosition.y, _sideItemPosition.z);
    }

    private void TurnSprite()
    {
        if (_movingRight == true)
        {
            _itemSpriteParent.localPosition = new Vector3(-_sideItemPosition.x, _sideItemPosition.y, _itemSpriteParent.localPosition.z);
            _spriteRenderer.transform.localRotation = Quaternion.Euler(0, 180, 0);
            _cardboardSpriteRenderer.transform.localRotation = Quaternion.Euler(0, 180, 0);
            _itemSpriteRenderer.transform.localRotation = Quaternion.Euler(0, 180, 0);
            _cardboardItemSpriteRenderer.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else if (_movingRight == false)
        {
            _itemSpriteParent.localPosition = new Vector3(_sideItemPosition.x, _sideItemPosition.y, _itemSpriteParent.localPosition.z);
            _spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _cardboardSpriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _itemSpriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _cardboardItemSpriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void SwapFrontBackSprite()
    {
        if (_movingUp == true)
        {
            _itemSpriteParent.localPosition = new Vector3(_itemSpriteParent.localPosition.x, _sideItemPosition.y, -_sideItemPosition.z);
        }
        else if (_movingUp == false)
        {
            _itemSpriteParent.localPosition = new Vector3(_itemSpriteParent.localPosition.x, _sideItemPosition.y, _sideItemPosition.z);
        }
    }

    public bool AddBuildingNode(BuildingNode node)
    {
        if (_buildingNodesList.Count == 0)
        {
            _buildingNodesList.Add(node);
            BuildingNodeHighlight(node, true);

            _behaviorAgent.BlackboardReference.SetVariableValue("Target", node);
            ResetNpcAgentExecution();

            return true;
        }

        BuildingNode previousNode = _buildingNodesList[^1];

        if (previousNode as OutputNode)
        {
            // previous is output
            if (node as OutputNode) return false;

            if (_buildingNodesList.Count == 1 && _inventory.weight != 0)
            {
                _behaviorAgent.BlackboardReference.SetVariableValue("Target", node);
                ResetNpcAgentExecution();
            }
        }
        else
        {
            // previous is input
            if (node as InputNode) return false;

            if (_buildingNodesList.Count == 1)
            {
                _buildingNodesList[0] = node;
                _buildingNodesList.Add(previousNode);

                BuildingNodeHighlight(node, true);
                BuildingNodeHighlight(previousNode, true);

                if (_inventory.weight == 0)
                {
                    _behaviorAgent.BlackboardReference.SetVariableValue("Target", node);
                    ResetNpcAgentExecution();
                }

                _npcPathRenderer.SetVisibilityOfLineRenderer(true);
                _npcPathRenderer.DrawPath(_buildingNodesList);

                return true;
            }
        }

        _buildingNodesList.Add(node);

        BuildingNodeHighlight(node, true);
        BuildingNodeHighlight(previousNode, true);

        _npcPathRenderer.SetVisibilityOfLineRenderer(true);
        _npcPathRenderer.DrawPath(_buildingNodesList);

        return true;
    }

    public void RemoveBuildingNode(BuildingNode node, bool moveNpc = true)
    {
        if (_buildingNodesList[_buildingNodesIndex] == node)
        {
            if (moveNpc)
            {
                NextNode();
                ResetNpcAgentExecution();
            }
        }

        int nodeIndex = _buildingNodesList.IndexOf(node);

        BuildingNodeHighlight(node, false);
        _buildingNodesList.Remove(node);

        // no need to remove one to the count since we removed the node
        if (nodeIndex >= _buildingNodesList.Count)
        {
            if (_buildingNodesList.Count == 0)
            {
                _npcPathRenderer.SetVisibilityOfLineRenderer(false);
                if (moveNpc)
                {
                    _behaviorAgent.BlackboardReference.SetVariableValue<BuildingNode>("Target", null);
                    _behaviorAgent.BlackboardReference.SetVariableValue("PositionToGo", transform.position);
                    ResetNpcAgentExecution();
                }
            }
            else
            {
                BuildingNodeHighlight(_buildingNodesList[^1], true);
            }

            return;
        }

        if (_buildingNodesList.Count == 1)
        {
            _npcPathRenderer.SetVisibilityOfLineRenderer(false);
            BuildingNode lastNode = _buildingNodesList[0];

            BuildingNodeHighlight(lastNode, true);

            if (moveNpc)
            {
                _behaviorAgent.BlackboardReference.SetVariableValue("Target", lastNode);
                ResetNpcAgentExecution();
            }
            return;
        }

        BuildingNodeHighlight(_buildingNodesList[nodeIndex], false);
        _buildingNodesList.Remove(_buildingNodesList[nodeIndex]);

        BuildingNodeHighlight(_buildingNodesList[^1], true);
    }

    public void NextNode()
    {
        if (_buildingNodesList.Count <= 1) return;

        _buildingNodesIndex++;
        if (_buildingNodesIndex >= _buildingNodesList.Count)
        {
            _buildingNodesIndex = 0;
        }

        _behaviorAgent.BlackboardReference.SetVariableValue("Target", _buildingNodesList[_buildingNodesIndex]);
    }

    public BuildingNode GetNextNode(BuildingNode node)
    {
        if (_buildingNodesList.Count == 1) return null;

        int nextNodeIndex = _buildingNodesIndex + 1;
        if (nextNodeIndex >= _buildingNodesList.Count)
        {
            nextNodeIndex = 0;
        }

        return _buildingNodesList[nextNodeIndex];
    }

    public void ResetNpcAgentExecution()
    {
        _behaviorAgent.Restart();
    }

    /// <summary>
    /// Tells the NPC to pick up a resource. Picks up max at a time.
    /// </summary>
    /// <param name="target">The target</param>
    public bool PickUp(OutputNode outputNode)
    {
        // need to give outputNode.RessourceAccesibleFromList(); the previous input node
        // and then delete the tempory code after it

        RessourceAndAmount[] ressourcesAndAmountToTake;
        InputNode inputNode = GetNextNode(outputNode) as InputNode;

        if (inputNode != null)
        {
            //Change this to next node, not previous!
            ressourcesAndAmountToTake = outputNode.RessourceAccesibleFromList(inputNode);
        }
        else
        {
            RessourceSO mostRessource = outputNode.inventory.MostRessourceInside();

            // NPC Idle handled in the Behaviour tree.
            if (mostRessource == null) return false;

            ressourcesAndAmountToTake = new RessourceAndAmount[] { new RessourceAndAmount(mostRessource, outputNode.inventory.ContainsHowMany(mostRessource)) };
        }

        if (ressourcesAndAmountToTake.Length == 0)
        {
            // there is nothing in the output that can be taken
            return false;
        }

        int ressourceOutput = 0;

        foreach (RessourceAndAmount ressourceAndAmount in ressourcesAndAmountToTake)
        {
            int ressourceInputToInventory = Mathf.Min(outputNode.HowMuchCanOutput(ressourceAndAmount.ressourceSO), _inventory.CanAddHowMany(ressourceAndAmount.ressourceSO));
            ressourceInputToInventory = Mathf.Min(ressourceAndAmount.amount, ressourceInputToInventory);

            RessourceAndAmount ressourceAndAmountToGet = new RessourceAndAmount(ressourceAndAmount.ressourceSO, ressourceInputToInventory);

            outputNode.Output(ressourceAndAmountToGet);
            _inventory.Add(ressourceAndAmountToGet, false);

            ressourceOutput += ressourceInputToInventory;
        }

        // NPC Idle handled in the Behaviour tree.
        if (ressourceOutput == 0 && _inventory.weight == 0) return false;

        _itemSpriteRenderer.sprite = _inventory.MostRessourceInside().sprite;
        _animator.SetBool("IsCarrying", true);
        return true;
    }

    /// <summary>
    /// Calls the NPC to drop of resource. Drops of all the resource at a time.
    /// </summary>
    /// <returns>Returns information about the dropped of resource.</returns>
    public bool DropOff(InputNode inputNode)
    {
        if (_inventory.ressourcesStored.Count == 0)
            return true; //Returns success here because this function will never succeed in such a senario, so better to move the NPC along.

        // need to change where we drop everything in our inventory and if the inventory still have item inside wait until there is none
        foreach (KeyValuePair<RessourceSO, int> ressourceAndSpace in _inventory.ressourcesStored)
        {
            int ressourceOutputFromInventory = inputNode.Input(new RessourceAndAmount(ressourceAndSpace));
            _inventory.Remove(new RessourceAndAmount(ressourceAndSpace.Key, ressourceOutputFromInventory));
        }

        if (_inventory.ressourcesStored.Count != 0)
        {
            // make npc idle
            // Wait for content to refresh using : inputNode.inventory.onContentChange += Function that check if we can input item again
            return false;
        }

        _inventory.ClearInventory();
        _itemSpriteRenderer.sprite = null;
        _animator.SetBool("IsCarrying", false);
        return true;
    }

    private void HandleSelection(bool isSelected)
    {
        _isSelected = isSelected;
        _npcPathRenderer.SetVisibilityOfLineRenderer(_isSelected);
        AllBuildingNodeHighlight(_isSelected);

        if (_isSelected)
        {
            _npcSound.PlayRandomMeow();
            _npcPathRenderer.DrawPath(buildingNodesList);
        }

        OnSelfSelected.Invoke(this, _isSelected);
    }
    void AllBuildingNodeHighlight(bool highlight)
    {
        foreach (BuildingNode buildingNode in _buildingNodesList)
        {
            BuildingNodeHighlight(buildingNode, highlight);
        }
    }
    void BuildingNodeHighlight(BuildingNode buildingNode, bool highlight)
    {
        if (!highlight)
        {
            buildingNode.RemoveHighlight();
            return;
        }
        if (_buildingNodesList[^1] == buildingNode)
        {
            buildingNode.HighlightLast();
        }
        else
        {
            buildingNode.HighlightSelected();
        }
    }

    public void ResetAllPath()
    {
        _buildingNodesIndex = 0;

        while (_buildingNodesList.Count != 0)
        {
            RemoveBuildingNode(_buildingNodesList[0], false);
        }
    }

    public void GoToPositionWithoutNode(Vector3 position)
    {
        ResetAllPath();
        _behaviorAgent.BlackboardReference.SetVariableValue<BuildingNode>("Target", null);
        _behaviorAgent.BlackboardReference.SetVariableValue("PositionToGo", position);
        ResetNpcAgentExecution();
    }
}
