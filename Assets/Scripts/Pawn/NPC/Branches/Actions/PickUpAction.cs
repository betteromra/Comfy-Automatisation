using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PickUp", story: "[Npc] pick up item from [Target]", category: "Action", id: "b109b7ea51840a03659dadce8130fa81")]
public partial class PickUpAction : Action
{
    [SerializeReference] public BlackboardVariable<Npc> Npc;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private Npc _npc;
    private OutputNode _outputNode;
    private bool _pickupSucceeded;

    protected override Status OnStart()
    {
        _pickupSucceeded = false;

        _npc = Npc.Value;
        _npc.OnTargetUnlinked += TargetUnlik;

        _outputNode = Target.Value.GetComponent<OutputNode>();

        if (_outputNode)
        {
            bool success = _npc.PickUp(_outputNode);

            if (!success)
            {
                _outputNode.inventory.onContentChange += NPCPickup;
                return Status.Running;
            }

            return Status.Success;
        }

        return Status.Failure;
    }

    private void TargetUnlik() => _pickupSucceeded = true;

    private void NPCPickup()
    {
        bool success = _npc.PickUp(_outputNode);

        Debug.Log("trying pickup : " + success);
        if (success)
        {
            _outputNode.inventory.onContentChange -= NPCPickup;
            _pickupSucceeded = true;
        }
    }

    protected override Status OnUpdate()
    {
        if (_pickupSucceeded)
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _pickupSucceeded = false;
        _npc.OnTargetUnlinked -= TargetUnlik;

        if (_outputNode) _outputNode.inventory.onContentChange -= NPCPickup;
        _outputNode = null;
    }
}

