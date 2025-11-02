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
    private RessourceAndAmount _carrying;
    private BehaviorGraphAgent _behaviorAgent;
    private bool _isSelected = false;

    void Awake()
    {
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();

        _behaviorAgent.BlackboardReference.SetVariableValue("Npc", this);
        _behaviorAgent.BlackboardReference.SetVariableValue("NPCSpeed", _nonPlayableCharacterSO.Speed);
        _behaviorAgent.BlackboardReference.SetVariableValue("NPCWaitDuration", _nonPlayableCharacterSO.WaitDuration);
        
        _carrying = null;

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
        if (_carrying.amount >= _nonPlayableCharacterSO.MaxCarryingCapacity)
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

        if (_carrying.ressourceSO == null)
        {
            //Currently the NPC chooses the resource based on amount.
            RessourceSO ressource = inventory.MostRessourceInside();
            _carrying.ressourceSO = ressource;
        }

        RessourceAndAmount ressourceAndAmount = new RessourceAndAmount(_carrying.ressourceSO);
        if (inventory.ContainsAmount(ressourceAndAmount))
        {
            while (_carrying.amount < _nonPlayableCharacterSO.MaxCarryingCapacity && 0 < inventory.Contains(_carrying.ressourceSO))
            {
                inventory.Remove(ressourceAndAmount);
                _carrying.amount++;
            }
        }

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

        Inventory inventory = inputNode.inventory;
        if (inventory == null)
        {
            Debug.LogError("InputNode inventory is set to null!");
        }

        if (inventory.ContainsAmount(_carrying))
            return;

        inventory.Add(_carrying);

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
        while(_isSelected)
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
