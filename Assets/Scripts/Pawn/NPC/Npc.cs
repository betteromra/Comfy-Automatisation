using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(Selectable))]
public class Npc : Pawn
{
    [SerializeField] private NpcSO _nonPlayableCharacterSO;
    [SerializeField] private NpcPathRenderer _npcPathRenderer;
    public event Action<Npc, bool> OnSelfSelected;
    private GameObject _tempClickTarget;
    private RessourceAndAmount _carrying;
    private BehaviorGraphAgent _behaviorAgent;
    private bool _isSelected = false;

    void Awake()
    {
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();

        _behaviorAgent.BlackboardReference.SetVariableValue("NPCSpeed", _nonPlayableCharacterSO.Speed);
        _behaviorAgent.BlackboardReference.SetVariableValue("NPCWaitDuration", _nonPlayableCharacterSO.WaitDuration);

        _carrying = null;
    }

    void OnEnable()
    {
        GetComponent<Selectable>().onSelfSelected += HandleSelection;
    }

    void OnDisable()
    {
        GetComponent<Selectable>().onSelfSelected -= HandleSelection;
    }

    /// <summary>
    /// Links the GameObject to the NPC
    /// </summary>
    /// <param name="gameObject">Position to walk to</param>
    public void Link(GameObject gameObject)
    {
        if (_behaviorAgent.BlackboardReference.GetVariable("WalkingPoints", out BlackboardVariable<List<GameObject>> walkingPoints))
        {
            if (walkingPoints.Value.Count == 2)
            {
                Unlink(gameObject);
                CalculatePath();
                return;
            }

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

            foreach (var item in walkingPoints.Value)
            {
                Debug.LogWarning(item);
            }

            Debug.LogWarning(index);
            Debug.LogWarning(target.Value);

            walkingPoints.Value[index] = gameObject;
            target.Value = gameObject;

            foreach (var item in walkingPoints.Value)
            {
                Debug.LogWarning(item);
            }

            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", walkingPoints.Value);
        }
    }

    /// <summary>
    /// Tells the NPC to pick up a resource. Picks up max at a time.
    /// </summary>
    /// <param name="target">The target</param>
    public void PickUp(GameObject target)
    {
        if (!target.TryGetComponent<OutputNode>(out var outputNode))
        {
            Debug.LogWarning("NPC tried to pickup at non output node!");
            return;
        }

        // need to give outputNode.RessourceAccesibleFromList(); the previous input node
        // and then delete the tempory code after it
        RessourceAndAmount[] ressourcesAndAmountToTake = /* outputNode.RessourceAccesibleFromList(); */ new RessourceAndAmount[] { new RessourceAndAmount(outputNode.inventory.MostRessourceInside()) };

        int carryAmount = 0;
        // the npc carry something
        if (_carrying != null)
        {
            // update carry amount
            carryAmount = _carrying.amount;

            // !!!
            // Check if the _carrying.ressourceSO is inside the ressourcesAndAmountToTake.
            // if it is inside then change the ressourceAndAmountToTake to be the same as the carrying one. Else { it is the player fault
            // because if all securities are added, this should only happen by moving an npc with an item that shouldn't go there.
            // We need remove all link and make npc idol, the player tried to unput ressource that wasn't approriate }
        }

        // need to change all the line under this function where we pick every ressourcesAndAmountToTake we can take in our inventory

        // make sure we can t take more than the limit
        ressourcesAndAmountToTake[0].amount = _nonPlayableCharacterSO.MaxCarryingCapacity - carryAmount;

        int ressourceOutput = outputNode.Output(ressourcesAndAmountToTake[0]);
        if (ressourceOutput == 0)
        {
            // make npc idle
            // Wait for content to refresh using : outputNode.inventory.onContentChange += Function that check if we can output item again
            return;
        }

        _carrying = new RessourceAndAmount(ressourcesAndAmountToTake[0].ressourceSO, ressourceOutput);

        //Debug.Log($"NPC currently carrying {_carrying.CurrenltyCarrying.actualName}, {_carrying.Amount}");
    }

    /// <summary>
    /// Calls the NPC to drop of resource. Drops of all the resource at a time.
    /// </summary>
    /// <returns>Returns information about the dropped of resource.</returns>
    public void DropOff(GameObject target)
    {
        if (_carrying == null)
            return;

        if (!target.TryGetComponent<InputNode>(out var inputNode))
        {
            Debug.LogWarning("NPC tried to drop of at non input node!");
            return;
        }

        // need to change where we drop everything in our inventory and if the inventory still have item inside wait until there is none
        int ressourceInput = inputNode.Input(_carrying);
        if (ressourceInput != _carrying.amount)
        {
            _carrying.amount -= ressourceInput;
            // make npc idle
            // Wait for content to refresh using : inputNode.inventory.onContentChange += Function that check if we can input item again
            return;
        }

        _carrying = null;
    }

    private void HandleSelection(bool isSelected)
    {
        _isSelected = isSelected;
        _npcPathRenderer.SetVisibilityOfLineRenderer(isSelected);

        if (isSelected)
        {
            //CalculatePath();
            //Trying this out, but if it becomes to computationally expensive, just uncomment the line above.
            StartCoroutine(DrawNPCPath());
        }

        OnSelfSelected.Invoke(this, isSelected);
    }

    private IEnumerator DrawNPCPath()
    {
        WaitForSeconds wait = new(0.5f);
        while (_isSelected)
        {
            CalculatePath();
            yield return wait;
        }
    }

    private void CalculatePath()
    {
        if (_behaviorAgent.BlackboardReference.GetVariable("WalkingPoints", out BlackboardVariable<List<GameObject>> walkingPoints))
        {
            List<GameObject> walkingPointsList = walkingPoints.Value;
            if (walkingPointsList.Count == 2)
            {
                _npcPathRenderer.DrawPathThroughNPC(walkingPointsList[0].transform.position, walkingPointsList[1].transform.position);
            }
        }
    }
}
