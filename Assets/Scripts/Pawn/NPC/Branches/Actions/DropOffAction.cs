using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DropOff", story: "[Npc] drops of item to [Target]", category: "Action", id: "c4d3a837c214a1d6813070cdfcb68ebc")]
public partial class DropOfAction : Action
{
    [SerializeReference] public BlackboardVariable<Npc> Npc;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private Npc _npc;
    private InputNode _inputNode;
    private bool _dropOffSucceeded;
    private bool _canTryDropOff = true;
    Timer _dropOffTimer;

    protected override Status OnStart()
    {
        _dropOffSucceeded = false;
        _canTryDropOff = true;

        _npc = Npc.Value;

        _inputNode = Target.Value.GetComponent<InputNode>();

        if (_inputNode)
        {
            _dropOffTimer = new Timer(_npc.nonPlayableCharacterSO.WaitDuration);

            _inputNode.inventory.onInventoryChange += InventoryChange;

            return Status.Running;
        }

        return Status.Failure;
    }

    private void NPCDropOff()
    {
        if (_npc.DropOff(_inputNode))
        {
            _inputNode.inventory.onInventoryChange -= InventoryChange;
            _dropOffSucceeded = true;
        }

        _canTryDropOff = false;
    }

    void InventoryChange()
    {
        if (!_canTryDropOff)
        {
            _dropOffTimer.Restart();
            _canTryDropOff = true;
        }
    }

    protected override Status OnUpdate()
    {
        if (_canTryDropOff && _dropOffTimer.IsOver())
        {
            NPCDropOff();

            if (_dropOffSucceeded)
                return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _npc.NextNode();
        _dropOffSucceeded = false;

        if (_inputNode) _inputNode.inventory.onContentChange -= InventoryChange;
        _inputNode = null;
    }
}

