using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PickUp", story: "[Npc] pick up item from [Target]", category: "Action", id: "b109b7ea51840a03659dadce8130fa81")]
public partial class PickUpAction : Action
{
    [SerializeReference] public BlackboardVariable<Npc> Npc;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private Npc _npc;
    private OutputNode _outputNode;
    private bool _pickupSucceeded;
    private bool _canTryPickup = true;
    Timer _pickUpTimer;

    protected override Status OnStart()
    {
        _pickupSucceeded = false;
        _canTryPickup = true;

        _npc = Npc.Value;

        _outputNode = Target.Value.GetComponent<OutputNode>();

        if (_outputNode)
        {
            _pickUpTimer = new Timer(_npc.nonPlayableCharacterSO.WaitDuration);

            _outputNode.inventory.onInventoryChange += InventoryChange;

            return Status.Running;
        }

        return Status.Failure;
    }

    private void NPCPickup()
    {
        if (_npc.PickUp(_outputNode))
        {
            _outputNode.inventory.onInventoryChange -= InventoryChange;
            _pickupSucceeded = true;
        }

        _canTryPickup = false;
    }

    void InventoryChange()
    {
        if (!_canTryPickup)
        {
            _pickUpTimer.Restart();
            _canTryPickup = true;
        }
    }

    protected override Status OnUpdate()
    {
        if (_canTryPickup && _pickUpTimer.IsOver())
        {
            NPCPickup();

            if (_pickupSucceeded)
                return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _npc.NextNode();
        _pickupSucceeded = false;

        if (_outputNode) _outputNode.inventory.onContentChange -= InventoryChange;
        _outputNode = null;
    }
}

