using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public struct Carrying
{
    public RessourceSO CurrenltyCarrying;
    public int Amount;

    public Carrying(RessourceSO carrying, int amount)
    {
        CurrenltyCarrying = carrying;
        Amount = amount;
    }
}

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(NPCPathRenderer))]
public class NPC : MonoBehaviour
{
    public event Action<NPC, bool> OnSelfSelected;
    [SerializeField] private int _maxCarryingCapacity = 1;
    private Carrying _carrying;
    private BehaviorGraphAgent _behaviorAgent;
    private NPCPathRenderer _npcPathRenderer;

    void Awake()
    {
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();
        _npcPathRenderer = GetComponent<NPCPathRenderer>();
        
        _carrying = new(null, 0);

        GetComponent<Selectable>().onSelfSelected += HandleSelection;
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

            Debug.Log(gameObject.name);

            if(_behaviorAgent.BlackboardReference.GetVariable("Target", out BlackboardVariable<GameObject> target))
            {
                if(target.Value == null)
                {
                    _behaviorAgent.BlackboardReference.SetVariableValue("Target", gameObject);
                }
            }
        }
        else
        {
            // If the variable doesn't exist, create a new one
            var newList = new List<GameObject> { gameObject };
            Debug.Log("Doesnt'");
            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", newList);
        }
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
        if (_carrying.Amount >= _maxCarryingCapacity)
            return;

        if (!target.TryGetComponent<OutputNode>(out var outputNode))
        {
            Debug.LogWarning("NPC tried to pickup at non output node!");
            return;
        }

        Inventory inventory = outputNode.inventory;
        if (inventory == null)
        {
            Debug.LogError("OutputNode inventory is set to null!");
        }

        if (_carrying.CurrenltyCarrying == null)
        {
            //Currently the NPC chooses the resource based on amount.
            RessourceSO ressource = inventory.MostRessourceInside();
            _carrying.CurrenltyCarrying = ressource;
        }

        RessourceAndAmount ressourceAndAmount = new RessourceAndAmount(_carrying.CurrenltyCarrying, 1);
        if (inventory.ContainsAmount(ressourceAndAmount))
        {
            while (_carrying.Amount < _maxCarryingCapacity && 0 < inventory.Contains(_carrying.CurrenltyCarrying))
            {
                inventory.Remove(ressourceAndAmount);
                _carrying.Amount++;
            }
        }

        Debug.Log($"NPC currently carrying {_carrying.CurrenltyCarrying.actualName}, {_carrying.Amount}");
    }

    /// <summary>
    /// Calls the NPC to drop of resource. Drops of all the resource at a time.
    /// </summary>
    /// <returns>Returns information about the dropped of resource.</returns>
    public void DropOff(GameObject target)
    {
        if (_carrying.Amount <= 0)
            return;

        if (!target.TryGetComponent<InputNode>(out var inputNode))
        {
            Debug.LogWarning("NPC tried to drop of at non input node!");
            return;
        }

        Inventory inventory = inputNode.inventory;
        if (inventory == null)
        {
            Debug.LogError("InputNode inventory is set to null!");
        }

        if (inventory.ContainsAmount(new RessourceAndAmount(_carrying.CurrenltyCarrying, 0)))
            return;

        inventory.Add(new RessourceAndAmount(_carrying.CurrenltyCarrying, _carrying.Amount));

        _carrying.Amount = 0;
        _carrying.CurrenltyCarrying = null;
    }

    private void HandleSelection(bool isSelected)
    {
        _npcPathRenderer.SetVisibilityOfLineRenderer(isSelected);

        if (isSelected)
        {
            CalculatePath();
        }

        OnSelfSelected.Invoke(this, isSelected);
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
