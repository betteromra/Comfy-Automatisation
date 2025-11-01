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

    protected override Status OnStart()
    {
        Npc npc = Npc.Value;

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

