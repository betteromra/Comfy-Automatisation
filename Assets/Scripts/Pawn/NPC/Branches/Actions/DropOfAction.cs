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
    private bool _dropOffSucceeded;

    protected override Status OnStart()
    {
        _dropOffSucceeded = false;

        _npc = Npc.Value;
        _npc.OnTargetUnlinked += TargetUnlik;

        _inputNode = Target.Value.GetComponent<InputNode>();

        if (_inputNode)
        {
            bool success = _npc.DropOff(_inputNode);

            if (!success)
            {
                _inputNode.inventory.onContentChange += NPCDropOff;
                return Status.Running;
            }

            return Status.Success;
        }

        return Status.Failure;
    }

    private void TargetUnlik() => _dropOffSucceeded = true;

    private void NPCDropOff()
    {
        bool success = _npc.DropOff(_inputNode);

        Debug.Log("trying dropoff : " + success);
        if (success)
        {
            _inputNode.inventory.onContentChange -= NPCDropOff;
            _dropOffSucceeded = true;
        }
    }

    protected override Status OnUpdate()
    {
        if (_dropOffSucceeded)
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _dropOffSucceeded = false;
        _npc.OnTargetUnlinked -= TargetUnlik;

        if (_inputNode) _inputNode.inventory.onContentChange -= NPCDropOff;
        _inputNode = null;
    }
}

