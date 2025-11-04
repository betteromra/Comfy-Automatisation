using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DropOf", story: "[Npc] drops of item to [Target]", category: "Action", id: "c4d3a837c214a1d6813070cdfcb68ebc")]
public partial class DropOfAction : Action
{
    [SerializeReference] public BlackboardVariable<Npc> Npc;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private Npc _npc;
    private InputNode _inputNode;
    private bool _pickupSucceeded;

    protected override Status OnStart()
    {
        _pickupSucceeded = false;

        _npc = Npc.Value;
        _npc.OnTargetUnlinked += TargetUnlik;
        
        bool success = _npc.DropOff(Target.Value);

        if (Target.Value.TryGetComponent(out InputNode inputNode) && !success)
        {
            _inputNode = inputNode;
            _inputNode.inventory.onContentChange += NPCDropOff;
            return Status.Running;
        }

        return success ? Status.Success : Status.Failure;
    }

    private void TargetUnlik() => _pickupSucceeded = true;

    private void NPCDropOff()
    {
        bool success = _npc.DropOff(Target.Value);

        if (success && _inputNode != null)
        {
            _inputNode.inventory.onContentChange -= NPCDropOff;
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
        if (_inputNode != null)
        {
            _inputNode.inventory.onContentChange -= NPCDropOff;
            _inputNode = null;
        }
    }
}

