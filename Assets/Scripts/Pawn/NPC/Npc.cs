using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(Selectable))]
public class Npc : Pawn
{
    [SerializeField] private NpcSO _nonPlayableCharacterSO;
    [SerializeField] private NpcPathRenderer _npcPathRenderer;
    [SerializeField] private SpriteRenderer _itemSpriteRenderer;
    public event Action<Npc, bool> OnSelfSelected;
    private List<NodeLink> _linkedNodeList = new();

    private GameObject _tempClickTarget;
    private RessourceAndAmount _carrying;
    private BehaviorGraphAgent _behaviorAgent;
    private Selectable _selectable;
    private bool _isSelected = false;
    NavMeshAgent _agent;

    void Awake()
    {
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();
        _selectable = GetComponent<Selectable>();

        _behaviorAgent.BlackboardReference.SetVariableValue("NPCSpeed", _nonPlayableCharacterSO.Speed);
        _behaviorAgent.BlackboardReference.SetVariableValue("NPCWaitDuration", _nonPlayableCharacterSO.WaitDuration);

        _carrying = null;
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    void OnEnable()
    {
        _selectable.onSelfSelected += HandleSelection;
    }

    void OnDisable()
    {
        _selectable.onSelfSelected -= HandleSelection;
    }

    public void LinkNode(NodeLink nodeLink)
    {
        _linkedNodeList.Add(nodeLink);
        Link(nodeLink.NodeA);
        Link(nodeLink.NodeB);

        if (_isSelected)
            _npcPathRenderer.DrawPathBetween(_linkedNodeList);

        if(_linkedNodeList.Count == 1)
        {
            _behaviorAgent.BlackboardReference.SetVariableValue("PreviousTarget", nodeLink.NodeB);
        }
    }

    public void UnlinkNode(NodeLink nodeLink)
    {
        bool exists = _linkedNodeList.Exists(l => l == nodeLink);

        if (!exists)
            return;

        _linkedNodeList.Remove(nodeLink);
        Unlink(nodeLink.NodeA); //Add check so the node isn't used elsewhere
        Unlink(nodeLink.NodeB); //Add check so the node isn't used elsewhere


        bool isOutputNode = nodeLink.NodeA.TryGetComponent<OutputNode>(out _);

        //This is needed so that we in NonPlayableCharacterManager store the node link in only one direction
        GameObject nodeA = isOutputNode ? nodeLink.NodeA : nodeLink.NodeB;
        GameObject nodeB = isOutputNode ? nodeLink.NodeB : nodeLink.NodeA;

        OutputNode outputNode = nodeA.GetComponent<OutputNode>();
        InputNode inputNode = nodeB.GetComponent<InputNode>();

        outputNode.Unlink(inputNode);
        inputNode.Unlink(outputNode);

        if (_isSelected)
            _npcPathRenderer.DrawPathBetween(_linkedNodeList);
    }

    /// <summary>
    /// To be used when no GameObject is present.
    /// </summary>
    /// <param name="position"></param>
    public void Link(Vector3 position)
    {
        if (_tempClickTarget == null)
        {
            //Please forgive me for this crime of a code.
            _tempClickTarget = new("ClickTarget");
            _tempClickTarget.transform.SetParent(GameManager.instance.nonPlayableCharacter.transform, worldPositionStays: true);
        }

        for (int i = _linkedNodeList.Count - 1; i >= 0; i--)
        {
            UnlinkNode(_linkedNodeList[i]);
        }

        _tempClickTarget.transform.position = position;
        Link(_tempClickTarget);
    }

    /// <summary>
    /// Unlinks current target and sets it equal to provided GameObject
    /// </summary>
    /// <param name="gameObject">Position to walk</param>
    public void Unlink(GameObject gameObject)
    {
        if (_behaviorAgent.BlackboardReference.GetVariable("WalkingPoints", out BlackboardVariable<List<GameObject>> walkingPoints))
        {
            _behaviorAgent.BlackboardReference.GetVariable("WalkingPointsIndex", out BlackboardVariable<int> index);
            _behaviorAgent.BlackboardReference.GetVariable("Target", out BlackboardVariable<GameObject> target);

            int currentIndex = index.Value;
            int removeIndex = walkingPoints.Value.IndexOf(gameObject);

            if (removeIndex < 0)
                return;

            if (removeIndex == currentIndex)
                currentIndex++;

            walkingPoints.Value.RemoveAt(removeIndex);

            currentIndex = Mathf.Clamp(currentIndex, 0, walkingPoints.Value.Count - 1);

            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", walkingPoints.Value);

            if (walkingPoints.Value.Count > 0)
            {
                GameObject nextTarget = walkingPoints.Value[currentIndex];
                _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPointsIndex", currentIndex);
                _behaviorAgent.BlackboardReference.SetVariableValue("Target", nextTarget);
            }
            else
            {
                _behaviorAgent.BlackboardReference.SetVariableValue<GameObject>("Target", null);
                _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPointsIndex", 0);
            }
        }
    }

    /// <summary>
    /// Tells the NPC to pick up a resource. Picks up max at a time.
    /// </summary>
    /// <param name="target">The target</param>
    public bool PickUp(GameObject target)
    {
        if (!target.TryGetComponent<OutputNode>(out var outputNode))
        {
            Debug.LogWarning("NPC tried to pickup at non output node!");
            return true; //Returns success here because this function will never succeed in such a senario, so better to move the NPC along.
        }

        // need to give outputNode.RessourceAccesibleFromList(); the previous input node
        // and then delete the tempory code after it
        _behaviorAgent.GetVariable("PreviousTarget", out BlackboardVariable<GameObject> previousTarget);
        RessourceAndAmount[] ressourcesAndAmountToTake;

        if (previousTarget.Value.TryGetComponent(out InputNode previousInputNode))
        {
            ressourcesAndAmountToTake = outputNode.RessourceAccesibleFromList(previousInputNode);
        }
        else
        {
            Debug.LogWarning("Walked from output node -> output node");
            return true; //Returns success here because this function will never succeed in such a senario, so better to move the NPC along.
        }

        int carryAmount = 0;
        // the npc carry something
        if (_carrying != null)
        {
            // update carry amount
            if (ressourcesAndAmountToTake.Any(r => r.ressourceSO == _carrying.ressourceSO))
                carryAmount = _carrying.amount;
            else
                return false;

            // !!!
            // Check if the _carrying.ressourceSO is inside the ressourcesAndAmountToTake.
            // if it is inside then change the ressourceAndAmountToTake to be the same as the carrying one. Else { it is the player fault
            // because if all securities are added, this should only happen by moving an npc with an item that shouldn't go there.
            // We need remove all link and make npc idol, the player tried to unput ressource that wasn't approriate }
        }
        else
        {
            _carrying = new RessourceAndAmount(ressourcesAndAmountToTake[0].ressourceSO, 0);
        }

        // make sure we can t take more than the limit
        _carrying.amount = _nonPlayableCharacterSO.MaxCarryingCapacity - carryAmount;

        int ressourceOutput = outputNode.Output(ressourcesAndAmountToTake[0]);
        if (ressourceOutput == 0)
        {
            // NPC Idle handled in the Behaviour tree.
            return false;
        }

        _itemSpriteRenderer.sprite = _carrying.ressourceSO.sprite;
        return true;
    }

    /// <summary>
    /// Calls the NPC to drop of resource. Drops of all the resource at a time.
    /// </summary>
    /// <returns>Returns information about the dropped of resource.</returns>
    public bool DropOff(GameObject target)
    {
        if (_carrying == null)
            return true; //Returns success here because this function will never succeed in such a senario, so better to move the NPC along.

        if (!target.TryGetComponent<InputNode>(out var inputNode))
        {
            Debug.LogWarning("NPC tried to drop of at non input node!");
            return true;  //Returns success here because this function will never succeed in such a senario, so better to move the NPC along.
        }

        // need to change where we drop everything in our inventory and if the inventory still have item inside wait until there is none
        int ressourceInput = inputNode.Input(_carrying);
        if (ressourceInput != _carrying.amount)
        {
            _carrying.amount -= ressourceInput;
            // make npc idle
            // Wait for content to refresh using : inputNode.inventory.onContentChange += Function that check if we can input item again
            return false;
        }

        _carrying = null;
        _itemSpriteRenderer.sprite = null;
        return true;
    }

    /// <summary>
    /// Links the GameObject to the NPC
    /// </summary>
    /// <param name="gameObject">Position to walk to</param>
    private void Link(GameObject gameObject)
    {
        if (_behaviorAgent.BlackboardReference.GetVariable("WalkingPoints", out BlackboardVariable<List<GameObject>> walkingPoints))
        {
            //Stops the duplicate linking of same gameobject resulting in multidropof/pickup
            if (walkingPoints.Value.Count > 0 && walkingPoints.Value[^1] == gameObject)
                return;

            walkingPoints.Value.Add(gameObject);
            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", walkingPoints.Value);

            if (_behaviorAgent.BlackboardReference.GetVariable("Target", out BlackboardVariable<GameObject> target))
            {
                if (target.Value == null)
                {
                    _behaviorAgent.BlackboardReference.SetVariableValue("Target", gameObject);
                }
            }
        }
        else
        {
            // If the variable doesn't exist, create a new one
            var newList = new List<GameObject> { gameObject };
            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", newList);
        }
    }

    private void HandleSelection(bool isSelected)
    {
        _isSelected = isSelected;
        _npcPathRenderer.SetVisibilityOfLineRenderer(isSelected);

        if (isSelected && _linkedNodeList.Count > 0)
        {
            _npcPathRenderer.DrawPathBetween(_linkedNodeList);
        }

        OnSelfSelected.Invoke(this, isSelected);
    }
}
