using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DropOf", story: "[NPC] drops of item to [Target]", category: "Action", id: "c4d3a837c214a1d6813070cdfcb68ebc")]
public partial class DropOfAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> NPC;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnStart()
    {
        NPC npc = NPC.Value;

        npc.DropOff(Target.Value);
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

