using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal.Commands;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(NavMeshAgent))]
public class Npc : Pawn
{
    [SerializeField] private NpcSO _nonPlayableCharacterSO;
    [SerializeField] private NpcPathRenderer _npcPathRenderer;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _cardboardSpriteRenderer;
    [SerializeField] private Transform _itemSpriteParent;
    [SerializeField] private SpriteRenderer _itemSpriteRenderer;
    [SerializeField] private SpriteRenderer _cardboardItemSpriteRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private NPCSound _npcSound;

    public event Action<Npc, bool> OnSelfSelected;
    public event System.Action OnTargetUnlinked;

    private List<NodeLink> _linkedNodeList = new();

    private GameObject _tempClickTarget;
    private Vector3 _sideItemPosition;

    private Inventory _inventory;
    private BehaviorGraphAgent _behaviorAgent;
    private Selectable _selectable;
    private NavMeshAgent _agent;

    private bool _isSelected = false;
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
        _behaviorAgent.BlackboardReference.SetVariableValue("NPCWaitDuration", _nonPlayableCharacterSO.WaitDuration);

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
        if (_agent.velocity.magnitude < 0.1f)
        {
            Idle();
            return;
        }

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

        if(direction.x != 0 || direction.z != 0)
            _npcSound.PlayRandomWalk();
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

    public void LinkNode(NodeLink nodeLink)
    {
        _linkedNodeList.Add(nodeLink);
        Link(nodeLink.NodeA);
        Link(nodeLink.NodeB);

        if (_isSelected)
            _npcPathRenderer.DrawPathBetween(_linkedNodeList);

        if (_linkedNodeList.Count == 1)
        {
            // temporary >
            InputNode inputNode1 = nodeLink.NodeA.GetComponent<InputNode>();
            InputNode inputNode2 = nodeLink.NodeB.GetComponent<InputNode>();
            if (inputNode1)
                _behaviorAgent.BlackboardReference.SetVariableValue("PreviousTarget", inputNode1.gameObject);

            if (inputNode2)
                _behaviorAgent.BlackboardReference.SetVariableValue("PreviousTarget", inputNode2.gameObject);
            // < temporary

            //_behaviorAgent.BlackboardReference.SetVariableValue("PreviousTarget", nodeLink.NodeB);
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

            if (target.Value == gameObject)
                OnTargetUnlinked?.Invoke();

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
    public bool PickUp(OutputNode outputNode)
    {
        // need to give outputNode.RessourceAccesibleFromList(); the previous input node
        // and then delete the tempory code after it
        _behaviorAgent.GetVariable("PreviousTarget", out BlackboardVariable<GameObject> previousTarget);
        RessourceAndAmount[] ressourcesAndAmountToTake;
        InputNode inputNode = previousTarget.Value.GetComponent<InputNode>();

        if (inputNode != null)
        {
            //Change this to next node, not previous!
            ressourcesAndAmountToTake = outputNode.RessourceAccesibleFromList(inputNode);
        }
        else
        {
            Debug.LogWarning("Walked from output node -> output node");
            return true; //Returns success here because this function will never succeed in such a senario, so better to move the NPC along.
        }

        if (ressourcesAndAmountToTake.Length == 0)
        {
            // there is nothing in the output that can be taken
            return false;
        }

        int ressourceOutput = 0;
        
        foreach (RessourceAndAmount ressourceAndAmount in ressourcesAndAmountToTake)
        {
            Debug.Log(ressourceAndAmount.ressourceSO + "  " + ressourceAndAmount.amount);
            int ressourceInputToInventory = Mathf.Min(outputNode.HowMuchCanOutput(ressourceAndAmount.ressourceSO), _inventory.CanAddHowMany(ressourceAndAmount.ressourceSO));
            ressourceInputToInventory = Mathf.Min(ressourceAndAmount.amount, ressourceInputToInventory);

            RessourceAndAmount ressourceAndAmountToGet = new RessourceAndAmount(ressourceAndAmount.ressourceSO, ressourceInputToInventory);
            
            outputNode.Output(ressourceAndAmountToGet);
            _inventory.Add(ressourceAndAmountToGet, false);

            ressourceOutput += ressourceInputToInventory;
        }

        if (ressourceOutput == 0)
        {
            // NPC Idle handled in the Behaviour tree.
            return false;
        }

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

        if(isSelected)
            _npcSound.PlayRandomMeow();
        
        if (isSelected && _linkedNodeList.Count > 0)
        {
            _npcPathRenderer.DrawPathBetween(_linkedNodeList);
        }

        OnSelfSelected.Invoke(this, isSelected);
    }
}
