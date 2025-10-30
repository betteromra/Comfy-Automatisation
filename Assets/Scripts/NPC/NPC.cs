using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

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
public class NPC : MonoBehaviour
{
    [SerializeField] private int _maxCarryingCapacity = 1;

    private Carrying _carrying;

    private BehaviorGraphAgent _behaviorAgent;

    void Awake()
    {
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();
        _carrying = new(null, 0);
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
                return;
            }

            walkingPoints.Value.Add(gameObject);

            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", walkingPoints);
        }
        else
        {
            // If the variable doesn't exist, create a new one
            var newList = new List<GameObject> { gameObject };
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
            _behaviorAgent.BlackboardReference.GetVariable("Index", out BlackboardVariable<int> index);
            _behaviorAgent.BlackboardReference.GetVariable("Target", out BlackboardVariable<GameObject> target);
            
            walkingPoints.Value[index] = gameObject;
            target.Value = gameObject;

            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", walkingPoints);
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

        RessourceSO ressource = outputNode.ressourceSO;
        if(ressource == null)
        {
            Debug.LogWarning("OutputNode resource set to null!");
        }

        if (_carrying.CurrenltyCarrying == null)
        {
            _carrying.CurrenltyCarrying = ressource;
        }

        if (_carrying.CurrenltyCarrying == ressource)
        {
            Inventory inventory = outputNode.inventory;

            while (_carrying.Amount < _maxCarryingCapacity && 0 < inventory.Contains(ressource))
            {
                inventory.Remove(ressource, 1);
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

        if (inputNode.ressourceSO != _carrying.CurrenltyCarrying)
            return;

        _carrying.Amount = 0;
        _carrying.CurrenltyCarrying = null;

        inputNode.inventory.Add(_carrying.CurrenltyCarrying, _carrying.Amount);
    }
}
