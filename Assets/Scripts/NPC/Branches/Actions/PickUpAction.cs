using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PickUp", story: "[NPC] pick up item from [Target]", category: "Action", id: "b109b7ea51840a03659dadce8130fa81")]
public partial class PickUpAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> NPC;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnStart()
    {
        NPC npc = NPC.Value;

        npc.PickUp(Target);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        //TODO Place queueing code here!
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

